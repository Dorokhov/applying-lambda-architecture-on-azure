using System;

namespace BatchLayer.Job
{
    public class BatchViewCalculationException : Exception
    {
        public BatchViewCalculationException()
        {
        }

        public BatchViewCalculationException(string message) : base(message)
        {
        }
    }
}