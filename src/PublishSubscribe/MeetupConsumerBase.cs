using System.Threading;
using System.Threading.Tasks;
using DataContracts;
using Microsoft.ServiceBus.Messaging;
using ResourceConfiguration;

namespace PublishSubscribe
{
    public abstract class MeetupConsumerBase
    {
        private readonly DeploymentConfiguration _deploymentConfiguration;
        private readonly SubscriptionClient _client;
        private int _processedMessagesCount;

        public int ProcessedMessagesCount
        {
            get { return _processedMessagesCount; }
        }

        protected MeetupConsumerBase(string connectionString, string topicName, string subscriptionName)
        {
            _client = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, subscriptionName);
            _client.OnMessageAsync(async (message) =>
            {
                var meetupMessage = message.GetBody<MeetupMessage>();
                await OnMessageAsync(meetupMessage);
                message.Complete();
                Interlocked.Increment(ref _processedMessagesCount);
            });
        }

        public abstract Task OnMessageAsync(MeetupMessage meetupMessage);
    }
}
