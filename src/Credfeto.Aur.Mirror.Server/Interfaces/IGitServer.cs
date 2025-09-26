using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Server.Models.Git;
using Microsoft.AspNetCore.Http;

namespace Credfeto.Aur.Mirror.Server.Interfaces;

public interface IGitServer
{
    ValueTask<GitCommandResponse> ExecuteResultAsync( GitCommandOptions options, HttpContext httpContext, CancellationToken cancellationToken);
}