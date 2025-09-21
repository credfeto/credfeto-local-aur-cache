using System.Diagnostics;
using System.IO;

namespace Credfeto.Aur.Mirror.Server.Git;

[DebuggerDisplay("Type: {ContentType} Size: {Size}")]
internal readonly record struct GitCommandResponse(Stream Content, string ContentType)
{
    public long Size => this.Content.Length;
}