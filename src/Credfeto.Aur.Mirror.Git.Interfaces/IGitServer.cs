using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Models.Git;

namespace Credfeto.Aur.Mirror.Git.Interfaces;

public interface IGitServer
{
    ValueTask<GitCommandResponse> ExecuteResultAsync(GitCommandOptions options, Stream source, CancellationToken cancellationToken);

    ValueTask EnsureRepositoryHasBeenClonedAsync(string repoName, string upstreamRepo, bool changed, CancellationToken cancellationToken);

    ValueTask<GitCommandResponse> GetFileAsync(string repoName, string path, CancellationToken cancellationToken);
}