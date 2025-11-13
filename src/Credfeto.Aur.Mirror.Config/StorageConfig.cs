using System.Diagnostics;

namespace Credfeto.Aur.Mirror.Config;

[DebuggerDisplay("Storage: {Metadata} Repos: {Repos} ")]
public sealed class StorageConfig
{
    public StorageConfig()
    {
        this.Metadata = "/data/metadata";
        this.Repos = "/data/repos";
    }

    public string Metadata { get; set; }

    public string Repos { get; set; }
}
