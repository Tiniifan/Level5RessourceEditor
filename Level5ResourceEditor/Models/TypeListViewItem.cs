using System;
using System.ComponentModel;
using StudioElevenLib.Level5.Resource;

namespace Level5ResourceEditor.Models
{
    public class TypeListViewItem : INotifyPropertyChanged
    {
        private string _displayName;
        private int _elementCount;

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public RESType Type { get; set; }

        public Type FilterType { get; set; }

        public int ElementCount
        {
            get => _elementCount;
            set
            {
                _elementCount = value;
                OnPropertyChanged(nameof(ElementCount));
                OnPropertyChanged(nameof(DisplayCount));
            }
        }

        public string DisplayCount => ElementCount.ToString();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}