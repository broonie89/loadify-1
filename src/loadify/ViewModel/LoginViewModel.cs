using System;
using System.Windows.Input;
using Caliburn.Micro;
using loadify.Configuration;
using loadify.Properties;
using loadify.Spotify;
using loadify.View;
using MahApps.Metro.Controls.Dialogs;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly LoadifySession _Session;
        private readonly ISettingsManager _SettingsManager;

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

        public LoginViewModel(IEventAggregator eventAggregator, IWindowManager windowManager, ISettingsManager netSettingsManager) :
            base(eventAggregator, windowManager)
        {
            _User = new UserViewModel();
            _Session = new LoadifySession();
            _SettingsManager = netSettingsManager;
        }


        public void Login()
        {
            LoginProcessActive = true;

            // since you can't bind the passwordbox to a property, the viewmodel needs to be aware of the view to access the password entered
            var loginView = GetView() as LoginView;
            var password = loginView.Password.Password;

            if (RememberMe)
            {
                _SettingsManager.CredentialsSetting.Username = User.Name;
                _SettingsManager.CredentialsSetting.Password = loginView.Password.Password;
            }
            else
            {
                _SettingsManager.CredentialsSetting.Username = String.Empty;
                _SettingsManager.CredentialsSetting.Password = String.Empty;
            }

            _Session.Login(User.Name, password, async error =>
            {
                if (error == SpotifyError.Ok)
                {
                    _WindowManager.ShowWindow(new MainViewModel(_Session, _User, _EventAggregator, _WindowManager, _SettingsManager));
                    loginView.Close();            
                }
                else
                {
                    LoginProcessActive = false;

                    switch (error)
                    {
                        case SpotifyError.BadUsernameOrPassword:
                        {
                            await loginView.ShowMessageAsync("Login failed", "Name or password is wrong");
                            break;
                        }
                        case SpotifyError.UnableToContactServer:
                        {
                            await loginView.ShowMessageAsync("Login failed", "No connection to the Spotify servers could be made");
                            break;
                        }
                        default:
                        {
                            await loginView.ShowMessageAsync("Login failed", "Unknown error: " + error);
                            break;
                        }
                    }
                }
            });          
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
                User.Name = _SettingsManager.CredentialsSetting.Username;
                loginView.Password.Password = _SettingsManager.CredentialsSetting.Password;
            }        
        }
    }
}
