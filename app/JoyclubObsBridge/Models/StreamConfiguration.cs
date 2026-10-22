namespace JoyclubObsBridge.Models;

/// <summary>Lokale Eingaben. Sie werden derzeit bewusst nicht an JOYclub übertragen.</summary>
public sealed class StreamConfiguration
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Gender { get; set; } = "Mann";
    public bool Anonymous { get; set; }
}
