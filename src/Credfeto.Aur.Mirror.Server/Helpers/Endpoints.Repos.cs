using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Config;
using LibGit2Sharp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Credfeto.Aur.Mirror.Server.Helpers;

internal static partial class Endpoints
{
    const string REPOS_PREFIX = "/repos";
    private static readonly int PrefixLength = REPOS_PREFIX.Length + 1;

    [RequiresUnreferencedCode("Calls Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.MapGet(String, Delegate)")]
    private static WebApplication ConfigureAurRepoEndpoints(this WebApplication app)
    {
        Console.WriteLine("Configuring Aur Repo Endpoint");

        RouteGroupBuilder group = app.MapGroup(REPOS_PREFIX);

        group.MapGet(pattern: "{repo}/info/refs",
                     handler: ([FromRoute] string repo, [FromQuery] string service, IOptions<ServerConfig> config, CancellationToken cancellationToken) =>
                                  GetServiceRefsFileAsync(
                                      repo, service,  config: config, cancellationToken: cancellationToken));

        group.MapGet(pattern: "{repo}/{file}",
                     handler: (HttpContext httpContext, IOptions<ServerConfig> config, CancellationToken cancellationToken) =>
                                  GetFileAsync(httpContext: httpContext, config: config, cancellationToken: cancellationToken));

        group.MapGet(pattern: "{repo}/{folder1}/{file}",
                     handler: (HttpContext httpContext, IOptions<ServerConfig> config, CancellationToken cancellationToken) =>
                                  GetFileAsync(httpContext: httpContext, config: config, cancellationToken: cancellationToken));

        group.MapGet(pattern: "{repo}/{folder1}/{folder2}/{file}",
                     handler: (HttpContext httpContext, IOptions<ServerConfig> config, CancellationToken cancellationToken) =>
                                  GetFileAsync(httpContext: httpContext, config: config, cancellationToken: cancellationToken));

        group.MapGet(pattern: "{repo}/{folder1}/{folder2}/{folder3}/{file}",
                     handler: (HttpContext httpContext, IOptions<ServerConfig> config, CancellationToken cancellationToken) =>
                                  GetFileAsync(httpContext: httpContext, config: config, cancellationToken: cancellationToken));

        group.MapGet(pattern: "{repo}/{folder1}/{folder2}/{folder3}/{folder4}/{file}",
                     handler: (HttpContext httpContext, IOptions<ServerConfig> config, CancellationToken cancellationToken) =>
                                  GetFileAsync(httpContext: httpContext, config: config, cancellationToken: cancellationToken));

        // app.MapGet(pattern: "/rpc",
        //            handler: static (HttpContext httpContext, IAurRpc aurRpc, CancellationToken cancellationToken) =>
        //                         ExecuteRpcAsync(httpContext: httpContext, aurRpc: aurRpc, cancellationToken: cancellationToken));

        return app;
    }

    private static async ValueTask<IResult> GetServiceRefsFileAsync(string repo, string service, IOptions<ServerConfig> config, CancellationToken cancellationToken)
    {
        string path = Path.Combine(path1: config.Value.Storage.Repos, path2: repo, path3: service);

        string? repoPath = Repository.Discover(path);

        if (repoPath is null)
        {
            return Results.NotFound();
        }

        using (Repository repository = new(repoPath))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ValueTask.CompletedTask;

            // repository.Info.
            // IGitService GitService;
            //
            // GitService.ExecuteGitUploadPack(
            //     Guid.NewGuid().ToString("N"),
            //     repo,
            //     GetInputStream(),
            //     outStream);
            //
            //
            // GitService.ExecuteServiceByName(
            //     correlationId: Guid.NewGuid().ToString("N"),
            //     repositoryName: repo,
            //     "upload-pack",
            //     new ExecutionOptions() { AdvertiseRefs = false, endStreamWithClose = true },
            //     GetInputStream(),
            //     outStream);

            return Results.NoContent();
        }
    }

    private static ValueTask<IResult> GetFileAsync(HttpContext httpContext, IOptions<ServerConfig> config, in CancellationToken cancellationToken)
    {
        string relativeRequestPath = httpContext.Request.Path.Value?[PrefixLength..] ?? string.Empty;

        return GetFileContentsAsync(config: config, relativeRequestPath: relativeRequestPath, cancellationToken: cancellationToken);
    }

    [SuppressMessage(category: "Microsoft.Security", checkId: "CA3003", Justification = "Explicit checks here")]
    [SuppressMessage(category: "SecurityCodeScan.VS2019", checkId: "SCS0018", Justification = "Explicit checks here")]
    private static async ValueTask<IResult> GetFileContentsAsync(IOptions<ServerConfig> config, string relativeRequestPath, CancellationToken cancellationToken)
    {
        string path = Path.Combine(path1: config.Value.Storage.Repos, path2: relativeRequestPath);

        DirectoryInfo d = new(path);

        if (!d.FullName.StartsWith(config.Value.Storage.Repos + "/", comparisonType: StringComparison.Ordinal))
        {
            return Results.NotFound();
        }

        if (File.Exists(path))
        {
            byte[] content = await File.ReadAllBytesAsync(path: path, cancellationToken: cancellationToken);

            return Results.File(content);
        }

        return Results.NotFound();
    }
}