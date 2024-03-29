﻿using LiveCharts;
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
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;

namespace LogVisualizer {

    public partial class MainWindow : Window, ILogAnalizerCallback {

        private AnalysisData analysisData;
        private List<string> analysislogFiles;

        private LogAnalizer logAnalizer;
        private BackgroundWorker worker;

        private string? logText;

        private int maxAxisYValue = 1000;

        public MainWindow() {
            InitializeComponent();

            LoadJson();
            InitView();
            InitLogAnalsisData();

            DataContext = this;
        }

        private void LoadJson() {
            var jsonString = JsonUtil.LoadJsonString();

            if (!JsonUtil.CheckJsonFormat(jsonString)) {
                MessageBox.Show("Can't convert json. Please check json format again");
                this.Close();
            }

            analysisData = JsonConvert.DeserializeObject<AnalysisData>(jsonString);
        }

        private void InitView() {
            SeriesCollection = new SeriesCollection { };

            AxisX.Labels = new List<string>();

            Formatter = value => value.ToString("n");

            MouseDown += Window_MouseDown;
        }

        private void InitLogAnalsisData() {
            logAnalizer = new LogAnalizer(this);
            analysislogFiles = new List<string>();

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(DoBackgroundWorker);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompletedBackgroundWorker);
        }

        void DoBackgroundWorker(object sender, DoWorkEventArgs e) {
            foreach (string logFile in e.Argument as List<string>) {
                logAnalizer.Analyze(logFile, analysisData.filters);
            }
        }

        void CompletedBackgroundWorker(object sender, RunWorkerCompletedEventArgs e) {
            BusyIndicator.IsBusy = false;

            if (SeriesCollection.Count == 0) {
                InitGuideBox.Visibility = Visibility.Visible;
            }
        }

        private void DragDropLogFile(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) {
                return;
            }

            string[] droppedLogFiles = e.Data.GetData(DataFormats.FileDrop) as string[];

