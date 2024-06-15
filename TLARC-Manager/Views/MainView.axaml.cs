using Avalonia.Controls;
using System.Windows.Input;

namespace TLARC_Manager.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        var border = this.FindControl<Border>("DrawWindowBorder");
        border.PointerPressed += OnWindowDrag;
        border.DoubleTapped += OnMaximizeWindows;
    }

    public void OnWindowDrag(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        (Parent as Window).BeginMoveDrag(e);
    }

    public void OnMaximizeWindows(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if ((Parent as Window ?? throw new()).WindowState == WindowState.Normal)
            (Parent as Window ?? throw new()).WindowState = WindowState.Maximized;
        else
            (Parent as Window ?? throw new()).WindowState = WindowState.Normal;
    }
    public void OnMinimizeWindows(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        (Parent as Window ?? throw new()).WindowState = WindowState.Minimized;
    }
}
