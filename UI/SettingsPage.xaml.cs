using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using Core.Models;
using UI.Services;

using Application = System.Windows.Application;

namespace UI
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page, INotifyPropertyChanged
    {
        private readonly ISessionExecutionService sessionExecutionService;

        public event PropertyChangedEventHandler? PropertyChanged;

        public List<string> EnvironmentOptionsDisplayNameList { get; set; } = new List<string>()
        {
            Env.Development,
            Env.Staging,
            Env.Production
        };

        private Settings _settings;
        public Settings Settings
        {
            get => _settings;
        }

        private bool _isEditing = false;

        public bool IsEditing
        {
            get
            {
                Debug.WriteLine("[ SettingsPage.IsEditing ] IsEditing get called, returning: " + _isEditing);
                return _isEditing;
            }
            set
            {
                _isEditing = value;
                Debug.WriteLine("[ SettingsPage.IsEditing ] IsEditing set to: " + value);
                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(IsNotEditing));
            }
        }

        public bool IsNotEditing => !IsEditing;


        public SettingsPage()
        {
            this.sessionExecutionService = ((App) Application.Current).SessionExecutionService;

            DataContext = this;

            InitializeComponent();

            _settings = Settings.Current;

            backendProtocolText.Text = _settings.ServerParams.BackendProtocol;
            backendHostText.Text = _settings.ServerParams.BackendHost;
            backendPortText.Text = _settings.ServerParams.BackendPort.ToString();
            backendPrefixText.Text = _settings.ServerParams.BackendPrefix;


            var index = EnvironmentOptionsDisplayNameList.FindIndex(env => env == _settings.Environment);
            EnvironmentComboBox.SelectedIndex = index; // Default to Production if saved value is invalid
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _settings.Environment = EnvironmentOptionsDisplayNameList[EnvironmentComboBox.SelectedIndex];
            sessionExecutionService.UpdateServerParamsFromSettings();
            Trace.WriteLine("[ SettingsPage ] User selected environment: " + _settings.Environment);
            backendProtocolText.Text = _settings.ServerParams.BackendProtocol;
            backendHostText.Text = _settings.ServerParams.BackendHost;
            backendPortText.Text = _settings.ServerParams.BackendPort.ToString();
            backendPrefixText.Text = _settings.ServerParams.BackendPrefix;
        }
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void EditOnClick(object sender, RoutedEventArgs e)
        {
            IsEditing = true;
        }

        private void SaveOnClick(object sender, RoutedEventArgs e)
        {
            IsEditing = false;
        }
    }
}
