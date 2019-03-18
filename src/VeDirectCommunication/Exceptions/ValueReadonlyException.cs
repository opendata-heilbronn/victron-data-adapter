using System;

namespace VeDirectCommunication.Exceptions
{
    [Serializable]
    public class ValueReadonlyException : Exception
    {
        public ValueReadonlyException() { }
        public ValueReadonlyException(string message) : base(message) { }
        public ValueReadonlyException(string message, Exception inner) : base(message, inner) { }
        protected ValueReadonlyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
