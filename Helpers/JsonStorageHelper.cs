using System.IO;
using System.Text.Json;

namespace CryptoDashboard.Helpers
{
    public static class JsonStorageHelper
    {
        public static void Save<T>(string filePath, T data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, json);
        }

        public static T? Load<T>(string filePath)
        {
            if (!File.Exists(filePath))
                return default;

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
