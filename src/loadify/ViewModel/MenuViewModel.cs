using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using loadify.Event;
using loadify.View;

namespace loadify.ViewModel
{
    public class MenuViewModel : ViewModelBase
    {
        public MenuViewModel(IEventAggregator eventAggregator, IWindowManager windowManager):
            base(eventAggregator, windowManager)
        { }

        public void OpenAbout()
        {
            _WindowManager.ShowWindow(new AboutViewModel());
        }
    }
}
