using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Caliburn.Micro;
using loadify.Event;
using loadify.Model;

namespace loadify.ViewModel
{
    public class TrackViewModel : ViewModelBase
    {
        private TrackModel _Track;
        public TrackModel Track
        {
            get { return _Track; }
            set
            {
                if (_Track == value) return;
                _Track = value;
                NotifyOfPropertyChange(() => Track);
            }
        }

        public string Name
        {
            get { return Track.Name; }
            set
            {
                if (Track.Name == value) return;
                Track.Name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public TimeSpan Duration
        {
            get { return Track.Duration; }
            set
            {
                if (Track.Duration == value) return;
                Track.Duration = value;
                NotifyOfPropertyChange(() => Duration);
            }
        }

        public AlbumModel Album
        {
            get { return Track.Album; }
            set
            {
                if (Track.Album == value) return;
                _Track.Album = value;
                NotifyOfPropertyChange(() => Album);
            }
        }

        public string Artists
        {
            get
            {
                var artists = "";
                foreach (var artist in Track.Artists)
                {
                    artists += artist.Name;
                    if (artist != Track.Artists.LastOrDefault())
                        artists += ", ";
                }

                return artists;
            }
        }

        private bool _Selected;
        public bool Selected
        {
            get { return _Selected; }
            set
            {
                if (_Selected == value) return;
                _Selected = value;
                NotifyOfPropertyChange(() => Selected);
                _EventAggregator.PublishOnUIThread(new TrackSelectedChangedEvent(this, _Selected));
            }
        }

        private bool _Expanded;
        public bool Expanded
        {
            get { return _Expanded; }
            set
            {
                if (_Expanded == value) return;
                _Expanded = value;
                NotifyOfPropertyChange(() => Expanded);
            }
        }

        public TrackViewModel(TrackModel track, IEventAggregator eventAggregator):
            base(eventAggregator)
        {
            _Track = track;
        }

        public TrackViewModel(IEventAggregator eventAggregator) :
            this(new TrackModel(), eventAggregator)
        { }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Artists, Name);
        }
    }
}
