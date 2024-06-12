using System.Windows.Controls;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages;

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
