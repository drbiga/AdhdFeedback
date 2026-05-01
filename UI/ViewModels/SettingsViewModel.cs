using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;


// Commands
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Core.Models;

using UI.Services;

namespace UI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly ISessionExecutionService sessionExecutionService;
        
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

        private bool _useRealSessionExecutionService = false;
        public bool UseRealSessionExecutionService
        {
            get => _useRealSessionExecutionService;
            set
            {
                if (_useRealSessionExecutionService == value)
                    return;
                _useRealSessionExecutionService = value;
                UseMockSessionExecutionService = !value;
                OnPropertyChanged(nameof(UseRealSessionExecutionService));
                Debug.WriteLine("[ SettingsViewModel.UseRealSessionExecutionService[set] ] Setting value=" + value);
                Trace.WriteLine("[ SettingsViewModel.UseRealSessionExecutionService[set] ] Setting value=" + value);
            }
        }
        private bool _useMockSessionExecutionService = true;
        public bool UseMockSessionExecutionService
        {
            get => _useMockSessionExecutionService;
            set
            {
                if (_useMockSessionExecutionService == value)
                    return;
                _useMockSessionExecutionService = value;
                UseRealSessionExecutionService = !value;
                OnPropertyChanged(nameof(UseMockSessionExecutionService));
                Debug.WriteLine("[ SettingsViewModel.UseMockSessionExecutionService[set] ] Setting value=" + value);
                Trace.WriteLine("[ SettingsViewModel.UseMockSessionExecutionService[set] ] Setting value=" + value);
            }
        }

        public SettingsViewModel()
        {
            this.sessionExecutionService = MockSessionExecutionService.GetOrCreate();

            _settings = Settings.Current;
        }
    }
}
