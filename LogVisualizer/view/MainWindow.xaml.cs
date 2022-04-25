using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using LogVisualizer.data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using static LogVisualizer.JsonData;
using LogVisualizer.view;
using System.Windows.Media.Effects;
using LogVisualizer.util;

namespace LogVisualizer {

    public partial class MainWindow : Window, ILogAnalizerCallback {

        private AnalysisData analysisData;
        private LogAnalizer logAnalizer;
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
            logAnalizer = new LogAnalizer(this);

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(DoBackgroundWorker);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedBackgroundWorker);
        }

        void DoBackgroundWorker(object sender, DoWorkEventArgs e) {
            foreach (string logFile in e.Argument as string[]) {
                logAnalizer.Analyze(logFile, analysisData.filters);
            }
        }

        void CompletedBackgroundWorker(object sender, RunWorkerCompletedEventArgs e) {
            BusyIndicator.IsBusy = false;
        }

        private void DragDropLogFile(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) {
                return;
            }

            AnalyzeLogFiles(e.Data.GetData(DataFormats.FileDrop) as string[]);
        }

        private void AnalyzeLogFiles(string[]? logFileNames) {
            int totalNum = logFileNames.Length + SeriesCollection.Count;
            if (totalNum > 4) {
                MessageBox.Show("You can load log up to 4 files.", "Load Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BusyIndicator.IsBusy = true;
            worker.RunWorkerAsync(logFileNames);
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
                AnalyzeLogFiles(openFileDialog.FileNames);
            }
        }

        public void OnFilteredLogSection(string sectionName, int sectionCount, int interval) {
            Labels[sectionCount] = sectionName;

            if (maxAxisYValue < interval) {
                maxAxisYValue = interval;
                this.Dispatcher.Invoke(() => { AxisY.MaxValue = interval; });
            }
        }

        public void OnCompletedLogAnalysis(string logFile, Dictionary<string, LogData> logDictionary) {
            var values = new List<double>();

            logText += GetFileName(logFile) + "\n";
            foreach (var data in analysisData.filters) {
                logText += "[ " + data.name + " - " + logDictionary[data.name].interval + "ms ]\n";
                logText += logDictionary[data.name].startLog + "\n";
                logText += logDictionary[data.name].endLog + "\n\n";
                values.Add(logDictionary[data.name].interval);
            }

            this.Dispatcher.Invoke(() => { drawChart(logFile, values); });
        }

        private void drawChart(string logFile, List<double> logValues) {
            ClearLogPanel.Visibility = Visibility.Visible;
            ExtractLogPanel.Visibility = Visibility.Visible;

            SeriesCollection.Add(new ColumnSeries {
                Title = GetFileName(logFile),
                Values = new ChartValues<double>(logValues),
                Fill = ChartColor.colorList[SeriesCollection.Count]
            });
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
