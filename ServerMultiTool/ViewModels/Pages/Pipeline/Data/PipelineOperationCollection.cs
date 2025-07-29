using ServerMultiTool.ViewModels.Pages.Pipeline.Enums;
using ServerMultiTool.ViewModels.Pages.Pipeline.Wrappers;
using System.Collections.ObjectModel;
using System.Linq;

namespace ServerMultiTool.ViewModels.Pages.Pipeline.Data;

public class PipelineOperationCollection : ObservableCollection<PipelineOperationWrapper>
{
    public void ClearStatuses()
    {
        foreach (var item in this)
            item.ClearStatus();
    }

    public void CancelWaitingOperations()
    {
        this.Where(op => op.PipelineOperationStatus == PipelineOperationStatus.Wait)
            .ToList()
            .ForEach(op => op.CancelOperation());
    }
}