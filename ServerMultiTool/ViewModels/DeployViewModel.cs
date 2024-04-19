using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ServerMultiTool.Models.GeneralPipeline.Contracts;
using ServerMultiTool.Models.Settings.Global.Contracts;
using ServerMultiTool.Models.Settings.Profiles.Contracts;
using ServerMultiTool.ViewModels.Contracts;

namespace ServerMultiTool.ViewModels;

public partial class DeployViewModel : BaseViewModel
{
    private static IServiceProvider _serviceProvider = null!;
    private static bool _deployInProcess;
    
    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _deployInProcess = false;
    }

    public AsyncRelayCommand DeployCommand { get; } = new(ExecuteDeployCommand);

    [RelayCommand(CanExecute = nameof(CanExecuteDeploy))]
    private static async Task ExecuteDeployCommand()
    {
        _deployInProcess = true;
        
        var globalSettingsService = _serviceProvider.GetRequiredService<IGlobalSettingsService>();
        await globalSettingsService.LoadSettingsAsync();
        
        var profilesSettings = _serviceProvider.GetRequiredService<ISettingsProfilesService>();
        await profilesSettings.LoadSettingsAsync();
        
        var profileSettingId = globalSettingsService.GlobalSettings.ProfileSettingId;
        var profileSetting = profilesSettings.GetProfilesSettings().FirstOrDefault(x => x.Id == profileSettingId);

        var generalPipelineService = _serviceProvider.GetRequiredService<IGeneralPipelineService>();
        await generalPipelineService.ExecuteGeneralPipeline(profileSetting);
        
        MessageBox.Show("Сборка завершена!");

        _deployInProcess = false;
    }

    private static bool CanExecuteDeploy() =>
        _deployInProcess is not true;
}