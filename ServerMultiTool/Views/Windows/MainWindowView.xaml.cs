using System.Windows;
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
    }
}