using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Extensions;
using Credfeto.Aur.Mirror.Server.Git;
using Credfeto.Aur.Mirror.Server.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Credfeto.Aur.Mirror.Server.Helpers;

internal static partial class Endpoints
{
    [RequiresUnreferencedCode("Calls Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.MapGet(String, Delegate)")]
    private static WebApplication ConfigureAurRepoEndpoints(this WebApplication app)
    {
        Console.WriteLine("Configuring Aur Repo Endpoint");

        RouteGroupBuilder group = app.MapGroup("/repos");

        group.MapGet(pattern: "packages.gz",
                     handler: static (IAurRepos aurRepos, HttpContext httpContext, CancellationToken cancellationToken) =>
                                  GetPackagesAsync(httpContext: httpContext, aurRepos: aurRepos, cancellationToken: cancellationToken));

        group.MapPost(pattern: "/{repoName}.git/git-upload-pack",
                      handler: static ([FromRoute] string repoName, IAurRepos aurRepos, HttpContext httpContext, CancellationToken cancellationToken) =>
                                   GitUploadPackAsync(repoName: repoName, aurRepos: aurRepos, httpContext: httpContext, cancellationToken: cancellationToken));

        group.MapGet(pattern: "/{repoName}.git/git-receive-pack",
                     handler: static ([FromRoute] string repoName, IAurRepos aurRepos, HttpContext httpContext, CancellationToken cancellationToken) =>
                                  GitReceivePackAsync(repoName: repoName, aurRepos: aurRepos, httpContext: httpContext, cancellationToken: cancellationToken));

        group.MapGet(pattern: "/{repoName}.git/info/refs",
                     handler: static ([FromRoute] string repoName, [FromQuery] string? service, IAurRepos aurRepos, HttpContext httpContext, CancellationToken cancellationToken) =>
                                  GitInfoRefsAsync(repoName: repoName, service: service, aurRepos: aurRepos, httpContext: httpContext, cancellationToken: cancellationToken));

        return app;
    }

    private static  ValueTask<IResult> GitUploadPackAsync(string repoName, IAurRepos aurRepos, HttpContext httpContext, in CancellationToken cancellationToken)
    {
        return  GitCommandAsync(repoName: repoName,
                                     service: "git-upload-pack",
                                     aurRepos: aurRepos,
                                     advertiseRefs: false,
                                     endStreamWithNull: false,
                                     httpContext: httpContext,
                                     cancellationToken: cancellationToken);

    }

    private static  ValueTask<IResult> GitReceivePackAsync(string repoName, IAurRepos aurRepos, HttpContext httpContext, in CancellationToken cancellationToken)
    {
        return  GitCommandAsync(repoName: repoName,
                                            service: "git-receive-pack",
                                            aurRepos: aurRepos,
                                            advertiseRefs: false,
                                            endStreamWithNull: true,
                                            httpContext: httpContext,
                                            cancellationToken: cancellationToken);

    }

    private static async ValueTask<IResult> GitInfoRefsAsync(string repoName, string? service, IAurRepos aurRepos, HttpContext httpContext, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(service))
        {
            return Results.BadRequest("Missing Service");
        }

        return await GitCommandAsync(repoName: repoName,
                                     service: service,
                                     aurRepos: aurRepos,
                                     advertiseRefs: true,
                                     endStreamWithNull: true,
                                     httpContext: httpContext,
                                     cancellationToken: cancellationToken);
    }

    private static async ValueTask<IResult> GetPackagesAsync(HttpContext httpContext, IAurRepos aurRepos, CancellationToken cancellationToken)
    {
        ProductInfoHeaderValue? userAgent = httpContext.GetUserAgent();

        byte[]? result = await aurRepos.GetPackagesAsync(userAgent: userAgent, cancellationToken: cancellationToken);

        if (result is null)
        {
            return Results.NotFound();
        }

        return Results.File(fileContents: result, contentType: "application/gzip");
    }

    private static async ValueTask<IResult> GitCommandAsync(string repoName,
                                                            string service,
                                                            IAurRepos aurRepos,
                                                            bool advertiseRefs,
                                                            bool endStreamWithNull /* = true*/,
                                                            HttpContext httpContext,
                                                            CancellationToken cancellationToken)
    {
        const string gitPath = "/usr/bin/git";
        string repoBasePath = aurRepos.GetRepoBasePath(repoName);

        GitCommandResponse commandResponse = await GitCommandExecutor.ExecuteResultAsync(gitPath: gitPath,
                                                                                         new(Repository: repoBasePath,
                                                                                             Service: service,
                                                                                             AdvertiseRefs: advertiseRefs,
                                                                                             EndStreamWithNull: endStreamWithNull),
                                                                                         httpContext: httpContext,
                                                                                         cancellationToken: cancellationToken);

        httpContext.Response.Headers.Append(key: "Expires", value: "Fri, 01 Jan 1980 00:00:00 GMT");
        httpContext.Response.Headers.Append(key: "Pragma", value: "no-cache");
        httpContext.Response.Headers.Append(key: "Cache-Control", value: "no-cache, max-age=0, must-revalidate");

        return Results.File(fileStream: commandResponse.Content, contentType: commandResponse.ContentType, fileDownloadName: null);
    }
}