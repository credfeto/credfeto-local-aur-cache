using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Constants;
using Credfeto.Aur.Mirror.Rpc.Extensions;
using Credfeto.Aur.Mirror.Rpc.Helpers;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Rpc.Services;

public sealed class GitHubMirrorRpc : IGitHubMirrorRpc
{
    private const string GitHubMirrorBaseUrl = "https://raw.githubusercontent.com/archlinux/aur";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GitHubMirrorRpc> _logger;

    public GitHubMirrorRpc(IHttpClientFactory httpClientFactory, ILogger<GitHubMirrorRpc> logger)
    {
        this._httpClientFactory = httpClientFactory;
        this._logger = logger;
    }

    public async ValueTask<RpcResponse> InfoAsync(IReadOnlyList<string> packages, CancellationToken cancellationToken)
    {
        if (packages is [])
        {
            return RpcResults.InfoNotFound;
        }

        this._logger.FetchingFromGitHubMirror(packages);

        List<SearchResult> results = [];

        foreach (string package in packages)
        {
            SearchResult? result = await this.FetchPackageAsync(package: package, cancellationToken: cancellationToken);

            if (result is not null)
            {
                results.Add(result);
            }
        }

        return new(count: results.Count, [.. results], rpcType: "multiinfo", version: RpcResults.RpcVersion);
    }

    private async ValueTask<SearchResult?> FetchPackageAsync(string package, CancellationToken cancellationToken)
    {
        HttpClient client = this._httpClientFactory.CreateClient(nameof(GitHubMirrorRpc));
        Uri requestUri = new($"{GitHubMirrorBaseUrl}/{Uri.EscapeDataString(package)}/.SRCINFO", UriKind.Absolute);

        try
        {
            using (
                HttpResponseMessage response = await client.GetAsync(
                    requestUri: requestUri,
                    cancellationToken: cancellationToken
                )
            )
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    this._logger.PackageNotFoundOnMirror(package);

                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    this._logger.HttpErrorFetchingPackage(
                        package: package,
                        message: response.StatusCode.GetName(),
                        exception: new HttpRequestException($"HTTP {response.StatusCode.GetName()}")
                    );

                    return null;
                }

                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                SearchResult? result = SrcinfoParser.Parse(content: content, packageName: package);

                if (result is null)
                {
                    this._logger.FailedToParseSrcinfo(package);
                }

                return result;
            }
        }
        catch (HttpRequestException exception)
        {
            this._logger.HttpErrorFetchingPackage(package: package, message: exception.Message, exception: exception);

            return null;
        }
    }
}
