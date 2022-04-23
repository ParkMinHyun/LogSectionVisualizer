using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace LogVisualizer.view {
    /// <summary>
    /// JsonFilterDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class JsonFilterDialog : Window {

        private static string JSON_FORMAT_FILE_NAME = "JsonFormat.txt";
        private static string DEFAULT_JSON_FORMAT_FILE_NAME = @"sample.json";

        private MainWindow parentWindow;

        public JsonFilterDialog() {
            InitializeComponent();

            InitView();
            LoadJson();
        }

        private void InitView() {
            MouseDown += Window_MouseDown;
            Closing += OnWindowClosing;
        }

        private void LoadJson() {
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

            JsonFormatView.Text = sampleJsonText;
        }

        public JsonFilterDialog(DependencyObject depObject) : this() {
            Owner = Window.GetWindow(depObject);
            parentWindow = Owner as MainWindow;
        }

        private void ChangeButtonClicked(object sender, RoutedEventArgs e) {
            parentWindow.JsonFormatChanged(JsonFormatView.Text);
            // analysisData = JsonConvert.DeserializeObject<AnalysisData>(sampleJsonText);

            this.Close();
        }
        private void OnWindowClosing(object sender, CancelEventArgs e) {
            parentWindow.Effect = null;
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton != MouseButton.Left) {
                return;
            }

            this.DragMove();
        }
    }
}
