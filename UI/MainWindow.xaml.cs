using System.Diagnostics;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using UI.Util;
using UI.Services;
using Point = System.Windows.Point;
using System.Windows.Forms;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window
    {
        static double POSITION_TICK_TIME = 0.1; // timer ticks every 100 miliseconds
        static double POSITION_TOTAL_TIME = 1;
        static double LIGHT_TICK_TIME = 1;
        static double FEEDBACK_BEEP_PERIOD = 30;


        // Timer to track time once mouse enters the window
        private DispatcherTimer positionTimer;
        private double remainingTime;

        private DispatcherTimer lightTimer;
        private MediaPlayer beepPlayer;
        private double timeSinceLastFeedbackBeep;


        public IWindowPositionState positionState;
        public Screen screen;

        private Feedback? previousFeedback = null;
        private Feedback? currentFeedback = null;

        public MainWindow()
        {
            InitializeComponent();
            this.MouseEnter += MainWindow_MouseEnter;
            this.MouseLeave += MainWindow_MouseLeave;
            this.MouseDown += MainWindow_MouseClick;

            // -----------------------------------------------------------------
            // Timer for the position evaluation
            positionTimer = new DispatcherTimer();
            positionTimer.Interval = TimeSpan.FromSeconds(POSITION_TICK_TIME);
            positionTimer.Tick += PositionTimer_Tick;
            positionState = new TopCenterPositionState(this);

            // -----------------------------------------------------------------
            // Timer to update current light based on feedback
            lightTimer = new DispatcherTimer();
            lightTimer.Interval = TimeSpan.FromSeconds(LIGHT_TICK_TIME);
            lightTimer.Tick += LightTimer_Tick;
            lightTimer.Start();
            beepPlayer = new MediaPlayer();
            beepPlayer.Open(new Uri(@"Media\Beep.mp3", UriKind.Relative));
            timeSinceLastFeedbackBeep = 0;

            // We need to do this so that the instance gets created and it tries
            // to connect to the local server.
            // It also needs to start checking for IAM sessions and feedback as
            // soon as the light timer ticks, so we need the service initialized
            // prior to that.
            SessionExecutionService.GetOrCreate();
        }

        // -----------------------------------------------------------------
        // Position management methods
        private void MainWindow_MouseClick(object sender, System.Windows.RoutedEventArgs e)
        {
            positionTimer.Stop();
            this.positionState.Next();
            this.positionState.Move();
        }

        // Event handler for mouse movement inside the window
        private void MainWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Start the timer if the mouse is inside and not already started
            remainingTime = MainWindow.POSITION_TOTAL_TIME;
            positionTimer.Start(); // Start the timer

            AnimateWindowStyle(this, 0.75, 0.4, POSITION_TOTAL_TIME); // Start animation
        }

        // Event handler for mouse leaving the window
        private void MainWindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            positionTimer.Stop();
            AnimateWindowStyle(this, 1.0, 1.0, 0.2);
        }

        public void AnimateWindowStyle(Window window, double targetScale, double targetOpacity, double durationSeconds = 3.0)
        {
            if (window == null) return;

            // Ensure the window has a ScaleTransform
            if (window.RenderTransform is not ScaleTransform scaleTransform)
            {
                scaleTransform = new ScaleTransform(1.0, 1.0);
                window.RenderTransform = scaleTransform;
                window.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5); // Scale from center
            }

            var scaleXAnim = new DoubleAnimation
            {
                To = targetScale,
                Duration = TimeSpan.FromSeconds(durationSeconds),
            };

            var scaleYAnim = scaleXAnim.Clone();

            var opacityAnim = new DoubleAnimation
            {
                To = targetOpacity,
                Duration = TimeSpan.FromSeconds(durationSeconds),
            };

            // Begin animations
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);
            window.BeginAnimation(Window.OpacityProperty, opacityAnim);
        }


        // Timer Tick event handler
        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            // We just need to trigger the move once every time the user hovers the pointer
            // over the traffic light, so we stop the timer and wait to start it again
            // once the user is hovering over the traffic light again.
            remainingTime -= MainWindow.POSITION_TICK_TIME;

            if (remainingTime <= 0)
            {
                positionTimer.Stop();
                this.positionState.Next();
                this.positionState.Move();
            }
        }

        // Method to move the window
        public void Move()
        {
            this.positionState.Move();
        }

        public void SetTrafficLightHorizontal()
        {
            EllipseStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            UpdateLayout();
        }

        public void SetTrafficLightVertical()
        {
            EllipseStackPanel.Orientation = System.Windows.Controls.Orientation.Vertical;
            UpdateLayout();
        }

        // -----------------------------------------------------------------
        // Lighting management methods
        public void LightTimer_Tick(object sender, EventArgs e)
        {
            // We will update the feedback every time here and control the frequency with the
            // tick interval variable defined at the start of the class
            try
            {
                previousFeedback = currentFeedback;
                currentFeedback = SessionExecutionService.GetOrCreate().GetCurrentFeedback();
            }
            catch (StudentSessionNotStartedException)
            {
                SetGray();
                return;
            }
            timeSinceLastFeedbackBeep += LIGHT_TICK_TIME;
            if (currentFeedback == null)
            {
                Debug.WriteLine("Feedback is currently null");
                return;
            }
            Debug.WriteLine(currentFeedback.ToString());
            bool gotWorse = false;
            if (previousFeedback != null)
            {
                gotWorse = (
                    (previousFeedback.output == FeedbackType.FOCUSED && currentFeedback.output == FeedbackType.NORMAL) ||
                    (previousFeedback.output == FeedbackType.NORMAL && currentFeedback.output == FeedbackType.DISTRACTED) ||
                    (previousFeedback.output == FeedbackType.FOCUSED && currentFeedback.output == FeedbackType.DISTRACTED)
                );
            }
                
            if (gotWorse)
            {
                PulseAnimation();
            }
            Debug.WriteLine(currentFeedback.ToString());
            switch (currentFeedback.output)
            {
                case FeedbackType.DISTRACTED:
                    SetRed();
                    if (timeSinceLastFeedbackBeep > FEEDBACK_BEEP_PERIOD)
                    {
                        timeSinceLastFeedbackBeep = 0;
                        PlayBeep();
                    }
                    break;
                case FeedbackType.NORMAL:
                    SetYellow();
                    break;
                case FeedbackType.FOCUSED:
                    SetGreen();
                    break;
                default:
                    Debug.WriteLine("Invalid Feedback");
                    throw new Exception("Invalid feedback type returned from backend");
            }
        }

        public void PlayBeep()
        {
            beepPlayer.Play();
            beepPlayer.Position = TimeSpan.Zero;
        }

        public void SetRed()
        {
            RedLight.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#f33"));
            YellowLight.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#333"));
            GreenLight.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#333"));
        }

        public void SetYellow()
        {
            RedLight.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#333"));
            YellowLight.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ee3"));
            GreenLight.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#333"));
        }

        public void SetGreen()
        {
            RedLight.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#333"));
            YellowLight.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#333");
            GreenLight.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#3c3"));
        }

        public void SetGray()
        {
            RedLight.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#333"));
            YellowLight.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#333");
            GreenLight.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#333"));
        }

        public void SetColor(TrafficLightColor color)
        {
            if (color == TrafficLightColor.Red)
                SetRed();
            else if (color == TrafficLightColor.Yellow)
                SetYellow();
            else
                SetGreen();
        }

        private void PulseAnimation()
        {
            var transform = this.RenderTransform as ScaleTransform;
            if (transform == null)
            {
                transform = new ScaleTransform(1.0, 1.0);
                this.RenderTransform = transform;
                this.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            var scaleUp = new DoubleAnimation
            {
                To = 1.1,
                Duration = TimeSpan.FromMilliseconds(150),
                AutoReverse = true,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);
        }
        //private void PulseWindow()
        //{
        //    if (this.RenderTransform is not ScaleTransform scaleTransform)
        //    {
        //        scaleTransform = new ScaleTransform(1.0, 1.0);
        //        this.RenderTransform = scaleTransform;
        //        this.RenderTransformOrigin = new Point(0.5, 0.5);
        //    }

        //    var pulseAnim = new DoubleAnimation
        //    {
        //        From = 1.0,
        //        To = 1.1,
        //        Duration = TimeSpan.FromMilliseconds(150),
        //        AutoReverse = true,
        //        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        //    };

        //    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, pulseAnim);
        //    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, pulseAnim);
        //}


    }

    public enum TrafficLightColor
    {
        Red = 0,
        Yellow,
        Green
    }

    // ------------------------------------------------------------------------
    // Position state management

    public interface IWindowPositionState
    {
        void Move();
        void Next();
    }

    class TopCenterPositionState : IWindowPositionState
    {
        private MainWindow window;
        public TopCenterPositionState(MainWindow window)
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
        private MainWindow window;
        public LeftCenterPositionState(MainWindow window)
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
}