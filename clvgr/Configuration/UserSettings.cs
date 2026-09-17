using System.Text.Json.Serialization;

namespace clvgr.Configuration;

internal partial record UserSettings
{
    public static UserSettings Defaults { get; set; } = new();

    public string Theme { get; set; } = "clvgr";
    public int ClipboardPopupDurationSeconds { get; set; } = 3;
    public string? LastSecretsFile { get; set; }

    public class Container
    {
        public required UserSettings UserSettings { get; set; }
    }

    [JsonSerializable(typeof(UserSettings))]
    [JsonSerializable(typeof(Container))]
    internal partial class SerializerContext : JsonSerializerContext;
}
