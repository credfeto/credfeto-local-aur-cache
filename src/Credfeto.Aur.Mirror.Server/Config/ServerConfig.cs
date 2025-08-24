using System.Collections.Generic;
using System.Diagnostics;

namespace Credfeto.Aur.Mirror.Server.Config;

[DebuggerDisplay("Storage: {Storage} Sites: {Sites.Count} ")]
public sealed class ServerConfig
{
    public ServerConfig()
    {
        this.Sites = [];
        this.Storage = "/data";
    }

    public List<CacheServerConfig> Sites { get; set; }

    public string Storage { get; set; }
}
