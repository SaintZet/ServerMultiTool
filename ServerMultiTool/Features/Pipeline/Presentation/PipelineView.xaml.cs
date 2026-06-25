using System.Windows.Controls;
namespace ServerMultiTool.Features.Pipeline.Presentation;

public partial class PipelineView : Page
{
    public PipelineView(PipelineViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}

