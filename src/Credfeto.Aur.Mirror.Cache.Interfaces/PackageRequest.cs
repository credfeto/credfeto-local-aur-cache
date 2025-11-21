using System;
using System.Diagnostics;

namespace Credfeto.Aur.Mirror.Cache.Interfaces;

[DebuggerDisplay("{PackageName}")]
public readonly record struct PackageRequest(string PackageName, Action<Package> Update);