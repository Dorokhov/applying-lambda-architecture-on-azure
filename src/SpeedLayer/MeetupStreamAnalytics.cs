using System;
using System.Threading.Tasks;
using DataContracts;
using ResourceConfiguration;
using StackExchange.Redis;

namespace SpeedLayer
{
    public class MeetupStreamAnalytics
    {
        private readonly RedisConfiguration _redisConfiguration;
        private readonly ConnectionMultiplexer _multiplexer;

        public MeetupStreamAnalytics(RedisConfiguration redisConfiguration)
        {
            _redisConfiguration = redisConfiguration;
            _multiplexer = ConnectionMultiplexer.Connect(redisConfiguration.ConnectionString);
        }

        public async Task ProcessMessageAsync(MeetupMessage message)
        {
            DateTime messageTime = new DateTime(message.Timestamp.Year, message.Timestamp.Month, message.Timestamp.Day, message.Timestamp.Hour, message.Timestamp.Minute, 0);
            long basket = messageTime.Ticks;

            string counterKey = $"counter_{message.EventId}:{message.Group.Country}:{message.Group.City}";
            string basketsKey = $"basket_{message.EventId}:{message.Group.Country}:{message.Group.City}";

            IDatabase db = _multiplexer.GetDatabase(_redisConfiguration.Database);

            ITransaction transaction = db.CreateTransaction();
            transaction.SortedSetIncrementAsync(counterKey, basket, 1);
            transaction.SortedSetAddAsync(basketsKey, basket, 0);
            await transaction.ExecuteAsync();
        }
    }
}
