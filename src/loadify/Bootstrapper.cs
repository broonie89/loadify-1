using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using loadify.Configuration;
using loadify.ViewModel;

namespace loadify
{
    public class Bootstrapper : BootstrapperBase
    {
        /// <summary>
        /// IoC container for dependency injection
        /// </summary>
        private readonly SimpleContainer _Container = new SimpleContainer();

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            // Callback for redirecting pressed keys to the corresponding viewmodel
            MessageBinder.SpecialValues.Add("$pressedkey", (context) =>
            {
                var keyArgs = context.EventArgs as KeyEventArgs;

                if (keyArgs != null)
                    return keyArgs.Key;

                return null;
            });

            _Container.Singleton<IEventAggregator, EventAggregator>();
            _Container.Singleton<IWindowManager, WindowManager>();
            _Container.Singleton<ISettingsManager, NETSettingsManager>();
            _Container.PerRequest<LoginViewModel>();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<LoginViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = _Container.GetInstance(service, key);
            if (instance != null)
                return instance;
            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _Container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _Container.BuildUp(instance);
        }
    }
}
