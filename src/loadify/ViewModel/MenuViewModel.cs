using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using loadify.Event;

namespace loadify.ViewModel
{
    public class MenuViewModel : ViewModelBase
    {
        public MenuViewModel(IEventAggregator eventAggregator):
            base(eventAggregator)
        { }

        public void RefreshData()
        {
            _EventAggregator.PublishOnUIThread(new DataRefreshRequestEvent());
        }
    }
}
