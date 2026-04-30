using System.Windows;
using System.Windows.Controls;
using ComboBox = System.Windows.Controls.ComboBox;

using Core.Repositories;
using Core.Models;

namespace UI;

/// <summary>
/// Interaction logic for SettingsView.xaml
/// </summary>
public partial class SettingsView : Window
{
    public List<string> EnvironmentOptionsDisplayNameList { get; set; }

    public SettingsView()
    {
        InitializeComponent();
    }
}
