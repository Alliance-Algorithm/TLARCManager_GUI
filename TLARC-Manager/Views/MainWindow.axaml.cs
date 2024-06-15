using Avalonia.Controls;
using TLARC_Manager.ViewModels;

namespace TLARC_Manager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(this);
    }
}
