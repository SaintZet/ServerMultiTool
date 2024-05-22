using System.Windows.Controls;
using ServerMultiTool.ViewModels.Controls;
using ServerMultiTool.ViewModels.Pages;

namespace ServerMultiTool.Views.Pages
{
    public partial class PipelineView : Page
    {
        public PipelineView(GeneralInfoViewModel generalInfo)
        {
            DataContext = new PipelineViewModel
            {
                GeneralInfo = generalInfo
            };
            
            InitializeComponent();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox) sender;
            textBox.ScrollToEnd();
        }
    }
}
