using System.Threading;
using System.Threading.Tasks;
using DataContracts;
using PublishSubscribe;
using ResourceConfiguration;

namespace BatchLayer
{
    public class MeetupMessageConsumer : MeetupConsumerBase
    {
        private readonly MeetupRepository _meetupRepository;

        public MeetupMessageConsumer(MeetupRepository meetupRepository, ServiceBusConfiguration serviceBusConfiguration) 
            : base(serviceBusConfiguration.ConnectionString, serviceBusConfiguration.TopicName, serviceBusConfiguration.BatchLayerSubscription)
        {
            _meetupRepository = meetupRepository;
        }

        public override async Task OnMessageAsync(MeetupMessage meetupMessage)
        {
            await _meetupRepository.SaveAsync(meetupMessage);
        }
    }
}