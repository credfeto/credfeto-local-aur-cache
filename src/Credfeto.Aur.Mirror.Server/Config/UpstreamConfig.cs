using System.Diagnostics;

namespace Credfeto.Aur.Mirror.Server.Config;

[DebuggerDisplay("Rpc: {Rpc} Repos: {Repos} ")]
public sealed class UpstreamConfig
{
    public UpstreamConfig()
    {
        this.Rpc = "https://aur.archlinux.org/rpc?";
        this.Repos = "https://aur.archlinux.org";
    }

    public string Rpc { get; set; }

    public string Repos { get; set; }
}
