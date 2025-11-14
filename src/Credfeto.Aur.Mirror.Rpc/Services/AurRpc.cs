using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Models.AurRpc;
using Credfeto.Aur.Mirror.Rpc.Constants;
using Credfeto.Aur.Mirror.Rpc.Interfaces;
using Credfeto.Aur.Mirror.Rpc.Services.LoggingExtensions;
using Microsoft.Extensions.Logging;

namespace Credfeto.Aur.Mirror.Rpc.Services;

public sealed class AurRpc : IAurRpc
{
    private readonly ILocalAurRpc _localAurRpc;
    private readonly ILogger<AurRpc> _logger;
    private readonly IRemoteAurRpc _remoteAurRpc;

    public AurRpc(IRemoteAurRpc remoteAurRpc, ILocalAurRpc localAurRpc, ILogger<AurRpc> logger)
    {
        this._remoteAurRpc = remoteAurRpc;
        this._localAurRpc = localAurRpc;
        this._logger = logger;

        // TASK: Look locally for everything and ONLY look in RPC if a significant amount of time has occured since the last query for that same data
    }

    public async ValueTask<RpcResponse> SearchAsync(string keyword, string by, ProductInfoHeaderValue? userAgent, CancellationToken cancellationToken)
    {
        this._logger.SearchingFor(keyword: keyword, by: by);

        try
        {
            RpcResponse upstream = await this._remoteAurRpc.SearchAsync(keyword: keyword, by: by, userAgent: userAgent, cancellationToken: cancellationToken);

            await this._localAurRpc.SyncUpstreamReposAsync(upstream: upstream, userAgent: userAgent);

            return upstream;
        }
        catch (HttpRequestException exception)
        {
            this._logger.FailedToSearchUpstreamPackageInfo(keyword: keyword, by: by, message: exception.Message, exception: exception);

            return await this._localAurRpc.SearchAsync(keyword: keyword, by: by, userAgent: userAgent, cancellationToken: cancellationToken);
        }
    }

    public async ValueTask<RpcResponse> InfoAsync(IReadOnlyList<string> packages, ProductInfoHeaderValue? userAgent, CancellationToken cancellationToken)
    {
        this._logger.PackageInfo(packages);

        try
        {
            if (packages is [])
            {
                return RpcResults.InfoNotFound;
            }

            // RpcResponse found = await this._localAurRpc.InfoAsync(packages, userAgent, cancellationToken);
            //
            // // All packages in locally no need to hit upstream
            // if(packages.All(package => found.Results.Any( p => StringComparer.Ordinal.Equals(p.Name, package))))
            // {
            //     return found;
            // }

            RpcResponse upstream = await this._remoteAurRpc.InfoAsync(packages: packages, userAgent: userAgent, cancellationToken: cancellationToken);

            await this._localAurRpc.SyncUpstreamReposAsync(upstream: upstream, userAgent: userAgent);

            return upstream;
        }
        catch (HttpRequestException exception)
        {
            this._logger.FailedToGetUpstreamPackageInfo(packages: packages, message: exception.Message, exception: exception);

            return await this._localAurRpc.InfoAsync(packages: packages, userAgent: userAgent, cancellationToken: cancellationToken);
        }
    }
}