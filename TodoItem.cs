using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace TodoAppWpf
{
    public class TodoItem : INotifyPropertyChanged
    {
        private string _status = string.Empty;
        private Brush _statusColor = Brushes.Gray;
        private DateTime? _reminderDate;

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

        public DateTime? ReminderDate
        {
            get => _reminderDate;
            set
            {
                _reminderDate = value;
                OnPropertyChanged(nameof(ReminderDate));
                OnPropertyChanged(nameof(HasReminder));
            }
        }

        [JsonIgnore]
        public bool HasReminder => ReminderDate.HasValue && ReminderDate > DateTime.Now;

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

        [JsonIgnore]
        public Brush StatusColor
        {
            get => _statusColor;
            set
            {
                _statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public string StatusColorString
        {
            get
            {
                if (StatusColor is SolidColorBrush solidColorBrush)
                    return solidColorBrush.Color.ToString();
                return StatusColor.ToString();
            }
            set
            {
                try
                {
                    var converter = new System.Windows.Media.BrushConverter();
                    StatusColor = (Brush)converter.ConvertFromString(value)!;
                }
                catch
                {
                    StatusColor = Brushes.Gray;
                }
            }
        }

        public string ReminderDateString
        {
            get => ReminderDate?.ToString("o") ?? string.Empty;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    ReminderDate = null;
                }
                else
                {
                    if (DateTime.TryParse(value, out DateTime date))
                    {
                        ReminderDate = date;
                    }
                    else
                    {
                        ReminderDate = null;
                    }
                }
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
