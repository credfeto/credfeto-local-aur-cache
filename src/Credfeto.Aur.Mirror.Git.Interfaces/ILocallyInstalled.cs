using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Models.Cache;

namespace Credfeto.Aur.Mirror.Git.Interfaces;

public interface ILocallyInstalled
{
    ValueTask MarkAsClonedAsync(string packageName, CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<RepoCloneInfo>> GetRecentlyClonedAsync(CancellationToken cancellationToken);
}
