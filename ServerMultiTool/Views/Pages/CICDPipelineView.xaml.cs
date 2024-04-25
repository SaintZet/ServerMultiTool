using System.Windows.Controls;
using ServerMultiTool.ViewModels;

namespace ServerMultiTool.Views.Pages
{
    public partial class CICDPipelineView : Page
    {
        public CICDPipelineView()
        {
            InitializeComponent();
            DataContext = new CICDPipelineViewModel();
        }
    }
}
