using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using ServerMultiTool.Models.GeneralPipeline.Contracts;
using ServerMultiTool.Models.Settings.Global.Contracts;
using ServerMultiTool.Models.Settings.Profiles.Contracts;
using ServerMultiTool.ViewModels.Contracts;

namespace ServerMultiTool.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly IGlobalSettingsService _globalSettingsService;
    private readonly ISettingsProfilesService _settingsProfilesService;
    private readonly IGeneralPipelineService _generalPipelineService;

    public MainViewModel(
        IGlobalSettingsService globalSettingsService, 
        ISettingsProfilesService settingsProfilesService, 
        IGeneralPipelineService generalPipelineService)
    {
        _globalSettingsService = globalSettingsService;
        _globalSettingsService.LoadSettingsAsync();
        
        _settingsProfilesService = settingsProfilesService;
        _settingsProfilesService.LoadSettingsAsync();
        
        _generalPipelineService = generalPipelineService;
    }

    [RelayCommand(CanExecute = nameof(CanBuildProjects))]
    private async Task BuildProjects()
    {
        var profileSettingId = _globalSettingsService.GlobalSettings.ProfileSettingId;
        var profilesSettings = _settingsProfilesService.GetProfilesSettings();
        var profileSetting = profilesSettings.FirstOrDefault(x => x.Id == profileSettingId);

        await _generalPipelineService.ExecuteGeneralPipeline(profileSetting);
        
        MessageBox.Show("Сборка завершена!");
    }
    
    private bool CanBuildProjects() => true;
}