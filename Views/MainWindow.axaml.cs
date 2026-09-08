using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using PNGTA.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using PNGTA.Services;

namespace PNGTA.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void OnBrowseStateClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string stateName)
        {
            var file = await OpenImageFileAsync();
            if (file != null && DataContext is MainViewModel vm)
            {
                vm.UpdateImagePath(stateName, file);
            }
        }
    }

    private async Task<string?> OpenImageFileAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select PNG Image",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
        });

        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    private async void OnExportClick(object sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || DataContext is not MainViewModel vm) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export PNGTuber Avatar",
            DefaultExtension = "pngvt",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("PNGTuber Avatar") { Patterns = new[] { "*.pngvt" } }
            }
        });

        if (file != null)
        {
            var service = App.Current.Services.GetRequiredService<AvatarProjectService>();
            await service.ExportProjectAsync(vm.BuildCurrentConfig(), file.Path.LocalPath);
        }
    }

    private async void OnImportClick(object sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || DataContext is not MainViewModel vm) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import PNGTuber Avatar",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("PNGTuber Avatar") { Patterns = new[] { "*.pngvt" } }
            }
        });

        if (files.Count > 0)
        {
            var service = App.Current.Services.GetRequiredService<AvatarProjectService>();
            var config = await service.ImportProjectAsync(files[0].Path.LocalPath);
            vm.LoadConfig(config);
        }
    }

    private void OnLaunchAvatarClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            var pngViewModel = App.Current.Services.GetRequiredService<PngTuberViewModel>();
            var avatarWindow = new PngTuberWindow { DataContext = pngViewModel };
            pngViewModel.LoadImages(vm.IdleImagePath, vm.TalkingImagePath);
            avatarWindow.Show();
        }
    }
}