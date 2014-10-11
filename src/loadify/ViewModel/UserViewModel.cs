using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Model;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class UserViewModel : ViewModelBase
    {
        private UserModel _UserModel;

        public string Name
        {
            get { return _UserModel.Name; }
            set
            {
                if (_UserModel.Name == value) return;
                _UserModel.Name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public UserViewModel():
            this(new UserModel())
        { }

        public UserViewModel(UserModel userModel)
        {
            _UserModel = userModel;
        }
    }
}
