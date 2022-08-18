using System.Threading.Tasks;
using AElf.Client.Core;
using AElf.Client.Core.Options;
using AElf.Nethereum.Bridge;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;

namespace AElf.EventHandler;

public interface ITransmitTransactionProvider
{
}

public class TransmitTransactionProvider : AbpRedisCache, ITransmitTransactionProvider, ISingletonDependency
{
    private readonly IDistributedCacheSerializer _serializer;
    private readonly IAElfClientService _aelfClientService;
    private readonly AElfClientConfigOptions _aelfClientConfigOptions;
    private readonly IBridgeInService _bridgeInService;

    public ILogger<TransmitTransactionProvider> Logger { get; set; }

    private const string TransmitSendingQueue = "TransmitSendingQueue";
    private const string TransmitCheckingQueue = "TransmitCheckingQueue";
    private const string TransmitArchivingQueue = "TransmitCheckingQueue";

    public TransmitTransactionProvider(IOptions<RedisCacheOptions> optionsAccessor,
        IOptions<AElfClientConfigOptions> aelfClientConfigOptions,
        IDistributedCacheSerializer serializer, IAElfClientService aelfClientService, IBridgeInService bridgeInService)
        : base(optionsAccessor)
    {
        _serializer = serializer;
        _aelfClientService = aelfClientService;
        _bridgeInService = bridgeInService;
        _aelfClientConfigOptions = aelfClientConfigOptions.Value;
    }

    public async Task EnqueueAsync(SendTransmitArgs args)
    {
        await EnqueueAsync(TransmitSendingQueue, args);
    }

    public async Task SendByLibAsync(string libHash, long libHeight)
    {
        var item = await GetFirstItemAsync(TransmitSendingQueue);
        while (item != null)
        {
            if (item.BlockHeight > libHeight)
            {
                break;
            }

            var block = await _aelfClientService.GetBlockByHeightAsync(_aelfClientConfigOptions.ClientAlias, item.BlockHeight);
            if (block.BlockHash == item.BlockHash)
            {
                var sendResult = await _bridgeInService.TransmitAsync(item.TargetChainId, item.TargetContractAddress,
                    item.Report, item.Rs, item.Ss, item.RawVs);
                if (string.IsNullOrWhiteSpace(sendResult))
                {
                    Logger.LogError("Failed to transmit.");
                    break;
                }

                item.TransactionId = sendResult;
                await EnqueueAsync(TransmitCheckingQueue, item);
            }
            
            await DequeueAsync(TransmitSendingQueue);
            item = await GetFirstItemAsync(TransmitSendingQueue);
        }
    }

    private async Task EnqueueAsync(string queueName, SendTransmitArgs item)
    {
        await ConnectAsync();

        await RedisDatabase.ListRightPushAsync((RedisKey)queueName, _serializer.Serialize(item));
    }

    private async Task<SendTransmitArgs> DequeueAsync(string queueName)
    {
        await ConnectAsync();

        var value = await RedisDatabase.ListLeftPopAsync((RedisKey)queueName);
        return value.IsNullOrEmpty ? null : _serializer.Deserialize<SendTransmitArgs>(value);
    }
    
    private async Task<SendTransmitArgs> GetFirstItemAsync(string queueName)
    {
        await ConnectAsync();

        var value = await RedisDatabase.ListGetByIndexAsync((RedisKey)queueName,0);
        return !value.HasValue ? null : _serializer.Deserialize<SendTransmitArgs>(value);
    }
}

public class SendTransmitArgs
{
    public string BlockHash { get; set; }
    public long BlockHeight { get; set; }
    public string TargetChainId { get; set; }
    public string TargetContractAddress { get; set; }
    public string TransactionId { get; set; }
    public byte[] Report { get; set; }
    public byte[][] Rs { get; set; }
    public byte[][] Ss{ get; set; }
    public byte[] RawVs { get; set; }
    public int RetryTimes { get; set; }
}