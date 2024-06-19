using Avalonia.Controls;
using TLARC_Manager.ViewModels;

namespace TLARC_Manager.Views
{
    public partial class HeadBar : UserControl
    {
        public HeadBar()
        {
            InitializeComponent();
            var border = this.FindControl<Border>("DrawWindowBorder");
            border.PointerPressed += OnWindowDrag;
            border.DoubleTapped += OnMaximizeWindows;
        }
        public void OnWindowDrag(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            (DataContext as MainViewModel).MainWindowRef.BeginMoveDrag(e);
        }

        public void OnMaximizeWindows(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            if ((DataContext as MainViewModel).MainWindowRef.WindowState == WindowState.Normal)
                (DataContext as MainViewModel).MainWindowRef.WindowState = WindowState.Maximized;
            else
                (DataContext as MainViewModel).MainWindowRef.WindowState = WindowState.Normal;
        }
        public void OnMinimizeWindows(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            (DataContext as MainViewModel).MainWindowRef.WindowState = WindowState.Minimized;
        }
    }
}
