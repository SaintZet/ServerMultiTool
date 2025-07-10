using ServerMultiTool.ViewModels.Windows;
using System.Windows;
using System.Windows.Input;

namespace ServerMultiTool.Views.Windows
{
    public partial class MainWindowView : Window
    {
        private bool _restoreForDragMove = false;

        public MainWindowView()
        {
            DataContext = new MainWindowViewModel();
            InitializeComponent();
        }

        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                return;
            }

            if (WindowState == WindowState.Maximized)
            {
                _restoreForDragMove = true;
                Mouse.Capture((UIElement)sender);
            }
            else
            {
                DragMove();
            }
        }

        private void DragArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (_restoreForDragMove && e.LeftButton == MouseButtonState.Pressed)
            {
                _restoreForDragMove = false;
                Mouse.Capture(null);

                var mouseX = e.GetPosition(this).X;
                double percentX = mouseX / ActualWidth;

                WindowState = WindowState.Normal;

                var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle).WorkingArea;
                Left = e.GetPosition(null).X - (ActualWidth * percentX);
                Top = 2;

                DragMove();
            }
        }

        private void DragArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _restoreForDragMove = false;
            Mouse.Capture(null);
        }
    }
}