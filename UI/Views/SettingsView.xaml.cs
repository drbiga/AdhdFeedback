using Core.Models;
using Core.Repositories;
using System.Windows;
using System.Windows.Controls;
using ComboBox = System.Windows.Controls.ComboBox;


using CommunityToolkit.Mvvm.DependencyInjection;
using UI.Services;
using UI.ViewModels;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        public SettingsView(SettingsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
