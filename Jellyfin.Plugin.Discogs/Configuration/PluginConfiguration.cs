using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Discogs.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the Discogs API server.
    /// </summary>
    public string ApiServer { get; set; } = "https://api.discogs.com/";

    /// <summary>
    /// Gets or sets the Discogs Image server.
    /// </summary>
    public string ImageServer { get; set; } = "https://i.discogs.com/";

    /// <summary>
    /// Gets or sets the Discogs API token.
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the preferred Discogs text format for metadata.
    /// </summary>
    public DiscogsTextFormat TextFormat { get; set; } = DiscogsTextFormat.Html;
}
