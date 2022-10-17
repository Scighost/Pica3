// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pica3.Services;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Pica3.Controls;

[INotifyPropertyChanged]
public sealed partial class AvatarUploader : UserControl
{


    private readonly PicaService picaService;


    StorageFile file;


    public AvatarUploader(StorageFile file)
    {
        this.InitializeComponent();
        picaService = ServiceProvider.GetService<PicaService>()!;
        this.file = file;
    }



    private async void UserControl_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            await ImageCropper.LoadImageFromFile(file);
            UpdateRectString();
        }
        catch
        {
            NotificationProvider.Warning("无法打开图片");
            GoBack();
        }
    }



    /// <summary>
    /// 返回
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        MainWindow.Current.CloseFullWindowContent();
    }



    /// <summary>
    /// 选择图片
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task PickerFileAsync()
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, MainWindow.Current.HWND);
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                this.file = file;
                await ImageCropper.LoadImageFromFile(file);
                UpdateRectString();
            }
        }
        catch (COMException ex)
        {
            Logger.Error(ex);
            NotificationProvider.Warning("无法打开图片");
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }






    /// <summary>
    /// 重置区域
    /// </summary>
    [RelayCommand]
    private void ResetRegion()
    {
        ImageCropper.Reset();
        UpdateRectString();
    }


    /// <summary>
    /// 自定义大小
    /// </summary>
    [RelayCommand]
    private void CustomLength()
    {
        try
        {
            if (int.TryParse(TextBox_CustomLength.Text, out int length))
            {
                var rect = ImageCropper.CroppedRegion;
                rect.Width = length;
                rect.Height = length;
                if (rect.Right > ImageCropper.Source.PixelWidth)
                {
                    rect.X = ImageCropper.Source.PixelWidth - length;
                }
                if (rect.Bottom > ImageCropper.Source.PixelHeight)
                {
                    rect.Y = ImageCropper.Source.PixelHeight - length;
                }
                ImageCropper.TrySetCroppedRegion(rect);
                UpdateRectString();
            }
        }
        catch { }

    }



    /// <summary>
    /// 切换形状
    /// </summary>
    [RelayCommand]
    private void ChangeCropShape()
    {
        if (ImageCropper.CropShape is CropShape.Rectangular)
        {
            ImageCropper.CropShape = CropShape.Circular;
        }
        else
        {
            ImageCropper.CropShape = CropShape.Rectangular;
        }
    }



    /// <summary>
    /// 直接上传
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    [RelayCommand]
    private async Task UploadWithoutCropAsync(string format)
    {
        try
        {
            if (file != null)
            {
                var ms = new MemoryStream();
                using var fs = await file.OpenStreamForReadAsync();
                await fs.CopyToAsync(ms);
                await picaService.ChangeUserAvatarAsync(ms.ToArray(), "png");
                NotificationProvider.Success("上传成功", "请刷新个人信息");
                GoBack();
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            NotificationProvider.Warning("图片过大");
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }



    /// <summary>
    /// 剪裁上传，但不调整大小
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    [RelayCommand]
    private async Task UploadAsync(string format)
    {
        try
        {
            var ms = new MemoryStream();
            var rect = ImageCropper.CroppedRegion;
            rect = new Rect(Math.Round(rect.Left), Math.Round(rect.Top), Math.Round(rect.Width), Math.Round(rect.Height));
            ImageCropper.TrySetCroppedRegion(rect);
            if (format is "jpg")
            {
                await ImageCropper.SaveAsync(ms.AsRandomAccessStream(), BitmapFileFormat.Jpeg, true);
            }
            else
            {
                await ImageCropper.SaveAsync(ms.AsRandomAccessStream(), BitmapFileFormat.Png, true);
            }
            await picaService.ChangeUserAvatarAsync(ms.ToArray(), format);
            NotificationProvider.Success("上传成功", "请刷新个人信息");
            GoBack();
        }
        catch (ArgumentOutOfRangeException)
        {
            NotificationProvider.Warning("图片过大");
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }



    /// <summary>
    /// 上传，自动调整大小
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    [RelayCommand]
    private async Task UploadAndResizeAsync(string format)
    {
        const int MAX = 689493;
        try
        {
            Guid encodeId;
            var ms = new MemoryStream();
            var rect = ImageCropper.CroppedRegion;
            rect = new Rect(Math.Round(rect.Left), Math.Round(rect.Top), Math.Round(rect.Width), Math.Round(rect.Height));
            ImageCropper.TrySetCroppedRegion(rect);
            NotificationProvider.Information("调整中，请稍等。。。");
            var length = ImageCropper.CroppedRegion.Width;
            if (format is "jpg")
            {
                await ImageCropper.SaveAsync(ms.AsRandomAccessStream(), BitmapFileFormat.Jpeg, true);
                encodeId = BitmapEncoder.JpegEncoderId;
            }
            else
            {
                await ImageCropper.SaveAsync(ms.AsRandomAccessStream(), BitmapFileFormat.Png, true);
                encodeId = BitmapEncoder.PngEncoderId;
            }
            var array = ms.ToArray();
            SoftwareBitmap? softwareBitmap = null;
            while (true)
            {
                var avatar = $"data:image/{format};base64,{Convert.ToBase64String(array)}";
                if (avatar.Length <= MAX)
                {
                    break;
                }
                if (softwareBitmap is null)
                {
                    ms.Position = 0;
                    var decoder = await BitmapDecoder.CreateAsync(ms.AsRandomAccessStream());
                    softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                    length = softwareBitmap.PixelWidth;
                }
                var bis = new MemoryStream();
                var encoder = await BitmapEncoder.CreateAsync(encodeId, bis.AsRandomAccessStream());
                length = Math.Round(length * 0.9);
                encoder.BitmapTransform.ScaledWidth = (uint)length;
                encoder.BitmapTransform.ScaledHeight = (uint)length;
                encoder.IsThumbnailGenerated = false;
                encoder.SetSoftwareBitmap(softwareBitmap);
                await encoder.FlushAsync();
                array = bis.ToArray();
            }
            await picaService.ChangeUserAvatarAsync(array, format);
            NotificationProvider.Success("上传成功", "请刷新个人信息");
            GoBack();
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }






    [ObservableProperty]
    private string rectString;



    /// <summary>
    /// 更新剪裁区域大小
    /// </summary>
    private void UpdateRectString()
    {
        try
        {
            var rect = ImageCropper.CroppedRegion;
            RectString = $"{Math.Round(rect.Width)} x {Math.Round(rect.Height)}";
        }
        catch { }
    }




    private void ImageCropper_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        try
        {
            var width = ImageCropper.Source.PixelWidth;
            var rect = ImageCropper.CroppedRegion;
            var centerWidth = rect.Left + rect.Width / 2;
            var centerHeight = rect.Top + rect.Height / 2;
            var delta = e.GetCurrentPoint(null).Properties.MouseWheelDelta;
            var zoom = rect.Width / width - delta / 5000.0;
            var length = width * zoom;
            rect = new Rect(centerWidth - length / 2, centerHeight - length / 2, length, length);
            ImageCropper.TrySetCroppedRegion(rect);
            UpdateRectString();
        }
        catch { }
    }



    private void ImageCropper_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        UpdateRectString();
    }


    private void ImageCropper_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        UpdateRectString();
    }


}
