using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TodoAppWpf.Controls
{
    public partial class AnalogClock : UserControl
    {
        public static readonly DependencyProperty SelectedTimeProperty =
            DependencyProperty.Register("SelectedTime", typeof(TimeSpan?), typeof(AnalogClock),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedTimeChanged));

        private List<Button> hourButtons = new List<Button>();
        private List<Button> hour24Buttons = new List<Button>();
        private List<Button> minuteButtons = new List<Button>();

        public TimeSpan? SelectedTime
        {
            get => (TimeSpan?)GetValue(SelectedTimeProperty);
            set => SetValue(SelectedTimeProperty, value);
        }

        public AnalogClock()
        {
            InitializeComponent();
            DrawClockFace();
            UpdateClockHands();
            HighlightSelectedButtons();
        }

        private void DrawClockFace()
        {
            // Draw 12-hour buttons (middle circle)
            for (int i = 1; i <= 12; i++)
            {
                double angle = ((i - 12) * 30 - 90) * Math.PI / 180;
                double x = 120 + 100 * Math.Cos(angle);
                double y = 120 + 100 * Math.Sin(angle);

                Button button = new Button
                {
                    Content = i.ToString(),
                    Width = 40,
                    Height = 40,
                    Background = new SolidColorBrush(Color.FromArgb(255, 255, 165, 0)),
                    Foreground = Brushes.White,
                    FontSize = 12,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(0),
                    Tag = i // Store the hour value in Tag
                };

                button.Template = CreateRoundButtonTemplate();
                button.Click += HourButton_Click;

                Canvas.SetLeft(button, x - 20);
                Canvas.SetTop(button, y - 20);
                ClockCanvas.Children.Add(button);
                hourButtons.Add(button);
            }

            // Draw 24-hour buttons (inner circle)
            for (int i = 13; i <= 24; i++)
            {
                double angle = (i * 30 - 90) * Math.PI / 180;
                double x = 120 + 70 * Math.Cos(angle);
                double y = 120 + 70 * Math.Sin(angle);

                int hourValue = i == 24 ? 0 : i;

                Button button = new Button
                {
                    Content = i == 24 ? "00" : i.ToString(),
                    Width = 40,
                    Height = 40,
                    Background = new SolidColorBrush(Color.FromArgb(255, 255, 200, 100)),
                    Foreground = Brushes.White,
                    FontSize = 12,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(0),
                    Tag = hourValue // Store the hour value in Tag
                };

                button.Template = CreateRoundButtonTemplate();
                button.Click += Hour24Button_Click;

                Canvas.SetLeft(button, x - 20);
                Canvas.SetTop(button, y - 20);
                ClockCanvas.Children.Add(button);
                hour24Buttons.Add(button);
            }

            // Draw minute buttons (outer circle)
            string[] minuteLabels = { "00", "05", "10", "15", "20", "25", "30", "35", "40", "45", "50", "55" };
            for (int i = 0; i < 12; i++)
            {
                double angle = (i * 30 - 90) * Math.PI / 180;
                double x = 120 + 140 * Math.Cos(angle);
                double y = 120 + 140 * Math.Sin(angle);

                Button button = new Button
                {
                    Content = minuteLabels[i],
                    Width = 40,
                    Height = 40,
                    Background = new SolidColorBrush(Color.FromArgb(255, 255, 220, 140)),
                    Foreground = Brushes.White,
                    FontSize = 14,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(0),
                    Tag = minuteLabels[i] // Store the minute value in Tag
                };

                button.Template = CreateRoundButtonTemplate();
                button.Click += MinuteButton_Click;

                Canvas.SetLeft(button, x - 20);
                Canvas.SetTop(button, y - 20);
                ClockCanvas.Children.Add(button);
                minuteButtons.Add(button);
            }
        }

        private void HourButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int hour = (int)button.Tag;

            UpdateTime(hour, SelectedTime?.Minutes ?? 0);
        }

        private void Hour24Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int hour = (int)button.Tag;
            UpdateTime(hour, SelectedTime?.Minutes ?? 0);
        }

        private void MinuteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string minuteString = (string)button.Tag;
            int minutes = int.Parse(minuteString);
            UpdateTime(SelectedTime?.Hours ?? 0, minutes);
        }

        private void UpdateTime(int hours, int minutes)
        {
            SelectedTime = new TimeSpan(hours, minutes, 0);
            HighlightSelectedButtons();
        }

        private void HighlightSelectedButtons()
        {
            SolidColorBrush orange = new SolidColorBrush(Color.FromArgb(255, 255, 145, 0));
            SolidColorBrush orangeB = new SolidColorBrush(Color.FromArgb(255, 225, 135, 0));
            SolidColorBrush blueCyan = new SolidColorBrush(Color.FromArgb(255, 0, 152, 157));
            SolidColorBrush purple = new SolidColorBrush(Color.FromArgb(255, 80, 0, 160));
            SolidColorBrush red = new SolidColorBrush(Color.FromArgb(255, 255, 10, 0));

            // Reset all buttons to default colors
            foreach (var button in hourButtons)
            {
                button.Background = orangeB;
            }

            foreach (var button in hour24Buttons)
            {
                button.Background = orange;
            }

            foreach (var button in minuteButtons)
            {
                button.Background = blueCyan;
            }

            if (!SelectedTime.HasValue) return;

            // Highlight selected minute button
            int minutes = SelectedTime.Value.Minutes;
            string minuteString = minutes.ToString("00");
            if (minutes % 5 != 0) // Round to nearest 5 minutes
            {
                minutes = (minutes / 5) * 5;
                minuteString = minutes.ToString("00");
            }

            foreach (var button in minuteButtons)
            {
                if ((string)button.Tag == minuteString)
                {
                    button.Background = purple;
                    break;
                }
            }

            // Highlight selected hour button (24-hour format)
            foreach (var button in hour24Buttons)
            {
                if ((int)button.Tag == SelectedTime.Value.Hours)
                {
                    button.Background = red;
                    return;
                }
            }

            // Highlight selected hour button (12-hour format) - ONLY if it matches the 24h time
            int hour12 = SelectedTime.Value.Hours % 12;
            if (hour12 == 0) hour12 = 12;

            foreach (var button in hourButtons)
            {
                if ((int)button.Tag == hour12)
                {
                    button.Background = red;
                    break;
                }
            }
        }

        private ControlTemplate CreateRoundButtonTemplate()
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(20));
            borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(0));

            FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            borderFactory.AppendChild(contentPresenter);
            template.VisualTree = borderFactory;
            return template;
        }

        private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var clock = (AnalogClock)d;
            clock.UpdateClockHands();
            clock.HighlightSelectedButtons();
        }

        private void UpdateClockHands()
        {
            if (!SelectedTime.HasValue) return;

            double hourAngle = (SelectedTime.Value.Hours % 12 + SelectedTime.Value.Minutes / 60.0) * 30 * Math.PI / 180;
            double minuteAngle = SelectedTime.Value.Minutes * 6 * Math.PI / 180;

            HourHand.X2 = 120 + 40 * Math.Cos(hourAngle - Math.PI / 2);
            HourHand.Y2 = 120 + 40 * Math.Sin(hourAngle - Math.PI / 2);

            MinuteHand.X2 = 120 + 60 * Math.Cos(minuteAngle - Math.PI / 2);
            MinuteHand.Y2 = 120 + 60 * Math.Sin(minuteAngle - Math.PI / 2);
        }
    }
}
