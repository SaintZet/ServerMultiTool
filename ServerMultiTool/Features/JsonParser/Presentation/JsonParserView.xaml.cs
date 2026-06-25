using System.Windows.Controls;
namespace ServerMultiTool.Features.JsonParser.Presentation;

public partial class JsonParserView : Page
{
    public JsonParserView(JsonParserViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}

