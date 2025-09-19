using System.Windows;
using System.Windows.Controls;

namespace TodoAppWpf.Controls
{
    public partial class DateTimePicker : UserControl
    {
        public static readonly DependencyProperty SelectedDateTimeProperty =
            DependencyProperty.Register("SelectedDateTime", typeof(DateTime?), typeof(DateTimePicker),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateTimeChanged));
        public static readonly DependencyProperty SelectedTimeProperty =
            DependencyProperty.Register("SelectedTime", typeof(TimeSpan?), typeof(DateTimePicker),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedTimeChanged));

        public DateTime? SelectedDateTime
        {
            get => (DateTime?)GetValue(SelectedDateTimeProperty);
            set => SetValue(SelectedDateTimeProperty, value);
        }
        public TimeSpan? SelectedTime
        {
            get => (TimeSpan?)GetValue(SelectedTimeProperty);
            set => SetValue(SelectedTimeProperty, value);
        }

        private bool _isLoaded = false;
        private bool _updatingFromUI = false;

        public DateTimePicker()
        {
            InitializeComponent();
            this.Loaded += DateTimePicker_Loaded;
        }

        private void DateTimePicker_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            UpdateDisplay();
        }

        private static void OnSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DateTimePicker)d;
            if (control._isLoaded && !control._updatingFromUI)
            {
                control.UpdateDisplay();
            }
        }

        private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DateTimePicker)d;
            if (control._isLoaded && !control._updatingFromUI)
            {
                control.UpdateDateTimeFromTime();

                // Update the time display when SelectedTime changes
                if (control.SelectedTime.HasValue)
                {
                    control.txtTime.Text = $"{control.SelectedTime.Value.Hours:00}:{control.SelectedTime.Value.Minutes:00}";
                }
                else
                {
                    control.txtTime.Text = "--:--";
                }
            }
        }

        private void UpdateDateTimeFromTime()
        {
            if (datePicker.SelectedDate.HasValue && SelectedTime.HasValue)
            {
                _updatingFromUI = true;
                SelectedDateTime = datePicker.SelectedDate.Value + SelectedTime.Value;
                _updatingFromUI = false;
            }
        }

        private void UpdateDisplay()
        {
            if (!_isLoaded || _updatingFromUI) return;

            _updatingFromUI = true;

            if (SelectedDateTime.HasValue)
            {
                datePicker.SelectedDate = SelectedDateTime.Value.Date;
                SelectedTime = SelectedDateTime.Value.TimeOfDay;

                txtSelectedDateTime.Text = SelectedDateTime.Value.ToString("dd/MM/yyyy HH:mm");

                string hFormat =    $"{SelectedDateTime.Value.TimeOfDay.Hours:00}";
                string minFormat =  $"{SelectedDateTime.Value.TimeOfDay.Minutes:00}";
                txtTime.Text =      $"{hFormat}:{minFormat}";
            }
            else
            {
                datePicker.SelectedDate = null;
                SelectedTime = null;
                txtSelectedDateTime.Text = "No date selected";
                txtTime.Text = "--:--";
            }

            _updatingFromUI = false;
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_updatingFromUI) return;

            if (datePicker.SelectedDate.HasValue)
            {
                var currentTime = SelectedDateTime.HasValue ? SelectedDateTime.Value.TimeOfDay : new TimeSpan(12, 0, 0);
                SelectedDateTime = datePicker.SelectedDate.Value.Add(currentTime);
            }
            else
            {
                SelectedDateTime = null;
            }
        }

        private void BtnClearDate_Click(object sender, RoutedEventArgs e)
        {
            datePicker.SelectedDate = null;
        }

        private void BtnNow_Click(object sender, RoutedEventArgs e)
        {
            SelectedDateTime = DateTime.Now;
        }

        private void BtnClearTime_Click(object sender, RoutedEventArgs e)
        {
            SelectedTime = null;
            if (datePicker.SelectedDate.HasValue)
            {
                SelectedDateTime = datePicker.SelectedDate.Value;
            }
        }
    }
}
