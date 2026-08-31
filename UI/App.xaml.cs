using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

using Core.Models;
using UI.Services;
using UI.Views;
using UI.ViewModels;

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

        private SettingsView? _settingsView;

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
            Trace.WriteLine("----------------------------------------------------");
            Trace.WriteLine("Application started at " + DateTime.Now);

            var settings = Settings.Current;
            Trace.WriteLine("Loaded settings: Environment = " + settings.Environment);

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

            IServiceCollection services = new ServiceCollection();
            if (settings.UseRealSessionExecutionService)
            {
                Trace.WriteLine("[ App.xaml.cs ] Using REAL SessionExecutionService");
                services.AddSingleton<ISessionExecutionService, SessionExecutionService>();
            }
            else
            {
                Trace.WriteLine("[ App.xaml.cs ] Using MOCK SessionExecutionService");
                services.AddSingleton<ISessionExecutionService, MockSessionExecutionService>();
            }
            services
                .AddSingleton<UI.ViewModels.ISettingsNavigationService, UI.Views.SettingsNavigationService>()
                .AddSingleton<SettingsLoginViewModel>()
                .AddSingleton<SettingsLoginView>()

                .AddSingleton<SettingsMainViewModel>()
                .AddSingleton<SettingsMainView>()

                .AddSingleton<SettingsViewModel>()
                .AddSingleton<SettingsView>()

                .AddSingleton<TrafficLightViewModel>(s => new TrafficLightViewModel(s.GetRequiredService<ISessionExecutionService>()))
                .AddSingleton<TrafficLightWindow>(s => new TrafficLightWindow()
                {
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    screen = Screen.AllScreens.FirstOrDefault()
                });

            Ioc.Default.ConfigureServices(services.BuildServiceProvider());

            var mainWindow = Ioc.Default.GetRequiredService<TrafficLightWindow>();
            mainWindow.setWindowId(0);
            mainWindow.Move();
            activeWindows.Add(mainWindow);
            mainWindow.Show();
            // ------------------------------------------------------------------
            // Http Server
            HttpServer.Start();
        }


        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();

            menu.Items.Add("Show Traffic Light", null, (_, _) => ShowMainWindow());
            menu.Items.Add("Settings", null, (_, _) => ShowSettings());
            menu.Items.Add("Exit", null, (_, _) => ExitApp());

            return menu;
        }

        private void ShowMainWindow()
        {
            foreach (var window in activeWindows)
            {
                window.Show();
                window.WindowState = WindowState.Normal;
                window.Activate();
            }
        }

        private void ShowSettings()
        {
            if (_settingsView == null)
            {
                _settingsView = Ioc.Default.GetRequiredService<SettingsView>();
                _settingsView.Closed += (s, e) => { _settingsView = null; };
                _settingsView.Show();
                Ioc.Default.GetRequiredService<ISettingsNavigationService>().NavigateTo<SettingsLoginViewModel>();
            }
            else
            {
                _settingsView.Activate();
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
