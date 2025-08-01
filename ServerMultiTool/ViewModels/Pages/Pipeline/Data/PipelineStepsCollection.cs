using ServerMultiTool.Model.Common;
using ServerMultiTool.ViewModels.Pages.Pipeline.Enums;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ServerMultiTool.ViewModels.Pages.Pipeline.Data;

public class PipelineStepsCollection : ObservableCollection<PipelineStepWrapper>
{
    public PipelineStepsCollection()
        : base()
    {

    }

    public PipelineStepsCollection(IEnumerable<PipelineStepWrapper> collection)
        : base(collection)
    {

    }

    public void ClearStatuses()
    {
        foreach (var item in this)
            item.ClearStatus();
    }

    public void CancelWaiting()
    {
        this.Where(op => op.PipelineStepStatus == PipelineStepStatus.Wait)
            .ToList()
            .ForEach(op => op.Cancel());
    }

    public void UpdateSolutionDirectory(DirectoryModel directory)
    {
        foreach (var step in this)
        {
            step.UpdateSolutionDirectory(directory);
        }
    }

    public void UpdateHttpDirectory(DirectoryModel directory)
    {
        foreach (var step in this)
        {
            step.UpdateHttpDirectory(directory);
        }
    }
}