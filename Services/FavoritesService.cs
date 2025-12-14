using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CryptoDashboard.Services
{
    public class FavoritesService
    {
        private readonly string _filePath = "favorites.json";

        public List<string> LoadFavorites()
        {
            if (!File.Exists(_filePath))
                return new List<string>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<string>>(json)
                   ?? new List<string>();
        }

        public void SaveFavorites(List<string> favorites)
        {
            var json = JsonSerializer.Serialize(favorites, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_filePath, json);
        }
    }
}
