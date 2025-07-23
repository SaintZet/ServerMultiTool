using ServerMultiTool.ViewModels.Pages.JsonParser;
using System.Windows.Controls;

namespace ServerMultiTool.Views.Pages
{
    public partial class JsonParserView : Page
    {
        public JsonParserView(JsonParserViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
