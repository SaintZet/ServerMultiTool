using System.Collections.ObjectModel;
using ServerMultiTool.ViewModels.Pages.Pipeline.Wrappers;

namespace ServerMultiTool.ViewModels.Pages.Pipeline.Data;

public class PipelineOperationCollection : ObservableCollection<PipelineOperationWrapper>
{
    public void ClearStatuses()
    {
        foreach (var item in this)
            item.ClearStatus();
    }
}