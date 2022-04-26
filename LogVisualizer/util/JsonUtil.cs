using LogVisualizer.data;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using static LogVisualizer.JsonData;

namespace LogVisualizer.util {
    internal class JsonUtil {
        private const string JSON_FORMAT = "jsonFormat";

        public static bool CheckJsonFormat(String jsonString) {
            try {
                JsonConvert.DeserializeObject<AnalysisData>(jsonString);
                return true;
            } catch {
                return false;
            }
        }

        public static string LoadJsonString() {
            string jsonText = GetAppJsonString();

            if (jsonText != null) {
                return jsonText;
            }

            return SampleData.json;
        }

        public static void SaveJsonString(String jsonString) => SetAppJsonString(jsonString);

        private static string GetAppJsonString() => ConfigurationManager.AppSettings[JSON_FORMAT];

        private static void SetAppJsonString(String jsonString) {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            AppSettingsSection appSettingsSections = configuration.AppSettings;

            if (appSettingsSections.Settings[JSON_FORMAT] == null) {
                appSettingsSections.Settings.Add(JSON_FORMAT, jsonString);
            } else {
                appSettingsSections.Settings[JSON_FORMAT].Value = jsonString;
            }

            configuration.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
