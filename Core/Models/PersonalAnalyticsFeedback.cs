using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class PersonalAnalyticsFeedback
    {
        public static int NOT_INITIALIZED = -1;

        public int keystrokes = NOT_INITIALIZED;
        public int mouseClicks = NOT_INITIALIZED;
        public float scrollDistance = NOT_INITIALIZED;
        public float movedDistance = NOT_INITIALIZED;

        public PersonalAnalyticsFeedback() { }

        public PersonalAnalyticsFeedback(int keystrokes, int mouseClicks, float scrollDistance, float movedDistance)
        {
            this.keystrokes = keystrokes;
            this.mouseClicks = mouseClicks;
            this.scrollDistance = scrollDistance;
            this.movedDistance = movedDistance;
        }

        public bool IsGap()
        {
            if (keystrokes == NOT_INITIALIZED ||
                mouseClicks == NOT_INITIALIZED ||
                scrollDistance == NOT_INITIALIZED ||
                movedDistance == NOT_INITIALIZED)
            {
                throw new InvalidOperationException("Cannot determine gap status for uninitialized feedback.");
            }

            if (keystrokes == 0 && mouseClicks == 0 && scrollDistance == 0 && movedDistance == 0)
            {
                return true;
            }
            return false;
        }
    }
}
