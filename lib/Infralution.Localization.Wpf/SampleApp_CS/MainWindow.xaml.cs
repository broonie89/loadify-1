using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Globalization;
using Infralution.Localization.Wpf;

namespace WpfApp
{
    /// <summary>
    /// Demonstrates a small WPF application localized using Globalizer.NET.  Also
    /// demonstrates using the Infralution.Localization.ResourceEnumConverter
    /// class to translate Enum values.
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Create a new instance of the window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // set the initial application UI Culture based on the users
            // current regional settings
            //
            CultureManager.UICulture = Thread.CurrentThread.CurrentCulture;
            CultureManager.UICultureChanged += new EventHandler(CultureManager_UICultureChanged);
            UpdateLanguageMenus();
        }

        /// <summary>
        /// Detach from UICultureChanged event
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// If we don't detach from the event then the window will not get garbage collected
        /// </remarks>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            CultureManager.UICultureChanged -= new EventHandler(CultureManager_UICultureChanged);
        }
 
        /// <summary>
        /// Update the check state of the Language menu
        /// </summary>
        private void UpdateLanguageMenus()
        {
            string lang = CultureManager.UICulture.TwoLetterISOLanguageName.ToLower();
            _frenchMenuItem.IsChecked = (lang == "fr");
            _englishMenuItem.IsChecked = (lang == "en");
            _fileListBox.ItemsSource = System.Enum.GetValues(typeof(SampleEnum));
        }

        /// <summary>
        /// Update the language menus when the UI culture changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CultureManager_UICultureChanged(object sender, EventArgs e)
        {
            UpdateLanguageMenus();
        }

        /// <summary>
        /// Select English as the User Interface language
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _englishMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CultureManager.UICulture = new CultureInfo("en");
        }

        /// <summary>
        /// Select French as the User Interface language
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _frenchMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CultureManager.UICulture = new CultureInfo("fr");
        }

        /// <summary>
        /// Close the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
