using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace loadify
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Configure Caliburn.Micro to look for views and viewmodels at the specified namespaces.
            // By default, Caliburn.Micro looks in the root namespace which'd be just 'loadify'
            var config = new TypeMappingConfiguration
            {
                DefaultSubNamespaceForViews = "loadify.Views",
                DefaultSubNamespaceForViewModels = "loadify.ViewModels"
            };

            ViewLocator.ConfigureTypeMappings(config);
            ViewModelLocator.ConfigureTypeMappings(config);

            ShutdownMode = System.Windows.ShutdownMode.OnLastWindowClose;
        }
    }
}
