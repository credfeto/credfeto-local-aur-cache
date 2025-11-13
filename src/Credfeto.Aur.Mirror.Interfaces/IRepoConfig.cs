namespace Credfeto.Aur.Mirror.Interfaces;

public interface IRepoConfig
{
    string GitExecutable { get; }

    string GetRepoBasePath(string repoName);
}
