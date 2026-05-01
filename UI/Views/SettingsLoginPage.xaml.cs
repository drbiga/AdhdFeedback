using System.Windows.Controls;

using UI.ViewModels;

namespace UI.Views
{
    /// <summary>
    /// Interaction logic for SettingsLoginPage.xaml
    /// </summary>
    public partial class SettingsLoginPage : Page
    {

        public SettingsLoginPage(SettingsLoginViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
