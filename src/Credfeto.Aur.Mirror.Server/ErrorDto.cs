using System.Diagnostics;

namespace Credfeto.Aur.Mirror.Server;

[DebuggerDisplay("Error: {Error}")]
internal sealed record ErrorDto(string Error);
