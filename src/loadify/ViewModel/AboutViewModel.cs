using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace loadify.ViewModel
{ 
    public class AboutViewModel : ViewModelBase
    {
        public string License
        {
            get { return "The MIT License (MIT) - Copyright (c) 2014"; }
        }

        public string Contributors
        {
            get { return String.Join(", ", new List<string>() { "Mostey", "snowfalk13", "greensn0w" }); }
        }

        public string Version
        {
            get { return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion; }
        }

        public AboutViewModel()
        { }
    }
}
