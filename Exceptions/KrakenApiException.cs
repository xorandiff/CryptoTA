using System;

namespace CryptoTA.Exceptions
{
    public class KrakenApiException : ApiException
    {
        public override string ApiName => "Kraken";

        public KrakenApiException()
        {

        }

        public KrakenApiException(string message) : base(message)
        {

        }

        public KrakenApiException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
