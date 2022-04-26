using LogVisualizer.util;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace LogVisualizer.view {
    /// <summary>
    /// JsonFilterDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class JsonFilterDialog : Window {

        private MainWindow parentWindow;
        private string initText;

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
            initText = JsonUtil.LoadJsonString();

            JsonFormatView.Text = initText;
        }

        public JsonFilterDialog(DependencyObject depObject) : this() {
            Owner = Window.GetWindow(depObject);
            parentWindow = Owner as MainWindow;
        }

        private void JsonFormatView_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            if (initText == JsonFormatView.Text) {
                ChangeButton.Visibility = Visibility.Collapsed;
                return;
            } 

            ChangeButton.Visibility = Visibility.Visible;
        }

        private void ChangeButtonClicked(object sender, RoutedEventArgs e) {
            var jsonString = JsonFormatView.Text;

            if (!JsonUtil.CheckJsonFormat(jsonString)) {
                MessageBox.Show("Can't convert json. Please check json format again");
                return;
            }

            JsonUtil.SaveJsonString(jsonString);
            parentWindow.JsonFormatChanged(jsonString);
            this.Close();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e) => parentWindow.Effect = null;

        private void CloseButtonClicked(object sender, RoutedEventArgs e) => this.Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton != MouseButton.Left) {
                return;
            }

            this.DragMove();
        }
    }
}
