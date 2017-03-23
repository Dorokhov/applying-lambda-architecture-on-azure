using System.Collections.Generic;
using DataContracts;
using Microsoft.ServiceBus.Messaging;
using ResourceConfiguration;

namespace LambdaArchitecture.E2ETest
{
    public class StreamProducer
    {
        private readonly ServiceBusConfiguration _serviceBusConfiguration;
        private readonly TopicClient _client;
        public StreamProducer(ServiceBusConfiguration serviceBusConfiguration)
        {
            _serviceBusConfiguration = serviceBusConfiguration;

            _client = TopicClient.CreateFromConnectionString(_serviceBusConfiguration.ConnectionString, _serviceBusConfiguration.TopicName);
        }

        public void Produce(MeetupMessage message)
        {
            _client.Send(new BrokeredMessage(message));
        }

        public void Produce(IEnumerable<MeetupMessage> messages)
        {
            foreach (var message in messages)
            {
                _client.Send(new BrokeredMessage(message));
            }
        }
    }
}