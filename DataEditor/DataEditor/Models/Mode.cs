using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataEditor.Models
{
    public class Mode : ObservableObject, INotifyPropertyChanged
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
        public Mode(int id, string name, int maxBottleNumber, int maxUsedTips)
        {
            //_id = id;
            ID = GetNextID();
            _name = name;
            _maxBottleNumber = maxBottleNumber;
            _maxUsedTips = maxUsedTips;
        }

        public Mode()
        {
            ID = GetNextID();
            Name = "";
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

        private string? _name;
        public string? Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _maxBottleNumber;
        public int MaxBottleNumber
        {
            get => _maxBottleNumber;
            set
            {
                if (_maxBottleNumber != value)
                {
                    _maxBottleNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _maxUsedTips;
        public int MaxUsedTips
        {
            get => _maxUsedTips;
            set
            {
                if (_maxUsedTips != value)
                {
                    _maxUsedTips = value;
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
            return $"{ID}\t{Name}\t{MaxBottleNumber}\t{MaxUsedTips}";
        }
    }
}
