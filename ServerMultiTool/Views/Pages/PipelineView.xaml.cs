using System.Windows.Controls;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages;
using PipelineViewModel = ServerMultiTool.ViewModels.Pages.Pipeline.PipelineViewModel;

namespace ServerMultiTool.Views.Pages
{
    public partial class PipelineView : Page
    {
        public PipelineView(GeneralInfoViewModel generalInfo)
        {
            var viewModel = new PipelineViewModel
            {
                GeneralInfo = generalInfo
            };
            
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
