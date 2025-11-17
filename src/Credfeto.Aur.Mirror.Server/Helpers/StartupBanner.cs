using System;
using Figgle;

namespace Credfeto.Aur.Mirror.Server.Helpers;

// https://www.figlet.org/examples.html
[GenerateFiggleText(memberName: "Banner", fontName: "basic", sourceText: "AUR Cache")]
internal static partial class StartupBanner
{
    public static void Show()
    {
        Console.WriteLine(Banner);

        Console.WriteLine("Starting version " + VersionInformation.Version + "...");
    }
}