using Microsoft.AspNetCore.Builder;

namespace Credfeto.Aur.Mirror.Server.Helpers;

internal static partial class Endpoints
{
    public static WebApplication ConfigureEndpoints(this WebApplication app)
    {
        return app.ConfigureTestEndpoints()
                  .ConfigureAurRpcEndpoints()
                  .ConfigureAurRepoEndpoints()
                  .ConfigureCacheEndpoints();
    }
}