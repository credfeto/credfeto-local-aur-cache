using System.Diagnostics;

namespace Credfeto.Aur.Mirror.Server.Config;

[DebuggerDisplay("Storage: {Storage} Upstream: {Upstream} ")]
public sealed class ServerConfig
{
    public ServerConfig()
    {
        this.Git = new();
        this.Upstream = new();
        this.Storage = new();
    }

    public GitConfig Git { get; set; }

    public UpstreamConfig Upstream { get; set; }

    public StorageConfig Storage { get; set; }
}