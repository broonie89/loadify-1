using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.ViewModel
{
    public class StatusViewModel : ViewModelBase
    {
        private UserViewModel _UserViewModel;
        public UserViewModel UserViewModel
        {
            get { return _UserViewModel; }
            set
            {
                if (_UserViewModel == value) return;
                _UserViewModel = value;
                NotifyOfPropertyChange(() => UserViewModel);
            }
        }

        public StatusViewModel(UserViewModel userViewModel)
        {
            _UserViewModel = userViewModel;
        }

        public StatusViewModel():
            this(new UserViewModel())
        { }
    }
}
