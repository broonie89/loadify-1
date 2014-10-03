using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using loadify.Model;

namespace loadify.ViewModels
{
    public class LoginViewModel : PropertyChangedBase
    {
        private LoginModel _LoginModel = new LoginModel();
        public string Username
        {
            get { return _LoginModel.Username; }
            set
            {
                if (_LoginModel.Username == value) return;
                _LoginModel.Username = value;
                NotifyOfPropertyChange(() => _LoginModel.Username);
            }
        }

        public LoginViewModel()
        { }
    }
}
