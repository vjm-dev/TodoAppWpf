using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TodoAppWpf.Views;

namespace TodoAppWpf
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<TodoItem> TodoItems { get; set; }
        public ObservableCollection<TodoItem> FilteredTodoItems { get; set; }
        private int editId = -1;
        private readonly JsonDataService _dataService;
        private string currentFilter = "All";
        private string currentSort = "Creation Date (Newest)";
        private DispatcherTimer? reminderTimer;
        private DispatcherTimer? statusMessageTimer;

        // Validation constants
        private const int MAX_TITLE_LENGTH = 100;
        private const int MAX_DESCRIPTION_LENGTH = 750;

        public MainWindow()
        {
            InitializeComponent();

            statusMessageTimer = new DispatcherTimer();

            _dataService = new JsonDataService();

            var loadedTodos = _dataService.LoadTodos();
            TodoItems = loadedTodos != null
                ? new ObservableCollection<TodoItem>(loadedTodos)
                : new ObservableCollection<TodoItem>();

            FilteredTodoItems = new ObservableCollection<TodoItem>();

            // Make sure all items have updated their status color
            foreach (var item in TodoItems)
            {
                item.UpdateStatusColor();
            }

            ApplyFilterAndSort();
            lstTasks.ItemsSource = FilteredTodoItems;

            TodoItems.CollectionChanged += (s, e) =>
            {
                ApplyFilterAndSort();
                SaveTodos();
            };

            // Initialize and start reminder timer
            InitializeReminderTimer();

            // Initial validation check
            ValidateForm();

            // Check for any pending reminders on startup
            NotificationService.CheckReminders(TodoItems);
        }

        private void InitializeReminderTimer()
        {
            reminderTimer = new DispatcherTimer();
            reminderTimer.Interval = TimeSpan.FromMinutes(1); // Check every minute
            reminderTimer.Tick += ReminderTimer_Tick!;
            reminderTimer.Start();
        }

        private void ReminderTimer_Tick(object sender, EventArgs e)
        {
            NotificationService.CheckReminders(TodoItems);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var title = txtTitle.Text.Trim();
                if (string.IsNullOrWhiteSpace(title))
                {
                    ShowErrorMessage(txtTitleError, "Title is required.");
                    return;
                }

                var desc = txtDescription.Text.Trim();
                if (string.IsNullOrWhiteSpace(desc))
                {
                    ShowErrorMessage(txtDescriptionError, "Description is required.");
                    return;
                }

                if (desc.Length < 10)
                {
                    ShowErrorMessage(txtDescriptionError, "Cannot be less than 10 characters.");
                    return;
                }

                if (!ValidateForm())
                {
                    NotificationService.ShowTaskNotification("ERROR", "Please fix validation errors before adding a task.", ToastType.ERROR);
                    return;
                }

                var reminderDate = dpReminderDate.SelectedDate;
                if (reminderDate.HasValue && cmbReminderTime.SelectedItem is ComboBoxItem timeItem)
                {
                    var timeStr = timeItem.Content.ToString();
                    if (TimeSpan.TryParse(timeStr, out var time))
                    {
                        reminderDate = reminderDate.Value.Add(time);
                    }
                }

                var status = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString();

                if (editId == -1)
                {
                    // Check for duplicate titles (case-insensitive)
                    if (TodoItems.Any(item =>
                        item.Title.Equals(txtTitle.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        NotificationService.ShowTaskNotification("ERROR", "A task with this title already exists!", ToastType.ERROR);
                        return;
                    }

                    // Add new item
                    var newItem = new TodoItem
                    {
                        Id = GetNextId(),
                        Title = txtTitle.Text.Trim(),
                        Description = txtDescription.Text.Trim(),
                        Status = status!,
                        ReminderDate = reminderDate
                    };
                    newItem.UpdateStatusColor();

                    TodoItems.Add(newItem);

                    // Show notification
                    NotificationService.ShowTaskNotification("TASK ADDED", $"Task '{newItem.Title}' has been added successfully!", ToastType.SUCCESS);
                }
                else
                {
                    // Update existing item
                    var existingItem = TodoItems.FirstOrDefault(item => item.Id == editId);
                    if (existingItem != null)
                    {
                        // Check for duplicate titles excluding the current item
                        if (TodoItems.Any(item =>
                            item.Id != editId &&
                            item.Title.Equals(txtTitle.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
                        {
                            NotificationService.ShowTaskNotification("ERROR", "A task with this title already exists!", ToastType.ERROR);
                            return;
                        }

                        existingItem.Title = txtTitle.Text.Trim();
                        existingItem.Description = txtDescription.Text.Trim();
                        existingItem.Status = status!;
                        existingItem.ReminderDate = reminderDate;
                        existingItem.UpdateStatusColor();
                        existingItem.UpdateModifiedDate();

                        // Notify UI that the item has changed
                        existingItem.OnPropertyChanged(nameof(existingItem.Title));
                        existingItem.OnPropertyChanged(nameof(existingItem.Description));
                        existingItem.OnPropertyChanged(nameof(existingItem.Status));
                        existingItem.OnPropertyChanged(nameof(existingItem.StatusColor));
                    }

                    editId = -1;
                    btnAdd.Content = "Add";
                    NotificationService.ShowTaskNotification("TASK UPDATED", "Task updated successfully!", ToastType.SUCCESS);
                }

                SaveTodos();
                ClearForm();
            }
            catch (Exception ex)
            {
                NotificationService.ShowTaskNotification("ERROR", $"Error: {ex.Message}", ToastType.ERROR);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstTasks.SelectedItem is TodoItem selectedItem)
                {
                    txtTitle.Text = selectedItem.Title;
                    txtDescription.Text = selectedItem.Description;
                    dpReminderDate.SelectedDate = selectedItem.ReminderDate;

                    // Set the status in the combobox
                    for (int i = 0; i < cmbStatus.Items.Count; i++)
                    {
                        var item = (ComboBoxItem)cmbStatus.Items[i];
                        if (item.Content.ToString() == selectedItem.Status)
                        {
                            cmbStatus.SelectedIndex = i;
                            break;
                        }
                    }

                    editId = selectedItem.Id;
                    btnAdd.Content = "Save";

                    // Validate the form when editing
                    ValidateForm();
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowTaskNotification("ERROR", $"Error: {ex.Message}", ToastType.ERROR);
            }
        }

        private void btnMarkComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstTasks.SelectedItem is TodoItem selectedItem)
                {
                    selectedItem.Status = "Done";
                    selectedItem.UpdateStatusColor();

                    // Notify UI that the item has changed
                    selectedItem.OnPropertyChanged(nameof(selectedItem.Status));
                    selectedItem.OnPropertyChanged(nameof(selectedItem.StatusColor));
                    selectedItem.OnPropertyChanged(nameof(selectedItem.ModifiedDate));

                    // Show notification
                    NotificationService.ShowTaskNotification("TASK COMPLETED", $"Task '{selectedItem.Title}' has been marked as done!", ToastType.SUCCESS);
                    SaveTodos(); // Save after marked as done
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowTaskNotification("ERROR", $"Error: {ex.Message}", ToastType.ERROR);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (((Button)sender).Tag is int id)
                {
                    var itemToRemove = TodoItems.FirstOrDefault(item => item.Id == id);
                    if (itemToRemove != null)
                    {
                        TodoItems.Remove(itemToRemove);
                        NotificationService.ShowTaskNotification("TASK DELETED", $"Task '{itemToRemove.Title}' has been deleted!", ToastType.SUCCESS);
                        SaveTodos(); // Save after deletion
                        ClearForm();
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.ShowTaskNotification("ERROR", $"Error: {ex.Message}", ToastType.ERROR);
            }
        }

        private void lstTasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isItemSelected = lstTasks.SelectedItem != null;
            btnUpdate.IsEnabled = isItemSelected;
            btnMarkComplete.IsEnabled = isItemSelected;
        }

        private void cmbFilterStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFilterStatus.SelectedItem is ComboBoxItem selectedItem)
            {
                currentFilter = selectedItem.Content.ToString()!;
                ApplyFilterAndSort();
            }
        }

        private void cmbSortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSortBy.SelectedItem is ComboBoxItem selectedItem)
            {
                currentSort = selectedItem.Content.ToString()!;
                ApplyFilterAndSort();
            }
        }

        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            cmbFilterStatus.SelectedIndex = 0;
            cmbSortBy.SelectedIndex = 0;
            ApplyFilterAndSort();
        }

        private void txtTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateTitle();
            ValidateForm();
        }

        private void txtDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateDescription();
            ValidateForm();
        }

        private void btnClearReminder_Click(object sender, RoutedEventArgs e)
        {
            dpReminderDate.SelectedDate = null;
        }

        private void ReminderDate_Changed(object sender, SelectionChangedEventArgs e)
        {
            // Optional: Add any validation or feedback for reminder date changes
        }

        private void ApplyFilterAndSort()
        {
            if (TodoItems == null) return;

            // Apply filter
            var filtered = currentFilter == "All"
                ? TodoItems
                : TodoItems.Where(item => item.Status == currentFilter);

            if (filtered == null)
                filtered = Enumerable.Empty<TodoItem>();

            // Apply sorting
            switch (currentSort)
            {
                case "Creation Date (Newest)":
                    filtered = filtered.OrderByDescending(item => item.CreatedDate);
                    break;
                case "Creation Date (Oldest)":
                    filtered = filtered.OrderBy(item => item.CreatedDate);
                    break;
                case "Modification Date (Newest)":
                    filtered = filtered.OrderByDescending(item => item.ModifiedDate);
                    break;
                case "Modification Date (Oldest)":
                    filtered = filtered.OrderBy(item => item.ModifiedDate);
                    break;
                case "Title (A-Z)":
                    filtered = filtered.OrderBy(item => item.Title);
                    break;
                case "Title (Z-A)":
                    filtered = filtered.OrderByDescending(item => item.Title);
                    break;
                case "Status":
                    filtered = filtered.OrderBy(item => item.Status);
                    break;
            }

            // Update the filtered collection
            FilteredTodoItems.Clear();
            foreach (var item in filtered)
            {
                FilteredTodoItems.Add(item);
            }
        }

        private bool ValidateTitle()
        {
            var title = txtTitle.Text.Trim();

            if (title.Length > MAX_TITLE_LENGTH)
            {
                ShowErrorMessage(txtTitleError, $"Title cannot exceed {MAX_TITLE_LENGTH} characters.");
                return false;
            }

            // Check for invalid characters
            if (Regex.IsMatch(title, @"[<>""&'\\]"))
            {
                ShowErrorMessage(txtTitleError, "Title contains invalid characters.");
                return false;
            }

            ClearErrorMessage(txtTitleError);
            return true;
        }

        private bool ValidateDescription()
        {
            var description = txtDescription.Text;

            if (description.Length > MAX_DESCRIPTION_LENGTH)
            {
                ShowErrorMessage(txtDescriptionError, $"Description cannot exceed {MAX_DESCRIPTION_LENGTH} characters.");
                return false;
            }

            // Check for invalid characters
            if (Regex.IsMatch(description, @"[<>""&'\\]"))
            {
                ShowErrorMessage(txtDescriptionError, "Description contains invalid characters.");
                return false;
            }

            ClearErrorMessage(txtDescriptionError);
            return true;
        }

        private bool ValidateForm()
        {
            bool isTitleValid = ValidateTitle();
            bool isDescriptionValid = ValidateDescription();

            // Enable/disable add button based on validation
            btnAdd.IsEnabled = isTitleValid && isDescriptionValid;

            return isTitleValid && isDescriptionValid;
        }

        private void ShowErrorMessage(TextBlock errorControl, string message)
        {
            errorControl.Text = message;
            errorControl.Visibility = Visibility.Visible;
        }

        private void ClearErrorMessage(TextBlock errorControl)
        {
            errorControl.Text = string.Empty;
            errorControl.Visibility = Visibility.Collapsed;
        }

        private void ClearForm()
        {
            txtTitle.Clear();
            txtDescription.Clear();
            cmbStatus.SelectedIndex = 0;
            dpReminderDate.SelectedDate = null;
            lstTasks.SelectedIndex = -1;
            ClearErrorMessage(txtTitleError);
            ClearErrorMessage(txtDescriptionError);
            btnAdd.Content = "Add";
            editId = -1;
            ValidateForm();
            statusMessageTimer?.Stop();
        }

        private int GetNextId()
        {
            return TodoItems.Any() ? TodoItems.Max(item => item.Id) + 1 : 1;
        }

        private void SaveTodos()
        {
            _dataService.SaveTodos(TodoItems);
        }

        // Save when closed
        protected override void OnClosed(EventArgs e)
        {
            // Stop the timer when the window closes
            reminderTimer?.Stop();
            SaveTodos();
            base.OnClosed(e);
        }
    }
}
