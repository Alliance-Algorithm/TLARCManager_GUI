using Avalonia.Controls;
using TLARC_Manager.ViewModels;

namespace TLARC_Manager.Views;
    public partial class ProcessBarWindows : Window
{
    public ProcessBarWindows()
    {
        InitializeComponent();
        DataContext = new ProcessBarViewModel();
    }

    public void SetProcess(float value)
    {
        (DataContext as ProcessBarViewModel).Value = value;
    }
}
