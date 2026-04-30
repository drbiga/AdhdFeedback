using Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UI.Services;

namespace UI.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
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
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
