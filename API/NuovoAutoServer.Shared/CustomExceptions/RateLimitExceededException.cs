using System.Runtime.Serialization;

namespace NuovoAutoServer.Shared.CustomExceptions
{
    [Serializable]
    public class RateLimitExceededException : Exception
    {
        public RateLimitExceededException()
        {
        }

        public RateLimitExceededException(string? message) : base(message)
        {
        }

        public RateLimitExceededException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected RateLimitExceededException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}