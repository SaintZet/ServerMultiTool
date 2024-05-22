using System.Windows.Controls;
using ServerMultiTool.ViewModels;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages;

namespace ServerMultiTool.Views.Pages
{
    public partial class JsonParserView : Page
    {
        public JsonParserView(GeneralInfoViewModel generalInfoViewModel)
        {
            InitializeComponent();
            
            var vm = (JsonParserViewModel)DataContext;
            vm.GeneralInfo = generalInfoViewModel;
        }
    }
}
