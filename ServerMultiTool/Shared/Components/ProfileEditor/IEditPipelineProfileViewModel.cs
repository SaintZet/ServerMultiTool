using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineProfile;

namespace ServerMultiTool.Shared.Components.ProfileEditor;

public interface IEditPipelineProfileViewModel
{
    PipelineProfileWrapper? Profile { get; set; }
}


