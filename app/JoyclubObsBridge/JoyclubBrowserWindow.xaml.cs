using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace JoyclubObsBridge;

public partial class JoyclubBrowserWindow : Window
{
    private static readonly Uri StreamUri = new("https://www.joyclub.de/livestream/");

    public JoyclubBrowserWindow()
    {
        InitializeComponent();
        Loaded += InitializeBrowserAsync;
    }

    private async void InitializeBrowserAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            var profileDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "JoyclubObsBridge", "WebViewProfile");
            Directory.CreateDirectory(profileDirectory);
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: profileDirectory);
            await Browser.EnsureCoreWebView2Async(environment);
            Browser.CoreWebView2.Settings.AreDevToolsEnabled = false;
            Browser.CoreWebView2.Settings.IsStatusBarEnabled = false;
            Browser.Source = StreamUri;
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, $"Der JOYclub-Browser konnte nicht geöffnet werden.\n\n{exception.Message}", "JOYclub OBS Bridge", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    private void OpenStreamPage_Click(object sender, RoutedEventArgs e) => Browser.Source = StreamUri;
    private void Reload_Click(object sender, RoutedEventArgs e) => Browser.Reload();
}
