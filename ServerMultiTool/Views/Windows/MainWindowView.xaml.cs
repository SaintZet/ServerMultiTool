using ServerMultiTool.ViewModels.Windows;
using System;
using System.Diagnostics;
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

                var mouseScreen = System.Windows.Forms.Control.MousePosition;

                // Get the absolute mouse position relative to the window
                var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                var mousePosRelativeToWindow = transform.Transform(new System.Windows.Point(mouseScreen.X, mouseScreen.Y));
                var relativeX = mousePosRelativeToWindow.X;
                var relativeY = mousePosRelativeToWindow.Y;

                Debug.WriteLine($"MouseScreen: {mouseScreen.X},{mouseScreen.Y}");
                Debug.WriteLine($"RelativeToWindow before restore: {relativeX},{relativeY}");

                // Get the screen where the current window is located
                var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                var workingArea = screen.WorkingArea;

                // Restore the window from Maximized to Normal
                WindowState = WindowState.Normal;

                // Adjust the window size to fit within the working area
                Width = Math.Min(Width, workingArea.Width - 40);
                Height = Math.Min(Height, workingArea.Height - 40);

                // Calculate new window position so that the mouse stays at the same position inside the window
                Left = mouseScreen.X - Width * (relativeX / ActualWidth);
                Top = mouseScreen.Y - Height * (relativeY / ActualHeight);

                // Clamp position to ensure the window stays within screen bounds
                if (Left < workingArea.Left) Left = workingArea.Left;
                if (Left + Width > workingArea.Right) Left = workingArea.Right - Width;
                if (Top < workingArea.Top) Top = workingArea.Top;
                if (Top + Height > workingArea.Bottom) Top = workingArea.Bottom - Height;

                Debug.WriteLine($"Window: Left={Left}, Top={Top}, Width={Width}, Height={Height}");

                try
                {
                    DragMove();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DragMove error: {ex.Message}");
                    throw;
                }
            }
        }


        private void DragArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _restoreForDragMove = false;
            Mouse.Capture(null);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.M:
                        WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
                        break;
                    case Key.Enter:
                        ResetWindowSize();
                        break;
                }
            }

            base.OnKeyDown(e);
        }

        private void ResetWindowSize()
        {
            WindowState = WindowState.Normal;

            var screen = System.Windows.Forms.Screen.PrimaryScreen?.WorkingArea;

            if (screen != null)
            {
                Width = 800;
                Height = 600;

                Left = screen.Value.X + 100;
                Top = screen.Value.Y + 100;
            }
            else
            {
                Debug.WriteLine("PrimaryScreen or WorkingArea is null. Cannot reset window size.");
            }
        }
    }
}