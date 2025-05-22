using System;
using System.Windows;

using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

using AdhdFeedback.util;

namespace AdhdFeedback
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Timer to track time once mouse enters the window
        private DispatcherTimer timer;
        private bool isMouseInside = false;
        private double timeRemaining = 3.0; // 3 seconds timer
        public IWindowPositionState positionState;
        public Screen screen;

        public MainWindow()
        {
            InitializeComponent();
            this.MouseMove += MainWindow_MouseMove;
            this.MouseLeave += MainWindow_MouseLeave;
            this.MouseDown += MainWindow_MouseClick;


            // Initialize the timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // Timer will tick every second
            timer.Tick += Timer_Tick;

            positionState = new TopCenterPositionState(this);
        }

        private void MainWindow_MouseClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (isMouseInside)
            {
                isMouseInside = false;
                timer.Stop();
            }
            this.positionState.Next();
            this.positionState.Move();
        }

        // Event handler for mouse movement inside the window
        private void MainWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Start the timer if the mouse is inside and not already started
            if (!isMouseInside)
            {
                isMouseInside = true;
                timeRemaining = 3.0;  // Reset the timer to 3 seconds
                timer.Start(); // Start the timer
            }
        }

        // Event handler for mouse leaving the window
        private void MainWindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Stop the timer if the mouse leaves
            if (isMouseInside)
            {
                isMouseInside = false;
                timer.Stop();
            }
        }

        // Timer Tick event handler
        private void Timer_Tick(object sender, EventArgs e)
        {
            timeRemaining -= 1;  // Decrease the time remaining by 1 second

            if (timeRemaining <= 0)
            {
                timer.Stop(); // Stop the timer once it hits 0
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
            YellowLight.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#333"));
            GreenLight.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#3c3"));
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
    }

    public enum TrafficLightColor
    {
        Red = 0,
        Yellow,
        Green
    }

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
            this.window.Left = PixelUnitConverter.PixelsToDipsX(this.window.screen.Bounds.Width / 2, this.window) - this.window.Width;
            this.window.Top = 0;
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
            // If on left-center, then move to top center
            // and make it horizontal
            this.window.Left = 0;
            this.window.Top = PixelUnitConverter.PixelsToDipsY(this.window.screen.Bounds.Height / 2, this.window) - this.window.Height;
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
