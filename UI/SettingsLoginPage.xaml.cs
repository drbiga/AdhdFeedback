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

using MessageBox = System.Windows.MessageBox;

namespace UI
{
    /// <summary>
    /// Interaction logic for SettingsLoginPage.xaml
    /// </summary>
    public partial class SettingsLoginPage : Page
    {
        public SettingsLoginPage()
        {
            InitializeComponent();
        }

        private void Unlock_Click(object sender, RoutedEventArgs e)
        {
            // The "Simple/Hardcoded" way
            if (PassBox.Password == "1234")
            {
                // "this.NavigationService" is the magic tool that swaps the page
                this.NavigationService.Navigate(new SettingsPage());
            }
            else
            {
                MessageBox.Show("Wrong password!");
            }
        }
    }
}
