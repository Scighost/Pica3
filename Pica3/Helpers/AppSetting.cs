using Microsoft.Win32;
using System.ComponentModel;

namespace Pica3.Helpers;

internal static class AppSetting
{


    private const string KEY = @"HKEY_CURRENT_USER\Software\Pica3";

    private static Dictionary<string, string?> cache = new();

    public static T? GetValue<T>(string key, T? defaultValue = default, bool throwError = false)
    {
        try
        {
            if (!cache.TryGetValue(key, out var value))
            {
                value = Registry.GetValue(KEY, key, null) as string;
                cache[key] = value;
            }
            if (value is null)
            {
                return defaultValue;
            }
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter == null)
                {
                    return defaultValue;
                }
                return (T?)converter.ConvertFromString(value);
            }
            catch (NotSupportedException)
            {
                return defaultValue;
            }
        }
        catch
        {
            if (throwError)
            {
                throw;
            }
            return defaultValue;
        }

    }


    public static void SetValue<T>(string key, T value)
    {
        try
        {
            if (value?.ToString() is string str)
            {
                Registry.SetValue(KEY, key, str);
                cache[key] = str;
            }
        }
        catch { }
    }


    public static bool TryGetValue<T>(string key, out T? result, T? defaultValue = default)
    {
        result = defaultValue;
        try
        {
            if (!cache.TryGetValue(key, out var value))
            {
                value = Registry.GetValue(KEY, key, null) as string;
                cache[key] = value;
            }
            if (value is null)
            {
                return false;
            }
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter == null)
                {
                    return false;
                }
                result = (T?)converter.ConvertFromString(value);
                return true;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }


    public static bool TrySetValue<T>(string key, T value)
    {
        try
        {
            if (value?.ToString() is string str)
            {
                Registry.SetValue(KEY, key, str);
                cache[key] = str;
                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }





}
