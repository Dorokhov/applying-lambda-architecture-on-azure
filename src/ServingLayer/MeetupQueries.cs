using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ResourceConfiguration;
using StackExchange.Redis;
using Utils;

namespace ServingLayer
{
    public class MeetupQueries
    {
        private static readonly RedisConfiguration _redisConfiguration = DeploymentConfiguration.Default.Redis;
        private static readonly StorageConfiguration _storageConfiguration = DeploymentConfiguration.Default.Storage;
        private static readonly ConnectionMultiplexer _multiplexer = ConnectionMultiplexer.Connect(_redisConfiguration.ConnectionString);
        private static string _blobName = "000000_0";

        private static readonly CloudStorageAccount _storageAccount = CloudStorageAccount.Parse(_storageConfiguration.ConnectionString);
        private static readonly CloudBlobClient _blobClient = _storageAccount.CreateCloudBlobClient();

        public long GetAllMeetupVisitors(long meetupId, string country, string city)
        {
            // load batch view
            string batchViewKey = $"data/batchviews/rsvp_id={meetupId}/group_country={country}/group_city={city}";
            CloudBlobContainer container = _blobClient.GetContainerReference(batchViewKey);
            CloudBlockBlob blob = container.GetBlockBlobReference(_blobName);

            string text = blob.DownloadText();
            var tuple = text.Split(new[] { "\u0001" }, StringSplitOptions.RemoveEmptyEntries);
            double time = double.Parse(tuple[0]);
            long from = time.UnixTimeStampToDateTime().Ticks;
            long batchLayerResult = long.Parse(tuple[1]);

            // load speed layer result
            string redisBasketKey = $"basket_{meetupId}:{country}:{city}";
            string redisCounterKey = $"counter_{meetupId}:{country}:{city}";
            string resultKey = "analyticsResult";

            var db = _multiplexer.GetDatabase(_redisConfiguration.Database);
            long speedLayerResult = (long)db.ScriptEvaluate(LuaScript.Prepare(@"
            redis.call('ZREMRANGEBYLEX', @redisBasketKey, '[0', @value);
            redis.call('ZINTERSTORE', @resultKey, 2, @redisBasketKey, @redisCounterKey);

            local sum=0
            local z=redis.call('ZRANGE', @resultKey, 0, -1, 'WITHSCORES')

            for i=2, #z, 2 do 
                sum=sum+z[i]
            end

            return sum"), new
            {
                redisBasketKey = (RedisKey)redisBasketKey,
                resultKey = (RedisKey)resultKey,
                redisCounterKey = (RedisKey)redisCounterKey,
                value = (RedisValue)$"[{from - 1}"
            });

            // merge
            long sum = batchLayerResult + speedLayerResult;
            return sum;
        }
    }
}
