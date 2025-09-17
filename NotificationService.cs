using System.Collections.ObjectModel;
using System.Windows;
using TodoAppWpf.Views;

namespace TodoAppWpf
{
    public static class NotificationService
    {
        public static void ShowTaskNotification(string title, string message, ToastType type = ToastType.INFO)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var toast = new ToastNotification(title, message, type);
                toast.Show();
            });
        }

        public static void CheckReminders(ObservableCollection<TodoItem> todoItems)
        {
            var now = DateTime.Now;

            foreach (var item in todoItems.Where(i => i.HasReminder && i.ReminderDate <= now && i.Status != "Done"))
            {
                ShowTaskNotification("Reminder", $"Task '{item.Title}' is due now!", ToastType.WARNING);

                // Clear the reminder after showing it
                item.ReminderDate = null;
            }
        }

        public static void ScheduleReminder(TodoItem item)
        {
            if (item.HasReminder && item.ReminderDate > DateTime.Now)
            {
                ShowTaskNotification("Reminder Set", $"Reminder set for task '{item.Title}'", ToastType.INFO);
            }
        }
    }
}