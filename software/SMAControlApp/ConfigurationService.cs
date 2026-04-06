using SMAControlApp.Models;
using System;
using System.IO;
using System.Text.Json;

namespace SMAControlApp
{
    public static class ConfigurationService
    {
        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true
        };

        public static void Save(Configuration config)
        {
            string json = JsonSerializer.Serialize(config, Options);
            File.WriteAllText(FilePath, json);
        }

        public static Configuration Load()
        {
            if (!File.Exists(FilePath))
                return new Configuration();

            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<Configuration>(json, Options)
                   ?? new Configuration();
        }

        public static bool Exists() => File.Exists(FilePath);
    }
}