﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogVisualizer.util {
    internal class LogAnalizer {

        private ILogAnalizerCallback callback;

        public LogAnalizer(ILogAnalizerCallback callback) {
            this.callback = callback;
        }
        public void Analyze(string logFile, List<JsonData.Filters> jsonFilters) {
            FileStream fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read);

            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8)) {
                string line;
                int sectionCount = 0;

                Dictionary<string, LogData> logDictionary = new Dictionary<string, LogData>();
                while ((line = streamReader.ReadLine()) != null) {
                    foreach (var data in jsonFilters) {
                        // Todo : %d%d-%d%d regex 일 경우에만 수행
                        if (line.Contains(data.start) && !logDictionary.ContainsKey(data.name)) {
                            logDictionary.Add(data.name, new LogData(line));
                        } else if (line.Contains(data.end) && logDictionary.ContainsKey(data.name)) {
                            logDictionary[data.name].endLog = line;
                            logDictionary[data.name].calculateInterval();

                            int interval = (int)logDictionary[data.name].interval;
                            callback.OnFilteredLogSection(data.name, sectionCount++, interval);
                        }
                    }

                    if (sectionCount == jsonFilters.Count) {
                        callback.OnCompletedLogAnalysis(logFile, logDictionary);
                        return;
                    }
                }
            }
        }
    }


    interface ILogAnalizerCallback {

        void OnFilteredLogSection(string sectionName, int sectionCount, int interval);

        void OnCompletedLogAnalysis(string logFile, Dictionary<string, LogData> logDictionary);
    }
}