using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Diagnostics;

using UI.Models;
using System.Threading.Tasks;
using Core.Models;

using IamSession = UI.Models.IamSession;
using Feedback = UI.Models.Feedback;

namespace UI.Services
{


    public class Session
    {
        public class SessionStage
        {
            public static string WAITING = "waiting";
            public static string READCOMP = "readcomp";
            public static string HOMEWORK = "homework";
            public static string SURVEY = "survey";
            public static string FINISHED = "finished";
        }

        public int seqnum;
        public string start_link;
        public bool is_passthrough;
        public bool has_feedback;
        public bool no_equipment;
        public int remaining_time_seconds;
        public string stage;

    }

    public class StudentSessionNotStartedException : Exception { }
    public class IamSessionNotSetException : Exception { }

    public class SessionExecutionStudent
    {
        public string name;
        public Session[] sessions_done;
        // public SessionAnalytics[] sessions_analytics;
        public Session active_session;
    }

    public class SessionExecutionService : ISessionExecutionService
    {

        private static SessionExecutionService instance;
        private string backendProtocol;
        private string backendHost;
        private int backendPort;
        private string backendPrefix;
        private string localServerHost;
        private int localServerPort;
        private IamSession? iamSession;
        private Feedback? currentFeedback;

        private bool sessionHasFeedback;

        private DateTime datetimeLastUpdate;

        // Singleton class
        // Enforcing one single instance
        public static SessionExecutionService GetOrCreate()
        {
            if (instance == null)
            {
                string message = "[ SessionExecutionService.GetOrCreate ] Creating new instance of the session execution service";
                Debug.WriteLine(message);
                Trace.WriteLine(message);
                instance = new SessionExecutionService();
            }
            return instance;
        }

        public SessionExecutionService()
        {
            this.iamSession = null;
            this.currentFeedback = null;

            UpdateServerParamsFromSettings();

//            // ------------------------------------------------------------------------
//            // Local backend config for dev purposes
//#if DEBUG
//            UseDevBackend();
//#endif
//            // ------------------------------------------------------------------------
//            // Staging backend config
//#if STAGING
//            UseStagingBackend();
//#endif

//            // ------------------------------------------------------------------------
//            // Production backend config
//#if RELEASE
//            UseProductionBackend();
//#endif

            // ------------------------------------------------------------------------
            localServerHost = "localhost";
            localServerPort = 8001;

            datetimeLastUpdate = DateTime.Now;

            this.sessionHasFeedback = true;

            InitializeIamSession();
        }

        public void UpdateServerParamsFromSettings()
        {
            var serverParams = Settings.Current.ServerParams;
            this.backendProtocol = serverParams.BackendProtocol;
            this.backendHost = serverParams.BackendHost;
            this.backendPort = serverParams.BackendPort;
            this.backendPrefix = serverParams.BackendPrefix;

            Debug.WriteLine(serverParams);
        }

        private async Task InitializeIamSession()
        {
            while (true)
            {
                await Task.Delay(1 * 1000);
                try
                {
                    IamSession iamSession = await this.GetCurrentIamSession();
                    if (iamSession == null)
                    {
                        continue;
                    }
                    if (this.iamSession == null)
                    {
                        this.iamSession = iamSession;
                        continue;
                    }
                    if (!iamSession.token.Equals(this.iamSession.token))
                    {
                        this.iamSession = iamSession;
                    }
                    // The session can change and unity will not see that change unless it
                    // continuously polls the local server.
                    // break;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(String.Format("[ SessionExecutionService.InitializeIamSession ] Exception when getting the IAM session: {0}", e.ToString()));
                    continue;
                }
            }
        }

        //public void UseDevBackend()
        //{
        //    this.backendProtocol = "http";
        //    this.backendHost = "localhost";
        //    this.backendPort = 8000;
        //    this.backendPrefix = "/";
        //}

        //public void UseStagingBackend()
        //{
        //    this.backendProtocol = "https";
        //    this.backendHost = "testlsuadhd.centralus.cloudapp.azure.com";
        //    this.backendPort = 443;
        //    this.backendPrefix = "/api";
        //    System.Windows.MessageBox.Show("Switched to staging backend");
        //}

        //public void UseProductionBackend()
        //{
        //    this.backendProtocol = "https";
        //    this.backendHost = "lsuadhd.centralus.cloudapp.azure.com";
        //    this.backendPort = 443;
        //    this.backendPrefix = "/api";
        //    System.Windows.MessageBox.Show("Switched to staging backend");
        //}