            AnalyzeLogFiles(droppedLogFiles.ToList());
        }

        private void AnalyzeLogFiles(List<string>? logFileNames) {
            int currentLogFileCount = logFileNames.Count + SeriesCollection.Count;
            if (currentLogFileCount > 4) {
                MessageBox.Show("You can load log up to 4 files.", "Load Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            analysislogFiles.AddRange(logFileNames);

            BusyIndicator.IsBusy = true;
            InitGuideBox.Visibility = Visibility.Collapsed;

            worker.RunWorkerAsync(logFileNames);
        }


        public void JsonFormatChanged(string str) {
            logText = null;
            maxAxisYValue = 1000;
            AxisY.MaxValue = maxAxisYValue;
            SeriesCollection.Clear();

            AxisX.Labels = new List<string>();
            analysisData = JsonConvert.DeserializeObject<AnalysisData>(str);


            BusyIndicator.IsBusy = true;
            worker.RunWorkerAsync(analysislogFiles);
        }

        private void TakeScreenShot(object sender, RoutedEventArgs e) {
            UIElement source = BusyIndicator.Content as UIElement;
            var dirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Uri path = new Uri(dirPath + @"\screenShot.png");

            try {
                double Height, renderHeight, Width, renderWidth;

                Height = renderHeight = source.RenderSize.Height;
                Width = renderWidth = source.RenderSize.Width;

                //Specification for target bitmap like width/height pixel etc.
                RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
                //creates Visual Brush of UIElement
                VisualBrush visualBrush = new VisualBrush(source);

                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
                    //draws image of element
                    drawingContext.DrawRectangle(visualBrush, null, new Rect(new Point(0, 0), new Point(Width, Height)));
                }
                //renders image
                renderTarget.Render(drawingVisual);

                //PNG encoder for creating PNG file
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTarget));
                using (FileStream stream = new FileStream(path.LocalPath, FileMode.Create, FileAccess.Write)) {
                    encoder.Save(stream);
                }

                MessageBox.Show("screenshot saved in " + path);
            } catch (Exception exception) {
                MessageBox.Show(exception.ToString());
            }
        }

        private void ClearLogFiles(object sender, RoutedEventArgs e) {
            logText = null;
            maxAxisYValue = 1000;

            InitGuideBox.Visibility = Visibility.Visible;
            ClearLogPanel.Visibility = Visibility.Collapsed;
            ScreenShotPanel.Visibility = Visibility.Collapsed;
            ExtractLogPanel.Visibility = Visibility.Collapsed;

            analysislogFiles.Clear();
            SeriesCollection.Clear();
        }

        private void ExtractLogSection(object sender, RoutedEventArgs e) => Clipboard.SetText(logText);

        private void ChangeJsonFilter(object sender, RoutedEventArgs e) {
            this.Effect = new BlurEffect();
            JsonFilterDialog jsonFilterDialog = new JsonFilterDialog(sender as DependencyObject);
            jsonFilterDialog.ShowDialog();
        }

        private void OpenLogFile(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 3;
            openFileDialog.Multiselect = true;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true) {
                List<string> openLogFiles = openFileDialog.FileNames.ToList();

                AnalyzeLogFiles(openLogFiles);
            }
        }

        public void OnFilteredLogSection(string sectionName, int sectionCount, int interval) {
            if (!AxisX.Labels.Contains(sectionName)) {
                AxisX.Labels.Add(sectionName);
            }

            if (maxAxisYValue < interval) {
                maxAxisYValue = interval;
                this.Dispatcher.Invoke(() => { AxisY.MaxValue = interval; });
            }
        }

        public void OnCompletedLogAnalysis(string logFile, Dictionary<string, LogData> logDictionary) {
            var logSectionIntervals = new List<double>();

            logText += GetFileName(logFile) + "\n";
            foreach (var label in AxisX.Labels) {
                var logSection = logDictionary[label];

                logText += "[ " + label + " - " + logSection.interval + "ms ]\n";
                logText += logSection.startLog + "\n";
                logText += logSection.endLog + "\n\n";
                logSectionIntervals.Add(logSection.interval);
            }

            DrawChart(logFile, logSectionIntervals);
        }

        public void OnFailedLogAnalysis(string fileName) {
            analysislogFiles.Remove(fileName);
            MessageBox.Show(fileName + "\n: Some log filters were not found.");
        }

        private void DrawChart(string logFile, List<double> logSectionIntervals) {
            this.Dispatcher.Invoke(() => {
                ClearLogPanel.Visibility = Visibility.Visible;
                ScreenShotPanel.Visibility = Visibility.Visible;
                ExtractLogPanel.Visibility = Visibility.Visible;

                SeriesCollection.Add(new ColumnSeries {
                    Title = GetFileName(logFile),
                    DataLabels = true,
                    LabelPoint = point => point.Y + "ms",
                    Values = new ChartValues<double>(logSectionIntervals),
                    Fill = ChartColor.colorList[SeriesCollection.Count]
                });
            });
        }

        private static string GetFileName(string fileName) => fileName.Split('\\')[^1];

        private void DragOverView(object sender, DragEventArgs e) => e.Handled = true;

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton != MouseButton.Left) {
                return;
            }

            try { this.DragMove(); } catch { }
        }

        private void InitGuideBox_MouseEnter(object sender, MouseEventArgs e) {
            InitGuideBox.Source = new BitmapImage(new Uri("pack://application:,,,/icon/init_mouse_over.png"));
            InitGuideBox.Margin = new Thickness(124);
        }

        private void InitGuideBox_MouseLeave(object sender, MouseEventArgs e) {
            InitGuideBox.Source = new BitmapImage(new Uri("pack://application:,,,/icon/init.png"));
            InitGuideBox.Margin = new Thickness(125);
        }

        private void MinimizeButtonClicked(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private void CloseButtonClicked(object sender, RoutedEventArgs e) => this.Close();

        public SeriesCollection SeriesCollection { get; set; }

        public Func<double, string> Formatter { get; set; }
    }
}
