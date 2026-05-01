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
        private readonly ISessionExecutionService sessionExecutionService;

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
            //this.sessionExecutionService = ((App) System.Windows.Application.Current).SessionExecutionService;
            this.sessionExecutionService = MockSessionExecutionService.GetOrCreate();

            colorTimer.Tick += LightTick;
            colorTimer.Start();

            enabledTimer.Tick += (s, e) =>
            {
                //Enabled = !Enabled;
            };
            enabledTimer.Start();

            Enabled = true;
        }

        private void LightTick(object sender, EventArgs args)
        {
            UpdateLightsFromBackend();
            //NextLight();
        }


        private void UpdateLightsFromBackend()
        {
            string m = "[ TrafficLightViewModel ] Updating traffic light from backend feedback";
            Debug.WriteLine(m);
            UpdateSessionHasFeedback();
            Feedback state;
            try
            {
                state = sessionExecutionService.GetCurrentFeedback();
                IsReady = true;
                if (state == null)
                {
                    string message = "[ TrafficLightViewModel.UpdateLightsFromBackend ] No feedback received but the session has feedback. Setting light to GREEN";
                    Debug.WriteLine(message);
                    Trace.WriteLine(message);
                    CurrentLight = LightColor.Green;
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

        private void UpdateSessionHasFeedback()
        {
            // It is possible to not have any feedbacks simply because the
            // session has not started yet.
            // In this case, we just show green.
            if (sessionExecutionService.SessionHasFeedback())
            {
                if (!Enabled)
                {
                    Enabled = true;
                    string message = "[ TrafficLightViewModel ] This session has feedback. Toggling Enabled from false to true";
                    Debug.WriteLine(message);
                    Trace.WriteLine(message);
                }
            }
            else
            {
                if (Enabled)
                {
                    Enabled = false;
                    string message = "[ TrafficLightViewModel ] This session has feedback. Toggling Enabled from true to false";
                    Trace.WriteLine(message);
                    Debug.WriteLine(message);
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}
