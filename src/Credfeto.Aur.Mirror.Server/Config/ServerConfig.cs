using System.Diagnostics;

namespace Credfeto.Aur.Mirror.Server.Config;

[DebuggerDisplay("Storage: {Storage} Upstream: {Upstream} ")]
public sealed class ServerConfig
{
    public ServerConfig()
    {
        this.Upstream = new();
        this.Storage = new();
    }

    public UpstreamConfig Upstream { get; set; }

    public StorageConfig Storage { get; set; }
}