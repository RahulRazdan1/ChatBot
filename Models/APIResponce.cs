using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBotApp
{
    public class GoogleSigninResponse
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Data
        {
            public string _id { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            public string status { get; set; }
            public DateTime createdAt { get; set; }
            public string profile { get; set; }
            public bool isActive { get; set; }
            public DateTime deviceUpdatedAt { get; set; }
            public string refreshTokenCode { get; set; }
            public string serverAuthCode { get; set; }
            public string authToken { get; set; }
        }

        public class ReqBody
        {
            public string email { get; set; }
            public string name { get; set; }
            public string profile { get; set; }
            public string deviceType { get; set; }
            public string deviceToken { get; set; }
        }

        public class ReqParams
        {
        }

        public class Root
        {
            public bool success { get; set; }
            public string msg { get; set; }
            public Data data { get; set; }
            public ReqParams reqParams { get; set; }
            public ReqBody reqBody { get; set; }
            public string url { get; set; }
            public string timestamp { get; set; }
            public string ip { get; set; }
        }
    }

    public class GetProfileResponse
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Data
        {
            public string _id { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            public string status { get; set; }
            public object createdBy { get; set; }
            public string userType { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime modifiedAt { get; set; }
            public object modifiedBy { get; set; }
            public string profile { get; set; }
            public bool isActive { get; set; }
            public string deviceToken { get; set; }
            public string deviceType { get; set; }
            public DateTime deviceUpdatedAt { get; set; }
            public string refreshTokenCode { get; set; }
            public string serverAuthCode { get; set; }
        }

        public class ReqBody
        {
        }

        public class ReqParams
        {
            public string id { get; set; }
        }

        public class Root
        {
            public bool success { get; set; }
            public string msg { get; set; }
            public Data data { get; set; }
            public ReqParams reqParams { get; set; }
            public ReqBody reqBody { get; set; }
            public string url { get; set; }
            public string timestamp { get; set; }
            public string ip { get; set; }
        }


    }

    public class StartChatResponse
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Data
        {
            public string type { get; set; }
            public string question { get; set; }
            public List<string> options { get; set; }
        }

        public class ReqBody
        {
            public string from { get; set; }
            public string createdBy { get; set; }
            public string modifiedBy { get; set; }
        }

        public class ReqParams
        {
        }

        public class Root
        {
            public bool success { get; set; }
            public string msg { get; set; }
            public Data data { get; set; }
            public ReqParams reqParams { get; set; }
            public ReqBody reqBody { get; set; }
            public string url { get; set; }
            public string timestamp { get; set; }
            public string ip { get; set; }
        }


    }

    public class AddChatReesponeseCheck
    {
        public bool success { get; set; }
        public string msg { get; set; }
        public string error { get; set; }
    }

    public class AddChatResponse
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Data
        {
            public string type { get; set; }
            public string question { get; set; }
            public string answer { get; set; }
            public List<string> options { get; set; }
        }

        public class ReqBody
        {
            public string to { get; set; }
            public string from { get; set; }
            public string convId { get; set; }
            public string message { get; set; }
            public string typeOfMessage { get; set; }
            public string createdBy { get; set; }
            public string modifiedBy { get; set; }
            public string level { get; set; }
            public string timestamp { get; set; }
        }

        public class ReqParams
        {
        }

        public class Root
        {
            public bool success { get; set; }
            public string msg { get; set; }
            public Data data { get; set; }
            public ReqParams reqParams { get; set; }
            public ReqBody reqBody { get; set; }
            public string url { get; set; }
            public string timestamp { get; set; }
            public string ip { get; set; }
        }
    }
}
