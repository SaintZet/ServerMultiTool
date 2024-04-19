using System.Windows.Controls;
using ServerMultiTool.ViewModels;

namespace ServerMultiTool.Views.Pages
{
    public partial class DeployView : Page
    {
        public DeployView()
        {
            InitializeComponent();
            DataContext = new DeployViewModel();
        }
    }
}
