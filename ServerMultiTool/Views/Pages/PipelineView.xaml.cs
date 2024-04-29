using System.Windows.Controls;
using ServerMultiTool.ViewModels;

namespace ServerMultiTool.Views.Pages
{
    public partial class PipelineView : Page
    {
        public PipelineView()
        {
            InitializeComponent();
            DataContext = new PipelineViewModel();
        }
    }
}
