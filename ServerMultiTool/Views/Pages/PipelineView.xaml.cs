using System.Windows.Controls;
using ServerMultiTool.ViewModels;

namespace ServerMultiTool.Views.Pages
{
    public partial class PipelineView : Page
    {
        public PipelineView()
        {
            InitializeComponent();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox) sender;
            textBox.ScrollToEnd();
        }
    }
}
