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

        private string BuildQueryString(Dictionary<string, string>? queryStringParams = null)
        {
            if (queryStringParams is null)
            {
                return "";
            }

            List<string> paramList = new List<string>();
            foreach (var parameter in queryStringParams)
            {
                paramList.Add(parameter.Key + "=" + parameter.Value);
            }
            return "?" + string.Join("&", paramList);
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

        public string CreateAuthenticationSignature(string apiPath, string endpointName, string nonce, string inputParams)
        {
            byte[] sha256Hash = ComputeSha256Hash(nonce, inputParams);
            byte[] sha512Hash = ComputeSha512Hash(apiSecret, sha256Hash, apiPath, endpointName);
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

        public JToken QueryPublicEndpoint(string endpointName, string[]? responsePath = null, Dictionary<string, string>? queryParams = null)
        {
            string apiEndpointFullURL = baseDomain + publicPath + endpointName + BuildQueryString(queryParams);

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

        public async Task<JToken> QueryPublicEndpointAsync(string endpointName, string[]? responsePath = null, Dictionary<string, string>? queryParams = null)
        {
            string apiEndpointFullURL = baseDomain + publicPath + endpointName + BuildQueryString(queryParams);

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

        public Dictionary<string, T> QueryPublicEndpoint<T>(string endpointName, string[]? responsePath = null, Dictionary<string, string>? queryParams = null)
        {
            var jToken = QueryPublicEndpoint(endpointName, responsePath, queryParams);

            if (JsonConvert.DeserializeObject<Dictionary<string, T>>(jToken.ToString()) is not Dictionary<string, T> resultDictionary)
            {
                throw new KrakenApiException("Error during deserializing response into dictionary.");
            }

            return resultDictionary;
        }

        public async Task<Dictionary<string, T>> QueryPublicEndpointAsync<T>(string endpointName, string[]? responsePath = null, Dictionary<string, string>? queryParams = null)
        {
            var jToken = await QueryPublicEndpointAsync(endpointName, responsePath, queryParams);

            if (JsonConvert.DeserializeObject<Dictionary<string, T>>(jToken.ToString()) is not Dictionary<string, T> resultDictionary)
            {
                throw new KrakenApiException("Error during deserializing response into dictionary.");
            }

            return resultDictionary;
        }

        public JToken QueryPrivateEndpoint(string endpointName, string[]? responsePath = null, string inputParameters = "")
        {
            string apiEndpointFullURL = baseDomain + privatePath + endpointName;
            string nonce = DateTimeUtils.ToTimestamp(DateTime.UtcNow).ToString();

            if (string.IsNullOrWhiteSpace(inputParameters) == false)
            {
                inputParameters = "&" + inputParameters;
            }

            string apiPostBodyData = "nonce=" + nonce + inputParameters;
            string signature = CreateAuthenticationSignature(privatePath, endpointName, nonce, inputParameters);

            string jsonData;

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("API-Key", apiKey);
                client.DefaultRequestHeaders.Add("API-Sign", signature);
                client.DefaultRequestHeaders.Add("User-Agent", "CryptoTA Client");

                StringContent data = new(apiPostBodyData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var request = new HttpRequestMessage(HttpMethod.Post, apiEndpointFullURL)
                {
                    Content = data
                };

                HttpResponseMessage response = client.Send(request);

                jsonData = response.Content.ReadAsStringAsync().Result;
            }

            return ParseJsonAndCheckForError(jsonData, responsePath);
        }

        public async Task<JToken> QueryPrivateEndpointAsync(string endpointName, string[]? responsePath = null, string inputParameters = "")
        {
            string apiEndpointFullURL = baseDomain + privatePath + endpointName;
            string nonce = DateTimeUtils.ToTimestamp(DateTime.UtcNow).ToString();

            if (string.IsNullOrWhiteSpace(inputParameters) == false)
            {
                inputParameters = "&" + inputParameters;
            }

            string apiPostBodyData = "nonce=" + nonce + inputParameters;
            string signature = CreateAuthenticationSignature(privatePath, endpointName, nonce, inputParameters);

            string jsonData;

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("API-Key", apiKey);
                client.DefaultRequestHeaders.Add("API-Sign", signature);
                client.DefaultRequestHeaders.Add("User-Agent", "CryptoTA Client");

                StringContent data = new(apiPostBodyData, Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage response = await client.PostAsync(apiEndpointFullURL, data);

                jsonData = response.Content.ReadAsStringAsync().Result;
            }

            return ParseJsonAndCheckForError(jsonData, responsePath);
        }

        public async Task<Dictionary<string, T>> QueryPrivateEndpointAsync<T>(string endpointName, string[]? responsePath = null, string inputParameters = "")
        {
            var jToken = await QueryPrivateEndpointAsync(endpointName, responsePath, inputParameters);

            if (JsonConvert.DeserializeObject<Dictionary<string, T>>(jToken.ToString()) is not Dictionary<string, T> resultDictionary)
            {
                throw new KrakenApiException("Error during deserializing response into dictionary.");
            }

            return resultDictionary;
        }

        public Dictionary<string, T> QueryPrivateEndpoint<T>(string endpointName, string[]? responsePath = null, string inputParameters = "")
        {
            var jToken = QueryPrivateEndpoint(endpointName, responsePath, inputParameters);

            if (JsonConvert.DeserializeObject<Dictionary<string, T>>(jToken.ToString()) is not Dictionary<string, T> resultDictionary)
            {
                throw new KrakenApiException("Error during deserializing response into dictionary.");
            }

            return resultDictionary;
        }
    }
}
