using Caliburn.Micro;

namespace loadify.ViewModel
{
    public class MainViewModel : PropertyChangedBase
    {
        private LoadifySession _Session;

        private MenuViewModel _MenuViewModel;
        public MenuViewModel MenuViewModel
        {
            get { return _MenuViewModel; }
            set
            {
                if (_MenuViewModel == value) return;
                _MenuViewModel = value;
                NotifyOfPropertyChange(() => MenuViewModel);
            }
        }

        public MainViewModel(LoadifySession session):
            this()
        {
            _Session = session;
        }

        public MainViewModel()
        {
            _MenuViewModel = new MenuViewModel();
        }
    }
}
