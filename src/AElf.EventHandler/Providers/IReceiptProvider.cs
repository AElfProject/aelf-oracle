using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AElf.Client.Bridge;
using AElf.Client.Core.Options;
using AElf.Client.MerkleTreeContract;
using AElf.Client.Oracle;
using AElf.Contracts.MerkleTreeContract;
using AElf.Contracts.Oracle;
using AElf.EventHandler.Workers;
using AElf.Nethereum.Bridge;
using AElf.Nethereum.Core;
using AElf.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.DependencyInjection;

namespace AElf.EventHandler;

public interface IReceiptProvider
{
    Task ExecuteAsync();
}

public class ReceiptProvider : IReceiptProvider, ITransientDependency
{
    private readonly BridgeOptions _bridgeOptions;
    private readonly IBridgeInService _bridgeInService;
    private readonly INethereumService _nethereumService;
    private readonly IOracleService _oracleService;
    private readonly AElfChainAliasOptions _aelfChainAliasOptions;
    private readonly IBridgeService _bridgeContractService;
    private readonly IMerkleTreeContractService _merkleTreeContractService;
    private readonly ILatestQueriedReceiptCountProvider _latestQueriedReceiptCountProvider;
    private readonly ILogger<ReceiptProvider> _logger;
    private readonly AElfContractOptions _contractOptions;
    private readonly BlockConfirmationOptions _blockConfirmationOptions;
    
