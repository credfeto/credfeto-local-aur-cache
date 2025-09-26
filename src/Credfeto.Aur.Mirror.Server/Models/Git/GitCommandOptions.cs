using System;
using System.Diagnostics;
using System.Text;

namespace Credfeto.Aur.Mirror.Server.Models.Git;

[DebuggerDisplay("Repo:{RepositoryName}, Service: {Service} Refs: {AdvertiseRefs} {EndStreamWithNull}")]
public readonly record struct GitCommandOptions(
    string RepositoryName,
    string Service,
    bool AdvertiseRefs,
    bool EndStreamWithNull
)
{
    public string BuildCommand(string repositoryBasePath)
    {
        if (!this.Service.StartsWith("git-", StringComparison.Ordinal))
        {
            throw new InvalidOperationException();
        }

        StringBuilder builder = new StringBuilder()
            .Append(this.Service, 4, this.Service.Length - 4)
            .Append(" --stateless-rpc");

        if (this.AdvertiseRefs)
        {
            builder = builder.Append(" --advertise-refs");
        }

        return builder.Append($@" ""{repositoryBasePath}""").ToString();
    }
}
