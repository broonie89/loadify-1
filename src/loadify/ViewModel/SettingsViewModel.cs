using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Caliburn.Micro;
using loadify.Configuration;
using loadify.Event;

namespace loadify.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        public string DownloadDirectory
        {
            get { return _SettingsManager.DirectorySetting.DownloadDirectory; }
            set
            {
                if (_SettingsManager.DirectorySetting.DownloadDirectory == value) return;
                _Logger.Debug(String.Format("DownloadDirectory setting has been changed. Old value: {0}, new value: {1}",
                                            _SettingsManager.DirectorySetting.DownloadDirectory, value));

                _SettingsManager.DirectorySetting.DownloadDirectory = value;
                NotifyOfPropertyChange(() => DownloadDirectory);
            }
        }

        public string CacheDirectory
        {
            get { return _SettingsManager.DirectorySetting.CacheDirectory; }
            set
            {
                if (_SettingsManager.DirectorySetting.CacheDirectory == value) return;
                _Logger.Debug(String.Format("CacheDirectory setting has been changed. Old value: {0}, new value: {1}",
                                            _SettingsManager.DirectorySetting.CacheDirectory, value));

                _SettingsManager.DirectorySetting.CacheDirectory = value;
                NotifyOfPropertyChange(() => CacheDirectory);
            }
        }

        public bool NotifyLocalTrackDetections
        {
            get { return _SettingsManager.BehaviorSetting.NotifyLocalTrackDetections; }
            set
            {
                if (_SettingsManager.BehaviorSetting.NotifyLocalTrackDetections == value) return;
                _Logger.Debug(String.Format("NotifyLocalTrackDetections setting has been changed. Old value: {0}, new value: {1}",
                                            _SettingsManager.BehaviorSetting.NotifyLocalTrackDetections, value));

                _SettingsManager.BehaviorSetting.NotifyLocalTrackDetections = value;
                NotifyOfPropertyChange(() => NotifyLocalTrackDetections);
            }
        }

        public bool CleanupAfterConversion
        {
            get { return _SettingsManager.BehaviorSetting.CleanupAfterConversion; }
            set
            {
                if (_SettingsManager.BehaviorSetting.CleanupAfterConversion == value) return;
                _Logger.Debug(String.Format("CleanupAfterConversion setting has been changed. Old value: {0}, new value: {1}",
                                            _SettingsManager.BehaviorSetting.CleanupAfterConversion, value));

                _SettingsManager.BehaviorSetting.CleanupAfterConversion = value;
                NotifyOfPropertyChange(() => CleanupAfterConversion);
            }
        }


        public bool SkipOnDownloadFailures
        {
            get { return _SettingsManager.BehaviorSetting.SkipOnDownloadFailures; }
            set
            {
                if (_SettingsManager.BehaviorSetting.SkipOnDownloadFailures == value) return;
                _Logger.Debug(String.Format("SkipOnDownloadFailures setting has been changed. Old value: {0}, new value: {1}",
                                            _SettingsManager.BehaviorSetting.SkipOnDownloadFailures, value));

                _SettingsManager.BehaviorSetting.SkipOnDownloadFailures = value;
                NotifyOfPropertyChange(() => SkipOnDownloadFailures);
            }
        }



        public SettingsViewModel(IEventAggregator eventAggregator, ISettingsManager settingsManager) :
            base(eventAggregator, settingsManager)
        { }

        public void BrowseCacheDirectory()
        {
            var dialog = new FolderBrowserDialog();
            var dialogResult = dialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
                CacheDirectory = dialog.SelectedPath;
        }

        public void BrowseDownloadDirectory()
        {
            var dialog = new FolderBrowserDialog();
            var dialogResult = dialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
                DownloadDirectory = dialog.SelectedPath;
        }
    }
}
