using System.Diagnostics;

namespace Credfeto.Aur.Mirror.Config;

[DebuggerDisplay("Exe: {Executable}")]
public sealed class GitConfig
{
    public GitConfig()
    {
        this.Executable = "/usr/bin/git";
    }

    public string Executable { get; set; }
}