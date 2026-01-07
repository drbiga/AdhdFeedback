using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Diagnostics;

using UI.Models;

namespace UI.Services
{
    class MockSessionExecutionService
    {
        private static MockSessionExecutionService instance;
        private List<Feedback> feedbackOptions;
        private Random rand;
        private Feedback currentFeedback;
        private bool isGoingUp;

        public static MockSessionExecutionService GetOrCreate()
        {
            if (instance == null)
            {
                instance = new MockSessionExecutionService();
            }
            return instance;
        }

        private MockSessionExecutionService()
        {
            Feedback focusedFeedback = new Feedback
            {
                personal_analytics_data = new Feedback.PersonalAnalyticsData
                {
                    num_mouse_clicks = 10,
                    mouse_move_distance = 5.0f,
                    mouse_scroll_distance = 2.0f,
                    num_keyboard_strokes = 20,
                    attention_feedback = Feedback.FeedbackType.FOCUSED
                },
                classifier_data = new Feedback.ClassifierData
                {
                    screenshot = "screenshot_url",
                    prediction = "focused"
                },
                output = Feedback.FeedbackType.FOCUSED
            };
            Feedback normalFeedback = new Feedback
            {
                personal_analytics_data = new Feedback.PersonalAnalyticsData
                {
                    num_mouse_clicks = 10,
                    mouse_move_distance = 5.0f,
                    mouse_scroll_distance = 2.0f,
                    num_keyboard_strokes = 20,
                    attention_feedback = Feedback.FeedbackType.NORMAL
                },
                classifier_data = new Feedback.ClassifierData
                {
                    screenshot = "screenshot_url",
                    prediction = "focused"
                },
                output = Feedback.FeedbackType.NORMAL
            };
            Feedback distractedFeedback = new Feedback
            {
                personal_analytics_data = new Feedback.PersonalAnalyticsData
                {
                    num_mouse_clicks = 10,
                    mouse_move_distance = 5.0f,
                    mouse_scroll_distance = 2.0f,
                    num_keyboard_strokes = 20,
                    attention_feedback = Feedback.FeedbackType.NORMAL
                },
                classifier_data = new Feedback.ClassifierData
                {
                    screenshot = "screenshot_url",
                    prediction = "focused"
                },
                output = Feedback.FeedbackType.DISTRACTED
            };
            feedbackOptions = new List<Feedback> { focusedFeedback, normalFeedback, distractedFeedback };
            rand = new Random();
            currentFeedback = focusedFeedback;
            isGoingUp = true;
        }

        public Feedback GetCurrentFeedback()
        {
            if (isGoingUp)
            {
                if (currentFeedback.output == Feedback.FeedbackType.FOCUSED)
                    currentFeedback = feedbackOptions[1];
                else if (currentFeedback.output == Feedback.FeedbackType.NORMAL)
                    currentFeedback = feedbackOptions[2];
                else if (currentFeedback.output == Feedback.FeedbackType.DISTRACTED)
                {
                    isGoingUp = false;
                    currentFeedback = feedbackOptions[2];
                }
            }
            else
            {
                if (currentFeedback.output == Feedback.FeedbackType.FOCUSED)
                {
                    isGoingUp = true;
                    currentFeedback = feedbackOptions[0];
                }
                else if (currentFeedback.output == Feedback.FeedbackType.NORMAL)
                    currentFeedback = feedbackOptions[0];
                else if (currentFeedback.output == Feedback.FeedbackType.DISTRACTED)
                    currentFeedback = feedbackOptions[1];
            }
                return currentFeedback;
        }

        //public bool SessionIsSet()
        //{
        //    return this.iamSession != null;
        //}

        //public IamSession GetIamSession()
        //{
        //    return this.iamSession;
        //}


        ///// <summary>
        ///// Gets the next session for the currently active student.
        ///// Assumes that the current IAM session is already set.
        ///// </summary>
        ///// <returns>The next session the student is supposed to do.</returns>
        //async public Task<Session> GetNextSession()
        //{
        //    if (iamSession == null)
        //        throw new Exception("Student does not have an active session");
        //    HttpClient client = new HttpClient();
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", iamSession.token);
        //    string jsonResponse = await client.GetStringAsync(
        //        String.Format(
        //            "{0}://{1}:{2}/session_execution/student/{3}/remaining_sessions",
        //            this.backendProtocol,
        //            this.backendHost,
        //            this.backendPort,
        //            iamSession.user.username
        //        )
        //    );
        //    var settings = new JsonSerializerSettings
        //    {
        //        NullValueHandling = NullValueHandling.Ignore,
        //        MissingMemberHandling = MissingMemberHandling.Ignore
        //    };
        //    Session[] sessions = JsonConvert.DeserializeObject<Session[]>(jsonResponse, settings);
        //    return sessions[0];
        //}

        //async public Task<Session> GetStudentActiveSession()
        //{
        //    if (iamSession == null)
        //        throw new Exception("Student does not have an active session");
        //    HttpClient client = new HttpClient();
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", iamSession.token);
        //    string jsonResponse = await client.GetStringAsync(
        //        String.Format(
        //            "{0}://{1}:{2}/session_execution/student?student_name={3}",
        //            this.backendProtocol,
        //            this.backendHost,
        //            this.backendPort,
        //            iamSession.user.username
        //        )
        //    );
        //    var settings = new JsonSerializerSettings
        //    {
        //        NullValueHandling = NullValueHandling.Ignore,
        //        MissingMemberHandling = MissingMemberHandling.Ignore
        //    };
        //    SessionExecutionStudent student = JsonConvert.DeserializeObject<SessionExecutionStudent>(jsonResponse, settings);
        //    return student.active_session;
        //}
    }
}
