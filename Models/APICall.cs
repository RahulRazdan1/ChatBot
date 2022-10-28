using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatBotApp
{
    public class APICall : IDisposable
    {
        #region Dispose Object

        private bool disposed;

        /// <summary>
        /// Construction
        /// </summary>
        public APICall()
        {
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~APICall()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// The dispose method that implements IDisposable.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The virtual dispose method that allows
        /// classes inherithed from this one to dispose their resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here.
                }

                // Dispose unmanaged resources here.
            }

            disposed = true;
        }

        #endregion

        #region Member Functions

        public static object SendPost(string endpoint, string Token, string request)
        {
            SendPost(endpoint, Token, request, out object response);
            return response;
        }
        public static bool SendPost(string endpoint, string Token, string request, out object response)
        {
            try
            {
                // Create REST client
                RestClient client = new RestClient(endpoint);
                client.Timeout = 1000 * 60 * 30; // 30 Minutes.
                // Set authentication credentials
                //client.Authenticator = new HttpBasicAuthenticator(username, password);

                // Set the SecurityProtocol to Tls protocols.
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | (SecurityProtocolType)192;

                // Create REST request
                RestRequest rest = null;
                rest = new RestRequest(endpoint, Method.POST);
                rest.RequestFormat = DataFormat.Json;
                rest.AddHeader("Content-Type", "application/json");
                rest.AddHeader("Accept", "application/json");

                if (!string.IsNullOrEmpty(Token))
                    rest.AddHeader("Authorization", Token);

                string data = (request != null) ? request.ToString() : "";
                rest.AddParameter("application/json", data, ParameterType.RequestBody);

                IRestResponse restResponse = client.Execute(rest);
                JsonTextReader jsreader = new JsonTextReader(new StringReader(restResponse.Content));

                response = new JsonSerializer().Deserialize(jsreader); // This throws an exception if response.Content isn't JSON

                return restResponse.IsSuccessful;
            }
            catch
            {
                response = null;
                return false;
            }
        }




        //public static bool APIPostCall<T>(T requestObject, string url, string AuthToken, out string result)
        //{
        //    bool isRequestSucess = false;
        //    result = string.Empty;
        //    try
        //    {
        //        string finalJsonRequest = JsonConvert.SerializeObject(requestObject).ToString();

        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //        request.Method = "POST";
        //        request.ContentType = "application/json";
        //        request.Timeout = 1000 * 60 * 30; // 30 Minutes.
        //        request.ContentLength = finalJsonRequest.Length;
        //        string data = (request != null) ? request.ToString() : "";
        //        request.("application/json", data, ParameterType.RequestBody);

        //        if (!string.IsNullOrEmpty(AuthToken))
        //            request.Headers.Add(HttpRequestHeader.Authorization, AuthToken);

        //        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
        //        {
        //            streamWriter.Write(finalJsonRequest);
        //            streamWriter.Close();
        //            var httpResponse = (HttpWebResponse)request.GetResponse();
        //            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        //            {
        //                result = streamReader.ReadToEnd();
        //                isRequestSucess = true;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result = ex.Message;
        //        isRequestSucess = false;
        //    }

        //    return isRequestSucess;
        //}

        public static bool APIGetCall(string url, string AuthToken, out string result, out HttpStatusCode httpStatusCode)
        {
            bool isRequestSucess = false;
            result = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 1000 * 60 * 30; // 30 Minutes.
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Headers.Add(HttpRequestHeader.Authorization, AuthToken);

            httpStatusCode = HttpStatusCode.InternalServerError;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    httpStatusCode = response.StatusCode;
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
                isRequestSucess = true;
            }
            catch
            {
                isRequestSucess = false;
            }

            return isRequestSucess;
        }

        //public bool ChatBotAPIGetCall(string url, out string result)
        //{
        //    return ChatBotAPIGetCall(url, out result, out _);
        //}

        //public bool SendGet(string endpoint, string authorizationHeader, out string result)
        //{
        //    bool isRequestSucess = false;
        //    result = string.Empty;
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint);
        //    request.Timeout = 1000 * 60 * 3; // 3 Minutes.
        //    request.AutomaticDecompression = DecompressionMethods.GZip;
        //    request.Accept = "application/json";
        //    if (authorizationHeader.Length > 0) request.Headers["Authorization"] = authorizationHeader;

        //    try
        //    {
        //        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        //        using (Stream stream = response.GetResponseStream())
        //        using (StreamReader reader = new StreamReader(stream))
        //        {
        //            result = reader.ReadToEnd();
        //        }
        //        isRequestSucess = true;
        //    }
        //    catch
        //    {
        //        isRequestSucess = false;
        //    }

        //    return isRequestSucess;
        //}


        //public object SendChatBotGet(string endpoint, string authorizationHeader = null, string ChatBotKey = null, JObject request = null)
        //{
        //    SendChatBotGet(out object response, endpoint, authorizationHeader, ChatBotKey, request);
        //    return response;
        //}

        //public bool SendChatBotGet(out object response, string endpoint, string authorizationHeader = null, string ChatBotKey = null, JObject request = null)
        //{
        //    try
        //    {
        //        // Create REST client
        //        RestClient client = new RestClient(endpoint);
        //        client.Timeout = 1000 * 60 * 30; // 30 Minutes.

        //        // Set the SecurityProtocol to Tls protocols.
        //        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | (SecurityProtocolType)192;

        //        // Create REST request
        //        RestRequest rest = null;
        //        rest = new RestRequest(endpoint, Method.GET);
        //        rest.RequestFormat = DataFormat.Json;
        //        rest.AddHeader("Content-Type", "application/json");
        //        rest.AddHeader("Accept", "application/json");
        //        if (!String.IsNullOrEmpty(authorizationHeader)) { rest.AddHeader("Authorization", authorizationHeader); }
        //        if (!String.IsNullOrEmpty(ChatBotKey)) { rest.AddHeader("ChatBotKEY", ChatBotKey); }

        //        string data = (request != null) ? request.ToString() : "";
        //        rest.AddParameter("application/json", data, ParameterType.RequestBody);

        //        IRestResponse restResponse = client.Execute(rest);
        //        JsonTextReader jsreader = new JsonTextReader(new StringReader(restResponse.Content));

        //        response = new JsonSerializer().Deserialize(jsreader); // This throws an exception if response.Content isn't JSON

        //        return true; //TODO - check http status?
        //    }
        //    catch
        //    {
        //        response = null;
        //        return false;
        //    }
        //}


        //public object SendChatBotPost(string endpoint, string authorizationHeader = null, string ChatBotKey = null, JObject request = null)
        //{
        //    SendChatBotPost(out object response, endpoint, authorizationHeader, ChatBotKey, request);
        //    return response;
        //}

        //public bool SendChatBotPost(out object response, string endpoint, string authorizationHeader = null, string ChatBotKey = null, JObject request = null)
        //{
        //    try
        //    {
        //        // Create REST client
        //        RestClient client = new RestClient(endpoint);
        //        client.Timeout = 1000 * 60 * 30; // 30 Minutes.

        //        // Set the SecurityProtocol to Tls protocols.
        //        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | (SecurityProtocolType)192;

        //        // Create REST request
        //        RestRequest rest = null;
        //        rest = new RestRequest(endpoint, Method.POST);
        //        rest.RequestFormat = DataFormat.Json;
        //        rest.AddHeader("Content-Type", "application/json");
        //        rest.AddHeader("Accept", "application/json");
        //        if (!String.IsNullOrEmpty(authorizationHeader)) { rest.AddHeader("Authorization", authorizationHeader); }
        //        if (!String.IsNullOrEmpty(ChatBotKey)) { rest.AddHeader("ChatBotKEY", ChatBotKey); }

        //        string data = (request != null) ? request.ToString() : "";
        //        rest.AddParameter("application/json", data, ParameterType.RequestBody);

        //        IRestResponse restResponse = client.Execute(rest);
        //        JsonTextReader jsreader = new JsonTextReader(new StringReader(restResponse.Content));

        //        response = new JsonSerializer().Deserialize(jsreader); // This throws an exception if response.Content isn't JSON

        //        return true; //TODO - check http status?
        //    }
        //    catch
        //    {
        //        response = null;
        //        return false;
        //    }
        //}

        //public object SendPost(string endpoint, string Token, string ChatBotKey, JObject request = null)
        //{
        //    SendPost(endpoint, Token, ChatBotKey, request, out object response);
        //    return response;
        //}


        //public object SendPost(string endpoint, string Token, string ChatBotKey, string request)
        //{
        //    SendPost(endpoint, Token, ChatBotKey, request, out object response);
        //    return response;
        //}
        //public bool SendPost(string endpoint, string Token, string ChatBotKey, string request, out object response)
        //{
        //    try
        //    {
        //        // Create REST client
        //        RestClient client = new RestClient(endpoint);
        //        client.Timeout = 1000 * 60 * 30; // 30 Minutes.
        //        // Set authentication credentials
        //        //client.Authenticator = new HttpBasicAuthenticator(username, password);

        //        // Set the SecurityProtocol to Tls protocols.
        //        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768 | (SecurityProtocolType)192;

        //        // Create REST request
        //        RestRequest rest = null;
        //        rest = new RestRequest(endpoint, Method.POST);
        //        rest.RequestFormat = DataFormat.Json;
        //        rest.AddHeader("Content-Type", "application/json");
        //        rest.AddHeader("Accept", "application/json");
        //        rest.AddHeader("Authorization", "Bearer " + Token);
        //        rest.AddHeader("ChatBotKEY", ChatBotKey);

        //        string data = (request != null) ? request.ToString() : "";
        //        rest.AddParameter("application/json", data, ParameterType.RequestBody);

        //        IRestResponse restResponse = client.Execute(rest);
        //        JsonTextReader jsreader = new JsonTextReader(new StringReader(restResponse.Content));

        //        response = new JsonSerializer().Deserialize(jsreader); // This throws an exception if response.Content isn't JSON

        //        return restResponse.IsSuccessful;
        //    }
        //    catch
        //    {
        //        response = null;
        //        return false;
        //    }
        //}

        #endregion
    }
}
