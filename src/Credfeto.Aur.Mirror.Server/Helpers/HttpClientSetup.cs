using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Credfeto.Aur.Mirror.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Credfeto.Aur.Mirror.Server.Helpers;

internal static class HttpClientSetup
{
    private const int CONCURRENT_ACTIONS = 30;
    private const int QUEUED_ACTIONS = 10;
    private static readonly TimeSpan HttpTimeout = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan HandlerTimeout = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan PollyTimeout = HttpTimeout.Add(TimeSpan.FromSeconds(1));

    public static IServiceCollection AddRpcClient(this IServiceCollection services)
    {
        return services
            .AddHttpClient(
                nameof(AurRpc),
                configureClient: httpClient => InitializeContentClient(httpClient: httpClient, httpTimeout: HttpTimeout)
            )
            .SetHandlerLifetime(HandlerTimeout)
            .ConfigurePrimaryHttpMessageHandler(configureHandler: _ => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
            })
            .AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(CONCURRENT_ACTIONS * 2, QUEUED_ACTIONS * 2))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(PollyTimeout))
            .Services;
    }

    public static IServiceCollection AddReposClient(this IServiceCollection services)
    {
        return services
               .AddHttpClient(
                   nameof(AurRepos),
                   configureClient: httpClient => InitializeContentClient(httpClient: httpClient, httpTimeout: HttpTimeout)
               )
               .SetHandlerLifetime(HandlerTimeout)
               .ConfigurePrimaryHttpMessageHandler(configureHandler: _ => new HttpClientHandler
                                                                          {
                                                                              AutomaticDecompression = DecompressionMethods.All,
                                                                          })
               .AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(CONCURRENT_ACTIONS * 2, QUEUED_ACTIONS * 2))
               .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(PollyTimeout))
               .Services;
    }

    private static void InitializeContentClient(HttpClient httpClient, in TimeSpan httpTimeout)
    {
        httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        httpClient.DefaultRequestVersion = HttpVersion.Version11;
        httpClient.DefaultRequestHeaders.Accept.Add(new(mediaType: "application/octet-stream"));
        httpClient.DefaultRequestHeaders.UserAgent.Add(
            new(new ProductHeaderValue(name: VersionInformation.Product, version: VersionInformation.Version))
        );
        httpClient.Timeout = httpTimeout;
    }
}
