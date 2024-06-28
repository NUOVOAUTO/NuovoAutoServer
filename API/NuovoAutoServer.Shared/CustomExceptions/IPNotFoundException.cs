using System.Runtime.Serialization;

namespace NuovoAutoServer.Shared.CustomExceptions
{
    [Serializable]
    public class IPNotFoundException : Exception
    {
        public IPNotFoundException()
        {
        }

        public IPNotFoundException(string? message) : base(message)
        {
        }

        public IPNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected IPNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}