using System.Windows.Threading;
using Caliburn.Micro;
using loadify.Configuration;
using loadify.Logging;

namespace loadify.ViewModel
{
    public class ViewModelBase : ViewAware
    {
        protected IEventAggregator _EventAggregator;
        protected IWindowManager _WindowManager;
        protected ISettingsManager _SettingsManager;
        protected ILogger _Logger;
        private readonly Dispatcher _Dispatcher;

        public ViewModelBase(IEventAggregator eventAggregator = null, IWindowManager windowManager = null, ISettingsManager settingsManager = null)
        {
            _WindowManager = windowManager;
            _SettingsManager = settingsManager;
            _Logger = new Log4NetLogger(GetType());
            _EventAggregator = eventAggregator;

            if (_EventAggregator != null)
                _EventAggregator.Subscribe(this);

            _Dispatcher = Dispatcher.CurrentDispatcher;
        }

         public ViewModelBase(IEventAggregator eventAggregator, ISettingsManager settingsManager):
             this(eventAggregator, null, settingsManager)
        {  }

        protected void Execute(System.Action action)
        {
            if (_Dispatcher.CheckAccess())
            {
                action.Invoke();
            }
            else
            {
                _Dispatcher.BeginInvoke(DispatcherPriority.DataBind, action);
            }
        }
    }
}
