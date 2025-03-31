using CommunityToolkit.Mvvm.ComponentModel;
using Splat.ModeDetection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace DataEditor.Models
{
    public class Step : ObservableObject, INotifyPropertyChanged
    {
        private static int _nextID = -1;
        public static int GetNextID()
        {
            return _nextID++;
        }
        public static void SetNextID(int val)
        {
            _nextID = val;
        }

        public Step(int id, int modeId, int timer, string destination, 
            int speed, string type, int volume)
        {
            //_id = id;
            ID = GetNextID();
            _modeId = modeId;
            _oldModeID = modeId;
            _timer = timer;
            _destination = destination;
            _speed = speed;
            _type = type;
            _volume = volume;
        }

        public Step()
        {
            ID = GetNextID();
            _oldModeID = _modeId;
            Destination = "";
            Type = "";
        }

        public int _id { get; set; }
        public int ID
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _modeId;
        private int _oldModeID;
        public int ModeId
        {
            get => _modeId;
            set
            {
                if (_modeId != value)
                {
                    _oldModeID = _modeId;
                    _modeId = value;
                    OnPropertyChanged();
                }
            }
        }
        public int GetOldModeID()
        {
            return _oldModeID;
        }

        private int _timer;
        public int Timer
        {
            get => _timer;
            set
            {
                if (_timer != value)
                {
                    _timer = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _destination;
        public string? Destination
        {
            get => _destination;
            set
            {
                if (_destination != value)
                {
                    _destination = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _speed;
        public int Speed
        {
            get => _speed;
            set
            {
                if (_speed != value)
                {
                    _speed = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _type;
        public string? Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _volume;
        public int Volume
        {
            get => _volume;
            set
            {
                if (_volume != value)
                {
                    _volume = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            return $"{ID}\t{ModeId}\t{Timer}\t{Destination}\t{Speed}\t{Type}\t{Volume}";
        }
    }
}