using Caliburn.Micro;

namespace loadify.ViewModel
{
    public class MainViewModel : PropertyChangedBase
    {
        private LoadifySession _Session;

        private MenuViewModel _Menu;
        public MenuViewModel Menu
        {
            get { return _Menu; }
            set
            {
                if (_Menu == value) return;
                _Menu = value;
                NotifyOfPropertyChange(() => Menu);
            }
        }

        private StatusViewModel _Status;
        public StatusViewModel Status
        {
            get { return _Status; }
            set
            {
                if (_Status == value) return;
                _Status = value;
                NotifyOfPropertyChange(() => Status);
            }
        }

        public MainViewModel(LoadifySession session):
            this()
        {
            _Session = session;
        }

        public MainViewModel()
        {
            _Menu = new MenuViewModel();
            _Status = new StatusViewModel();
        }
    }
}
