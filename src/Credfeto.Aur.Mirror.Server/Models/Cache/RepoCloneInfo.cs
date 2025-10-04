using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Credfeto.Aur.Mirror.Server.Models.Cache;

[DebuggerDisplay("Repo: {Repo} Last Cloned: {LastCloned}")]
public sealed class RepoCloneInfo
{
    [SuppressMessage(category: "Roslynator.Analyzers", checkId: "RCS1231:Make ref read only", Justification = "Not for json serialization")]
    [JsonConstructor]
    public RepoCloneInfo(string repo, DateTimeOffset lastCloned)
    {
        this.Repo = repo;
        this.LastCloned = lastCloned;
    }

    public string Repo { get; }

    public DateTimeOffset LastCloned { get; }
}