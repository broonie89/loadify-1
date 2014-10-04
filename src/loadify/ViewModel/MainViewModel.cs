using Caliburn.Micro;

namespace loadify.ViewModel
{
    public class MainViewModel : PropertyChangedBase
    {
        private LoadifySession _Session;

        public MainViewModel(LoadifySession session)
        {
            _Session = session;
        }
    }
}
