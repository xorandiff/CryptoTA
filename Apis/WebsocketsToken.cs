using System;

namespace CryptoTA.Apis
{
    /// <summary>
    /// Class <c>WebsocketsToken</c> containing websockets token and expiration 
    /// date.
    /// </summary>
    public class WebsocketsToken
    {
        public string? Token { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
