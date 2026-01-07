using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);
        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

        // List to track active windows
        private static List<TrafficLightWindow> activeWindows = new List<TrafficLightWindow>();

        private TestWindow? _testWindow;

        private NotifyIcon? _trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            string logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ADHD Feedback", "Logs");

            Directory.CreateDirectory(logDir);

            string logPath = Path.Combine(logDir, "app.log");

            Trace.Listeners.Add(new TextWriterTraceListener(logPath));
            Trace.AutoFlush = true;

            Trace.WriteLine("Application started at " + DateTime.Now);

            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            base.OnStartup(e);

            // ------------------------------------------------------------------------

            // 1. Load the PNG from your project resources
            // Ensure your PNG "Build Action" is set to "Resource" or "Content"
            var iconUri = new Uri("pack://application:,,,/favicon.png");
            var streamInfo = Application.GetResourceStream(iconUri);

            using (var stream = streamInfo.Stream)
            {
                // 2. Convert PNG stream to a Bitmap, then to an Icon handle
                using (var bitmap = new Bitmap(stream))
                {
                    _trayIcon = new NotifyIcon();
                    // Get the Hicon (handle) and create the Icon object
                    _trayIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
                    _trayIcon.Visible = true;
                    _trayIcon.Text = "ADHD Feedback";
                    _trayIcon.ContextMenuStrip = BuildContextMenu();
                }
            }

            _trayIcon.DoubleClick += (_, _) => ShowMainWindow();


            // ------------------------------------------------------------------
            // Main App
            // Iterate over all connected
            int wid = 0;
            foreach (var screen in Screen.AllScreens)
            {
                // Create a new window on this screen
                var window = new TrafficLightWindow
                {
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    screen = screen
                };
                window.setWindowId(wid);
                wid += 1;
                window.Move();
                //window.SetGreen();

                // Show the window
                window.Show();
                activeWindows.Add(window); // Add the window to the active list
            }
            // ------------------------------------------------------------------
            // Http Server
            HttpServer.Start();
        }


        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();

            menu.Items.Add("Show Traffic Light", null, (_, _) => ShowMainWindow());
            menu.Items.Add("Verify Data Collection", null, (_, _) => ShowTestWindow());
            menu.Items.Add("Exit", null, (_, _) => ExitApp());

            return menu;
        }

        private void ShowMainWindow()
        {
            //if (Current.MainWindow == null)
            //    return;

            //Current.MainWindow.Show();
            //Current.MainWindow.WindowState = WindowState.Normal;
            //Current.MainWindow.Activate();
            foreach (var window in activeWindows)
            {
                window.Show();
                window.WindowState = WindowState.Normal;
                window.Activate();
            }
        }

        private void ShowTestWindow()
        {
            if (_testWindow == null)
            {
                _testWindow = new TestWindow();
                _testWindow.Closed += (s, e) => { _testWindow = null; };
                _testWindow.Show();
            }
            else
            {
                _testWindow.Activate();
            }
        }

        private void ExitApp()
        {
            _trayIcon!.Visible = false;
            _trayIcon.Dispose();
            Shutdown();
        }
    }

}
