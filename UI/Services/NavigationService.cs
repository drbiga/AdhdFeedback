using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UI.Services
{
    public interface INavigationService
    {
        void NavigateTo(Uri pageUri);
        void NavigateTo(Page page);
    }
    public class NavigationService : INavigationService
    {
        private Frame? _frame;
        public void Initialize(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateTo(Uri pageUri)
        {
            if (_frame == null)
                throw new InvalidOperationException("NavigationService has not been initialized with a Frame.");

            _frame.Navigate(pageUri);
        }

        public void NavigateTo(Page page)
        {
            if (_frame == null)
                throw new InvalidOperationException("NavigationService has not been initialized with a Frame.");
            _frame.Navigate(page);
        }
    }
}
