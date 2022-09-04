using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CryptoTA.Exceptions;
using CryptoTA.Utils;
using Websocket.Client;
using System.Threading;
using System.Windows;
using System.Collections;
using System.Web;

namespace CryptoTA.Services
{
    public class KrakenApiService
    {
        private const string baseDomain = "https://api.kraken.com";
        private const string publicPath = "/0/public/";
        private const string privatePath = "/0/private/";

        private const string publicWsAddress = "wss://ws.kraken.com";
        private const string privateWsAddress = "wss://ws-auth.kraken.com";

        private readonly string apiKey;
        private readonly string apiSecret;

        public KrakenApiService(string publicKey = "", string privateKey = "")
        {
            apiKey = publicKey;
            apiSecret = privateKey;
        }

        public static string BuildQueryString(UriParams? queryStringParams = null)
        {
            if (queryStringParams is null || queryStringParams.Count == 0)
            {
                return "";
            }

            List<string> paramList = new ();

            foreach (DictionaryEntry parameterEntry in queryStringParams)
            {
                paramList.Add(parameterEntry.Key.ToString() + "=" + HttpUtility.UrlEncode(parameterEntry.Value!.ToString()));
            }

            return string.Join("&", paramList);
        }

        public static JToken ParseJsonAndCheckForError(string jsonData, string[]? responsePath = null)
        {
            if (JObject.Parse(jsonData) is null || JObject.Parse(jsonData) is not JToken jTokenData)
            {
                throw new KrakenApiException("No JSON returned from request.");
            }

            if (jTokenData["error"] is JToken jErrorsToken)
            {
                if (jErrorsToken is JArray jErrors && jErrors.Count > 0)
                {
                    throw new KrakenApiException(jErrors[0].ToString());
                }
            }

            if (responsePath is null || responsePath.Length == 0)
            {
                return jTokenData;
            }

            string responsePathString = responsePath[0];

            foreach (string path in responsePath)
            {
                if (Array.IndexOf(responsePath, path) > 0)
                {
                    responsePathString += " -> " + path;
                }

                if (jTokenData[path] is not JToken jTokenNext)
                {
                    throw new KrakenApiException($"Cannot deserialize Kraken API response for '{path}'.");
                }

                jTokenData = jTokenNext;
            }

            return jTokenData;
        }

        public static string CreateAuthenticationSignature(string apiPath, string endpointName, string nonce, UriParams bodyParams, string privateKey)
        {
            byte[] sha256Hash = ComputeSha256Hash(nonce, BuildQueryString(bodyParams).ToString());
            byte[] sha512Hash = ComputeSha512Hash(privateKey, sha256Hash, apiPath, endpointName);
            string signatureString = Convert.ToBase64String(sha512Hash);

            return signatureString;
        }

        public static byte[] ComputeSha256Hash(string nonce, string urlEncodedBodyString)
        {
            byte[] sha256Hash;

            string sha256HashData = nonce.ToString() + urlEncodedBodyString.ToString();

            using (var sha = SHA256.Create())
            {
                sha256Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(sha256HashData));
            }

            return sha256Hash;
        }

        public static byte[] ComputeSha512Hash(string apiPrivateKey, byte[] sha256Hash, string apiPath, string endpointName)
        {
            string apiEndpointPath = apiPath + endpointName;

            byte[] apiEndpointPathBytes = Encoding.UTF8.GetBytes(apiEndpointPath);
            byte[] sha512HashData = apiEndpointPathBytes.Concat(sha256Hash).ToArray();
            HMACSHA512 encryptor = new(Convert.FromBase64String(apiPrivateKey));
            byte[] sha512Hash = encryptor.ComputeHash(sha512HashData);

            return sha512Hash;
        }

        public JToken QueryPublicEndpoint(string endpointName, string[]? responsePath = null, UriParams? queryParams = null)
        {
            string queryParamsString = BuildQueryString(queryParams);
            string apiEndpointFullURL = $"{baseDomain}{publicPath}{endpointName}";
            if (!string.IsNullOrWhiteSpace(queryParamsString))
            {
                apiEndpointFullURL += $"?{queryParamsString}";
            }

            string jsonData;

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "CryptoTA");

                var request = new HttpRequestMessage(HttpMethod.Get, apiEndpointFullURL);
                HttpResponseMessage response = client.Send(request);

