using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using loadify.Model;
using loadify.Views;

namespace loadify.ViewModels
{
    public class LoginViewModel : ViewAware
    {
        private IWindowManager _WindowManager;

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

        private bool _LoginProcessActive = false;
        public bool LoginProcessActive
        {
            get { return _LoginProcessActive; }
            set
            {
                if (_LoginProcessActive == value) return;
                _LoginProcessActive = value;
                NotifyOfPropertyChange(() => LoginProcessActive);
            }
        }

        public LoginViewModel(IWindowManager windowManager)
        {
            _WindowManager = windowManager;
        }

        public LoginViewModel()
        { }

        public void Login()
        {
            // since you can't bind the passwordbox to a property, the viewmodel needs to be aware of the view to access the password entered
            var password = (GetView() as LoginView).Password.Password;
            LoginProcessActive = true;
        }
    }
}
