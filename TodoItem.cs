using System;
using System.ComponentModel;
using System.Windows.Media;

namespace TodoAppWpf
{
    public class TodoItem : INotifyPropertyChanged
    {
        private string _status = string.Empty;
        private Brush _statusColor = Brushes.Gray;

        public TodoItem()
        {
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                ModifiedDate = DateTime.Now;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(ModifiedDate));
            }
        }

        public Brush StatusColor
        {
            get => _statusColor;
            set
            {
                _statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public void UpdateStatusColor()
        {
            StatusColor = Status switch
            {
                "Pending" => Brushes.Orange,
                "Under review" => Brushes.Blue,
                "Done" => Brushes.Green,
                _ => Brushes.Gray
            };
        }

        public void UpdateModifiedDate()
        {
            ModifiedDate = DateTime.Now;
            OnPropertyChanged(nameof(ModifiedDate));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
