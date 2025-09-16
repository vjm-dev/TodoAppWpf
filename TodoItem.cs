using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace TodoAppWpf
{
    public class TodoItem : INotifyPropertyChanged
    {
        private string _status = string.Empty;
        private Brush _statusColor = Brushes.Gray;
        private bool _isDescriptionExpanded = false;
        private bool _showToggleButton = false;

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

        [JsonIgnore]
        public bool IsDescriptionExpanded
        {
            get => _isDescriptionExpanded;
            set
            {
                _isDescriptionExpanded = value;
                OnPropertyChanged(nameof(IsDescriptionExpanded));
                OnPropertyChanged(nameof(ToggleDescriptionText));
            }
        }

        [JsonIgnore]
        public bool ShowToggleButton
        {
            get => _showToggleButton;
            set
            {
                _showToggleButton = value;
                OnPropertyChanged(nameof(ShowToggleButton));
            }
        }

        [JsonIgnore]
        public string ToggleDescriptionText => IsDescriptionExpanded ? "Show less" : "Show more";

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

        public void CheckDescriptionLength()
        {
            // Show toggle button only if description has more than 3 lines
            var lineCount = Description.Split('\n').Length;
            ShowToggleButton = lineCount > 3;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
