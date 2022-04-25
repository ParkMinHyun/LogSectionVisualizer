using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static LogVisualizer.JsonData;

namespace LogVisualizer.util {
    internal class JsonUtil {

        private static string JSON_FORMAT_FILE_NAME = "JsonFormat.txt";
        private static string DEFAULT_JSON_FORMAT_FILE_NAME = @"sample.json";

        public static bool CheckJsonFormat(String jsonString) {
            try {
                JsonConvert.DeserializeObject<AnalysisData>(jsonString);
            } catch {
                return false;
            }

            return true;
        }

        public static string GetJsonString() {
            string applicationPath = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            string jsonFilePath = Path.Combine(applicationPath, JSON_FORMAT_FILE_NAME);
            string sampleJsonText;

            if (File.Exists(jsonFilePath)) {
                sampleJsonText = File.ReadAllText(jsonFilePath);
            } else {
                using (StreamReader reader = new StreamReader(DEFAULT_JSON_FORMAT_FILE_NAME)) {
                    sampleJsonText = reader.ReadToEnd();
                }
            }

            return sampleJsonText;
        }

        public static void SaveJsonString(String jsonString) {
            using (StreamWriter file = new StreamWriter(DEFAULT_JSON_FORMAT_FILE_NAME)) {
                file.Write(jsonString);
            }
        }
    }
}
