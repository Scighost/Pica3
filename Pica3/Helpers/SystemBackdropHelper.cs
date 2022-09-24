using Microsoft.UI.Xaml;
using Pica3.Models;
using Scighost.WinUILib.Helpers;

namespace Pica3.Helpers;


/// <summary>
/// 系统背景帮助类
/// </summary>
public class SystemBackdropHelper
{
    private readonly Window window;

    private readonly SystemBackdrop backdrop;


    public SystemBackdropHelper(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        this.window = window;
        backdrop = new(window);
    }



    /// <summary>
    /// 启动时设置背景材质
    /// </summary>
    /// <returns>设置成功</returns>
    public bool TrySetBackdrop()
    {
        if (AppSetting.TryGetValue(SettingKeys.WindowBackdrop, out uint value))
        {
            var alwaysActive = (value & 0x80000000) > 0;
            return (value & 0xF) switch
            {
                1 => backdrop.TrySetMica(alwaysActive: alwaysActive),
                2 => backdrop.TrySetAcrylic(alwaysActive: alwaysActive),
                3 => backdrop.TrySetMica(useMicaAlt: true, alwaysActive: alwaysActive),
                _ => false,
            };
        }
        else
        {
            return false;
        }
    }



    /// <summary>
    /// 修改背景材质
    /// </summary>
    /// <param name="value">设置值，最高位：一直激活，低位：材质值</param>
    /// <param name="backdropType">背景材质类型</param>
    /// <returns></returns>
    public bool TryChangeBackdrop(uint value, out uint backdropType)
    {
        var alwaysActive = (value & 0x80000000) > 0;
        backdropType = value & 0xF;
        return backdropType switch
        {
            1 => backdrop.TrySetMica(alwaysActive: alwaysActive),
            2 => backdrop.TrySetAcrylic(alwaysActive: alwaysActive),
            3 => backdrop.TrySetMica(useMicaAlt: true, alwaysActive: alwaysActive),
            _ => backdrop.Reset(),
        };
    }


}