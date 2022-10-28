using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBotApp
{
    public class GoogleSigninRequest
    {
        public string email;
        public string name;
        public string profile;
        public string deviceType;
        public string deviceToken;
    }

    public class StartChatRequest
    {
        public string from;
    }

    public class AddChatRequest
    {
        public string to;
        public string from;
        public string convId;
        public string message;
        public string typeOfMessage;
    }

}
