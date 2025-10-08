using System;

namespace dms.Api.Exceptions
{
    public class QueueUnavailableException : Exception
    {
        public QueueUnavailableException(string message, Exception? inner = null)
            : base(message, inner)
        {
        }
    }
}
