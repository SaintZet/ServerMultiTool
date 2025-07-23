using System.Windows.Controls;
using PipelineViewModel = ServerMultiTool.ViewModels.Pages.Pipeline.PipelineViewModel;

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
