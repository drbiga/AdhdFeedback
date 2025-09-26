using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Diagnostics;

namespace UI.Services
{
    class FeedbackType
    {
        public const string FOCUSED = "focused";
        public const string NORMAL = "normal";
        public const string DISTRACTED = "distracted";
    }

    class Feedback
    {
        public PersonalAnalyticsData personal_analytics_data;
        public ClassifierData classifier_data;
        public string output;
    }

    class PersonalAnalyticsData
    {
        public int num_mouse_clicks;
        public float mouse_move_distance;
        public float mouse_scroll_distance;
        public int num_keyboard_strokes;
        public string attention_feedback;
    }

    class ClassifierData { public string screenshot; public string prediction; }

    class IamSession
    {
        public string token;
        public User user;
        public string ip_address;
    }

    class User
    {
        public string username;
        private string password;
        public string role;
    }

    class Role
    {
        static string STUDENT = "student";
        static string MANAGER = "manager";
    }

    class Session
    {
        public int seqnum;
        public string start_link;
        public bool is_passthrough;
        public bool has_feedback;
        public bool no_equipment;
        public int remaining_time_seconds;
        public string stage;

    }
    class SessionStage
    {
        public static string WAITING = "waiting";
        public static string READCOMP = "readcomp";
        public static string HOMEWORK = "homework";
        public static string SURVEY = "survey";
        public static string FINISHED = "finished";
    }

    class StudentSessionNotStartedException : Exception { }

    class SessionExecutionStudent
    {
        public string name;
        public Session[] sessions_done;
        // public SessionAnalytics[] sessions_analytics;
        public Session active_session;
    }

    class SessionExecutionService
    {
        private static SessionExecutionService instance;
        private string backendProtocol;
        private string backendHost;
        private int backendPort;
        private string backendPrefix;
        private string localServerHost;
        private int localServerPort;
        private IamSession iamSession;
        private Feedback currentFeedback;

        private bool sessionHasFeedback;

        private DateTime datetimeLastUpdate;

        // Singleton class
        // Enforcing one single instance
        public static SessionExecutionService GetOrCreate()
        {
            if (instance == null)
            {
                Debug.WriteLine("[ SessionExecutionService.GetOrCreate ] Creating new instance of the session execution service");
                instance = new SessionExecutionService();
            }
            return instance;
        }

        private SessionExecutionService()
        {
            this.iamSession = null;
            this.currentFeedback = null;

            // ------------------------------------------------------------------------
            // Production backend config
            this.backendProtocol = "https";
            this.backendHost = "lsuadhd.centralus.cloudapp.azure.com";
            this.backendPort = 443;
            this.backendPrefix = "/api";

            // ------------------------------------------------------------------------
            // Test backend config
            //this.backendProtocol = "http";
            //this.backendHost = "127.0.0.1";
            //this.backendPort = 8000;
            //this.backendPrefix = "";

            // ------------------------------------------------------------------------
            localServerHost = "localhost";
            localServerPort = 8001;

            datetimeLastUpdate = DateTime.Now;

            this.sessionHasFeedback = true;

            Task.Run(async () => await InitializeIamSession());
        }

        private async Task InitializeIamSession()
        {
            while (true)
            {
                await Task.Delay(1 * 1000);
                try
                {
                    IamSession iamSession = Task.Run(async () => await this.GetCurrentIamSession()).Result;
                    if (this.iamSession == null && iamSession != null)
                    {
                        this.iamSession = iamSession;
                    }
                    else if (iamSession != null && iamSession.token.Equals(this.iamSession.token))
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

        async Task<IamSession> GetCurrentIamSession()
        {
            HttpClient client = new HttpClient();
            string jsonResponse = await client.GetStringAsync(
                String.Format("http://{0}:{1}/session", localServerHost, localServerPort)
            );
            Debug.WriteLine(jsonResponse);
            return JsonConvert.DeserializeObject<IamSession>(jsonResponse);
        }

        public Feedback GetCurrentFeedback()
        {
            if (this.iamSession == null)
            {
                throw new StudentSessionNotStartedException();
            }

            if (DateTime.Now - this.datetimeLastUpdate < TimeSpan.FromSeconds(0.5))
            {
                return this.currentFeedback;
            }
            this.datetimeLastUpdate = DateTime.Now;
            // Running in another thread in order not to block unity and make the game appear laggy.
            Task.Run(() =>
            {
                Task<Feedback?> task = Task.Run(async () =>
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
                        Debug.WriteLine(jsonResponse);
                        feedback = JsonConvert.DeserializeObject<Feedback?>(jsonResponse);
                        sessionHasFeedback = true;
                    }
                    catch (HttpRequestException error)
                    {
                        if (error.StatusCode == HttpStatusCode.BadRequest)
                        {
                            Debug.WriteLine("Feedback still not available. User possibly did not start session yet.");
                        }
                        if (error.StatusCode == HttpStatusCode.MethodNotAllowed)
                        {
                            this.sessionHasFeedback = false;
                            Debug.WriteLine("Feedback not available. User possibly has no feedback for this session.\n" + error.Data);
                            Trace.WriteLine("Feedback not available. User possibly has no feedback for this session.\n" + error.Data);
                        }
                    }
                    return feedback;
                });

                var newFeedback = task.Result;
                if (newFeedback != null)
                {
                    this.currentFeedback = newFeedback;
                }
            });

            return this.currentFeedback;
        }

        public bool SessionIsSet()
        {
            return this.iamSession != null;
        }

        public IamSession GetIamSession()
        {
            return this.iamSession;
        }


        /// <summary>
        /// Gets the next session for the currently active student.
        /// Assumes that the current IAM session is already set.
        /// </summary>
        /// <returns>The next session the student is supposed to do.</returns>
        async public Task<Session> GetNextSession()
        {
            if (iamSession == null)
                throw new Exception("Student does not have an active session");
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", iamSession.token);
            string jsonResponse = await client.GetStringAsync(
                String.Format(
                    "{0}://{1}:{2}{3}/session_execution/student/{4}/remaining_sessions",
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
            Session[] sessions = JsonConvert.DeserializeObject<Session[]>(jsonResponse, settings);
            return sessions[0];
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
