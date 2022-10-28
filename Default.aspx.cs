using System;
using System.Web;

namespace ChatBotApp
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnlogin_Click(object sender, EventArgs e)
        {
            string clientid = Application["ClientId"].ToString();
            string clientsecret = Application["ClientSecret"].ToString();
            string redirection_url = "https://" + HttpContext.Current.Request.Url.Authority + "/Index.aspx";
            string url = "https://accounts.google.com/o/oauth2/v2/auth?scope=https://mail.google.com https://www.googleapis.com/auth/calendar.readonly https://www.googleapis.com/auth/userinfo.profile&include_granted_scopes=true&redirect_uri=" + redirection_url + "&response_type=code&client_id=" + clientid + "";
            Response.Redirect(url);
        }
    }
}