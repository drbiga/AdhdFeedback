using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using Core.Models;
using UI.Services;
using UI.ViewModels;
using Application = System.Windows.Application;

namespace UI
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage(SettingsViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
