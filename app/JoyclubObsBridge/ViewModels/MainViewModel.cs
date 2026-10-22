using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JoyclubObsBridge.Services;

namespace JoyclubObsBridge.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly DiagnosticLog _diagnostics = new();
    private string _obsHost = "127.0.0.1";
    private string _obsPort = "4455";
    private string _obsStatus = "Nicht verbunden";
    private string _streamTitle = string.Empty;
    private string _streamDescription = string.Empty;
    private string _selectedGender = "Mann";
    private bool _isAnonymous;

    public MainViewModel()
    {
        _diagnostics.Changed += () => OnPropertyChanged(nameof(Diagnostics));
        _diagnostics.Write("Anwendung gestartet. Es werden keine Zugangsdaten gespeichert.");
        _diagnostics.Write(BrowserSessionAdapter.Status);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<string> StreamingGenders { get; } = ["Mann", "Frau", "Paar"];
    public string ObsPassword { private get; set; } = string.Empty;
    public string ObsHost { get => _obsHost; set => SetField(ref _obsHost, value); }
    public string ObsPort { get => _obsPort; set => SetField(ref _obsPort, value); }
    public string ObsStatus { get => _obsStatus; private set => SetField(ref _obsStatus, value); }
    public string StreamTitle { get => _streamTitle; set => SetField(ref _streamTitle, value); }
    public string StreamDescription { get => _streamDescription; set => SetField(ref _streamDescription, value); }
    public string SelectedGender { get => _selectedGender; set => SetField(ref _selectedGender, value); }
    public bool IsAnonymous { get => _isAnonymous; set => SetField(ref _isAnonymous, value); }
    public string ConfigurationStatus => "Lokale Eingaben; Übertragung an JOYclub ist nicht implementiert.";
    public string Diagnostics => _diagnostics.Content;

    public async Task ConnectObsAsync()
    {
        if (!int.TryParse(ObsPort, out var port) || port is < 1 or > 65535)
        {
            ObsStatus = "Ungültiger Port";
            _diagnostics.Write("OBS-Verbindung abgebrochen: ungültiger Port.");
            return;
        }

        ObsStatus = "Verbinde …";
        _diagnostics.Write($"Verbindungsversuch zu OBS unter {ObsHost}:{port}.");
        await using var client = new ObsWebSocketClient();
        try
        {
            var version = await client.ConnectAndGetVersionAsync(ObsHost, port, ObsPassword);
            ObsStatus = $"Verbunden – OBS {version}";
            _diagnostics.Write($"OBS-Verbindung erfolgreich (OBS {version}).");
        }
        catch (Exception exception)
        {
            ObsStatus = "Nicht verbunden";
            _diagnostics.Write($"OBS-Verbindung fehlgeschlagen: {Sanitize(exception.Message)}");
        }
        finally
        {
            ObsPassword = string.Empty;
        }
    }

    private static string Sanitize(string message) => message.Replace("password", "Passwort", StringComparison.OrdinalIgnoreCase);
    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
