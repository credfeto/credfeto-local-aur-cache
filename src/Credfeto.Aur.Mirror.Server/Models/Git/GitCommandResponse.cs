using System.Diagnostics;
using System.IO;

namespace Credfeto.Aur.Mirror.Server.Models.Git;

[DebuggerDisplay("Type: {ContentType} Size: {Size}")]
public readonly record struct GitCommandResponse(Stream Content, string ContentType)
{
    public long Size => this.Content.Length;
}
