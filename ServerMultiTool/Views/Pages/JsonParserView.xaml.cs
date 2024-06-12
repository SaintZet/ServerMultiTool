using System.Windows.Controls;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages;

namespace ServerMultiTool.Views.Pages
{
    public partial class JsonParserView : Page
    {
        public JsonParserView(GeneralInfoViewModel generalInfo)
        {
            var viewModel = new JsonParserViewModel
            {
                GeneralInfo = generalInfo
            };
            
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
