using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using System.Threading;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2.Responses;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Requests;

namespace ChatBotApp
{
    public partial class Calendar1 : System.Web.UI.Page
    {        
        static string[] scopes = { CalendarService.Scope.Calendar };
        static string ApplicationName = "Google Calendar API .NET Quickstart";

        protected void Page_Load(object sender, EventArgs e)
        {
            GetCalendar("", "");
        }

        private static void GetCalendar(string accesstoken, string refreshtoken)
        {
            string clientid = "1009113568321-3ir771og70lop7n6ecklu8k7psucok5v.apps.googleusercontent.com";
            string clientsecret = "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";
            string redirection_url = "https://" + HttpContext.Current.Request.Url.Authority + "/Index.aspx";
            string url = "https://accounts.google.com/o/oauth2/token";

            //// Part1 
            //var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            //{
            //    ClientSecrets = new ClientSecrets
            //    {
            //        ClientId = "1009113568321-l67fch45nmt05r5ahmpg2ipdal4q30tq.apps.googleusercontent.com",
            //        ClientSecret = "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"
            //    },
            //    Scopes = scopes,
            //    DataStore = new FileDataStore("Store"),

            //});

            //var token = new TokenResponse
            //{
            //    AccessToken = accesstoken,
            //    RefreshToken = refreshtoken
            //};

            //UserCredential credential = new UserCredential(flow, Environment.UserName, token);


            try
            {
                //Part 2
                UserCredential credential;
                // Load client secrets.
                string path = AppDomain.CurrentDomain.BaseDirectory.ToString();
                string Filepath = path + "credentials.json";

                //using (var stream = new FileStream(Filepath, FileMode.Open, FileAccess.Read))
                //{
                //    /* The file token.json stores the user's access and refresh tokens, and is created
                //     automatically when the authorization flow completes for the first time. */
                //    string credPath = "token.json";
                //    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                //        GoogleClientSecrets.FromStream(stream).Secrets,
                //        scopes,
                //        "user",
                //        CancellationToken.None).Result;
                //    Console.WriteLine("Credential file saved to: ");
                //}

                //Part 3

                //credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                //    new ClientSecrets
                //    {
                //        ClientId = "1009113568321-l67fch45nmt05r5ahmpg2ipdal4q30tq.apps.googleusercontent.com",
                //        ClientSecret = "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"
                //    },
                //    scopes,
                //    "user",
                //    CancellationToken.None).Result;
                //Console.WriteLine("Credential file saved to: ");

                // Part 4 
                dsAuthorizationBroker.RedirectUri = redirection_url;
                credential = dsAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = clientid,
                        ClientSecret = clientsecret
                    },
                    scopes,
                    "user",
                    CancellationToken.None).Result;
                Console.WriteLine("Credential file saved to: ");


                // Create Google Calendar API service.
                var service = new CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });



                // Define parameters of request.
                EventsResource.ListRequest request = service.Events.List("primary");
                request.TimeMin = DateTime.Now;
                request.ShowDeleted = false;
                request.SingleEvents = true;
                request.MaxResults = 10;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                // List events.
                Events events = request.Execute();
                Console.WriteLine("Upcoming events:");
                if (events.Items == null || events.Items.Count == 0)
                {
                    Console.WriteLine("No upcoming events found.");
                    return;
                }
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public class dsAuthorizationBroker : GoogleWebAuthorizationBroker
        {
            public static string RedirectUri;

            public static async Task<UserCredential> AuthorizeAsync(
                ClientSecrets clientSecrets,
                IEnumerable<string> scopes,
                string user,
                CancellationToken taskCancellationToken,
                IDataStore dataStore = null)
            {
                var initializer = new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets,
                };
                return await AuthorizeAsyncCore(initializer, scopes, user,
                    taskCancellationToken, dataStore).ConfigureAwait(false);
            }

            private static async Task<UserCredential> AuthorizeAsyncCore(
                GoogleAuthorizationCodeFlow.Initializer initializer,
                IEnumerable<string> scopes,
                string user,
                CancellationToken taskCancellationToken,
                IDataStore dataStore)
            {
                initializer.Scopes = scopes;
                initializer.DataStore = dataStore ?? new FileDataStore(Folder);
                var flow = new dsAuthorizationCodeFlow(initializer);
                return await new AuthorizationCodeInstalledApp(flow,
                    new LocalServerCodeReceiver())
                    .AuthorizeAsync(user, taskCancellationToken).ConfigureAwait(false);
            }
        }

        public class dsAuthorizationCodeFlow : GoogleAuthorizationCodeFlow
        {
            public dsAuthorizationCodeFlow(Initializer initializer)
                : base(initializer) { }

            public override AuthorizationCodeRequestUrl
                           CreateAuthorizationCodeRequest(string redirectUri)
            {
                return base.CreateAuthorizationCodeRequest(dsAuthorizationBroker.RedirectUri);
            }
        }

    }


}