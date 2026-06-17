using System.Windows;
using Ezvcc.App.ViewModels;

namespace Ezvcc.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainViewModel();
        DataContext = viewModel;
        Closed += (_, _) => viewModel.Dispose();
    }
}
