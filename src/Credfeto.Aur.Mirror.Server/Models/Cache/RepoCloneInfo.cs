using System;
using System.Diagnostics;

namespace Credfeto.Aur.Mirror.Server.Models.Cache;

[DebuggerDisplay("Repo: {Repo} Last Cloned: {LastCloned}")]
public readonly record struct RepoCloneInfo(string Repo, DateTimeOffset LastCloned);