using Caliburn.Micro;

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
