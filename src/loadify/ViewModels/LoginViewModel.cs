using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using loadify.Events;
using loadify.Model;
using loadify.Views;
using SpotifySharp;

namespace loadify.ViewModels
{
    public class LoginViewModel : ViewModelBase, IHandle<LoginFailedEvent>, IHandle<LoginSuccessfulEvent>
    {
        private LoginModel _LoginModel;
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

        public LoginViewModel(IEventAggregator eventAggregator, IWindowManager windowManager) :
            base(eventAggregator, windowManager)
        {
            _LoginModel = new LoginModel(new LoadifySession(_EventAggregator));
        }

        public void Login()
        {
            LoginProcessActive = true;
            var loginView = GetView() as LoginView;

            // since you can't bind the passwordbox to a property, the viewmodel needs to be aware of the view to access the password entered
            var password = loginView.Password.Password;
            _LoginModel.Session.Login(Username, password);          
        }

        public void Handle(LoginFailedEvent message)
        {
            LoginProcessActive = false;
        }

        public void Handle(LoginSuccessfulEvent message)
        {
            var loginView = GetView() as LoginView;
            _WindowManager.ShowWindow(new ShellViewModel());
            loginView.Close();
        }
    }
}
