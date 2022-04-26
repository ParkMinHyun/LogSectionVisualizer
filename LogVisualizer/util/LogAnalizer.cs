using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace LogVisualizer.util {
    internal class LogAnalizer {

        private ILogAnalizerCallback callback;
        private Dictionary<string, LogData> logDictionary;

        public LogAnalizer(ILogAnalizerCallback callback) {
            this.callback = callback;
        }

        public void Analyze(string logFile, List<JsonData.Filters> jsonFilters) {
            FileStream fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read);

            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8)) {
                string line;
                int sectionCount = 0;

                logDictionary = new Dictionary<string, LogData>();

                while ((line = streamReader.ReadLine()) != null) {
                    foreach (var data in jsonFilters) {
                        var sectionName = data.name;

                        // Todo : %d%d-%d%d regex 일 경우에만 수행
                        if (line.Contains(data.start) && isValidStartLog(sectionName)) {
                            logDictionary.Add(sectionName, new LogData(line));
                        } else if (line.Contains(data.end) && isValidEndLog(sectionName)) {
                            logDictionary[sectionName].endLog = line;
                            logDictionary[sectionName].calculateInterval();

                            int interval = (int) logDictionary[sectionName].interval;
                            callback.OnFilteredLogSection(sectionName, sectionCount++, interval);
                        }
                    }

                    if (sectionCount == jsonFilters.Count) {
                        callback.OnCompletedLogAnalysis(logFile, logDictionary);
                        return;
                    }
                }

                logDictionary.Clear();
                MessageBox.Show(logFile + "\n: Some log filters were not found.");
            }
        }

        private bool isValidStartLog(string sectionName) {
            return !logDictionary.ContainsKey(sectionName);
        }

        private bool isValidEndLog(string sectionName) {
            return logDictionary.ContainsKey(sectionName) && logDictionary[sectionName].endLog == null;
        }
    }

    interface ILogAnalizerCallback {

        void OnFilteredLogSection(string sectionName, int sectionCount, int interval);

        void OnCompletedLogAnalysis(string logFile, Dictionary<string, LogData> logDictionary);
    }
}
