using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using CommunityToolkit.Mvvm.DependencyInjection;

using UI.Services;
using UI.Views;

namespace UI.ViewModels
{
    public partial class SettingsLoginViewModel : ObservableObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool _isAuthenticated = false;
        public bool IsAuthenticated
        {
            get
            {
                Debug.WriteLine("[ SettingsPage.IsAuthenticated[get] ] Authentication status: " + _isAuthenticated);
                return _isAuthenticated;
            }
            set
            {
                _isAuthenticated = value;
                string message = "[ SettingsPage.IsAuthenticated ] IsAuthenticated set to: " + value;
                Debug.WriteLine(message);
                Trace.WriteLine(message);
                OnPropertyChanged(nameof(IsAuthenticated));
            }
        }

        public string Password { get; set; }

        [RelayCommand]
        public void Authenticate(object parameter)
        {
            var passBox = parameter as System.Windows.Controls.PasswordBox;
            if (passBox == null)
            {
                return;
            }
            Password = passBox.Password;
            IsAuthenticated = Password == "123";
            string message = "[ SettingsViewModel.Authenticate ] Authenticate method called. Result is " + IsAuthenticated;
            Debug.WriteLine(message);
            Trace.WriteLine(message);
            if (IsAuthenticated)
            {
                _navigationService.NavigateTo(Ioc.Default.GetRequiredService<SettingsPage>());
            }
        }

        public Visibility FormVisibility => IsAuthenticated ? Visibility.Collapsed : Visibility.Visible;
        public Visibility SetingsVisibility => IsAuthenticated ? Visibility.Visible : Visibility.Collapsed;

        private INavigationService _navigationService;
        public SettingsLoginViewModel(INavigationService nav) => _navigationService = nav;
    }
}
