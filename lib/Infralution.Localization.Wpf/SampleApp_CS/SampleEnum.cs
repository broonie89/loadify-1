#region File Header
//
// COPYRIGHT:   Copyright 2009 
//              Infralution
//
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Infralution.Localization.Wpf;
namespace WpfApp
{

    /// <summary>
    /// Sample enum illustrating used of a localized enum type converter
    /// </summary>
    [TypeConverter(typeof(SampleEnumConverter))]
    public enum SampleEnum
    {
        VerySmall,
        Small,
        Medium,
        Large,
        VeryLarge
    }

    /// <summary>
    /// Define the type converter for the Sample Enum
    /// </summary>
    class SampleEnumConverter : ResourceEnumConverter
    {
        /// <summary>
        /// Create a new instance of the converter using translations from the given resource manager
        /// </summary>
        public SampleEnumConverter()
            : base(typeof(SampleEnum), Properties.Resources.ResourceManager)
        {
        }
    }

}
