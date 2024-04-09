using System.Threading.Tasks;
using ServerMultiTool.Models.Build.Data;

namespace ServerMultiTool.Models.Build.Contracts;

public interface IBuildService
{
    Task ExecuteBuildAsync(BuildSettings settings);
}