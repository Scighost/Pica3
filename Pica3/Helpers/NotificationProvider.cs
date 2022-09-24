﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pica3.Helpers;

internal static class NotificationProvider
{

    private static readonly System.Timers.Timer _timer = new(30000);


    private static StackPanel _container;



    public static void Initialize(StackPanel container)
    {
        _container = container;
        _timer.AutoReset = true;
        _timer.Elapsed += _timer_Elapsed;
        _timer.Start();
    }

    private static void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_container is null)
        {
            return;
        }
        _container.DispatcherQueue?.TryEnqueue(() =>
        {
            var c = _container.Children;
            foreach (var item in c)
            {
                var size = item.ActualSize.X * item.ActualSize.Y;
                if (size == 0)
                {
                    c.Remove(item);
                }
            }
        });
    }


    private static void AddInfoBarToContainer(InfoBarSeverity severity, string? title, string? message, int delay)
    {
        if (_container is null)
        {
            return;
        }
        _container.DispatcherQueue?.TryEnqueue(async () =>
        {
            var infoBar = new InfoBar
            {
                Severity = severity,
                Title = title,
                Message = message,
                IsOpen = true,
            };
            _container.Children.Add(infoBar);
            if (delay > 0)
            {

                await Task.Delay(delay);
                infoBar.IsOpen = false;
            }
        });
    }



    public static void Information(string message, int delay = 3000)
    {
        AddInfoBarToContainer(InfoBarSeverity.Informational, null, message, delay);
    }


    public static void Information(string title, string message, int delay = 3000)
    {
        AddInfoBarToContainer(InfoBarSeverity.Informational, title, message, delay);
    }


    public static void Success(string message, int delay = 3000)
    {
        AddInfoBarToContainer(InfoBarSeverity.Success, null, message, delay);
    }


    public static void Success(string title, string message, int delay = 3000)
    {
        AddInfoBarToContainer(InfoBarSeverity.Success, title, message, delay);
    }


    public static void Warning(string message, int delay = 5000)
    {
        AddInfoBarToContainer(InfoBarSeverity.Warning, null, message, delay);
    }


    public static void Warning(string title, string message, int delay = 5000)
    {
        AddInfoBarToContainer(InfoBarSeverity.Warning, title, message, delay);
    }


    public static void Error(string message, int delay = 5000)
    {
        AddInfoBarToContainer(InfoBarSeverity.Error, null, message, delay);
    }


    public static void Error(string title, string message, int delay = 5000)
    {
        AddInfoBarToContainer(InfoBarSeverity.Error, title, message, delay);
    }


    public static void Error(Exception ex, int delay = 5000)
    {
        AddInfoBarToContainer(InfoBarSeverity.Error, ex.GetType().Name, ex.Message, delay);
    }


    public static void Error(Exception ex, string message, int delay = 5000)
    {
        AddInfoBarToContainer(InfoBarSeverity.Error, $"{ex.GetType().Name} - {message}", ex.Message, delay);
    }


    public static void Show(InfoBar infoBar, int delay = 0)
    {
        if (_container is null)
        {
            return;
        }
        _container.DispatcherQueue.TryEnqueue(async () =>
        {
            _container.Children.Add(infoBar);
            if (delay > 0)
            {
                await Task.Delay(delay);
                infoBar.IsOpen = false;
            }
        });
    }



    public static void ShowWithButton(InfoBarSeverity severity, string? title, string? message, string buttonContent, Action buttonAction, Action? closedAction = null, int delay = 0)
    {
        if (_container is null)
        {
            return;
        }
        _container.DispatcherQueue.TryEnqueue(async () =>
        {
            var infoBar = Create(severity, title, message, buttonContent, buttonAction, closedAction);
            _container.Children.Add(infoBar);
            if (delay > 0)
            {
                await Task.Delay(delay);
                infoBar.IsOpen = false;
            }
        });
    }



    public static InfoBar Create(InfoBarSeverity severity, string? title, string? message, string? buttonContent = null, Action? buttonAction = null, Action? closedAction = null)
    {
        Button? button = null;
        if (!string.IsNullOrWhiteSpace(buttonContent) && buttonAction != null)
        {
            button = new Button
            {
                Content = buttonContent,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            button.Click += (_, _) =>
            {
                try
                {
                    buttonAction();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"{title} - {message} - {buttonContent}");
                }
            };
        }
        var infoBar = new InfoBar
        {
            Severity = severity,
            Title = title,
            Message = message,
            ActionButton = button,
            IsOpen = true,
        };
        if (closedAction is not null)
        {
            infoBar.CloseButtonClick += (_, _) =>
            {
                try
                {
                    closedAction();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"{title} - {message}");
                }
            };
        }
        return infoBar;
    }


}
