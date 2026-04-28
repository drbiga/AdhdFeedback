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

    private Settings _settings;

    public SettingsView()
    {
        InitializeComponent();
        EnvironmentOptionsDisplayNameList = new List<string>()
        {
            Env.Production,
            Env.Development
        };

        DataContext = this;

        _settings = Settings.Current;
        var index = EnvironmentOptionsDisplayNameList.FindIndex(env => env == _settings.Environment);
        //EnvironmentComboBox.SelectedIndex = index; // Default to Production if saved value is invalid
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //_settings.Environment = EnvironmentOptionsDisplayNameList[EnvironmentComboBox.SelectedIndex];
#if DEBUG
        //System.Windows.MessageBox.Show("Selected environment: " + env);
#endif
    }
}