    public ReceiptProvider(
        IOptionsSnapshot<BridgeOptions> bridgeOptions,
        IOptionsSnapshot<AElfChainAliasOptions> aelfChainAliasOption,
        IOptionsSnapshot<BlockConfirmationOptions> blockConfirmation,
        IOptionsSnapshot<AElfContractOptions> contractOptions,
        IBridgeInService bridgeInService,
        INethereumService nethereumService,
        IOracleService oracleService,
        IBridgeService bridgeService,
        IMerkleTreeContractService merkleTreeContractService,
        ILatestQueriedReceiptCountProvider latestQueriedReceiptCountProvider,
        ILogger<ReceiptProvider> logger)
    {
        _bridgeOptions = bridgeOptions.Value;
        _bridgeInService = bridgeInService;
        _nethereumService = nethereumService;
        _oracleService = oracleService;
        _bridgeContractService = bridgeService;
        _merkleTreeContractService = merkleTreeContractService;
        _aelfChainAliasOptions = aelfChainAliasOption.Value;
        _latestQueriedReceiptCountProvider = latestQueriedReceiptCountProvider;
        _logger = logger;
        _contractOptions = contractOptions.Value;
        _blockConfirmationOptions = blockConfirmation.Value;
    }
    public async Task ExecuteAsync()
    {
        var bridgeItemsMap = new Dictionary<(string, string), List<BridgeItemIn>>();
        var sendQueryList = new Dictionary<string, BridgeItemIn>();
        var tokenIndex = new Dictionary<string, BigInteger>();
        foreach (var bridgeItem in _bridgeOptions.BridgesIn)
        {
            var key = (bridgeItem.ChainId, bridgeItem.EthereumBridgeInContractAddress);
            if (!bridgeItemsMap.TryGetValue(key, out var items))
            {
                items = new List<BridgeItemIn>();
            }
            items.Add(bridgeItem);
            bridgeItemsMap[key] = items;
            
        }

        foreach (var (aliasAddress, item) in bridgeItemsMap)
        {
            var tokenList = item.Select(i => i.OriginToken).ToList();
            var targetChainIdList = item.Select(i => i.TargetChainId).ToList();
            var sendReceiptIndexDto = await _bridgeInService.GetTransferReceiptIndexAsync(aliasAddress.Item1,
                aliasAddress.Item2, tokenList, targetChainIdList);
            for (var i = 0; i < tokenList.Count; i++)
            {
                tokenIndex[tokenList[i]] = sendReceiptIndexDto.Indexes[i];
                sendQueryList[item[i].SwapId] = item[i];
            }
        }

        foreach (var (swapId, item) in sendQueryList)
        {
            var targetChainId = _bridgeOptions.BridgesIn.Single(i => i.SwapId == swapId).TargetChainId;
            await SendQueryAsync(targetChainId, item, tokenIndex[item.OriginToken]);
        }
    }
    private async Task SendQueryAsync(string chainId, BridgeItemIn bridgeItem, BigInteger tokenIndex)
    {
        var swapId = bridgeItem.SwapId;
       
        var spaceId = await _bridgeContractService.GetSpaceIdBySwapIdAsync(chainId, Hash.LoadFromHex(swapId));
        var lastRecordedLeafIndex = (await _merkleTreeContractService.GetLastLeafIndexAsync(
            chainId, new GetLastLeafIndexInput
            {
                SpaceId = spaceId
            })).Value;
        if (lastRecordedLeafIndex == -2)
        {
            _logger.LogInformation($"Space of id {spaceId} is not created. ");
            return;
        }

        if (_latestQueriedReceiptCountProvider.Get(swapId) == 0)
        {
            _latestQueriedReceiptCountProvider.Set(swapId, lastRecordedLeafIndex + 1);
            _logger.LogInformation($"Next round to query should begin with tokenIndex:{_latestQueriedReceiptCountProvider.Get(swapId)}");
        }

        var nextRoundStartTokenIndex = _latestQueriedReceiptCountProvider.Get(swapId);

        _logger.LogInformation($"Last recorded leaf index : {lastRecordedLeafIndex}.");
        

        if (tokenIndex < nextRoundStartTokenIndex)
        {
            return;
        }
        var notRecordTokenNumber =  tokenIndex - nextRoundStartTokenIndex + 1;
        if (notRecordTokenNumber > 0)
        {
            var blockNumber = await _nethereumService.GetBlockNumberAsync(bridgeItem.ChainId);
            var getReceiptInfos = await _bridgeInService.GetSendReceiptInfosAsync(bridgeItem.ChainId,
                bridgeItem.EthereumBridgeInContractAddress, bridgeItem.OriginToken, bridgeItem.TargetChainId,
                nextRoundStartTokenIndex,(long)tokenIndex);
            var lastTokenIndexConfirm = nextRoundStartTokenIndex - 1;
            string receiptIdHash = null;
            for (var i = 0; i < notRecordTokenNumber; i++)
            {
                var blockHeight = getReceiptInfos.Receipts[i].BlockHeight;
                receiptIdHash = getReceiptInfos.Receipts[i].ReceiptId.Split(".").First();
                var blockConfirmationCount = _blockConfirmationOptions.ConfirmationCount[bridgeItem.ChainId];
                if (blockNumber - blockHeight > blockConfirmationCount)
                {
                    lastTokenIndexConfirm += (i+1);
                    continue;
                }
                break;
            }
            _logger.LogInformation($"Last confirmed token index:{lastTokenIndexConfirm}");
            
            _logger.LogInformation($"Token hash in receipt id:{receiptIdHash}");

            if (lastTokenIndexConfirm - nextRoundStartTokenIndex >= 0)
            {
                _logger.LogInformation($"Start to query token : from receipt index {nextRoundStartTokenIndex},end receipt index {lastTokenIndexConfirm}");
                var queryInput = new QueryInput
                {
                    Payment = _bridgeOptions.QueryPayment,
                    QueryInfo = new QueryInfo
                    {
                        Title = $"record_receipts_{swapId}",
                        Options = {$"{receiptIdHash}.{nextRoundStartTokenIndex}", $"{receiptIdHash}.{lastTokenIndexConfirm}"}
                    },
                    AggregatorContractAddress = _contractOptions.ContractAddressList[chainId]["StringAggregatorContract"].ConvertAddress(),
                    AggregateThreshold = 1,
                    CallbackInfo = new CallbackInfo
                    {
                        ContractAddress =
                            _contractOptions.ContractAddressList[chainId]["BridgeContract"].ConvertAddress(),
                        MethodName = "RecordReceiptHash"
                    },
                    DesignatedNodeList = new AddressList
                    {
                        Value = {bridgeItem.QueryToAddress.ConvertAddress()}
                    }
                };

                _logger.LogInformation($"About to send Query transaction for token swapping, QueryInput: {queryInput}");

                var sendTxResult = await _oracleService.QueryAsync(chainId, queryInput);
                _logger.LogInformation($"Query transaction id : {sendTxResult.TransactionResult.TransactionId}");
                _latestQueriedReceiptCountProvider.Set(swapId,  lastTokenIndexConfirm + 1);
                _logger.LogInformation(
                    $"Next token index should start with tokenindex:{_latestQueriedReceiptCountProvider.Get(swapId)}");
            }
            
        }
    }
}