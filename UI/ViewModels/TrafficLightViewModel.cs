using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using UI.Models;

using UI.Services;

namespace UI.ViewModels
{
    internal class TrafficLightViewModel : INotifyPropertyChanged
    {
        #region Attributes

        public event PropertyChangedEventHandler? PropertyChanged;

        public enum LightColor
        {
            None = 0,
            Red,
            Yellow,
            Green
        }
        private LightColor _currentLight;
        public LightColor CurrentLight
        {
            get => _currentLight;
            set
            {
                if (_currentLight != value)
                {
                    _currentLight = value;
                    OnPropertyChanged(nameof(CurrentLight));
                }
            }
        }

        private bool _enabled;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        private bool isReady;
        public bool IsReady
        {
            get => isReady;
            set
            {
                if (isReady != value)
                {
                    isReady = value;
                    OnPropertyChanged(nameof(IsReady));
                }
            }
        }


        private DispatcherTimer colorTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(2),
        };

        private int currentColorIndex = 2;

        private DispatcherTimer enabledTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(4),
        };

        #endregion

        #region methods
        public TrafficLightViewModel()
        {
            colorTimer.Tick += LightTick;
            colorTimer.Start();

            enabledTimer.Tick += (s, e) =>
            {
                //Enabled = !Enabled;
            };
            enabledTimer.Start();
        }

        private void LightTick(object sender, EventArgs args)
        {
            UpdateLightsFromBackend();
            //NextLight();
        }


        private void UpdateLightsFromBackend()
        {
            Feedback state;
            try
            {
                //state = GetCurrentFeedback();
                SessionExecutionService s = SessionExecutionService.GetOrCreate();
                state = s.GetCurrentFeedback();
                IsReady = true;
                if (state == null)
                {
                    // It is possible to not have any feedbacks simply because the
                    // session has not started yet.
                    // In this case, we just show green.
                    if (s.SessionHasFeedback())
                    {
                        Debug.WriteLine("[ TrafficLightViewModel ] No feedback received but the session has feedback. Setting light to GREEN");
                        CurrentLight = LightColor.Green;
                    }
                    else
                    {
                        Enabled = false;
                        Debug.WriteLine("[ TrafficLightViewModel ] Disabled Traffic Light because this session does not have feedback");
                    }
                    return;
                }
                var processedValue = state.output.ToLower();
                switch (processedValue)
                {
                    case "distracted":
                        CurrentLight = LightColor.Red;
                        break;
                    case "normal":
                        CurrentLight = LightColor.Yellow;
                        break;
                    case "focused":
                        CurrentLight = LightColor.Green;
                        break;
                    default:
                        break;
                }
            }
            catch (IamSessionNotSetException)
            {
                Debug.WriteLine("[ TrafficLightViewModel ] IAM Session not set");
                IsReady = false;
                return;
            }
        }

        /// <summary>
        /// Get the current feedback from the backend service
        /// </summary>
        /// <returns>Feedback</returns>
        private Feedback GetCurrentFeedback()
        {
            #region Real Feedback
            var state = SessionExecutionService.GetOrCreate().GetCurrentFeedback();
            return state;
            #endregion

            #region Mocked Feedback
            //if (CurrentLight == LightColor.Red)
            //{
            //    return new Feedback()
            //    {
            //        output = "distracted"
            //    };
            //}
            //else if (CurrentLight == LightColor.Yellow)
            //{
            //    return new Feedback()
            //    {
            //        output = "normal"
            //    };
            //}
            //else
            //{
            //    return new Feedback()
            //    {
            //        output = "focused"
            //    };
            //}
            #endregion
        }

        private void NextLight()
        {
            currentColorIndex = (currentColorIndex - 1);
            if (currentColorIndex == 0)
            {
                CurrentLight = LightColor.Red;
                currentColorIndex = 3;
            }
            else if (currentColorIndex == 1)
            {
                CurrentLight = LightColor.Yellow;
            }
            else
            {
                CurrentLight = LightColor.Green;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}
