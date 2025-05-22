using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace AdhdFeedback
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        // List to track active windows
        private static List<MainWindow> activeWindows = new List<MainWindow>();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            return;

            // Iterate over all connected screens
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
        }
    }
}
    
