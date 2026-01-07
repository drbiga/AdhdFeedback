using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using UI.Util;
using UI.Services;
using UI.ViewModels;

using LightColor = UI.ViewModels.TrafficLightViewModel.LightColor;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class TrafficLightWindow : Window
    {
        /// <summary>
        ///  Window ID, used to identify which screen this window belongs to.
        ///  This is used only when using multiple monitors.
        /// </summary>
        private int wid;

        /// <summary>
        ///  Period in seconds between beeps when the light is red
        /// </summary>
        static double BEEP_PERIOD = 60;
        static double BEEP_TICK_INTERVAL = 30;
        private DispatcherTimer beepTimer;
        private MediaPlayer beepPlayer;
        private double timeSinceLastFeedbackBeep;
        private bool beepingEnabled = true;

        /// <summary>
        /// State variable to control pulsing animations that depend
        /// on previous states of the traffig light.
        /// </summary>
        private LightColor previousColor = TrafficLightViewModel.LightColor.None;


        /// <summary>
        /// Position state management for the window
        /// </summary>
        public IWindowPositionState positionState;
        public Screen screen;

        public TrafficLightWindow()
        {
            InitializeComponent();
            
            TrafficLightViewModel vm = new TrafficLightViewModel();
            vm.PropertyChanged += Vm_PropertyChanged;

            DataContext = vm;

            positionState = new TopCenterPositionState(this);

            beepPlayer = new MediaPlayer();
            beepPlayer.Open(new Uri(@"Media\Beep.mp3", UriKind.Relative));
            beepTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(BEEP_TICK_INTERVAL) };
            beepTimer.Tick += BeepTick;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;      // Prevent app from closing
            this.Hide();          // Hide window to tray
        }

        public void setWindowId(int wid)
        {
            this.wid = wid;
        }

        private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = (TrafficLightViewModel) sender;
            if (e.PropertyName == nameof(TrafficLightViewModel.CurrentLight))
            {
                LightPropertyChanged(vm);
            }
            if (e.PropertyName == nameof(TrafficLightViewModel.Enabled))
            {
                EnabledPropertyChanged(vm);
            }
            if (e.PropertyName == nameof(TrafficLightViewModel.IsReady))
            {
                // Handle other property changes if needed
                IsReadyPropertyChanged(vm);
            }
        }

        #region Light change handling

        void LightPropertyChanged(TrafficLightViewModel vm)
        {
            LightColor currentColor = vm.CurrentLight;

            bool gotWorse = HasGottenWorse(previousColor, currentColor);
            bool isRed = vm.CurrentLight == LightColor.Red;

            if (gotWorse)
            {
                if (isRed && beepingEnabled)
                    PlayBeep();
                PulseAnimation();
            }
            if (isRed)
            {
                // Start beeping
                timeSinceLastFeedbackBeep = BEEP_PERIOD; // So that it beeps immediately
                StartBeeping();
                Debug.WriteLine("Started beeping");
            }
            else
            {
                // Stop beeping
                StopBeeping();
                Debug.WriteLine("Stopped beeping");
            }

            previousColor = currentColor;
        }

        bool HasGottenWorse(LightColor previous, LightColor current)
        {
            if (previous == LightColor.None)
                return false; // No previous state to compare
            if (previous == LightColor.Green && current == LightColor.Yellow)
                return true;
            if (previous == LightColor.Green && current == LightColor.Red)
                return true;
            if (previous == LightColor.Yellow && current == LightColor.Red)
                return true;
            return false;
        }

        #endregion

        void EnabledPropertyChanged(TrafficLightViewModel vm)
        {
            if (vm.Enabled)
            {
                this.beepingEnabled = true;
                this.Show();
            }
            else
            {
                this.beepingEnabled = false;
                this.Hide();
            }
        }

        void IsReadyPropertyChanged(TrafficLightViewModel vm)
        {
            if (vm.IsReady)
            {

            }
            else
            {
                this.Opacity = 0.5;
            }
        }

        #region Beeping

        void StartBeeping()
        {
            beepTimer.Start();
        }

        void StopBeeping()
        {
            beepTimer.Stop();
        }

        public void BeepTick(object sender, EventArgs e)
        {
            if (!beepingEnabled)
                return;
            if (timeSinceLastFeedbackBeep >= BEEP_PERIOD)
            {
                timeSinceLastFeedbackBeep = 0;
                PlayBeep();
            }
            timeSinceLastFeedbackBeep += BEEP_TICK_INTERVAL;
        }
        public void PlayBeep()
        {
            if (this.wid != 0)
                return; // Only play beep on main window
            beepPlayer.Play();
            beepPlayer.Position = TimeSpan.Zero;
        }
        #endregion

        #region Animations
        private void PulseAnimation()
        {
            var transform = this.RenderTransform as ScaleTransform;
            if (transform == null)
                transform = new ScaleTransform(1.0, 1.0);

            this.SizeToContent = SizeToContent.WidthAndHeight;
            RootComponent.LayoutTransform = transform;

            var scaleUp = new DoubleAnimation
            {
                To = 1.5,
                Duration = TimeSpan.FromMilliseconds(250),
                AutoReverse = true,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);
        }
        #endregion

        #region Position management methods
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Move();
        }

        // Method to move the window
        public void Move()
        {
            this.positionState.Next();
        }

        public void SetTrafficLightHorizontal()
        {
            TrafficLight.Orientation = System.Windows.Controls.Orientation.Horizontal;
            UpdateLayout();
        }

        public void SetTrafficLightVertical()
        {
            TrafficLight.Orientation = System.Windows.Controls.Orientation.Vertical;
            UpdateLayout();
        }
        #endregion
    }

    #region Position management classes

    public interface IWindowPositionState
    {
        void Move();
        void Next();
    }

    class TopCenterPositionState : IWindowPositionState
    {
        private TrafficLightWindow window;
        public TopCenterPositionState(TrafficLightWindow window)
        {
            this.window = window;
        }

        public void Move()
        {
            double screenLeftDips = PixelUnitConverter.PixelsToDipsX(this.window.screen.Bounds.Left, this.window);
            double screenWidthDips = PixelUnitConverter.PixelsToDipsX(this.window.screen.Bounds.Width, window);
            this.window.Left = screenLeftDips + (screenWidthDips - this.window.Width) / 2;
            double screenTopDips = PixelUnitConverter.PixelsToDipsY(this.window.screen.Bounds.Top, window);
            this.window.Top = screenTopDips;
        }

        public void Next()
        {
            // If on top-center, then move to left center
            // and make it vertical
            double auxHeight = this.window.Height;
            this.window.Height = this.window.Width;
            this.window.Width = auxHeight;
            this.window.SetTrafficLightVertical();
            this.window.positionState = new LeftCenterPositionState(this.window);
            this.window.positionState.Move();
        }
    }

    class LeftCenterPositionState : IWindowPositionState
    {
        private TrafficLightWindow window;
        public LeftCenterPositionState(TrafficLightWindow window)
        {
            this.window = window;
        }

        public void Move()
        {
            var screen = this.window.screen;

            // Convert screen Y position and height to DIPs
            double screenTopDips = PixelUnitConverter.PixelsToDipsY(screen.Bounds.Top, window);
            double screenHeightDips = PixelUnitConverter.PixelsToDipsY(screen.Bounds.Height, window);

            // Align to left of screen
            double screenLeftDips = PixelUnitConverter.PixelsToDipsX(screen.Bounds.Left, window);
            this.window.Left = screenLeftDips;

            // Center vertically
            this.window.Top = screenTopDips + (screenHeightDips - this.window.Height) / 2;
        }

        public void Next()
        {
            double auxHeight = this.window.Height;
            this.window.Height = this.window.Width;
            this.window.Width = auxHeight;
            this.window.SetTrafficLightHorizontal();
            this.window.positionState = new TopCenterPositionState(this.window);
            this.window.positionState.Move();
        }
    }
    #endregion
}