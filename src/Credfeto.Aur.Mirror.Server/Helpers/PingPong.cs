using Credfeto.Aur.Mirror.Models;

namespace Credfeto.Aur.Mirror.Server.Helpers;

internal static class PingPong
{
    public static PongDto Model { get; } = new("Pong!");
}