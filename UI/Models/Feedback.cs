using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Services;

namespace UI.Models
{
    public class Feedback
    {
        public class FeedbackType
        {
            public const string FOCUSED = "focused";
            public const string NORMAL = "normal";
            public const string DISTRACTED = "distracted";
        }

        public class PersonalAnalyticsData
        {
            public int num_mouse_clicks;
            public float mouse_move_distance;
            public float mouse_scroll_distance;
            public int num_keyboard_strokes;
            public string attention_feedback;
        }

        public class ClassifierData
        {
            public string screenshot;
            public string prediction;
        }

        public PersonalAnalyticsData personal_analytics_data;
        public ClassifierData classifier_data;
        public string output;
    }
}
