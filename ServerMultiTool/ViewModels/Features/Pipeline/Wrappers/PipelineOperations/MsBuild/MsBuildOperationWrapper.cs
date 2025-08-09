using CommunityToolkit.Mvvm.ComponentModel;
using ServerMultiTool.Model.Features.ContinuousIntegration.MsBuild;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using ServerMultiTool.ViewModels.Features.Settings.Wrappers;
using System.Collections.ObjectModel;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Operations.MsBuild;

public partial class MsBuildOperationWrapper : PipelineOperationWrapper
{
    public override string DefaultName => "Ms Build";

    [ObservableProperty] DirectoryModelWrapper _directory;
    [ObservableProperty] ObservableCollection<string> _parameters = [];
    [ObservableProperty] int _retryCount;

    public MsBuildOperationWrapper(MsBuildOperation operation) : base(operation)
    {
        if (operation.Project is not null)
            Directory = new DirectoryModelWrapper(operation.Project);

        if (operation.Parameters is not null)
            Parameters = new ObservableCollection<string>(operation.Parameters);

        RetryCount = operation.RetryCount;
    }
}