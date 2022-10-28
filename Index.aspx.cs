using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using Microsoft.ApplicationBlocks.Data;
using AjaxControlToolkit.HtmlEditor.ToolbarButtons;
using Microsoft.SqlServer.Server;
using Google.Apis.Calendar.v3.Data;
using System.Linq;

namespace ChatBotApp
{
    public partial class Index : System.Web.UI.Page
    {
        public class Tokenclass
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }

            [JsonProperty("id_token")]
            public string refresh_token { get; set; }
        }
        public class Userclass
        {
            public string id { get; set; }
            public string name { get; set; }
            public string given_name { get; set; }
            public string family_name { get; set; }
            public string link { get; set; }
            public string picture { get; set; }
            public string gender { get; set; }
            public string locale { get; set; }
        }
        public class UsersGmail
        {
            public string emailAddress { get; set; }
            public int messagesTotal { get; set; }
            public int threadsTotal { get; set; }
            public string historyId { get; set; }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                GetToken(Request.QueryString["code"].ToString());
            }

        }
        public void GetToken(string code)
        {
            string accesstoken = string.Empty;
            if (Session["accesstoken"] != null)
            {
                accesstoken = Convert.ToString(Session["accesstoken"]);
            }
            else
            {
                string clientid = Application["ClientId"].ToString();
                string clientsecret = Application["ClientSecret"].ToString();
                string redirection_url = "https://" + HttpContext.Current.Request.Url.Authority + "/Index.aspx";
                string url = "https://accounts.google.com/o/oauth2/token";

                string poststring = "grant_type=authorization_code&code=" + code + "&client_id=" + clientid + "&client_secret=" + clientsecret + "&redirect_uri=" + redirection_url + "";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                UTF8Encoding utfenc = new UTF8Encoding();
                byte[] bytes = utfenc.GetBytes(poststring);
                Stream outputstream = null;
                try
                {
                    request.ContentLength = bytes.Length;
                    outputstream = request.GetRequestStream();
                    outputstream.Write(bytes, 0, bytes.Length);
                }
                catch
                { }
                var response = (HttpWebResponse)request.GetResponse();
                var streamReader = new StreamReader(response.GetResponseStream());
                string responseFromServer = streamReader.ReadToEnd();
                JavaScriptSerializer js = new JavaScriptSerializer();
                Tokenclass obj = js.Deserialize<Tokenclass>(responseFromServer);
                Session["accesstoken"] = obj.access_token;
                accesstoken = obj.access_token;
            }
            GetuserProfile(accesstoken);
        }
        public void GetuserProfile(string accesstoken)
        {
            string url = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token=" + accesstoken + "";
            WebRequest request = WebRequest.Create(url);
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            JavaScriptSerializer js = new JavaScriptSerializer();
            Userclass userinfo = js.Deserialize<Userclass>(responseFromServer);

            url = "https://gmail.googleapis.com/gmail/v1/users/" + userinfo.id + "/profile?alt=json&access_token=" + accesstoken + "";
            request = WebRequest.Create(url);
            request.Credentials = CredentialCache.DefaultCredentials;
            response = request.GetResponse();
            dataStream = response.GetResponseStream();
            reader = new StreamReader(dataStream);
            responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            js = new JavaScriptSerializer();
            UsersGmail usersGmail = js.Deserialize<UsersGmail>(responseFromServer);

            imgprofile.ImageUrl = userinfo.picture;
            lblid.Text = userinfo.id;
            lblname.Text = userinfo.name;
            lblEmail.Text = usersGmail.emailAddress;
            lblgender.Text = userinfo.gender;
            lbllocale.Text = userinfo.locale;

            DataSet ds = new DataSet();
            using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["AppCn"].ConnectionString))
            {
                SqlParameter[] sqlParams = new SqlParameter[1];
                sqlParams[0] = new SqlParameter("@EmailId", SqlDbType.NVarChar);
                sqlParams[0].Value = lblEmail.Text;

                ds = SqlHelper.ExecuteDataset(cn, CommandType.StoredProcedure, "GetUserData", sqlParams);

                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    txtA1.Text = Convert.ToString(ds.Tables[0].Rows[0]["A1"]);
                    txtA2.Text = Convert.ToString(ds.Tables[0].Rows[0]["A2"]);
                    txtA3.Text = Convert.ToString(ds.Tables[0].Rows[0]["A3"]);
                    txtA4.Text = Convert.ToString(ds.Tables[0].Rows[0]["A4"]);
                    txtB1.Text = Convert.ToString(ds.Tables[0].Rows[0]["B1"]);
                    txtB2.Text = Convert.ToString(ds.Tables[0].Rows[0]["B2"]);
                    txtB3.Text = Convert.ToString(ds.Tables[0].Rows[0]["B3"]);
                    txtB4.Text = Convert.ToString(ds.Tables[0].Rows[0]["B4"]);
                }
            }

            //GoogleSignin(usersGmail.emailAddress, userinfo.name, userinfo.picture);
        }
        public void GoogleSignin(string email, string name, string profile)
        {
            GoogleSigninRequest googleSigninRequest = new GoogleSigninRequest()
            {
                email = email,
                name = name,
                profile = profile,
                deviceType = "Web",
                deviceToken = "sdfghj34567vikas",
            };
            string Url = string.Format("{0}/user/googleSignin", Application["ServerUrl"]);


            string Body = JsonConvert.SerializeObject(googleSigninRequest);
            var result = APICall.SendPost(Url, string.Empty, Body);

            JavaScriptSerializer js = new JavaScriptSerializer();
            GoogleSigninResponse.Root googleSigninResponse = js.Deserialize<GoogleSigninResponse.Root>(result.ToString());
            Session["APIAuthToken"] = googleSigninResponse.data.authToken;
            Session["APIUserId"] = googleSigninResponse.data._id;
        }
        public void GetuserCalendarEventList(DateTime timeMin, DateTime timeMax, string calendarId = "primary", Boolean IsNextDay = false)
        {
            string accesstoken = Convert.ToString(Session["accesstoken"]);

            //TimeZone time2 = TimeZone.CurrentTimeZone;
            //DateTime test = time2.ToUniversalTime(timeMin);
            //var singapore = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            //var singaporetimeMin = TimeZoneInfo.ConvertTimeFromUtc(test, singapore);

            //TimeZone time21 = TimeZone.CurrentTimeZone;
            //DateTime test1 = time21.ToUniversalTime(timeMax);
            //var singaporetimeMax = TimeZoneInfo.ConvertTimeFromUtc(test1, singapore);

            //string url = "https://www.googleapis.com/calendar/v3/calendars/" + calendarId + "/events?alt=json&timeMin="
            //                                                                 + singaporetimeMin.ToString("yyyy-MM-ddTHH:mm:ss.000Z") + "&timeMax="
            //                                                                 + singaporetimeMax.ToString("yyyy-MM-ddTHH:mm:ss.000Z") + "&access_token=" + accesstoken + "";
            
            string url = "https://www.googleapis.com/calendar/v3/calendars/" + calendarId + "/events?alt=json&timeMin="
                                                                             + timeMin.ToString("yyyy-MM-ddT00:00:00.000Z") + "&timeMax="
                                                                             + timeMax.ToString("yyyy-MM-ddT23:59:59.000Z") + "&access_token=" + accesstoken + "";
            WebRequest request = WebRequest.Create(url);
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            JavaScriptSerializer js = new JavaScriptSerializer();
            CalendarEvent Events = js.Deserialize<CalendarEvent>(responseFromServer);

            Table tb1 = (Table)Session["STable"];
            if (Events != null && Events.items.Count > 0)
            {
                System.Web.UI.HtmlControls.HtmlGenericControl createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                createlibot.Attributes.Add("class", "message bot appeared");

                System.Web.UI.HtmlControls.HtmlGenericControl createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                createDivAvatar.Attributes.Add("class", "message-avatar");

                System.Web.UI.HtmlControls.HtmlGenericControl createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                createDivWrapper.Attributes.Add("class", "message-wrapper");

                string OptionSelected = string.Empty;
                if (ViewState["CalendarId"] != null)
                    OptionSelected = Convert.ToString(ViewState["CalendarId"]);

                if (OptionSelected == "GENERAL")
                {
                    System.Web.UI.HtmlControls.HtmlGenericControl createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                    createDivText.Attributes.Add("class", "message-text");
                    if (calendarId == "2t361235hmgbfa5orl4njv6pfreku4di@import.calendar.google.com")
                        createDivText.InnerHtml = "HIGH SCHOOL DAYS DATA..";
                    else if (calendarId == "b206lbojuj97uutkjckd09vublgui1e9@import.calendar.google.com")
                        createDivText.InnerHtml = "HIGH SCHOOL EVENTS DATA..";
                    else if (calendarId == "en.singapore%23holiday@group.v.calendar.google.com")
                        createDivText.InnerHtml = "HOLIDAYS IN SINGAPORE DATA..";
                    else
                        createDivText.InnerHtml = "PRIMARY DATA..";
                    createDivWrapper.Controls.Add(createDivText);
                }

                int i = 0;
                foreach (Item Event in Events.items.Where(t => t.start.date == null))
                {
                    if (!string.IsNullOrEmpty(Event.start.dateTime))
                        Event.start.date = Event.start.dateTime.Substring(0, 10);
                }

                foreach (Item Event in Events.items.OrderBy(t => t.start.date).ThenBy(t => t.summary))
                {
                    if (!string.IsNullOrEmpty(Event.summary) && Event.summary.Trim() != "")
                    {
                        System.Web.UI.HtmlControls.HtmlGenericControl createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        if (i > 0)
                            createDivText.Style.Add("padding-left", "50px");

                        createDivText.InnerHtml = (Event.start.date != null ? Event.start.date.ToString() : "") + " | " + Event.summary.ToString();
                        createDivWrapper.Controls.Add(createDivText);

                        if (IsNextDay)
                        {
                            string EventName = Event.summary.Trim().ToUpper().ToString();
                            if (EventName == "DAY A" || EventName == "DAY B" || EventName == "DAY C" || EventName == "DAY D")
                                botinputbox.Value = EventName;
                        }
                    }
                    i = i + 1;
                }

                createlibot.Controls.Add(createDivAvatar);
                createlibot.Controls.Add(createDivWrapper);
                TableRow tr = new TableRow();
                TableCell tc1 = new TableCell();
                tc1.Controls.Add(createlibot);
                tr.Controls.Add(tc1);
                tb1.Controls.Add(tr);
            }
            else
            {
                System.Web.UI.HtmlControls.HtmlGenericControl createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                createlibot.Attributes.Add("class", "message bot appeared");

                System.Web.UI.HtmlControls.HtmlGenericControl createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                createDivAvatar.Attributes.Add("class", "message-avatar");

                System.Web.UI.HtmlControls.HtmlGenericControl createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                createDivWrapper.Attributes.Add("class", "message-wrapper");

                System.Web.UI.HtmlControls.HtmlGenericControl createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                createDivText.Attributes.Add("class", "message-text");

                if (calendarId == "2t361235hmgbfa5orl4njv6pfreku4di@import.calendar.google.com")
                    createDivText.InnerHtml = "HIGH SCHOOL DAYS DATA..";
                else if (calendarId == "b206lbojuj97uutkjckd09vublgui1e9@import.calendar.google.com")
                    createDivText.InnerHtml = "HIGH SCHOOL EVENTS DATA..";
                else if (calendarId == "en.singapore%23holiday@group.v.calendar.google.com")
                    createDivText.InnerHtml = "HOLIDAYS IN SINGAPORE DATA..";
                else
                    createDivText.InnerHtml = "PRIMARY DATA..";

                createDivWrapper.Controls.Add(createDivText);

                createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                createDivText.Attributes.Add("class", "message-text");
                createDivText.Style.Add("padding-left", "50px");
                createDivText.InnerHtml = "NO EVENT FOUND";
                createDivWrapper.Controls.Add(createDivText);

                createlibot.Controls.Add(createDivAvatar);
                createlibot.Controls.Add(createDivWrapper);
                TableRow tr = new TableRow();
                TableCell tc1 = new TableCell();
                tc1.Controls.Add(createlibot);
                tr.Controls.Add(tc1);
                tb1.Controls.Add(tr);

            }
            Session["STable"] = tb1;
            botmessagelist.Controls.Add(tb1);
        }
        public void GetUserEvents(string q, string calendarId = "primary")
        {
            string accesstoken = Convert.ToString(Session["accesstoken"]);

            TimeZone time2 = TimeZone.CurrentTimeZone;
            DateTime test = time2.ToUniversalTime(DateTime.Now);
            var singapore = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            var singaporetimeMin = TimeZoneInfo.ConvertTimeFromUtc(test, singapore);

            string url = "https://www.googleapis.com/calendar/v3/calendars/" + calendarId + "/events?alt=json&timeMin="
                                                                             + singaporetimeMin.ToString("yyyy-MM-ddTHH:mm:ss.562Z") + "&q=" + q + "&access_token=" + accesstoken + "";
            WebRequest request = WebRequest.Create(url);
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            JavaScriptSerializer js = new JavaScriptSerializer();
            CalendarEvent Events = js.Deserialize<CalendarEvent>(responseFromServer);

            Table tb1 = (Table)Session["STable"];

            System.Web.UI.HtmlControls.HtmlGenericControl createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
            createlibot.Attributes.Add("class", "message bot appeared");

            System.Web.UI.HtmlControls.HtmlGenericControl createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
            createDivAvatar.Attributes.Add("class", "message-avatar");

            System.Web.UI.HtmlControls.HtmlGenericControl createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
            createDivWrapper.Attributes.Add("class", "message-wrapper");

            int i = 0;
            foreach (Item Event in Events.items)
            {
                System.Web.UI.HtmlControls.HtmlGenericControl createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                createDivText.Attributes.Add("class", "message-text");
                if (i > 0)
                    createDivText.Style.Add("padding-left", "50px");
                createDivText.InnerHtml = (Event.start.date != null ? Event.start.date.ToString() : "") + " | " + Event.summary.ToString();
                createDivWrapper.Controls.Add(createDivText);
                i = i + 1;
            }

            if (Events.items.Count == 0)
            {
                System.Web.UI.HtmlControls.HtmlGenericControl createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                createDivText.Attributes.Add("class", "message-text");
                createDivText.InnerHtml = "NO EVENT AVAILABLE WITH : " + q;
                createDivWrapper.Controls.Add(createDivText);
            }

            createlibot.Controls.Add(createDivAvatar);
            createlibot.Controls.Add(createDivWrapper);

            TableRow tr = new TableRow();
            TableCell tc1 = new TableCell();
            tc1.Controls.Add(createlibot);
            tr.Controls.Add(tc1);
            tb1.Controls.Add(tr);

            Session["STable"] = tb1;
            botmessagelist.Controls.Add(tb1);
        }
        protected void btnSend_Click(object sender, EventArgs e)
        {
            string requestName = Convert.ToString(botinputbox.Value);

            Table tb1 = (Table)Session["STable"];
            if (tb1 != null && !string.IsNullOrEmpty(requestName) && requestName.Trim() != "")
            {
                //Black row
                TableRow tr = new TableRow();
                TableCell tc1 = new TableCell();
                tr.Controls.Add(tc1);
                tb1.Controls.Add(tr);

                System.Web.UI.HtmlControls.HtmlGenericControl createliuser = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                createliuser.Attributes.Add("class", "message user appeared");

                System.Web.UI.HtmlControls.HtmlGenericControl createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                createDivAvatar.Attributes.Add("class", "message-avatar");

                System.Web.UI.HtmlControls.HtmlGenericControl createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                createDivWrapper.Attributes.Add("class", "message-wrapper");

                System.Web.UI.HtmlControls.HtmlGenericControl createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                createDivText.Attributes.Add("class", "message-text");
                createDivText.InnerHtml = requestName.Trim().ToUpper();
                createDivWrapper.Controls.Add(createDivText);

                createliuser.Controls.Add(createDivAvatar);
                createliuser.Controls.Add(createDivWrapper);

                tr = new TableRow();
                tc1 = new TableCell();
                tc1.Controls.Add(createliuser);
                tr.Controls.Add(tc1);
                tb1.Controls.Add(tr);
                Session["STable"] = tb1;

                botinputbox.Value = string.Empty;

                System.Web.UI.HtmlControls.HtmlGenericControl createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");

                string CalendarId = "primary";
                switch (requestName.Trim().ToUpper())
                {
                    case "START":
                    case "RESET":
                        StartChat();
                        break;
                    case "CALENDAR":
                        #region CALENDAR

                        tb1 = (Table)Session["STable"];

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "SELECT A CALENDAR....";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "HIGH SCHOOL DAYS";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "HIGH SCHOOL EVENTS";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "HOLIDAYS IN SINGAPORE";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "GENERAL";
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);

                        Session["STable"] = tb1;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    case "HIGH SCHOOL DAYS":
                    case "HIGH SCHOOL EVENTS":
                    case "HOLIDAYS IN SINGAPORE":
                    case "GENERAL":
                        #region CALENDAR OPTION

                        if (requestName.Trim().ToUpper() == "HIGH SCHOOL DAYS")
                        {
                            ViewState["CalendarId"] = "2t361235hmgbfa5orl4njv6pfreku4di@import.calendar.google.com";
                        }
                        else if (requestName.Trim().ToUpper() == "HIGH SCHOOL EVENTS")
                        {
                            ViewState["CalendarId"] = "b206lbojuj97uutkjckd09vublgui1e9@import.calendar.google.com";
                        }
                        else if (requestName.Trim().ToUpper() == "HOLIDAYS IN SINGAPORE")
                        {
                            ViewState["CalendarId"] = "en.singapore%23holiday@group.v.calendar.google.com";
                        }
                        else if (requestName.Trim().ToUpper() == "GENERAL")
                        {
                            ViewState["CalendarId"] = "GENERAL";
                        }

                        tb1 = (Table)Session["STable"];

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "SELECT DAYS OPTION...";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "NEXT DAY";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "NEXT WEEK";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "BETWEEN DATES";
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);

                        Session["STable"] = tb1;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    case "NEXT DAY":
                        #region NEXT DAY
                        if (ViewState["CalendarId"] != null)
                            CalendarId = Convert.ToString(ViewState["CalendarId"]);

                        if (CalendarId == "GENERAL")
                        {
                            GetuserCalendarEventList(DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(1).Date, "2t361235hmgbfa5orl4njv6pfreku4di@import.calendar.google.com", true);
                            GetuserCalendarEventList(DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(1).Date, "b206lbojuj97uutkjckd09vublgui1e9@import.calendar.google.com", true);
                            GetuserCalendarEventList(DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(1).Date, "en.singapore%23holiday@group.v.calendar.google.com", true);
                            GetuserCalendarEventList(DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(1).Date, "primary", true);
                        }
                        else
                            GetuserCalendarEventList(DateTime.UtcNow.AddDays(1).Date, DateTime.UtcNow.AddDays(1).Date, CalendarId, true);
                        #endregion
                        break;
                    case "NEXT WEEK":
                        #region NEXT WEEK
                        if (ViewState["CalendarId"] != null)
                            CalendarId = Convert.ToString(ViewState["CalendarId"]);

                        DayOfWeek currentDay = DateTime.UtcNow.DayOfWeek;
                        int daysTillCurrentDay = currentDay - DayOfWeek.Monday;
                        DateTime currentWeekStartDate = DateTime.UtcNow.AddDays(-daysTillCurrentDay);

                        DateTime NextWeekStartDate = currentWeekStartDate.AddDays(7);

                        if (CalendarId == "GENERAL")
                        {
                            GetuserCalendarEventList(NextWeekStartDate.Date, NextWeekStartDate.AddDays(6).Date, "2t361235hmgbfa5orl4njv6pfreku4di@import.calendar.google.com", true);
                            GetuserCalendarEventList(NextWeekStartDate.Date, NextWeekStartDate.AddDays(6).Date, "b206lbojuj97uutkjckd09vublgui1e9@import.calendar.google.com", true);
                            GetuserCalendarEventList(NextWeekStartDate.Date, NextWeekStartDate.AddDays(6).Date, "en.singapore%23holiday@group.v.calendar.google.com", true);
                            GetuserCalendarEventList(NextWeekStartDate.Date, NextWeekStartDate.AddDays(6).Date, "primary", true);
                        }
                        else
                            GetuserCalendarEventList(NextWeekStartDate.Date, NextWeekStartDate.AddDays(6).Date, CalendarId, true);
                        #endregion
                        break;
                    case "BETWEEN DATES":
                        #region BETWEEN DATES
                        ViewState["SelectedOption"] = "BETWEEN DATES";

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "Date formate should be: YYYY-MM-DD | YYYY-MM-DD";
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);
                        Session["STable"] = tb1; ;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    case "EVENT":
                        #region EVENT
                        ViewState["SelectedOption"] = "EVENT";

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "Enter text for search..";
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);
                        Session["STable"] = tb1; ;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    case "SCHEDULE":
                        #region SCHEDULE

                        tb1 = (Table)Session["STable"];

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "WHICH SCHEDULE?";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "DAY A";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "DAY B";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "DAY C";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "DAY D";
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);

                        Session["STable"] = tb1;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    case "DAY A":
                        #region A

                        tb1 = (Table)Session["STable"];

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "A1: " + txtA1.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "A2: " + txtA2.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "A3: " + txtA3.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "A4: " + txtA4.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);

                        Session["STable"] = tb1;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    case "DAY B":
                        #region B

                        tb1 = (Table)Session["STable"];

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "B1: " + txtB1.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "B2: " + txtB2.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "B3: " + txtB3.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "B4: " + txtB4.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);

                        Session["STable"] = tb1;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    case "DAY C":
                        #region C

                        tb1 = (Table)Session["STable"];

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "A3: " + txtA3.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "A4: " + txtA4.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "A1: " + txtA1.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "A2: " + txtA2.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);

                        Session["STable"] = tb1;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    case "DAY D":
                        #region D

                        tb1 = (Table)Session["STable"];

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "B3: " + txtB3.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "B4: " + txtB4.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "B1: " + txtB1.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "B2: " + txtB2.Text;
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);

                        Session["STable"] = tb1;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    case "QUICK LINKS":
                        #region QUICK LINKS

                        tb1 = (Table)Session["STable"];

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "WHICH QUICK LINK?";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "CLUB LIST";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "SERVICE PORTAL";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "PROGRAM PLANNING GUIDE (PPG)";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "POWERSCHOOL";
                        createDivWrapper.Controls.Add(createDivText);

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = "SCHOOLOGY";
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);

                        Session["STable"] = tb1;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    case "CLUB LIST":
                        #region CLUB LIST
                        ViewState["SelectedOption"] = "EVENT";

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "https://sashsclubs.com/";
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);
                        Session["STable"] = tb1; ;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    case "SERVICE PORTAL":
                        #region SERVICE PORTAL
                        ViewState["SelectedOption"] = "EVENT";

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "https://www.sasserviceportal.com/accounts/login/?next=/";
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);
                        Session["STable"] = tb1; ;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    case "PROGRAM PLANNING GUIDE (PPG)":
                        #region PROGRAM PLANNING GUIDE (PPG)
                        ViewState["SelectedOption"] = "EVENT";

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "https://www.sas.edu.sg/uploaded/SAS/Learning_at_SAS/HS/docs/High_School_PPG.pdf";
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);
                        Session["STable"] = tb1; ;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    case "POWERSCHOOL":
                        #region POWERSCHOOL
                        ViewState["SelectedOption"] = "EVENT";

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "https://powerschool.sas.edu.sg/public/";
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);
                        Session["STable"] = tb1; ;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    case "SCHOOLOGY":
                        #region SCHOOLOGY
                        ViewState["SelectedOption"] = "EVENT";

                        createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                        createlibot.Attributes.Add("class", "message bot appeared");

                        createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivAvatar.Attributes.Add("class", "message-avatar");

                        createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivWrapper.Attributes.Add("class", "message-wrapper");

                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = "https://sas.schoology.com/";
                        createDivWrapper.Controls.Add(createDivText);

                        createlibot.Controls.Add(createDivAvatar);
                        createlibot.Controls.Add(createDivWrapper);

                        tr = new TableRow();
                        tc1 = new TableCell();
                        tc1.Controls.Add(createlibot);
                        tr.Controls.Add(tc1);
                        tb1.Controls.Add(tr);
                        Session["STable"] = tb1; ;
                        botmessagelist.Controls.Add(tb1);
                        #endregion
                        break;
                    default:
                        #region default
                        if (ViewState["SelectedOption"] != null && Convert.ToString(ViewState["SelectedOption"]) == "BETWEEN DATES")
                        {
                            try
                            {
                                string[] Dates = requestName.Trim().Split('|');
                                if (Dates.Length == 2)
                                {
                                    DateTime startDate = Convert.ToDateTime(Dates[0]);
                                    DateTime endDate = Convert.ToDateTime(Dates[1]);
                                    if (ViewState["CalendarId"] != null)
                                        CalendarId = Convert.ToString(ViewState["CalendarId"]);

                                    if (CalendarId == "GENERAL")
                                    {
                                        GetuserCalendarEventList(startDate.Date, endDate.Date, "2t361235hmgbfa5orl4njv6pfreku4di@import.calendar.google.com");
                                        GetuserCalendarEventList(startDate.Date, endDate.Date, "b206lbojuj97uutkjckd09vublgui1e9@import.calendar.google.com");
                                        GetuserCalendarEventList(startDate.Date, endDate.Date, "en.singapore%23holiday@group.v.calendar.google.com");
                                        GetuserCalendarEventList(startDate.Date, endDate.Date, "primary");
                                    }
                                    else
                                        GetuserCalendarEventList(startDate.Date, endDate.Date, CalendarId);
                                }
                                else
                                {
                                    createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                                    createlibot.Attributes.Add("class", "message bot appeared");

                                    createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                                    createDivAvatar.Attributes.Add("class", "message-avatar");

                                    createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                                    createDivWrapper.Attributes.Add("class", "message-wrapper");

                                    createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                                    createDivText.Attributes.Add("class", "message-text");
                                    createDivText.InnerHtml = "Date formate should be: YYYY-MM-DD | YYYY-MM-DD";
                                    createDivWrapper.Controls.Add(createDivText);

                                    createlibot.Controls.Add(createDivAvatar);
                                    createlibot.Controls.Add(createDivWrapper);

                                    tr = new TableRow();
                                    tc1 = new TableCell();
                                    tc1.Controls.Add(createlibot);
                                    tb1.Controls.Add(tr);
                                    Session["STable"] = tb1;
                                    botmessagelist.Controls.Add(tb1);
                                }
                            }
                            catch
                            {
                                createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                                createlibot.Attributes.Add("class", "message bot appeared");

                                createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                                createDivAvatar.Attributes.Add("class", "message-avatar");

                                createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                                createDivWrapper.Attributes.Add("class", "message-wrapper");

                                createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                                createDivText.Attributes.Add("class", "message-text");
                                createDivText.InnerHtml = "Date formate should be: YYYY-MM-DD | YYYY-MM-DD";
                                createDivWrapper.Controls.Add(createDivText);

                                createlibot.Controls.Add(createDivAvatar);
                                createlibot.Controls.Add(createDivWrapper);

                                tr = new TableRow();
                                tc1 = new TableCell();
                                tc1.Controls.Add(createlibot);
                                tb1.Controls.Add(tr);
                                Session["STable"] = tb1;
                                botmessagelist.Controls.Add(tb1);
                            }
                        }
                        else if (ViewState["SelectedOption"] != null && Convert.ToString(ViewState["SelectedOption"]) == "EVENT")
                        {
                            GetUserEvents(requestName.Trim());
                        }
                        else
                        {
                            createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                            createlibot.Attributes.Add("class", "message bot appeared");

                            createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                            createDivAvatar.Attributes.Add("class", "message-avatar");

                            createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                            createDivWrapper.Attributes.Add("class", "message-wrapper");

                            createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                            createDivText.Attributes.Add("class", "message-text");
                            createDivText.InnerHtml = "Please provide input.";
                            createDivWrapper.Controls.Add(createDivText);

                            createlibot.Controls.Add(createDivAvatar);
                            createlibot.Controls.Add(createDivWrapper);

                            tr = new TableRow();
                            tc1 = new TableCell();
                            tc1.Controls.Add(createlibot);
                            tr.Controls.Add(tc1);
                            tb1.Controls.Add(tr);
                            Session["STable"] = tb1;
                            botmessagelist.Controls.Add(tb1);
                        }
                        #endregion
                        break;
                }
            }
        }
        protected void btnClearChat_Click(object sender, EventArgs e)
        {
            Session["STable"] = null;
            Table tb1 = new Table();
            tb1.ID = "NewTable" + Guid.NewGuid().ToString("N");
            tb1.Style.Add("width", "100%");

            //Black row
            TableRow tr = new TableRow();
            TableCell tc1 = new TableCell();
            tr.Controls.Add(tc1);
            tb1.Controls.Add(tr);
            Session["STable"] = tb1;
            botmessagelist.Controls.Add(tb1);

        }
        protected void btnStartChat_Click(object sender, EventArgs e)
        {
            Session["STable"] = null;
            StartChat();
        }
        protected void btnProfile_Click(object sender, EventArgs e)
        {
            Table tb1 = (Table)Session["STable"];
            botmessagelist.Controls.Add(tb1);
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('Profile Detail');", true);
        }
        public void GET_PROFILE()
        {
            string Url = string.Format("{0}/user/getProfile/{1}", Application["ServerUrl"], Convert.ToString(Session["APIUserId"]));
            string result = string.Empty;
            HttpStatusCode httpStatusCode;
            string AuthToken = Convert.ToString(Session["APIAuthToken"]);
            APICall.APIGetCall(Url, AuthToken, out result, out httpStatusCode);
            JavaScriptSerializer js = new JavaScriptSerializer();
            GetProfileResponse.Root getProfileResponse = js.Deserialize<GetProfileResponse.Root>(result);
        }
        public void StartChat()
        {
            Session["STable"] = null;

            Table tb1 = new Table();
            tb1.ID = "NewTable" + Guid.NewGuid().ToString("N");
            tb1.Style.Add("width", "100%");

            TableRow tr = new TableRow();
            TableCell tc1 = new TableCell();
            tr.Controls.Add(tc1);
            tb1.Controls.Add(tr);

            #region START / Reset

            System.Web.UI.HtmlControls.HtmlGenericControl createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
            createlibot.Attributes.Add("class", "message bot appeared");

            System.Web.UI.HtmlControls.HtmlGenericControl createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
            createDivAvatar.Attributes.Add("class", "message-avatar");

            System.Web.UI.HtmlControls.HtmlGenericControl createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
            createDivWrapper.Attributes.Add("class", "message-wrapper");

            System.Web.UI.HtmlControls.HtmlGenericControl createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
            createDivText.Attributes.Add("class", "message-text");
            createDivText.Style.Add("padding-left", "50px");
            createDivText.InnerHtml = "WELCOME, PLEASE SELECT.";
            createDivWrapper.Controls.Add(createDivText);

            createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
            createDivText.Attributes.Add("class", "message-text");
            createDivText.InnerHtml = "CALENDAR";
            createDivWrapper.Controls.Add(createDivText);

            createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
            createDivText.Attributes.Add("class", "message-text");
            createDivText.Style.Add("padding-left", "50px");
            createDivText.InnerHtml = "EVENT";
            createDivWrapper.Controls.Add(createDivText);

            createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
            createDivText.Attributes.Add("class", "message-text");
            createDivText.Style.Add("padding-left", "50px");
            createDivText.InnerHtml = "SCHEDULE";
            createDivWrapper.Controls.Add(createDivText);

            createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
            createDivText.Attributes.Add("class", "message-text");
            createDivText.Style.Add("padding-left", "50px");
            createDivText.InnerHtml = "QUICK LINKS";
            createDivWrapper.Controls.Add(createDivText);

            createlibot.Controls.Add(createDivAvatar);
            createlibot.Controls.Add(createDivWrapper);

            tr = new TableRow();
            tc1 = new TableCell();
            tc1.Controls.Add(createlibot);
            tr.Controls.Add(tc1);
            tb1.Controls.Add(tr);

            Session["STable"] = tb1;
            botmessagelist.Controls.Add(tb1);
            #endregion


            //StartChatRequest startChatRequest = new StartChatRequest()
            //{
            //    from = Convert.ToString(Session["APIUserId"])
            //};
            //string Url = string.Format("{0}/chat/startChat", Application["ServerUrl"]);

            //string Body = JsonConvert.SerializeObject(startChatRequest);
            //var result = APICall.SendPost(Url, string.Empty, Body);

            //JavaScriptSerializer js = new JavaScriptSerializer();
            //StartChatResponse.Root responce = js.Deserialize<StartChatResponse.Root>(result.ToString());

            ////Black row
            //TableRow tr = new TableRow();
            //TableCell tc1 = new TableCell();
            //tr.Controls.Add(tc1);
            //tb1.Controls.Add(tr);

            //System.Web.UI.HtmlControls.HtmlGenericControl createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
            //createlibot.Attributes.Add("class", "message bot appeared");

            //System.Web.UI.HtmlControls.HtmlGenericControl createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
            //createDivAvatar.Attributes.Add("class", "message-avatar");

            //System.Web.UI.HtmlControls.HtmlGenericControl createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
            //createDivWrapper.Attributes.Add("class", "message-wrapper");

            //System.Web.UI.HtmlControls.HtmlGenericControl createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
            //createDivText.Attributes.Add("class", "message-text");
            //createDivText.InnerHtml = responce.data.question.ToString().ToUpper();
            //createDivWrapper.Controls.Add(createDivText);

            //for (int i = 0; i < responce.data.options.Count; i++)
            //{
            //    createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
            //    createDivText.Attributes.Add("class", "message-text");
            //    if (i > 0)
            //        createDivText.Style.Add("padding-left", "50px");
            //    createDivText.InnerHtml = responce.data.options[i].ToString().ToUpper();
            //    createDivWrapper.Controls.Add(createDivText);
            //}

            //createlibot.Controls.Add(createDivAvatar);
            //createlibot.Controls.Add(createDivWrapper);

            //tr = new TableRow();
            //tc1 = new TableCell();
            //tc1.Controls.Add(createlibot);
            //tr.Controls.Add(tc1);
            //tb1.Controls.Add(tr);

            //Session["STable"] = tb1;
            //botmessagelist.Controls.Add(tb1);
        }
        public void AddChat(string message)
        {
            string convId = string.Empty;
            if (Session["convId"] != null)
                convId = Convert.ToString(Session["convId"]);

            AddChatRequest addChatRequest = new AddChatRequest()
            {
                to = "62234a9c9e00d0f5b0c4abc0",
                from = Convert.ToString(Session["APIUserId"]),
                convId = convId,
                message = message,
                typeOfMessage = "typeOfMessage"
            };
            string Url = string.Format("{0}/chat/addChat", Application["ServerUrl"]);


            string Body = JsonConvert.SerializeObject(addChatRequest);
            var result = APICall.SendPost(Url, string.Empty, Body);

            JavaScriptSerializer js = new JavaScriptSerializer();
            AddChatReesponeseCheck response = js.Deserialize<AddChatReesponeseCheck>(result.ToString());

            Table tb1 = (Table)Session["STable"];
            if (response.success)
            {
                js = new JavaScriptSerializer();
                AddChatResponse.Root responce = js.Deserialize<AddChatResponse.Root>(result.ToString());

                if (responce.reqBody != null)
                    Session["convId"] = responce.reqBody.convId.ToString();

                //Black row
                TableRow tr = new TableRow();
                TableCell tc1 = new TableCell();
                tr.Controls.Add(tc1);
                tb1.Controls.Add(tr);

                if (responce.data.type == "question")
                {
                    System.Web.UI.HtmlControls.HtmlGenericControl createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                    createlibot.Attributes.Add("class", "message bot appeared");

                    System.Web.UI.HtmlControls.HtmlGenericControl createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                    createDivAvatar.Attributes.Add("class", "message-avatar");

                    System.Web.UI.HtmlControls.HtmlGenericControl createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                    createDivWrapper.Attributes.Add("class", "message-wrapper");

                    System.Web.UI.HtmlControls.HtmlGenericControl createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                    createDivText.Attributes.Add("class", "message-text");
                    createDivText.InnerHtml = responce.data.question.ToString().ToUpper();
                    createDivWrapper.Controls.Add(createDivText);

                    for (int i = 0; i < responce.data.options.Count; i++)
                    {
                        createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        if (i > 0)
                            createDivText.Style.Add("padding-left", "50px");
                        createDivText.InnerHtml = responce.data.options[i].ToString().ToUpper();
                        createDivWrapper.Controls.Add(createDivText);
                    }

                    createlibot.Controls.Add(createDivAvatar);
                    createlibot.Controls.Add(createDivWrapper);

                    tr = new TableRow();
                    tc1 = new TableCell();
                    tc1.Controls.Add(createlibot);
                    tr.Controls.Add(tc1);
                    tb1.Controls.Add(tr);
                }

                if (responce.data.type == "answer")
                {
                    System.Web.UI.HtmlControls.HtmlGenericControl createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                    createlibot.Attributes.Add("class", "message bot appeared");

                    System.Web.UI.HtmlControls.HtmlGenericControl createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                    createDivAvatar.Attributes.Add("class", "message-avatar");

                    System.Web.UI.HtmlControls.HtmlGenericControl createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                    createDivWrapper.Attributes.Add("class", "message-wrapper");

                    string[] strArray = responce.data.answer.Split('|');

                    if (strArray.Length > 0)
                    {
                        int i = 0;
                        foreach (string str in strArray)
                        {
                            System.Web.UI.HtmlControls.HtmlGenericControl createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                            createDivText.Attributes.Add("class", "message-text");
                            if (i > 0)
                                createDivText.Style.Add("padding-left", "50px");
                            createDivText.InnerHtml = str;
                            createDivWrapper.Controls.Add(createDivText);
                            i = i + 1;
                        }
                    }
                    else
                    {
                        System.Web.UI.HtmlControls.HtmlGenericControl createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                        createDivText.Attributes.Add("class", "message-text");
                        createDivText.InnerHtml = responce.data.answer.ToString();
                        createDivWrapper.Controls.Add(createDivText);
                    }

                    createlibot.Controls.Add(createDivAvatar);
                    createlibot.Controls.Add(createDivWrapper);

                    tr = new TableRow();
                    tc1 = new TableCell();
                    tc1.Controls.Add(createlibot);
                    tr.Controls.Add(tc1);
                    tb1.Controls.Add(tr);
                }
            }
            else
            {
                System.Web.UI.HtmlControls.HtmlGenericControl createlibot = new System.Web.UI.HtmlControls.HtmlGenericControl("li");
                createlibot.Attributes.Add("class", "message bot appeared");

                System.Web.UI.HtmlControls.HtmlGenericControl createDivAvatar = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                createDivAvatar.Attributes.Add("class", "message-avatar");

                System.Web.UI.HtmlControls.HtmlGenericControl createDivWrapper = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                createDivWrapper.Attributes.Add("class", "message-wrapper");

                System.Web.UI.HtmlControls.HtmlGenericControl createDivText = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
                createDivText.Attributes.Add("class", "message-text");
                createDivText.InnerHtml = response.msg.ToString();
                createDivWrapper.Controls.Add(createDivText);

                createlibot.Controls.Add(createDivAvatar);
                createlibot.Controls.Add(createDivWrapper);

                TableRow tr = new TableRow();
                TableCell tc1 = new TableCell();
                tc1.Controls.Add(createlibot);
                tr.Controls.Add(tc1);
                tb1.Controls.Add(tr);

            }
            Session["STable"] = tb1;
            botmessagelist.Controls.Add(tb1);
        }
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session["accesstoken"] = null;
            Session.Abandon();
            Response.Redirect("Default.aspx");
        }
        protected void SaveClose(object sender, EventArgs e)
        {
            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AppCn"].ConnectionString))
                {
                    using (var cmd = new SqlCommand("dbo.[AddEditUserData]", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        var EmailID = new SqlParameter("@EmailID", SqlDbType.NVarChar);
                        var A1 = new SqlParameter("@A1", SqlDbType.NVarChar);
                        var A2 = new SqlParameter("@A2", SqlDbType.NVarChar);
                        var A3 = new SqlParameter("@A3", SqlDbType.NVarChar);
                        var A4 = new SqlParameter("@A4", SqlDbType.NVarChar);
                        var B1 = new SqlParameter("@B1", SqlDbType.NVarChar);
                        var B2 = new SqlParameter("@B2", SqlDbType.NVarChar);
                        var B3 = new SqlParameter("@B3", SqlDbType.NVarChar);
                        var B4 = new SqlParameter("@B4", SqlDbType.NVarChar);

                        EmailID.Value = lblEmail.Text;
                        A1.Value = txtA1.Text;
                        A2.Value = txtA2.Text;
                        A3.Value = txtA3.Text;
                        A4.Value = txtA4.Text;
                        B1.Value = txtB1.Text;
                        B2.Value = txtB2.Text;
                        B3.Value = txtB3.Text;
                        B4.Value = txtB4.Text;

                        cmd.Parameters.Add(EmailID);
                        cmd.Parameters.Add(A1);
                        cmd.Parameters.Add(A2);
                        cmd.Parameters.Add(A3);
                        cmd.Parameters.Add(A4);
                        cmd.Parameters.Add(B1);
                        cmd.Parameters.Add(B2);
                        cmd.Parameters.Add(B3);
                        cmd.Parameters.Add(B4);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}