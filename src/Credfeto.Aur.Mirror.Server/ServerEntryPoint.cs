using System.Diagnostics;

namespace Credfeto.Aur.Mirror.Server;

[DebuggerDisplay("{" + nameof(AssemblyVersion) + "}")]
public sealed class ServerEntryPoint
{
    public string AssemblyVersion { get; } = VersionInformation.Version;
}
