using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using LogVisualizer.data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using static LogVisualizer.JsonData;
using LogVisualizer.view;
using System.Windows.Media.Effects;
using LogVisualizer.util;

namespace LogVisualizer {

    public partial class MainWindow : Window {

        private static string JSON_FORMAT_FILE_NAME = "JsonFormat.txt";
        private static string DEFAULT_JSON_FORMAT_FILE_NAME = @"sample.json";

        private AnalysisData analysisData;
        private BackgroundWorker worker;

        private string? logText;
        private int maxAxisYValue = 1000;

        public MainWindow() {
            InitializeComponent();

            LoadJson();
            InitView();
            InitBackgroundWorker();

            DataContext = this;
        }

        private void LoadJson() {
            var jsonString = JsonUtil.GetJsonString();

            if (!JsonUtil.CheckJsonFormat(jsonString)) {
                MessageBox.Show("Can't convert json. Please check json format again");
                this.Close();
            }

            analysisData = JsonConvert.DeserializeObject<AnalysisData>(jsonString);
        }

        private void InitView() {
            SeriesCollection = new SeriesCollection {};

            Labels = new string[analysisData.filters.Count];
            Formatter = value => value.ToString("n");

            MouseDown += Window_MouseDown;
        }

        private void InitBackgroundWorker() {
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(DoBackgroundWorker);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedBackgroundWorker);
        }

        void DoBackgroundWorker(object sender, DoWorkEventArgs e) {
            foreach (string logFile in e.Argument as string[]) {
                showLogFile(logFile);
            }
        }

        void CompletedBackgroundWorker(object sender, RunWorkerCompletedEventArgs e) {
            BusyIndicator.IsBusy = false;
        }

        private void DragDropLogFile(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) {
                return;
            }

            showLogFiles(e.Data.GetData(DataFormats.FileDrop) as string[]);
        }

        private void showLogFiles(string[]? logFileNames) {
            int totalNum = logFileNames.Length + SeriesCollection.Count;
            if (totalNum > 4) {
                MessageBox.Show("You can load log up to 4 files.", "Load Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BusyIndicator.IsBusy = true;
            worker.RunWorkerAsync(logFileNames);
        }

        private void showLogFile(string logFile) {
            FileStream fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read);

            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8)) {
                string line;
                int validCount = 0;

                Dictionary<string, LogData> logDictionary = new Dictionary<string, LogData>();

                while ((line = streamReader.ReadLine()) != null) {
                    foreach (var data in analysisData.filters) {
                        // Todo : %d%d-%d%d regex 일 경우에만 수행
                        if (line.Contains(data.start) && !logDictionary.ContainsKey(data.name)) {
                            logDictionary.Add(data.name, new LogData(line));
                        } else if (line.Contains(data.end) && logDictionary.ContainsKey(data.name)) {
                            Labels[validCount++] = data.name;
                            logDictionary[data.name].endLog = line;
                            logDictionary[data.name].calculateInterval();

                            int interval = (int)logDictionary[data.name].interval;
                            if (maxAxisYValue < interval) {
                                maxAxisYValue = interval;
                                this.Dispatcher.Invoke(() => { AxisY.MaxValue = interval; });
                            }
                        }
                    }

                    if (validCount == analysisData.filters.Count) {
                        this.Dispatcher.Invoke(() => { drawChart(logFile, logDictionary); });
                        return;
                    }
                }
            }
        }

        private void drawChart(string logFile, Dictionary<string, LogData> logDictionary) {
            var values = new List<double>();

            ClearLogPanel.Visibility = Visibility.Visible;
            ExtractLogPanel.Visibility = Visibility.Visible;

            logText += GetFileName(logFile) + "\n";
            foreach (var data in analysisData.filters) {
                logText += "[ " + data.name + " - " + logDictionary[data.name].interval + "ms ]\n";
                logText += logDictionary[data.name].startLog + "\n";
                logText += logDictionary[data.name].endLog + "\n\n";
                values.Add(logDictionary[data.name].interval);
            }

            SeriesCollection.Add(new ColumnSeries {
                Title = GetFileName(logFile),
                Values = new ChartValues<double>(values),
                Fill = ChartColor.colorList[SeriesCollection.Count]
            });
        }

        private static string GetFileName(string fileName) {
            string[] filePath = fileName.Split('\\');
            return filePath[^1];
        }


        public void JsonFormatChanged(string str) {
            analysisData = JsonConvert.DeserializeObject<AnalysisData>(str);
        }

        private void ClearLogFiles(object sender, RoutedEventArgs e) {
            logText = null;

            ClearLogPanel.Visibility = Visibility.Collapsed;
            ExtractLogPanel.Visibility = Visibility.Collapsed;

            SeriesCollection.Clear();
        }

        private void ExtractLogSection(object sender, RoutedEventArgs e) {
            Clipboard.SetText(logText);
        }

        private void ChangeJsonFilter(object sender, RoutedEventArgs e) {
            this.Effect = new BlurEffect();
            JsonFilterDialog jsonFilterDialog= new JsonFilterDialog(sender as DependencyObject);
            jsonFilterDialog.ShowDialog();
        }

        private void OpenLogFile(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 3;
            openFileDialog.Multiselect = true;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true) {
                showLogFiles(openFileDialog.FileNames);
            }
        }

        private void DragOverView(object sender, DragEventArgs e) {
            e.Handled = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton != MouseButton.Left) {
                return;
            }

            this.DragMove();
        }

        private void MinimizeButtonClicked(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
    }
}