        private async Task<IamSession> GetCurrentIamSession()
        {
            HttpClient client = new HttpClient();
            string jsonResponse = await client.GetStringAsync(
                String.Format("http://{0}:{1}/session", localServerHost, localServerPort)
            );
            Debug.WriteLine(jsonResponse);
            return JsonConvert.DeserializeObject<IamSession>(jsonResponse);
        }

        /// <summary>
        /// Gets the current feedback for the active student session.
        /// In case there is no active session, an exception is raised.
        /// If the session
        /// </summary>
        /// <returns></returns>
        /// <exception cref="StudentSessionNotStartedException"></exception>
        /// <exception cref="IamSessionNotSetException"></exception>
        public Feedback GetCurrentFeedback()
        {
            if (this.iamSession == null)
            {
                throw new IamSessionNotSetException();
            }

            // If less than half a second has passed, then we just use the cached feedback to prevent
            // making too many requests to the backend.
            if (DateTime.Now - this.datetimeLastUpdate < TimeSpan.FromSeconds(0.5))
            {
                return this.currentFeedback;
            }
            this.datetimeLastUpdate = DateTime.Now;
            _GetCurrentFeedback();

            return this.currentFeedback;
        }

        /// <summary>
        /// Gets the current feedback for the active student session.
        /// This method is async and updates the currentFeedback variable when the response is received.
        /// The async version is necessary to prevent blocking the main thread while waiting for the response
        /// from the backend, which can take some time, and we don't want to freeze the UI while waiting for it.
        /// </summary>
        private async void _GetCurrentFeedback()
        {
            HttpClient client = new HttpClient();
            Feedback? feedback = null;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", iamSession.token);
            try
            {

                string jsonResponse = await client.GetStringAsync(
                    String.Format(
                        "{0}://{1}:{2}{3}/session_execution/student/{4}/session/feedback",
                        this.backendProtocol,
                        this.backendHost,
                        this.backendPort,
                        backendPrefix,
                        iamSession.user.username
                    )
                );
                Debug.WriteLine("[ SessionExecutionService.GetCurrentFeedback ] Feedback response:");
                Debug.WriteLine(jsonResponse);
                feedback = JsonConvert.DeserializeObject<Feedback?>(jsonResponse);
                sessionHasFeedback = true;
            }
            catch (HttpRequestException error)
            {
                if (error.StatusCode == HttpStatusCode.BadRequest)
                {
                    Debug.WriteLine("[ SessionExecutionService.GetCurrentFeedback ] Feedback still not available. User possibly did not start session yet.");
                    Trace.WriteLine("[ SessionExecutionService.GetCurrentFeedback ] Feedback still not available. User possibly did not start session yet.");
                }
                else if (error.StatusCode == HttpStatusCode.MethodNotAllowed)
                {
                    this.sessionHasFeedback = false;
                    Debug.WriteLine("[ SessionExecutionService.GetCurrentFeedback ] Feedback not available. User possibly has no feedback for this session.\n" + error.Data);
                    Trace.WriteLine("[ SessionExecutionService.GetCurrentFeedback ] Feedback not available. User possibly has no feedback for this session.\n" + error.Data);
                }
                else
                {
                    Debug.WriteLine("[ SessionExecutionService.GetCurrentFeedback ] Unknown error");
                    Debug.WriteLine(error.ToString());
                    Trace.WriteLine("[ SessionExecutionService.GetCurrentFeedback ] Unknown error");
                    Trace.WriteLine(error.ToString());
                }
            }
            
            if (feedback != null)
            {
                this.currentFeedback = feedback;
            }
            else
            {
                Trace.WriteLine("[ SessionExecutionService.GetCurrentFeedback ] Feedback is null");
                Debug.WriteLine("[ SessionExecutionService.GetCurrentFeedback ] Feedback is null");
            }
        }

        public bool SessionIsSet()
        {
            return this.iamSession != null;
        }

        public IamSession GetIamSession()
        {
            return this.iamSession;
        }

        async public Task<Session> GetStudentActiveSession()
        {
            if (iamSession == null)
                throw new Exception("Student does not have an active session");
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", iamSession.token);
            string jsonResponse = await client.GetStringAsync(
                String.Format(
                    "{0}://{1}:{2}{3}/session_execution/student?student_name={4}",
                    this.backendProtocol,
                    this.backendHost,
                    this.backendPort,
                    backendPrefix,
                    iamSession.user.username
                )
            );
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            SessionExecutionStudent student = JsonConvert.DeserializeObject<SessionExecutionStudent>(jsonResponse, settings);
            return student.active_session;
        }

        public bool SessionHasFeedback()
        {
            return this.sessionHasFeedback;
        }
    }
}
