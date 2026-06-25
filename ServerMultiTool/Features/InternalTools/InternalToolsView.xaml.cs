using System.Windows.Controls;

namespace ServerMultiTool.Features.InternalTools;

public partial class InternalToolsView : Page
{
    public InternalToolsView(InternalToolsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}

