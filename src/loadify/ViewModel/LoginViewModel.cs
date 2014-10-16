using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using loadify.Event;
using loadify.Model;
using loadify.Properties;
using loadify.Spotify;
using loadify.View;
using MahApps.Metro.Controls.Dialogs;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class LoginViewModel : ViewModelBase, IHandle<LoginFailedEvent>,
                                                 IHandle<LoginSuccessfulEvent>
    {
        private LoadifySession _Session;

        private UserViewModel _User;
        public UserViewModel User
        {
            get { return _User; }
            set
            {
                if (_User == value) return;
                _User = value;
                NotifyOfPropertyChange(() => User);
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

        private bool _RememberMe = false;
        public bool RememberMe
        {
            get { return _RememberMe; }
            set
            {
                if (_RememberMe == value) return;
                _RememberMe = value;
                NotifyOfPropertyChange(() => RememberMe);
            }
        }

        public LoginViewModel(IEventAggregator eventAggregator, IWindowManager windowManager) :
            base(eventAggregator, windowManager)
        {
            _User = new UserViewModel();
            _Session = new LoadifySession(_EventAggregator);
        }


        public void Login()
        {
            LoginProcessActive = true;

            // since you can't bind the passwordbox to a property, the viewmodel needs to be aware of the view to access the password entered
            var loginView = GetView() as LoginView;
            var password = loginView.Password.Password;

            if (RememberMe)
            {
                Settings.Default.Username = User.Name;
                Settings.Default.Password = loginView.Password.Password;
                Settings.Default.Save();
            }

            _Session.Login(User.Name, password);          
        }

        public void OnKeyUp(Key key)
        {
            if (key == Key.Enter)
                Login();
        }

        protected override void OnViewLoaded(object view)
        {
            if (!String.IsNullOrEmpty(Settings.Default.Username) || !String.IsNullOrEmpty(Settings.Default.Password))
            {
                var loginView = GetView() as LoginView;
                RememberMe = true;
                User.Name = Settings.Default.Username;
                loginView.Password.Password = Settings.Default.Password;
            }        
        }

        public void Handle(LoginFailedEvent message)
        {
            var view = GetView() as LoginView;
            LoginProcessActive = false;

            switch (message.Error)
            {
                case SpotifyError.BadUsernameOrPassword:
                {
                    view.ShowMessageAsync("Login failed", "Name or password is wrong");
                    break;
                }
                case SpotifyError.UnableToContactServer:
                {
                    view.ShowMessageAsync("Login failed", "No connection to the Spotify servers could be made");
                    break;
                }
                default:
                {
                    view.ShowMessageAsync("Login failed", "Unknown error: " + message.Error);
                    break;
                }
            }
        }

        public void Handle(LoginSuccessfulEvent message)
        {
            var loginView = GetView() as LoginView;
            _WindowManager.ShowWindow(new MainViewModel(_Session, _User, _EventAggregator));
            loginView.Close();
        }
    }
}
