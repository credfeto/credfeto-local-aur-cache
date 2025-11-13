using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Models.Cache;

namespace Credfeto.Aur.Mirror.Interfaces;

public interface ILocallyInstalled
{
    ValueTask MarkAsClonedAsync(string repo, CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<RepoCloneInfo>> GetRecentlyClonedAsync(CancellationToken cancellationToken);
}
