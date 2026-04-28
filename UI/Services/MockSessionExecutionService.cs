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
            if (currentFeedback == null)
            {
                string message = "[ MockSessionExecutionService.GetCurrentFeedback ] The current feedback is null";
                Debug.WriteLine(message);
                Trace.WriteLine(message);
                throw new Exception(message);
            }
            return currentFeedback;
        }

        public bool SessionHasFeedback()
        {
            bool result = false;
            string message = String.Format("[ MockSessionExecutionService ] SessionHasFeedback called. Returning {0}", result);
            Debug.WriteLine(message);
            Trace.WriteLine(message);
            return result;
        }
    }
}
