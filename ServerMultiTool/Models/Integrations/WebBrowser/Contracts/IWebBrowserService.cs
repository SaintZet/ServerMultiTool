using System.Threading.Tasks;
using ServerMultiTool.Models.Integrations.WebBrowser.Data;

namespace ServerMultiTool.Models.Integrations.WebBrowser.Contracts;

public interface IWebBrowserService
{
    Task OpenUrlInDefaultBrowserAsync(WebBrowserSettings settings);
}