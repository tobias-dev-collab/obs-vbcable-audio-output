using JoyclubObsBridge.ViewModels;
using System.Windows;

namespace JoyclubObsBridge;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    private void ObsPasswordChanged(object sender, RoutedEventArgs e) => _viewModel.ObsPassword = ObsPassword.Password;

    private async void ConnectObs_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.ConnectObsAsync();
        ObsPassword.Clear();
    }

    private void OpenJoyclub_Click(object sender, RoutedEventArgs e) => new JoyclubBrowserWindow { Owner = this }.Show();
}
