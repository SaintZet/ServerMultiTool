using System.Collections.ObjectModel;

namespace ServerMultiTool.ViewModels.Data;

public class PipelineOperationCollection : ObservableCollection<PipelineOperationWrapper>
{
    public void ClearStatuses()
    {
        foreach (var item in this)
            item.ClearStatus();
    }
}