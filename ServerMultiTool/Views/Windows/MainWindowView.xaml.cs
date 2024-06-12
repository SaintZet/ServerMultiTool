using System.Windows;
using System.Windows.Input;
using ServerMultiTool.ViewModels.Windows;

namespace ServerMultiTool.Views.Windows
{
    public partial class MainWindowView : Window
    {

        public MainWindowView()
        {
            DataContext = new MainWindowViewModel();
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}