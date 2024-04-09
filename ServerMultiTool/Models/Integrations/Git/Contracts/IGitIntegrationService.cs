using System.Threading.Tasks;
using ServerMultiTool.Models.Integrations.Git.Data;

namespace ServerMultiTool.Models.Integrations.Git.Contracts;

public interface IGitIntegrationService
{
    Task ExecuteGitOperationsAsync(GitIntegrationSettings settings);
}