using System;
using System.IO;
using System.Threading.Tasks;
using AElf.Contracts.Consensus.AEDPoS;
using AElf.Contracts.Oracle;
using AElf.Kernel;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MTRecorder;
using Volo.Abp.DependencyInjection;

namespace AElf.EventHandler
{
    public class IrreversibleBlockFoundLogEventProcessor : LogEventProcessorBase<IrreversibleBlockFound>,
        ITransientDependency
    {
        private readonly ConfigOptions _configOptions;
        private readonly EthereumConfigOptions _ethereumConfigOptions;
        private readonly ContractAddressOptions _contractAddressOptions;
        private readonly LotteryOptions _lotteryOptions;
        private readonly ILatestQueriedReceiptCountProvider _latestQueriedReceiptCountProvider;
        private readonly ILogger<IrreversibleBlockFoundLogEventProcessor> _logger;
        private readonly string _lockAbi;

        public IrreversibleBlockFoundLogEventProcessor(
            IOptionsSnapshot<ContractAddressOptions> contractAddressOptions,
            IOptionsSnapshot<ConfigOptions> configOptions,
            IOptionsSnapshot<EthereumConfigOptions> ethereumConfigOptions,
            IOptionsSnapshot<ContractAbiOptions> contractAbiOptions,
            IOptionsSnapshot<LotteryOptions> lotteryOptions,
            ILatestQueriedReceiptCountProvider latestQueriedReceiptCountProvider,
            ILogger<IrreversibleBlockFoundLogEventProcessor> logger) : base(contractAddressOptions)
        {
            _latestQueriedReceiptCountProvider = latestQueriedReceiptCountProvider;
            _logger = logger;

            var contractAbiOptions1 = contractAbiOptions.Value;
            _configOptions = configOptions.Value;
            _ethereumConfigOptions = ethereumConfigOptions.Value;
            _contractAddressOptions = contractAddressOptions.Value;
            _lotteryOptions = lotteryOptions.Value;

            {
                var file = contractAbiOptions1.LockAbiFilePath;
                if (!string.IsNullOrEmpty(file))
                {
                    if (!File.Exists(file))
                    {
                        _logger.LogError($"Cannot found file {file}");
                    }

                    _lockAbi = JsonHelper.ReadJson(file, "abi");
                }
            }
        }

        public override string ContractName => "Consensus";

        public override async Task ProcessAsync(LogEvent logEvent)
        {
            var libFound = new IrreversibleBlockFound();
            libFound.MergeFrom(logEvent);
            _logger.LogInformation($"IrreversibleBlockFound: {libFound}");

            if (_lotteryOptions.IsDrawLottery)
            {
                // Just for logging.
                var startTimestamp = TimestampHelper.DurationFromSeconds(_lotteryOptions.StartTimestamp);
                var duration = TimestampHelper.GetUtcNow() - startTimestamp;
                _logger.LogInformation(
                    $"Trying to draw lottery of supposed period {(int) (duration.Seconds / 60 / _lotteryOptions.IntervalMinutes)}. StartTimestamp: {startTimestamp}. Duration: {duration.Seconds} seconds.");

                _logger.LogInformation(
                    DrawHelper.TryToDrawLottery(_configOptions.BlockChainEndpoint, _lotteryOptions, out var period,
                        out var txId)
                        ? $"Drew period {period}, Tx id: {txId}"
                        : $"Not draw.");
            }

            if (!_configOptions.SendQueryTransaction) return;

            foreach (var swapConfig in _configOptions.SwapConfigList)
            {
                await SendQueryAsync(swapConfig.LockMappingContractAddress, swapConfig.RecorderId,
                    swapConfig.TokenSymbol);
            }
        }

        private async Task SendQueryAsync(string lockMappingContractAddress, long recorderId, string symbol)
        {
            var web3ManagerForLock = new Web3Manager(_ethereumConfigOptions.Url, lockMappingContractAddress,
                _ethereumConfigOptions.PrivateKey, _lockAbi);
            var node = new NodeManager(_configOptions.BlockChainEndpoint, _configOptions.AccountAddress,
                _configOptions.AccountPassword);
            var merkleTreeRecorderContractAddress = _contractAddressOptions.ContractAddressMap["MTRecorder"];

            var lockTimes = await web3ManagerForLock.GetFunction(lockMappingContractAddress, "receiptCount")
                .CallAsync<long>();

            var lastRecordedLeafIndex = node.QueryView<Int64Value>(_configOptions.AccountAddress,
                merkleTreeRecorderContractAddress, "GetLastRecordedLeafIndex",
                new RecorderIdInput
                {
                    RecorderId = recorderId
                }).Value;

            var maxAvailableIndex = lockTimes - 1;
            if (_latestQueriedReceiptCountProvider.Get(symbol) == 0)
            {
                _latestQueriedReceiptCountProvider.Set(symbol, lastRecordedLeafIndex + 1);
            }

            var latestTreeIndex = _latestQueriedReceiptCountProvider.Get(symbol) / _configOptions.MaximumLeafCount;
            var almostTreeIndex = lockTimes / _configOptions.MaximumLeafCount;
            if (latestTreeIndex < almostTreeIndex)
            {
                maxAvailableIndex = (latestTreeIndex + 1) * _configOptions.MaximumLeafCount - 1;
            }

            _logger.LogInformation(
                $"Lock times: {lockTimes}; Latest tree index: {latestTreeIndex}; Almost tree index: {almostTreeIndex}; Max available index: {maxAvailableIndex};");

            if (maxAvailableIndex + 1 <= _latestQueriedReceiptCountProvider.Get(symbol))
            {
                return;
            }

            if (lastRecordedLeafIndex == -2)
            {
                _logger.LogError($"Recorder of id {recorderId} didn't created.");
                return;
            }

            _logger.LogInformation($"Last recorded leaf index: {lastRecordedLeafIndex}");
            var notRecordedReceiptsCount = maxAvailableIndex - lastRecordedLeafIndex;
            if (notRecordedReceiptsCount > 0)
            {
                var queryInput = new QueryInput
                {
                    Payment = _configOptions.QueryPayment,
                    QueryInfo = new QueryInfo
                    {
                        Title = $"record_receipts_{symbol}",
                        Options = {(lastRecordedLeafIndex + 1).ToString(), maxAvailableIndex.ToString()}
                    },
                    AggregatorContractAddress = _contractAddressOptions.ContractAddressMap["StringAggregator"]
                        .ConvertAddress(),
                    CallbackInfo = new CallbackInfo
                    {
                        ContractAddress = _contractAddressOptions.ContractAddressMap["Bridge"].ConvertAddress(),
                        MethodName = "RecordReceiptHash"
                    },
                    DesignatedNodeList = new AddressList
                        {Value = {_configOptions.TokenSwapOracleOrganizationAddress.ConvertAddress()}}
                };

                _logger.LogInformation($"About to send Query transaction for token swapping, QueryInput: {queryInput}");

                var txId = node.SendTransaction(_configOptions.AccountAddress,
                    _contractAddressOptions.ContractAddressMap["Oracle"], "Query", queryInput);
                _logger.LogInformation($"Query tx id: {txId}");
                _latestQueriedReceiptCountProvider.Set(symbol, maxAvailableIndex + 1);
                _logger.LogInformation(
                    $"Latest queried receipt count: {_latestQueriedReceiptCountProvider.Get(symbol)}");
            }
        }
    }
}