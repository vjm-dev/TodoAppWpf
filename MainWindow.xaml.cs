using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        public MainWindow()
        {
            InitializeComponent();

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
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                ShowStatusMessage("Title cannot be empty!", Brushes.Red);
                return;
            }

            var status = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString();

            if (editId == -1)
            {
                // Add new item
                var newItem = new TodoItem
                {
                    Id = GetNextId(),
                    Title = txtTitle.Text,
                    Description = txtDescription.Text,
                    Status = status!
                };
                newItem.UpdateStatusColor();

                TodoItems.Add(newItem);
                ShowStatusMessage("Task added successfully!", Brushes.Green);
            }
            else
            {
                // Update existing item
                var existingItem = TodoItems.FirstOrDefault(item => item.Id == editId);
                if (existingItem != null)
                {
                    existingItem.Title = txtTitle.Text;
                    existingItem.Description = txtDescription.Text;
                    existingItem.Status = status!;
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
                ShowStatusMessage("Task updated successfully!", Brushes.Green);
            }

            SaveTodos(); // Save after adding/modifying
            ClearForm();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (lstTasks.SelectedItem is TodoItem selectedItem)
            {
                txtTitle.Text = selectedItem.Title;
                txtDescription.Text = selectedItem.Description;

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
                txtStatusMessage.Text = $"Editing task: {selectedItem.Title}";
            }
        }

        private void btnMarkComplete_Click(object sender, RoutedEventArgs e)
        {
            if (lstTasks.SelectedItem is TodoItem selectedItem)
            {
                selectedItem.Status = "Done";
                selectedItem.UpdateStatusColor();

                // Notify UI that the item has changed
                selectedItem.OnPropertyChanged(nameof(selectedItem.Status));
                selectedItem.OnPropertyChanged(nameof(selectedItem.StatusColor));
                selectedItem.OnPropertyChanged(nameof(selectedItem.ModifiedDate));

                ShowStatusMessage($"Task '{selectedItem.Title}' marked as done!", Brushes.Green);
                SaveTodos(); // Save after marked as done
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is int id)
            {
                var itemToRemove = TodoItems.FirstOrDefault(item => item.Id == id);
                if (itemToRemove != null)
                {
                    TodoItems.Remove(itemToRemove);
                    ShowStatusMessage("Task deleted successfully!", Brushes.Green);
                    SaveTodos(); // Save after deletion
                    ClearForm();
                }
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

        private void ApplyFilterAndSort()
        {
            if (TodoItems == null) return;

            // Apply filter
            IEnumerable<TodoItem> filtered = currentFilter == "All"
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

        private void ClearForm()
        {
            txtTitle.Clear();
            txtDescription.Clear();
            cmbStatus.SelectedIndex = 0;
            lstTasks.SelectedIndex = -1;
        }

        private void ShowStatusMessage(string message, Brush color)
        {
            txtStatusMessage.Foreground = color;
            txtStatusMessage.Text = message;
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
            SaveTodos();
            base.OnClosed(e);
        }
    }
}
