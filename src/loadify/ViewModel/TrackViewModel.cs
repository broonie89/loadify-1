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

        public int Duration
        {
            get { return Track.Duration; }
            set
            {
                if (Track.Duration == value) return;
                Track.Duration = value;
                NotifyOfPropertyChange(() => Duration);
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
                _EventAggregator.PublishOnUIThread(new TrackSelectedEvent(this));
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
    }
}
