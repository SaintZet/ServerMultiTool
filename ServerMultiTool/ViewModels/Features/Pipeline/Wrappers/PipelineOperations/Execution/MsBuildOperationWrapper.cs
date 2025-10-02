using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.Features.Pipeline.Operations.Execution;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.Base;
using ServerMultiTool.ViewModels.Features.Settings.Wrappers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

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

        Parameters.CollectionChanged += Parameters_CollectionChanged;

        RetryCount = operation.RetryCount;
    }

    private void Parameters_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var op = (MsBuildOperation)Operation;
        op.UpdateParameters([.. Parameters]);
    }

    partial void OnRetryCountChanged(int value)
    {
        if (value >= 0)
        {
            var op = (MsBuildOperation)Operation;
            op.UpdateRetryCount(value);
        }
    }

    [RelayCommand]
    private void AddParameter(string? parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter))
            return;

        var trimmed = parameter.Trim();
        if (!Parameters.Contains(trimmed))
            Parameters.Add(trimmed);
    }

    [RelayCommand]
    private void RemoveParameter(string? parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter))
            return;

        Parameters.Remove(parameter);
    }

    public override ServerMultiTool.Model.Features.Pipeline.Operations.Base.PipelineOperationBase ToOriginal()
    {
        // Persist changes to the underlying model
        base.ToOriginal();

        var op = (MsBuildOperation)Operation;
        if (Directory is not null)
            op.UpdateProject(Directory.ToOriginal());

        op.UpdateRetryCount(RetryCount);
        op.UpdateParameters(Parameters.ToList());

        return op;
    }
}