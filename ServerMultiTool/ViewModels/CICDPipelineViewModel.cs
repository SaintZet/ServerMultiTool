using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Model.CICDPipeline;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;
using ServerMultiTool.ViewModels.Contracts;

namespace ServerMultiTool.ViewModels;

public partial class CICDPipelineViewModel : BaseViewModel
{
    private static IServiceProvider _serviceProvider = null!;
    private static bool _deployInProcess;

    [ObservableProperty]
    private int _index;

    public CICDPipelineViewModel()
    {
        DeployCommand = new AsyncRelayCommand(ExecuteDeployCommand);
        _index = 0;
    }

    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _deployInProcess = false;
    }

    public AsyncRelayCommand DeployCommand { get; }

    [RelayCommand(CanExecute = nameof(CanExecuteDeploy))]
    private async Task ExecuteDeployCommand()
    {
        _deployInProcess = true;
        
        var pipeline = PipelineProfilesService.PipelineProfiles.ToList().First(x => x.Id == _index);
        await PipelineService.ExecutePipeline(pipeline);
        
        MessageBox.Show("Сборка завершена!");

        _deployInProcess = false;
    }

    private static bool CanExecuteDeploy() =>
        _deployInProcess is not true;
}