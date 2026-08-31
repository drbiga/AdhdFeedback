using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UI.ViewModels;

namespace UI.Views
{
    internal class SettingsNavigationService : ISettingsNavigationService
    {
        private SettingsViewModel vm;
        public SettingsNavigationService(SettingsViewModel vm) : base() { this.vm = vm; }
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            vm.CurrentViewModel = Ioc.Default.GetRequiredService<TViewModel>();
        }
    }
}
