using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Application = System.Windows.Application;
using Point = System.Windows.Point;

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
        private static List<MainWindow> activeWindows = new List<MainWindow>();

        protected override void OnStartup(StartupEventArgs e)
        {
            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            base.OnStartup(e);

            // ------------------------------------------------------------------
            // Main App
            // Iterate over all connected 
            foreach (var screen in Screen.AllScreens)
            {
                // Create a new window on this screen
                var window = new MainWindow
                {
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    screen = screen
                };

                window.Move();
                window.SetGreen();

                // Show the window
                window.Show();
                activeWindows.Add(window); // Add the window to the active list
            }
            // ------------------------------------------------------------------
            // Http Server
            HttpServer.Start();
        }
    }

}
