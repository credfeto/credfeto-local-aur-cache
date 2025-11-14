using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Interfaces;
using Credfeto.Aur.Mirror.Models.Git;
using Credfeto.Aur.Mirror.Server.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        RegisterRepoGroup(group);

        RegisterRepoGroup(app);

        return app;
    }

    private static void RegisterRepoGroup(IEndpointRouteBuilder group)
    {
        RegisterPackagesGz(group);
        RegisterGitUploadPack(group);
        RegisterGitReceivePack(group);
        RegisterInfoRefs(group);
        RegisterFiles(group);
    }

    private static void RegisterFiles(IEndpointRouteBuilder group)
    {
        RegisterFile1(group);
        RegisterFile2(group);
        RegisterFile3(group);
        RegisterFile4(group);
        RegisterFile5(group);
    }

    private static void RegisterFile5(IEndpointRouteBuilder group)
    {
        group.MapGet(
            pattern: "/{repoName}.git/{file1}/{file2}/{file3}/{file4}/{file5}",
            handler: static (
                [FromRoute] string repoName,
                [FromRoute] string file1,
                [FromRoute] string file2,
                [FromRoute] string file3,
                [FromRoute] string file4,
                [FromRoute] string file5,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                RetrieveFileAsync(
                    repoName: repoName,
                    file1: file1,
                    file2: file2,
                    file3: file3,
                    file4: file4,
                    file5: file5,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
        group.MapGet(
            pattern: "/{repoName}/{file1}/{file2}/{file3}/{file4}/{file5}",
            handler: static (
                [FromRoute] string repoName,
                [FromRoute] string file1,
                [FromRoute] string file2,
                [FromRoute] string file3,
                [FromRoute] string file4,
                [FromRoute] string file5,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                RetrieveFileAsync(
                    repoName: repoName,
                    file1: file1,
                    file2: file2,
                    file3: file3,
                    file4: file4,
                    file5: file5,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
    }

    private static void RegisterFile4(IEndpointRouteBuilder group)
    {
        group.MapGet(
            pattern: "/{repoName}.git/{file1}/{file2}/{file3}/{file4}",
            handler: static (
                [FromRoute] string repoName,
                [FromRoute] string file1,
                [FromRoute] string file2,
                [FromRoute] string file3,
                [FromRoute] string file4,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                RetrieveFileAsync(
                    repoName: repoName,
                    file1: file1,
                    file2: file2,
                    file3: file3,
                    file4: file4,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
        group.MapGet(
            pattern: "/{repoName}/{file1}/{file2}/{file3}/{file4}",
            handler: static (
                [FromRoute] string repoName,
                [FromRoute] string file1,
                [FromRoute] string file2,
                [FromRoute] string file3,
                [FromRoute] string file4,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                RetrieveFileAsync(
                    repoName: repoName,
                    file1: file1,
                    file2: file2,
                    file3: file3,
                    file4: file4,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
    }

    private static void RegisterFile3(IEndpointRouteBuilder group)
    {
        group.MapGet(
            pattern: "/{repoName}.git/{file1}/{file2}/{file3}",
            handler: static (
                [FromRoute] string repoName,
                [FromRoute] string file1,
                [FromRoute] string file2,
                [FromRoute] string file3,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                RetrieveFileAsync(
                    repoName: repoName,
                    file1: file1,
                    file2: file2,
                    file3: file3,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
        group.MapGet(
            pattern: "/{repoName}/{file1}/{file2}/{file3}",
            handler: static (
                [FromRoute] string repoName,
                [FromRoute] string file1,
                [FromRoute] string file2,
                [FromRoute] string file3,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                RetrieveFileAsync(
                    repoName: repoName,
                    file1: file1,
                    file2: file2,
                    file3: file3,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
    }

    private static void RegisterFile2(IEndpointRouteBuilder group)
    {
        group.MapGet(
            pattern: "/{repoName}.git/{file1}/{file2}",
            handler: static (
                [FromRoute] string repoName,
                [FromRoute] string file1,
                [FromRoute] string file2,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                RetrieveFileAsync(
                    repoName: repoName,
                    file1: file1,
                    file2: file2,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
        group.MapGet(
            pattern: "/{repoName}/{file1}/{file2}",
            handler: static (
                [FromRoute] string repoName,
                [FromRoute] string file1,
                [FromRoute] string file2,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                RetrieveFileAsync(
                    repoName: repoName,
                    file1: file1,
                    file2: file2,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
    }

    private static void RegisterFile1(IEndpointRouteBuilder group)
    {
        group.MapGet(
            pattern: "/{repoName}.git/{file1}/",
            handler: static (
                [FromRoute] string repoName,
                [FromRoute] string file1,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                RetrieveFileAsync(
                    repoName: repoName,
                    file1: file1,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
        group.MapGet(
            pattern: "/{repoName}/{file1}/",
            handler: static (
                [FromRoute] string repoName,
                [FromRoute] string file1,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                RetrieveFileAsync(
                    repoName: repoName,
                    file1: file1,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
    }

    private static void RegisterInfoRefs(IEndpointRouteBuilder group)
    {
        group.MapGet(
            pattern: "/{repoName}.git/info/refs",
            handler: static (
                [FromRoute] string repoName,
                [FromQuery] string? service,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                GitInfoRefsAsync(
                    repoName: repoName,
                    service: service,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
        group.MapGet(
            pattern: "/{repoName}/info/refs",
            handler: static (
                [FromRoute] string repoName,
                [FromQuery] string? service,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                GitInfoRefsAsync(
                    repoName: repoName,
                    service: service,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
    }

    private static void RegisterGitReceivePack(IEndpointRouteBuilder group)
    {
        group.MapGet(
            pattern: "/{repoName}.git/git-receive-pack",
            handler: static (
                [FromRoute] string repoName,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                GitReceivePackAsync(
                    repoName: repoName,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
        group.MapGet(
            pattern: "/{repoName}/git-receive-pack",
            handler: static (
                [FromRoute] string repoName,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                GitReceivePackAsync(
                    repoName: repoName,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
    }

    private static void RegisterGitUploadPack(IEndpointRouteBuilder group)
    {
        group.MapPost(
            pattern: "/{repoName}.git/git-upload-pack",
            handler: static (
                [FromRoute] string repoName,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                GitUploadPackAsync(
                    repoName: repoName,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
        group.MapPost(
            pattern: "/{repoName}/git-upload-pack",
            handler: static (
                [FromRoute] string repoName,
                IGitServer gitServer,
                HttpContext httpContext,
                CancellationToken cancellationToken
            ) =>
                GitUploadPackAsync(
                    repoName: repoName,
                    gitServer: gitServer,
                    httpContext: httpContext,
                    cancellationToken: cancellationToken
                )
        );
    }

    private static void RegisterPackagesGz(IEndpointRouteBuilder group)
    {
        group.MapGet(
            pattern: "packages.gz",
            handler: static (IAurRepos aurRepos, HttpContext httpContext, CancellationToken cancellationToken) =>
                GetPackagesAsync(httpContext: httpContext, aurRepos: aurRepos, cancellationToken: cancellationToken)
        );
    }

    private static ValueTask<IResult> RetrieveFileAsync(
        string repoName,
        string file1,
        IGitServer gitServer,
        HttpContext httpContext,
        in CancellationToken cancellationToken
    )
    {
        return RetrieveFileCommonAsync(
            repoName: repoName,
            gitServer: gitServer,
            httpContext: httpContext,
            BuildPath(file1),
            cancellationToken: cancellationToken
        );
    }

    private static ValueTask<IResult> RetrieveFileAsync(
        string repoName,
        string file1,
        string file2,
        IGitServer gitServer,
        HttpContext httpContext,
        in CancellationToken cancellationToken
    )
    {
        return RetrieveFileCommonAsync(
            repoName: repoName,
            gitServer: gitServer,
            httpContext: httpContext,
            BuildPath(file1, file2),
            cancellationToken: cancellationToken
        );
    }

    private static ValueTask<IResult> RetrieveFileAsync(
        string repoName,
        string file1,
        string file2,
        string file3,
        IGitServer gitServer,
        HttpContext httpContext,
        in CancellationToken cancellationToken
    )
    {
        return RetrieveFileCommonAsync(
            repoName: repoName,
            gitServer: gitServer,
            httpContext: httpContext,
            BuildPath(file1, file2, file3),
            cancellationToken: cancellationToken
        );
    }

    private static ValueTask<IResult> RetrieveFileAsync(
        string repoName,
        string file1,
        string file2,
        string file3,
        string file4,
        IGitServer gitServer,
        HttpContext httpContext,
        in CancellationToken cancellationToken
    )
    {
        return RetrieveFileCommonAsync(
            repoName: repoName,
            gitServer: gitServer,
            httpContext: httpContext,
            BuildPath(file1, file2, file3, file4),
            cancellationToken: cancellationToken
        );
    }

    private static ValueTask<IResult> RetrieveFileAsync(
        string repoName,
        string file1,
        string file2,
        string file3,
        string file4,
        string file5,
        IGitServer gitServer,
        HttpContext httpContext,
        in CancellationToken cancellationToken
    )
    {
        return RetrieveFileCommonAsync(
            repoName: repoName,
            gitServer: gitServer,
            httpContext: httpContext,
            BuildPath(file1, file2, file3, file4, file5),
            cancellationToken: cancellationToken
        );
    }

    private static string BuildPath(params string[] files)
    {
        return string.Join(separator: Path.DirectorySeparatorChar, value: files);
    }

    private static async ValueTask<IResult> RetrieveFileCommonAsync(
        string repoName,
        IGitServer gitServer,
        HttpContext httpContext,
        string path,
        CancellationToken cancellationToken
    )
    {
        GitCommandResponse result = await gitServer.GetFileAsync(
            repoName: repoName,
            path: path,
            cancellationToken: cancellationToken
        );

        return NoCacheResult(httpContext: httpContext, commandResponse: result);
    }

    private static ValueTask<IResult> GitUploadPackAsync(
        string repoName,
        IGitServer gitServer,
        HttpContext httpContext,
        in CancellationToken cancellationToken
    )
    {
        return GitCommandAsync(
            repoName: repoName,
            service: "git-upload-pack",
            gitServer: gitServer,
            advertiseRefs: false,
            endStreamWithNull: false,
            httpContext: httpContext,
            cancellationToken: cancellationToken
        );
    }

    private static ValueTask<IResult> GitReceivePackAsync(
        string repoName,
        IGitServer gitServer,
        HttpContext httpContext,
        in CancellationToken cancellationToken
    )
    {
        return GitCommandAsync(
            repoName: repoName,
            service: "git-receive-pack",
            gitServer: gitServer,
            advertiseRefs: false,
            endStreamWithNull: true,
            httpContext: httpContext,
            cancellationToken: cancellationToken
        );
    }

    private static async ValueTask<IResult> GitInfoRefsAsync(
        string repoName,
        string? service,
        IGitServer gitServer,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrEmpty(service))
        {
            GitCommandResponse result = await gitServer.GetFileAsync(
                repoName: repoName,
                path: "info/refs",
                cancellationToken: cancellationToken
            );

            return NoCacheResult(httpContext: httpContext, commandResponse: result);
        }

        return await GitCommandAsync(
            repoName: repoName,
            service: service,
            gitServer: gitServer,
            advertiseRefs: true,
            endStreamWithNull: true,
            httpContext: httpContext,
            cancellationToken: cancellationToken
        );
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

    private static async ValueTask<IResult> GitCommandAsync(
        string repoName,
        string service,
        IGitServer gitServer,
        bool advertiseRefs,
        bool endStreamWithNull /* = true*/
        ,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        GitCommandResponse commandResponse = await gitServer.ExecuteResultAsync(
            new(
                RepositoryName: repoName,
                Service: service,
                AdvertiseRefs: advertiseRefs,
                EndStreamWithNull: endStreamWithNull
            ),
            source: GetInputStream(httpContext),
            cancellationToken: cancellationToken
        );

        return NoCacheResult(httpContext: httpContext, commandResponse: commandResponse);
    }

    private static IResult NoCacheResult(HttpContext httpContext, in GitCommandResponse commandResponse)
    {
        httpContext.Response.Headers.Append(key: "Expires", value: "Fri, 01 Jan 1980 00:00:00 GMT");
        httpContext.Response.Headers.Append(key: "Pragma", value: "no-cache");
        httpContext.Response.Headers.Append(key: "Cache-Control", value: "no-cache, max-age=0, must-revalidate");

        return Results.Bytes(
            contents: commandResponse.Content,
            contentType: commandResponse.ContentType,
            fileDownloadName: null
        );
    }

    [SuppressMessage(
        category: "Microsoft.Reliability",
        checkId: "CA2000:DisposeObjectsBeforeLosingScope",
        Justification = "For Review"
    )]
    private static Stream GetInputStream(HttpContext context)
    {
        return StringComparer.Ordinal.Equals(context.Request.Headers["Content-Encoding"], y: "gzip")
            ? new GZipStream(stream: context.Request.Body, mode: CompressionMode.Decompress)
            : context.Request.Body;
    }
}
