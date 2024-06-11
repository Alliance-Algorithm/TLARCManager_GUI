using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Diagnostics;
using System.IO;
using TLARCManager.ViewModels;

namespace TLARCManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public async void OnOpenDirectClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {  // 从当前控件获取 TopLevel。或者，您也可以使用 Window 引用。
        var topLevel = TopLevel.GetTopLevel(this);

        // 启动异步操作以打开对话框。
        var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Open Desciptor Folder",
            AllowMultiple = false
        });

        if (folder.Count >= 1)
        {
            var window = new WorkWindows(folder[0]);
            window.DataContext = new MainComponentsViewModel();
            window.Show();

            this.Close();
        }
    }
    public void OnExitClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
}
