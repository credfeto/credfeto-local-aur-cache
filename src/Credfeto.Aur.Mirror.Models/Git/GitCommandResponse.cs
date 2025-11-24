using System;
using System.Diagnostics;

namespace Credfeto.Aur.Mirror.Models.Git;

[DebuggerDisplay("Type: {ContentType} Size: {Size}")]
public readonly record struct GitCommandResponse(ReadOnlyMemory<byte> Content, string ContentType)
{
    public long Size => this.Content.Length;
}
