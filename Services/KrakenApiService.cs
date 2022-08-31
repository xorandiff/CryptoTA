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

namespace CryptoTA.Services
{
    using UriParams = Dictionary<string, string>;

    public class KrakenApiService
    {
        private const string baseDomain = "https://api.kraken.com";
        private const string publicPath = "/0/public/";
        private const string privatePath = "/0/private/";

        private readonly string apiKey;
        private readonly string apiSecret;

        public KrakenApiService(string publicKey = "", string privateKey = "")
        {
            apiKey = publicKey;
            apiSecret = privateKey;
        }

        public string BuildQueryString(UriParams? queryStringParams = null, bool addQuestionMark = false)
        {
            if (queryStringParams is null || queryStringParams.Count == 0)
            {
                return "";
            }

            List<string> paramList = new ();

            foreach (var parameter in queryStringParams)
            {
                paramList.Add(parameter.Key + "=" + parameter.Value);
            }

            string queryString = string.Join("&", paramList);

            if (addQuestionMark && !string.IsNullOrWhiteSpace(queryString))
            {
                queryString = $"?{queryString}";
            }

            return queryString;
        }

        public JToken ParseJsonAndCheckForError(string jsonData, string[]? responsePath = null)
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

        public string CreateAuthenticationSignature(string apiPath, string endpointName, string nonce, string inputParams, string? privateKey = null)
        {
            byte[] sha256Hash = ComputeSha256Hash(nonce, inputParams);
            byte[] sha512Hash = ComputeSha512Hash(privateKey ?? apiSecret, sha256Hash, apiPath, endpointName);
            string signatureString = Convert.ToBase64String(sha512Hash);

            return signatureString;
        }

        private static byte[] ComputeSha256Hash(string nonce, string inputParams)
        {
            byte[] sha256Hash;

            string sha256HashData = nonce.ToString() + "nonce=" + nonce.ToString() + inputParams;

            using (var sha = SHA256.Create())
            {
                sha256Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(sha256HashData));
            }

            return sha256Hash;
        }

        private static byte[] ComputeSha512Hash(string apiPrivateKey, byte[] sha256Hash, string apiPath, string endpointName)
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
            string apiEndpointFullURL = $"{baseDomain}{publicPath}{endpointName}{BuildQueryString(queryParams, true)}";

            string jsonData;

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "CryptoTA Client");

                var request = new HttpRequestMessage(HttpMethod.Get, apiEndpointFullURL);
                HttpResponseMessage response = client.Send(request);

                jsonData = response.Content.ReadAsStringAsync().Result;
            }

            return ParseJsonAndCheckForError(jsonData, responsePath);
        }

        public async Task<JToken> QueryPublicEndpointAsync(string endpointName, string[]? responsePath = null, UriParams? queryParams = null)
        {
            string apiEndpointFullURL = $"{baseDomain}{publicPath}{endpointName}{BuildQueryString(queryParams, true)}";

            string jsonData;

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "CryptoTA Client");

                HttpResponseMessage response = await client.GetAsync(apiEndpointFullURL);

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
            string apiEndpointFullURL = $"{baseDomain}{privatePath}{endpointName}";

            bodyParams ??= new UriParams();
            string nonce = DateTimeUtils.ToTimestamp(DateTime.UtcNow).ToString();

            string signature = CreateAuthenticationSignature(privatePath, endpointName, nonce, BuildQueryString(bodyParams));

            bodyParams.Add("nonce", nonce);
            string bodyData = BuildQueryString(bodyParams);

            string jsonData;

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("API-Key", apiKey);
                client.DefaultRequestHeaders.Add("API-Sign", signature);
                client.DefaultRequestHeaders.Add("User-Agent", "CryptoTA Client");

                StringContent data = new(bodyData, Encoding.UTF8, "application/x-www-form-urlencoded");

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
            string apiEndpointFullURL = $"{baseDomain}{privatePath}{endpointName}";

            bodyParams ??= new UriParams();
            string nonce = DateTimeUtils.ToTimestamp(DateTime.UtcNow).ToString();

            string signature = CreateAuthenticationSignature(privatePath, endpointName, nonce, BuildQueryString(bodyParams));

            bodyParams.Add("nonce", nonce);
            string bodyData = BuildQueryString(bodyParams);

            string jsonData;

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("API-Key", apiKey);
                client.DefaultRequestHeaders.Add("API-Sign", signature);
                client.DefaultRequestHeaders.Add("User-Agent", "CryptoTA Client");

                StringContent data = new(bodyData, Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage response = await client.PostAsync(apiEndpointFullURL, data);

                jsonData = response.Content.ReadAsStringAsync().Result;
            }

            return ParseJsonAndCheckForError(jsonData, responsePath);
        }

        public async Task<Dictionary<string, T>> QueryPrivateEndpointAsync<T>(string endpointName, string[]? responsePath = null, UriParams? bodyParams = null)
        {
            var jToken = await QueryPrivateEndpointAsync(endpointName, responsePath, bodyParams);

            if (JsonConvert.DeserializeObject<Dictionary<string, T>>(jToken.ToString()) is not Dictionary<string, T> resultDictionary)
            {
                throw new KrakenApiException("Error during deserializing response into dictionary.");
            }

            return resultDictionary;
        }

        public Dictionary<string, T> QueryPrivateEndpoint<T>(string endpointName, string[]? responsePath = null, UriParams? bodyParams = null)
        {
            var jToken = QueryPrivateEndpoint(endpointName, responsePath, bodyParams);

            if (JsonConvert.DeserializeObject<Dictionary<string, T>>(jToken.ToString()) is not Dictionary<string, T> resultDictionary)
            {
                throw new KrakenApiException("Error during deserializing response into dictionary.");
            }

            return resultDictionary;
        }
    }
}
