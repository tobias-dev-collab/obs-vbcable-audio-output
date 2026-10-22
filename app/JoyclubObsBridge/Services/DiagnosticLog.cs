using System.Text;

namespace JoyclubObsBridge.Services;

public sealed class DiagnosticLog
{
    private readonly StringBuilder _entries = new();
    public event Action? Changed;
    public string Content => _entries.ToString();

    public void Write(string message)
    {
        _entries.Append('[').Append(DateTime.Now.ToString("HH:mm:ss")).Append("] ").AppendLine(message);
        Changed?.Invoke();
    }
}
