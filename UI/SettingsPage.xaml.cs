using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UI
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public List<string> EnvironmentOptionsDisplayNameList { get; set; }

        private Settings _settings;

        public SettingsPage()
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
            EnvironmentComboBox.SelectedIndex = index; // Default to Production if saved value is invalid
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _settings.Environment = EnvironmentOptionsDisplayNameList[EnvironmentComboBox.SelectedIndex];
#if DEBUG
            //System.Windows.MessageBox.Show("Selected environment: " + env);
#endif
        }
    }
}
