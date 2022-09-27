using System.Net.Http;
using System.Runtime.CompilerServices;

namespace Pica3.Helpers;

internal static class ExceptionHelper
{

    public static void HandlePicaException(this Exception ex)
    {
        Logger.Error(ex);
        if (ex is HttpRequestException e1)
        {
            NotificationProvider.Warning(e1.GetType().Name, $"网络错误{(e1.StatusCode.HasValue ? $" ({e1.StatusCode})" : "")}", 1500);
            return;
        }
        if (ex is TaskCanceledException)
        {
            NotificationProvider.Warning("连接超时", 1500);
            return;
        }
        NotificationProvider.Error(ex);
    }



}
