using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;

namespace ServerMultiTool.Shared.Components.ProfileEditor;

public interface IEditPipelineProfileViewModel
{
    PipelineProfileWrapper? Profile { get; set; }
}