                jsonData = response.Content.ReadAsStringAsync().Result;
            }

            return ParseJsonAndCheckForError(jsonData, responsePath);
        }

        public async Task<JToken> QueryPublicEndpointAsync(string endpointName, string[]? responsePath = null, UriParams? queryParams = null)
        {
            string queryParamsString = BuildQueryString(queryParams);
            string apiEndpointFullURL = $"{baseDomain}{publicPath}{endpointName}";
            if (!string.IsNullOrWhiteSpace(queryParamsString))
            {
                apiEndpointFullURL += $"?{queryParamsString}";
            }

            string jsonData;

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "CryptoTA");

                var request = new HttpRequestMessage(HttpMethod.Get, apiEndpointFullURL);
                HttpResponseMessage response = await client.SendAsync(request);

                jsonData = response.Content.ReadAsStringAsync().Result;
            }

            return ParseJsonAndCheckForError(jsonData, responsePath);
        }

        public Dictionary<string, T> QueryPublicEndpoint<T>(string endpointName, string[]? responsePath = null, UriParams? queryParams = null)
        {
            var jToken = QueryPublicEndpoint(endpointName, responsePath, queryParams);

            if (JsonConvert.DeserializeObject<Dictionary<string, T>>(jToken.ToString()) is not Dictionary<string, T> resultDictionary)
            {
                throw new KrakenApiException("Error during deserializing response into dictionary.");
            }

            return resultDictionary;
        }

        public async Task<Dictionary<string, T>> QueryPublicEndpointAsync<T>(string endpointName, string[]? responsePath = null, UriParams? queryParams = null)
        {
            var jToken = await QueryPublicEndpointAsync(endpointName, responsePath, queryParams);

            if (JsonConvert.DeserializeObject<Dictionary<string, T>>(jToken.ToString()) is not Dictionary<string, T> resultDictionary)
            {
                throw new KrakenApiException("Error during deserializing response into dictionary.");
            }

            return resultDictionary;
        }

        public JToken QueryPrivateEndpoint(string endpointName, string[]? responsePath = null, UriParams? bodyParams = null)
        {
            string apiEndpointFullURL = baseDomain + privatePath + endpointName;

            bodyParams ??= new();
            string nonce = (new Random().Next()).ToString();
            bodyParams.Insert(0, "nonce", nonce.ToString());

            string signature = CreateAuthenticationSignature(privatePath, endpointName, nonce.ToString()!, bodyParams, apiSecret.ToString());

            string jsonData;

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("API-Key", apiKey.ToString());
                client.DefaultRequestHeaders.Add("API-Sign", signature);
                client.DefaultRequestHeaders.Add("User-Agent", "CryptoTA");

                StringContent data = new(BuildQueryString(bodyParams), Encoding.UTF8, "application/x-www-form-urlencoded");

                var request = new HttpRequestMessage(HttpMethod.Post, apiEndpointFullURL)
                {
                    Content = data
                };

                HttpResponseMessage response = client.Send(request);

                jsonData = response.Content.ReadAsStringAsync().Result;
            }

            return ParseJsonAndCheckForError(jsonData, responsePath);
        }

        public async Task<JToken> QueryPrivateEndpointAsync(string endpointName, string[]? responsePath = null, UriParams? bodyParams = null)
        {
            string apiEndpointFullURL = baseDomain + privatePath + endpointName;

            bodyParams ??= new();
            string nonce = (new Random().Next()).ToString();
            bodyParams.Insert(0, "nonce", nonce.ToString());

            string signature = CreateAuthenticationSignature(privatePath, endpointName, nonce.ToString()!, bodyParams, apiSecret.ToString());

            string jsonData;

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("API-Key", apiKey.ToString());
                client.DefaultRequestHeaders.Add("API-Sign", signature);
                client.DefaultRequestHeaders.Add("User-Agent", "CryptoTA");

                StringContent data = new(BuildQueryString(bodyParams), Encoding.UTF8, "application/x-www-form-urlencoded");

                var request = new HttpRequestMessage(HttpMethod.Post, apiEndpointFullURL)
                {
                    Content = data
                };

                HttpResponseMessage response = await client.SendAsync(request);

                jsonData = response.Content.ReadAsStringAsync().Result;
            }

            return ParseJsonAndCheckForError(jsonData, responsePath);
        }

        public async Task<Dictionary<string, T>> QueryPrivateEndpointAsync<T>(string endpointName, string[]? responsePath = null, UriParams? bodyParams = null)
        {
            var jToken = await QueryPrivateEndpointAsync(endpointName, responsePath, bodyParams);

            if (jToken is null || !jToken.HasValues)
            {
                throw new KrakenApiException("Kraken API returned empty response.");
            }

            if (JsonConvert.DeserializeObject<Dictionary<string, T>>(jToken.ToString()) is not Dictionary<string, T> resultDictionary)
            {
                throw new KrakenApiException("Error during deserializing response into dictionary.");
            }

            return resultDictionary;
        }

        public Dictionary<string, T> QueryPrivateEndpoint<T>(string endpointName, string[]? responsePath = null, UriParams? bodyParams = null)
        {
            var jToken = QueryPrivateEndpoint(endpointName, responsePath, bodyParams);

            if (jToken is null || !jToken.HasValues)
            {
                throw new KrakenApiException("Kraken API returned empty response.");
            }

            if (JsonConvert.DeserializeObject<Dictionary<string, T>>(jToken.ToString()) is not Dictionary<string, T> resultDictionary)
            {
                throw new KrakenApiException("Error during deserializing response into dictionary.");
            }

            return resultDictionary;
        }

        public async Task QueryWebsocketApi(object payload)
        {
            var exitEvent = new ManualResetEvent(false);

            using var client = new WebsocketClient(new Uri(publicWsAddress));
            client.Name = "Kraken";
            client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            client.ReconnectionHappened.Subscribe(type => MessageBox.Show($"Reconnection happened, type: {type}"));
            client.DisconnectionHappened.Subscribe(type => MessageBox.Show($"Disconnection happened, type: {type}"));
            client.MessageReceived.Subscribe(msg => MessageBox.Show($"Message received: {msg}"));

            await client.Start();

            await Task.Run(() => client.Send(JObject.FromObject(payload).ToString()));

            exitEvent.WaitOne();
        }
    }
}
