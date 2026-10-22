namespace JoyclubObsBridge.Services;

/// <summary>
/// Sicherheitsgrenze für eine spätere WebView2-Integration. Keine Login-Automation,
/// kein Token-Export und keine Nachbildung privater JOYclub-Schnittstellen.
/// </summary>
public sealed class BrowserSessionAdapter
{
    public const string Status = "Nicht aktiviert: Autorisierter, sichtbarer Browserablauf muss zuerst validiert werden.";
}
