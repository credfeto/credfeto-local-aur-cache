namespace Credfeto.Aur.Mirror.Server.Interfaces;

public interface IRepoConfig
{
    string GitExecutable { get; }

    string GetRepoBasePath(string repoName);
}
