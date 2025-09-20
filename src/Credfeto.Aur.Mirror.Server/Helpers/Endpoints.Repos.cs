using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Extensions;
using Credfeto.Aur.Mirror.Server.Git;
using Credfeto.Aur.Mirror.Server.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Credfeto.Aur.Mirror.Server.Helpers;

internal static partial class Endpoints
{
    [RequiresUnreferencedCode(
        "Calls Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.MapGet(String, Delegate)"
    )]
    private static WebApplication ConfigureAurRepoEndpoints(this WebApplication app)
    {
        Console.WriteLine("Configuring Aur Repo Endpoint");

        RouteGroupBuilder group = app.MapGroup("/repos");

        group.MapGet(
            pattern: "packages.gz",
            handler: static (IAurRepos aurRepos, HttpContext httpContext, CancellationToken cancellationToken) =>
                GetPackagesAsync(httpContext: httpContext, aurRepos: aurRepos, cancellationToken: cancellationToken)
        );

        group.MapGet(pattern: "/{repoName}.git/git-upload-pack",
                     handler: static (string repoName, IAurRepos aurRepos, HttpContext httpContext, CancellationToken cancellationToken) =>
                                  GitUploadPackAsync(repoName, aurRepos, httpContext, cancellationToken));

        group.MapGet(pattern: "/{repoName}.git/git-receive-pack",
                     handler: static (string repoName, IAurRepos aurRepos, HttpContext httpContext, CancellationToken cancellationToken) =>
                                  GitReceivePackAsync(repoName, aurRepos, httpContext, cancellationToken));

        group.MapGet(pattern: "/{repoName}.git/info/refs",
                     handler: static (string repoName, IAurRepos aurRepos, HttpContext httpContext, CancellationToken cancellationToken) =>
                                  GitInfoRefsAsync(repoName, aurRepos, httpContext, cancellationToken));

        return app;
    }

    private static async ValueTask<IResult> GitUploadPackAsync(string repoName, IAurRepos aurRepos, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var data = GitCommand(repoName, "git-upload-pack", false, false);

        await ValueTask.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();

        return Results.BadRequest();
    }

    private static async ValueTask<IResult> GitReceivePackAsync(string repoName, IAurRepos aurRepos, HttpContext httpContext, CancellationToken cancellationToken)
    {


        var data = GitCommand(repoName, "git-receive-pack", false);

        await ValueTask.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();

        return Results.BadRequest();
    }

    private static async ValueTask<IResult> GitInfoRefsAsync(string repoName, IAurRepos aurRepos, HttpContext httpContext, CancellationToken cancellationToken)
    {


        GitCommand(Path.Combine(userName, repoName), service, true));

        await ValueTask.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();

        return Results.BadRequest();
    }

    private static async ValueTask<IResult> GetPackagesAsync(
        HttpContext httpContext,
        IAurRepos aurRepos,
        CancellationToken cancellationToken
    )
    {
        ProductInfoHeaderValue? userAgent = httpContext.GetUserAgent();

        byte[]? result = await aurRepos.GetPackagesAsync(userAgent: userAgent, cancellationToken: cancellationToken);

        if (result is null)
        {
            return Results.NotFound();
        }

        return Results.File(fileContents: result, contentType: "application/gzip");
    }

    private static  GitCommandResult GitCommand(string repoName, string service, bool advertiseRefs, bool endStreamWithNull = true)
    {
        const string gitPath = "/usr/bin/git";
        return new GitCommandResult(gitPath, new GitCommandOptions(
                                        RepositoryService.GetRepository(repoName),
                                        service,
                                        advertiseRefs,
                                        endStreamWithNull
                                    ));
    }
}
