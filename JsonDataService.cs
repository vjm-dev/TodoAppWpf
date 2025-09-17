using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace TodoAppWpf
{
    public class JsonDataService
    {
        private readonly string _filePath;

        public JsonDataService()
        {
            // Save in C:\User\username\MyDocuments\TodoApp directory
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _filePath = Path.Combine(documentsPath, "TodoApp", "todos.json");

            // Make sure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        }

        public List<TodoItem> LoadTodos()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new List<TodoItem>();

                string json = File.ReadAllText(_filePath);
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonBrushConverter() },
                    WriteIndented = true,
                    PropertyNameCaseInsensitive = true
                };

                var items = JsonSerializer.Deserialize<List<TodoItem>>(json, options) ?? new List<TodoItem>();
                foreach (var item in items)
                {
                    item.UpdateStatusColor();
                }

                return items;
            }
            catch (Exception ex)
            {
                // Return empty list
                Console.WriteLine($"Error loading todos: {ex.Message}");
                return new List<TodoItem>();
            }
        }

        public void SaveTodos(IEnumerable<TodoItem> todos)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonBrushConverter() },
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(todos, options);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving todos: {ex.Message}");
            }
        }
    }

    public class JsonBrushConverter : JsonConverter<Brush>
    {
        public override Brush Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string colorString = reader.GetString()!;
            if (string.IsNullOrEmpty(colorString))
                return Brushes.Gray;

            try
            {
                var converter = new System.Windows.Media.BrushConverter();
                return (Brush)converter.ConvertFromString(colorString)!;
            }
            catch
            {
                return Brushes.Gray;
            }
        }

        public override void Write(Utf8JsonWriter writer, Brush value, JsonSerializerOptions options)
        {
            if (value is SolidColorBrush solidColorBrush)
            {
                writer.WriteStringValue(solidColorBrush.Color.ToString());
            }
            else
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}