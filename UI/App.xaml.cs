using System.Configuration;
using System.Data;
using System.Windows;
using Application = System.Windows.Application;

namespace UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // List to track active windows
        private static List<MainWindow> activeWindows = new List<MainWindow>();

        protected override void OnStartup(StartupEventArgs e)
        {
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
