using System.Threading.Tasks;
using DataContracts;
using PublishSubscribe;
using ResourceConfiguration;

namespace SpeedLayer
{
    public class MeetupMessageConsumer : MeetupConsumerBase
    {
        private readonly MeetupStreamAnalytics _streamAnalytics;

        public MeetupMessageConsumer(MeetupStreamAnalytics streamAnalytics, ServiceBusConfiguration serviceBusConfiguration) 
            : base(serviceBusConfiguration.ConnectionString, serviceBusConfiguration.TopicName, serviceBusConfiguration.SpeedLayerSubscription)
        {
            _streamAnalytics = streamAnalytics;
        }

        public override async Task OnMessageAsync(MeetupMessage meetupMessage)
        {
            await _streamAnalytics.ProcessMessageAsync(meetupMessage);
        }
    }
}