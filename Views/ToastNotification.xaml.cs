using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TodoAppWpf.Views
{
    public partial class ToastNotification : Window
    {
        private static List<ToastNotification> activeToasts = new List<ToastNotification>();
        private const int ToastMargin = 10;

        public string ToastTitle { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Brush BackgroundColor { get; set; } = Brushes.Transparent;
        public Brush ProgressColor { get; set; } = Brushes.Transparent;
        public string IconText { get; set; } = string.Empty;

        public ToastNotification()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ToastNotification(string title, string message, ToastType type = ToastType.INFO)
            : this()
        {
            ToastTitle = title;
            Message = message;

            switch (type)
            {
                case ToastType.SUCCESS:
                    BackgroundColor = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                    ProgressColor = new SolidColorBrush(Color.FromRgb(56, 142, 60));
                    IconText = "\xE001"; // Checkmark
                    break;
                case ToastType.ERROR:
                    BackgroundColor = new SolidColorBrush(Color.FromRgb(244, 67, 54));
                    ProgressColor = new SolidColorBrush(Color.FromRgb(211, 47, 47));
                    IconText = "\xE10A"; // Error X
                    break;
                case ToastType.WARNING:
                    BackgroundColor = new SolidColorBrush(Color.FromRgb(255, 152, 0));
                    ProgressColor = new SolidColorBrush(Color.FromRgb(245, 124, 0));
                    IconText = "\xE7BA"; // Warning triangle
                    break;
                default:
                    BackgroundColor = new SolidColorBrush(Color.FromRgb(33, 150, 243));
                    ProgressColor = new SolidColorBrush(Color.FromRgb(25, 118, 210));
                    IconText = "\xE946"; // Info icon
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double newTop = CalculateNewTopPosition();

            this.Top = newTop;
            this.Left = SystemParameters.WorkArea.Right - this.Width - 10;

            activeToasts.Add(this);

            // Start animations
            var slideIn = (Storyboard)Resources["SlideInAnimation"];
            slideIn.Begin();

            var progressAnimation = (Storyboard)Resources["ProgressBarAnimation"];
            progressAnimation.Completed += (s, args) => CloseToast();
            progressAnimation.Begin();
        }

        private double CalculateNewTopPosition()
        {
            double position = 10;
            foreach (var toast in activeToasts)
            {
                position += toast.ActualHeight + ToastMargin;
            }
            return position;
        }

        private void CloseToast()
        {
            activeToasts.Remove(this);
            AdjustExistingToasts();

            var slideOut = (Storyboard)Resources["SlideOutAnimation"];
            slideOut.Completed += (s, args) => Close();
            slideOut.Begin();
        }

        private void AdjustExistingToasts()
        {
            double currentTop = 10;
            foreach (var toast in activeToasts)
            {
                toast.Top = currentTop;
                currentTop += toast.ActualHeight + ToastMargin;
            }
        }
    }

    public enum ToastType
    {
        INFO,
        SUCCESS,
        WARNING,
        ERROR
    }
}
