using ServerMultiTool.ViewModels.Pages.Pipeline;
using System.Windows.Controls;

namespace ServerMultiTool.Views.Pages
{
    public partial class PipelineView : Page
    {
        public PipelineView(PipelineViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
