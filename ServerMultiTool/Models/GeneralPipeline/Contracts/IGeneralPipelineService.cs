using System.Threading.Tasks;
using ServerMultiTool.Models.Settings.Profiles.Data;

namespace ServerMultiTool.Models.GeneralPipeline.Contracts;

public interface IGeneralPipelineService
{
    Task ExecuteGeneralPipeline(SettingsProfile settings);
}