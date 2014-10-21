using System.Windows.Threading;
using Caliburn.Micro;

namespace loadify.ViewModel
{
    public class ViewModelBase : ViewAware
    {
        protected IEventAggregator _EventAggregator;
        protected IWindowManager _WindowManager;
        private Dispatcher _Dispatcher;

        public ViewModelBase(IEventAggregator eventAggregator, IWindowManager windowManager)
            : this(eventAggregator)
        {
            _WindowManager = windowManager;
        }

        public ViewModelBase(IEventAggregator eventAggregator)
            : this()
        {
            _EventAggregator = eventAggregator;
            _EventAggregator.Subscribe(this);
        }

        public ViewModelBase(IWindowManager windowManager)
            : this()
        {
            _WindowManager = windowManager;
        }

        public ViewModelBase()
        {
            _Dispatcher = Dispatcher.CurrentDispatcher;
        }

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
