using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

namespace UI.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private ViewModelBase _current;
        public ViewModelBase CurrentViewModel
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }
    }
}
