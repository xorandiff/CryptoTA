using System;

namespace CryptoTA.Exceptions
{
    public class ApiException : Exception
    {
        public virtual string ApiName => "Unknown";

        public ApiException()
        {

        }

        public ApiException(string message) : base(message)
        {
            
        }

        public ApiException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
