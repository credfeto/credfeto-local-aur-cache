using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Server.Helpers;
using Credfeto.Docker.HealthCheck.Http.Client;
using Microsoft.AspNetCore.Builder;

namespace Credfeto.Aur.Mirror.Server;

public static class Program
{
    private const int MIN_THREADS = 32;

    [SuppressMessage(
        category: "Meziantou.Analyzer",
        checkId: "MA0109: Add an overload with a Span or Memory parameter",
        Justification = "Won't work here"
    )]
    public static async Task<int> Main(string[] args)
    {
        return HealthCheckClient.IsHealthCheck(args: args, out string? checkUrl)
            ? await HealthCheckClient.ExecuteAsync(targetUrl: checkUrl, cancellationToken: CancellationToken.None)
            : await RunServerAsync(args);
    }

    private static async ValueTask<int> RunServerAsync(string[] args)
    {
        StartupBanner.Show();

        ServerStartup.SetThreads(MIN_THREADS);

        try
        {
            TestDeserialization();

            await using (WebApplication app = ServerStartup.CreateApp(args))
            {
                await RunAsync(app);

                return 0;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine("An error occurred:");
            Console.WriteLine(exception.Message);
            Console.WriteLine(exception.StackTrace);

            return 1;
        }
    }

    [Conditional("DEBUG")]
    private static void TestDeserialization()
    {
        const string s =
            @"{
                ""resultcount"":1,
                ""results"":[
                    {
                        ""Description"":""A fetch program written in C"",
                        ""FirstSubmitted"":1616899975,
                        ""ID"":922314,
                        ""Keywords"":[""fetch"",""neofetch"",""screenfetch""],
                        ""LastModified"":1623952808,
                        ""License"":[""GPL3""],
                        ""Maintainer"":""carlosal1015"",
                        ""Name"":""afetch"",
                        ""NumVotes"":2,
                        ""OutOfDate"":null,
                        ""PackageBase"":""afetch"",
                        ""PackageBaseID"":164969,
                        ""Popularity"":0.319567,
                        ""Submitter"":""lmartinez-mirror"",
                        ""URL"":""https://github.com/13-CF/afetch"",
                        ""URLPath"":""/cgit/aur.git/snapshot/afetch.tar.gz"",
                        ""Version"":""2.2.0-1""
                    }
                ],
                ""type"":""multiinfo"",
                ""version"":5
            }";
        RpcResponse rpcResponse =
            JsonSerializer.Deserialize<RpcResponse>(json: s, jsonTypeInfo: AppJsonContext.Default.RpcResponse)
            ?? throw new JsonException("Could not deserialize response");

        Console.WriteLine(rpcResponse.Count);
    }

    private static Task RunAsync(WebApplication application)
    {
        Console.WriteLine("App Created");

        return AddMiddleware(application).RunAsync();
    }

    private static WebApplication AddMiddleware(WebApplication application)
    {
        WebApplication app = (WebApplication)application.UseForwardedHeaders();

        return app.ConfigureEndpoints();
    }
}
