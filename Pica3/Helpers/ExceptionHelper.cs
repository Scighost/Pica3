using System.Net.Http;
using System.Runtime.CompilerServices;

namespace Pica3.Helpers;

internal static class ExceptionHelper
{

    public static void HandlePicaException(this Exception ex)
    {
        Logger.Error(ex);
        if (ex is HttpRequestException)
        {
            NotificationProvider.Warning("网络错误", 1500);
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
