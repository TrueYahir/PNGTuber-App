using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PNGTA.ViewModels;

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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (DataContext is PngTuberViewModel vm)
        {
            switch (e.Key)
            {
                case Key.D1: vm.SetEmotion("idle"); break;
                case Key.D2: vm.SetEmotion("sad"); break;
                case Key.D3: vm.SetEmotion("crying"); break;
                case Key.D4: vm.SetEmotion("mad"); break;
                case Key.D5: vm.SetEmotion("angry"); break;
                case Key.D6: vm.SetEmotion("laugh"); break;
                case Key.D7: vm.SetEmotion("thinking"); break;
            }
        }
        base.OnKeyDown(e);
    }
}