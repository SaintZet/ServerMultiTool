using System.Threading.Tasks;
using ServerMultiTool.Models.Build.Data;
using ServerMultiTool.Models.Deployment.Data;

namespace ServerMultiTool.Models.Deployment.Contracts;

public interface IDeployService
{
    Task ExecuteDeployAsync(BuildSettings buildSettings, DeploySettings deploySettings);
}