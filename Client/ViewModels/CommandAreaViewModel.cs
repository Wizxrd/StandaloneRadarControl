using System.ComponentModel;

namespace Client.Views
{
    public class CommandAreaViewModel : INotifyPropertyChanged
    {
        private string _commandText;

        public string CommandText
        {
            get => _commandText;
            set
            {
                if (_commandText != value)
                {
                    _commandText = value;
                    OnPropertyChanged(nameof(CommandText));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
