using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Discogs.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the Discogs API token.
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;
}
