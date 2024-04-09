using System.Threading.Tasks;

namespace ServerMultiTool.Models.Deployment.Contracts;

public interface IInternetInformationServices
{
    Task StartAsync();
    Task StopAsync();
}