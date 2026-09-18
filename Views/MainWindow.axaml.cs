using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using PNGTA.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using PNGTA.Services;
using System.Collections.ObjectModel;
using PNGTA.Models;

namespace PNGTA.Views;

public partial class MainWindow : Window
{
    private Control? _draggedItem;

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnLayerPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (sender is Control control && control.DataContext is LayerModel layer && !layer.IsLocked)
        {
            _draggedItem = control;
        }
    }

    private void OnLayerPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        if (_draggedItem != null && sender is Control targetControl && targetControl.DataContext is LayerModel targetLayer)
        {
            var sourceLayer = _draggedItem.DataContext as LayerModel;
            
            if (sourceLayer != null && sourceLayer != targetLayer && !targetLayer.IsLocked)
            {
                if (DataContext is MainViewModel vm)
                {
                    if (!IsDescendant(sourceLayer, targetLayer))
                    {
                        RemoveLayerRecursive(vm.Layers, sourceLayer);
                        targetLayer.Children.Add(sourceLayer);
                        sourceLayer.Parent = targetLayer;
                    }
                }
            }
        }
        _draggedItem = null;
    }

    private bool RemoveLayerRecursive(ObservableCollection<LayerModel> list, LayerModel target)
    {
        if (list.Contains(target))
        {
            list.Remove(target);
            return true;
        }
        foreach (var layer in list)
        {
            if (RemoveLayerRecursive(layer.Children, target))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsDescendant(LayerModel parent, LayerModel potentialChild)
    {
        if (potentialChild.Parent == null) return false;
        if (potentialChild.Parent == parent) return true;
        return IsDescendant(parent, potentialChild.Parent);
    }

    private void OnStateCardPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.Tag is string stateName)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SelectedPreviewState = stateName;
            }
        }
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

    private async void OnBrowseLayerImageClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm || vm.SelectedLayer == null) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Layer Image",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Images") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.gif", "*.webp" } }
            }
        });

        if (files.Count >= 1)
        {
            var path = files[0].Path.LocalPath;
            vm.SelectedLayer.ImagePath = path;
            
            try
            {
                vm.SelectedLayer.ImageBitmap = new Avalonia.Media.Imaging.Bitmap(path);
            }
            catch 
            {
            }
        }
    }

    private async Task<string?> OpenImageFileAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Image",
            AllowMultiple = false,
            FileTypeFilter = new[] 
            { 
                new FilePickerFileType("Supported Images") 
                { 
                    Patterns = new[] { "*.png", "*.gif", "*.webp", "*.apng", "*.jpg", "*.jpeg" } 
                } 
            }
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
            var service = ((App)Application.Current!).Services!.GetRequiredService<AvatarProjectService>();
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
            var service = ((App)Application.Current!).Services!.GetRequiredService<AvatarProjectService>();
            var config = await service.ImportProjectAsync(files[0].Path.LocalPath);
            vm.LoadConfig(config);
        }
    }

    private void OnLaunchAvatarClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            var pngViewModel = ((App)Application.Current!).Services!.GetRequiredService<PngTuberViewModel>();
            var avatarWindow = new PngTuberWindow { DataContext = pngViewModel };
            
            pngViewModel.LoadConfig(vm.BuildCurrentConfig());
            
            avatarWindow.Show();
        }
    }

    private void OnOpenSettingsClick(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow
        {
            DataContext = this.DataContext
        };
        settingsWindow.ShowDialog(this);
    }
}