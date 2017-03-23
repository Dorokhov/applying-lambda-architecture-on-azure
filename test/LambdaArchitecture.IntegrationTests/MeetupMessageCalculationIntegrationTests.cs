using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using BatchLayer;
using BatchLayer.Job;
using DataContracts;
using LambdaArchitecture.E2ETest;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using ResourceConfiguration;
using ServingLayer;
using SpeedLayer;
using StackExchange.Redis;
using Utils;
using Xunit;

namespace LambdaArchitecture.IntegrationTests
{
    /// <summary>
    /// http://meetup.github.io/stream/rsvpTicker/
    /// </summary>
    public class MeetupMessageCalculationIntegrationTests
    {
        public MeetupMessageCalculationIntegrationTests()
        {
            DeploymentConfiguration.Default.ServiceBus.Password = ConfigurationManager.AppSettings["ServiceBusAccessKey"];
            DeploymentConfiguration.Default.Redis.Password = ConfigurationManager.AppSettings["RedisAccessKey"];
            DeploymentConfiguration.Default.Storage.Password = ConfigurationManager.AppSettings["BlobStorageAccountKey"];
            DeploymentConfiguration.Default.HDInsight.DefaultStorageAccountKey = ConfigurationManager.AppSettings["BlobStorageAccountKey"];

            // clean up Redis
            var multiplexer = ConnectionMultiplexer.Connect(DeploymentConfiguration.Default.Redis.ConnectionString);
            var server = multiplexer.GetServer(DeploymentConfiguration.Default.Redis.Server);
            server.FlushDatabase(DeploymentConfiguration.Default.Redis.Database);

            // clean up Blob Storage
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(DeploymentConfiguration.Default.Storage.ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.GetContainerReference(DeploymentConfiguration.Default.Storage.MasterDataSetContainerName).DeleteIfExists();
            bool done = false;
            while (!done)
            {
                try
                {
                    blobClient
                        .GetContainerReference(DeploymentConfiguration.Default.Storage.MasterDataSetContainerName)
                        .Create();
                    done = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// 01/01/2016----------* UK, London(08:00:10)-----------------------------------------------------------------------------01/01/2016
        /// 01/01/2016---------------* UK, London(08:00:20)------------------------------------------------------------------------01/01/2016
        /// 01/01/2016-------------------------------* UK, London(08:00:50)--------------------------------------------------------01/01/2016
        /// 01/01/2016-------------------------------------------------^ Batch View Recalculation (08:01:00)-----------------------01/01/2016
        /// 01/01/2016------------------------------------------------------------------* UK, London(08:01:40)---------------------01/01/2016
        /// </summary>
        [Fact]
        public void Should_Merge_BatchAndSpeedLayerViews()
        {
            // arrange
            var eventId = 1;
            var meetupLondonMessages =
                new []
                {
                    CreateMeetupMessageFromTemplate(eventId, new DateTime(2016, 01, 01, 08, 00, 10), "UK", "London"),
                    CreateMeetupMessageFromTemplate(eventId, new DateTime(2016, 01, 01, 08, 00, 20), "UK", "London"),
                    CreateMeetupMessageFromTemplate(eventId, new DateTime(2016, 01, 01, 08, 00, 50), "UK", "London"),
                    CreateMeetupMessageFromTemplate(eventId, new DateTime(2016, 01, 01, 08, 01, 40), "UK", "London")
                };
            StreamProducer producer = new StreamProducer(DeploymentConfiguration.Default.ServiceBus);
            producer.Produce(meetupLondonMessages);

            // stream layer processing
            var streamLayerConsumer = new SpeedLayer.MeetupMessageConsumer(
                new MeetupStreamAnalytics(DeploymentConfiguration.Default.Redis),
                DeploymentConfiguration.Default.ServiceBus);
            while (streamLayerConsumer.ProcessedMessagesCount != meetupLondonMessages.Length) { }

            // batch layer processing
            var batchLayerConsumer = new BatchLayer.MeetupMessageConsumer(
                new MeetupRepository(DeploymentConfiguration.Default.Storage),
                DeploymentConfiguration.Default.ServiceBus);
            while (batchLayerConsumer.ProcessedMessagesCount != meetupLondonMessages.Length) { }

            // batch view recalculation
            RecalculationJob job = new RecalculationJob(
                DeploymentConfiguration.Default.Storage,
                DeploymentConfiguration.Default.HDInsight);
            job.Execute(new DateTime(2016, 01, 01, 08, 01, 00));

            // act
            long count = new MeetupQueries().GetAllMeetupVisitors(eventId, "UK", "London");

            // assert
            Assert.Equal(meetupLondonMessages.Length, count);
        }

        private MeetupMessage CreateMeetupMessageFromTemplate(long id, DateTime timestamp, string country, string city)
        {
            string json = MeetupMessageTemplate.Default
                .Replace("--id--", id.ToString())
                .Replace("--timestamp--", timestamp.DateTimeToUnixTimestamp().ToString())
                .Replace("--country--", country)
                .Replace("--city--", city);
            var message = JsonConvert.DeserializeObject<MeetupMessage>(json);
            return message;
        }
    }
}
