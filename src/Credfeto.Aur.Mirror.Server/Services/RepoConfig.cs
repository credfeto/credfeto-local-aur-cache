using System.IO;
using Credfeto.Aur.Mirror.Server.Config;
using Credfeto.Aur.Mirror.Server.Interfaces;
using Microsoft.Extensions.Options;

namespace Credfeto.Aur.Mirror.Server.Services;

public sealed class RepoConfig : IRepoConfig
{
    private readonly ServerConfig _serverConfig;

    public RepoConfig(IOptions<ServerConfig> config)
    {
        this._serverConfig = config.Value;
    }

    public string GitExecutable => this._serverConfig.Git.Executable;

    public string GetRepoBasePath(string repoName)
    {
        return Path.Combine(path1: this._serverConfig.Storage.Repos, $"{repoName}.git");
    }
}