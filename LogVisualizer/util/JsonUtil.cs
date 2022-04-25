using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using static LogVisualizer.JsonData;

namespace LogVisualizer.util {
    internal class JsonUtil {
        private const string JSON_FORMAT = "jsonFormat";
        private const string SAMPLE_JSON_FORMAT_FILE_NAME = @"sample.json";

        public static bool CheckJsonFormat(String jsonString) {
            try {
                JsonConvert.DeserializeObject<AnalysisData>(jsonString);
            } catch {
                return false;
            }

            return true;
        }

        public static string GetJsonString() {
            string jsonText = LoadJsonString();

            if (jsonText != null) {
                return jsonText;
            }

            using (StreamReader reader = new StreamReader(SAMPLE_JSON_FORMAT_FILE_NAME)) {
                return reader.ReadToEnd();
            }
        }

        public static string LoadJsonString() => ConfigurationManager.AppSettings[JSON_FORMAT];

        public static void SaveJsonString(String jsonString) {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (configuration.AppSettings.Settings[JSON_FORMAT] == null) {
                configuration.AppSettings.Settings.Add(JSON_FORMAT, jsonString);
            } else {
                configuration.AppSettings.Settings[JSON_FORMAT].Value = jsonString;
            }

            configuration.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
