using System;

namespace ChatBotApp
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnlogin_Click(object sender, EventArgs e)
        {
            string clientid = "1009113568321-l67fch45nmt05r5ahmpg2ipdal4q30tq.apps.googleusercontent.com";
            string clientsecret = "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";
            string redirection_url = "https://localhost:44327/Index.aspx";
            string url = "https://accounts.google.com/o/oauth2/v2/auth?scope=https://www.googleapis.com/auth/calendar.readonly&include_granted_scopes=true&redirect_uri=" + redirection_url + "&response_type=code&client_id=" + clientid + "";
            Response.Redirect(url);
        }        
    }
}