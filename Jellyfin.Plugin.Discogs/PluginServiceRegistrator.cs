using DiscogsApiClient;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.Discogs;

/// <inheritdoc />
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddDiscogsApiClient(options =>
        {
            // TODO: Add jellyfin & plugin version
            options.UserAgent = "Jellyfin/1.0.0";
        });
    }
}
