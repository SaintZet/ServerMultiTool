using System;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net;
using ServerMultiTool.Models.Integrations.WebBrowser.Contracts;
using ServerMultiTool.Models.Integrations.WebBrowser.Data;

namespace ServerMultiTool.Models.Integrations.WebBrowser.Services;

public class WebBrowserService : IWebBrowserService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(WebBrowserService));
    
    public async Task OpenUrlInDefaultBrowserAsync(WebBrowserSettings settings)
    {
        if (settings.Enable is false)
        {
            Log.Info("Открытие URL отключено настройками");
            return;
        }
        
        await Task.Run(() =>
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = settings.Url,
                    UseShellExecute = true,
                });
                Log.Info("Успешно открыта страница");
            }
            catch (Exception ex)
            {
                Log.Error($"Не удалось открыть URL: {ex.Message}");
            }
        });
    }
}