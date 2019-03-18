using System;

namespace VeDirectCommunication.Exceptions
{
    [Serializable]
    public class UnknownIdException : Exception
    {
        public UnknownIdException() { }
        public UnknownIdException(string message) : base(message) { }
        public UnknownIdException(string message, Exception inner) : base(message, inner) { }
        protected UnknownIdException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
