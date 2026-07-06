using System.Diagnostics;

namespace Credfeto.Aur.Mirror.Config;

[DebuggerDisplay("Rpc: {Rpc} Repos: {Repos} Mode: {Mode}")]
public sealed class UpstreamConfig
{
    public UpstreamConfig()
    {
        this.Rpc = "https://aur.archlinux.org/rpc?";
        this.Repos = "https://aur.archlinux.org";
        this.Mode = UpstreamMode.Direct;
    }

    public string Rpc { get; set; }

    public string Repos { get; set; }

    public UpstreamMode Mode { get; set; }
}
