using System;
using ResourceConfiguration;

namespace BatchLayer.Job
{
    public class HiveJob
    {
        static void Main(string[] args)
        {
            new RecalculationJob(DeploymentConfiguration.Default.Storage, DeploymentConfiguration.Default.HDInsight).Execute(DateTime.UtcNow);
        }
    }
}