using System;
using System.Diagnostics;

namespace Credfeto.Aur.Mirror.Models.Cache;

[DebuggerDisplay(
    "Repo: {Repo} Last Cloned: {LastCloned} Last Accessed: {LastAccessed} Last Modified Upstream: {LastModifiedUpstream}"
)]
public sealed record RepoCloneInfo(
    string Repo,
    DateTimeOffset LastCloned,
    DateTimeOffset LastAccessed,
    DateTimeOffset LastModifiedUpstream
);
