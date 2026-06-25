using ServerMultiTool.Model.Common;

namespace ServerMultiTool.Shared.Components.GeneralInfo;

public interface IGeneralInfoViewModel
{
    bool CanChangeStates { get; set; }
    string? CurrentGitBranch { get; set; }
    DirectoryModel[] SolutionDirectories { get; set; }
    DirectoryModel? SelectedSolutionDirectory { get; set; }
    DirectoryModel[] HttpDirectories { get; set; }
    DirectoryModel? SelectedHttpDirectory { get; set; }

    void UpdateData();
}

