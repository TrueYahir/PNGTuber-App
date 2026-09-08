using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace PNGTA.Views;

public partial class PngTuberWindow : Window
{
    public PngTuberWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Image_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }
}