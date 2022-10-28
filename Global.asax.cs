using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace ChatBotApp
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Application["ClientId"] = "1009113568321-3ir771og70lop7n6ecklu8k7psucok5v.apps.googleusercontent.com";
            Application["ClientSecret"] = "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";
            Application["ServerUrl"] = "http://localhost:2010"; //"http://109.106.255.139:2010";//
        }
    }
}