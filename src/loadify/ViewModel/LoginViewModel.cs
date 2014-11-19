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

        public LoginViewModel(IEventAggregator eventAggregator, IWindowManager windowManager, ISettingsManager settingsManager) :
            base(eventAggregator, windowManager, settingsManager)
        {
            _User = new UserViewModel();
            _Session = new LoadifySession();
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
                _Logger.Debug("Remember me option was enabled, credentials were saved into the configuration");
            }
            else
            {
                _SettingsManager.CredentialsSetting.Username = String.Empty;
                _SettingsManager.CredentialsSetting.Password = String.Empty;
            }

            _Logger.Debug(String.Format("Login triggered, trying to login with Username {0}", User.Name));
            _Session.Login(User.Name, password, async error =>
            {
                _Logger.Debug(String.Format("Login completed, Error: {0}", error));
                if (error == SpotifyError.Ok)
                {
                    _Logger.Info(String.Format("Login successful, logged in as User {0}", User.Name));
                    _Logger.Debug("Opening the main window...");
                    _WindowManager.ShowWindow(new MainViewModel(_Session, _User, _EventAggregator, _WindowManager, _SettingsManager));
                    _Logger.Debug("Main window opened. Attempting to close the login window...");
                    loginView.Close();
                    _Logger.Debug("Login window closed");
                }
                else
                {
                    LoginProcessActive = false;

                    switch (error)
                    {
                        case SpotifyError.BadUsernameOrPassword:
                        {
                            _Logger.Fatal("Login failed, wrong username or password has been entered");
                            await loginView.ShowMessageAsync("Login failed", "Name or password is wrong");
                            break;
                        }
                        case SpotifyError.UnableToContactServer:
                        {
                            _Logger.Fatal("Login failed, no connection to the Spotify servers could be made");
                            await loginView.ShowMessageAsync("Login failed", "No connection to the Spotify servers could be made");
                            break;
                        }
                        default:
                        {
                            _Logger.Fatal("Login failed due to unhandled reasons");
                            await loginView.ShowMessageAsync("Login failed", "Unknown error");
                            break;
                        }
                    }
                }
            });          
        }

        public void OnKeyUp(Key key)
        {
            if (key == Key.Enter)
            {
                _Logger.Debug("The enter key was pressed");
                Login();
            }
        }

        protected override void OnViewLoaded(object view)
        {
            if (!String.IsNullOrEmpty(_SettingsManager.CredentialsSetting.Username) || !String.IsNullOrEmpty(_SettingsManager.CredentialsSetting.Password))
            {
                var loginView = GetView() as LoginView;
                RememberMe = true;
                User.Name = _SettingsManager.CredentialsSetting.Username;
                loginView.Password.Password = _SettingsManager.CredentialsSetting.Password;
                _Logger.Info("Stored credentials were found and loaded");
            }
            else
            {
                _Logger.Info("No credential configuration was found");
            }
        }
    }
}
