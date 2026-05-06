using JmesPathWpfDemo.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace JmesPathWpfDemo.Services
{
    public static class ClientTimeZoneSettingsService
    {
        private const string SettingsFileName = "client-timezone-settings.json";
        private static readonly object SyncRoot = new object();
        private static ClientTimeZoneSettings _current;

        public static ClientTimeZoneSettings Current
        {
            get
            {
                lock (SyncRoot)
                {
                    _current ??= LoadInternal();
                    return _current.Clone();
                }
            }
        }

        public static void Save(ClientTimeZoneSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            lock (SyncRoot)
            {
                var normalized = Normalize(settings);
                var path = GetSettingsFilePath();
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, JsonConvert.SerializeObject(normalized, Formatting.Indented));
                _current = normalized;
            }
        }

        private static ClientTimeZoneSettings LoadInternal()
        {
            try
            {
                var path = GetSettingsFilePath();
                if (!File.Exists(path))
                {
                    return ClientTimeZoneSettings.Default;
                }

                var json = File.ReadAllText(path);
                var settings = JsonConvert.DeserializeObject<ClientTimeZoneSettings>(json);
                return Normalize(settings ?? ClientTimeZoneSettings.Default);
            }
            catch
            {
                return ClientTimeZoneSettings.Default;
            }
        }

        private static ClientTimeZoneSettings Normalize(ClientTimeZoneSettings settings)
        {
            var normalized = settings.Clone();
            if (string.IsNullOrWhiteSpace(normalized.TimeZoneId))
            {
                normalized.TimeZoneId = ClientTimeZoneSettings.Default.TimeZoneId;
            }

            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(normalized.TimeZoneId);
            }
            catch
            {
                normalized.TimeZoneId = ClientTimeZoneSettings.Default.TimeZoneId;
            }

            return normalized;
        }

        private static string GetSettingsFilePath()
        {
            var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JmesPathWpfDemo");
            return Path.Combine(root, SettingsFileName);
        }
    }
}