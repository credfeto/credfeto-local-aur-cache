using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Aur.Mirror.Git.Exceptions;

namespace Credfeto.Aur.Mirror.Git.Helpers;

internal static class GitCommandLine
{
    public static async ValueTask<(string[] Output, int ExitCode)> ExecAsync(
        string gitExecutable,
        string clonePath,
        string workingDirectory,
        string arguments,
        CancellationToken cancellationToken
    )
    {
        EnsureNotLocked(repoUrl: clonePath, workingDirectory: workingDirectory);

        ProcessStartInfo psi = new()
        {
            FileName = gitExecutable,
            WorkingDirectory = workingDirectory,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Environment = { ["GIT_REDIRECT_STDERR"] = "2>&1" },
        };

        using (Process? process = Process.Start(psi))
        {
            if (process is null)
            {
                throw new InvalidOperationException("Failed to start git");
            }

#if NET7_0_OR_GREATER
            string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);

            string error = await process.StandardError.ReadToEndAsync(cancellationToken);
#else
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
#endif

            await process.WaitForExitAsync(cancellationToken);

            string result = string.Join(separator: Environment.NewLine, output, error);

            return (
                result.Split(separator: Environment.NewLine, options: StringSplitOptions.RemoveEmptyEntries),
                process.ExitCode
            );
        }
    }

    public static void EnsureNotLocked(string repoUrl, string workingDirectory)
    {
        string gitDir = Path.Combine(workingDirectory, ".git");
        string lockFile = Directory.Exists(gitDir)
            ? Path.Combine(gitDir, "index.lock")
            : Path.Combine(workingDirectory, "index.lock");

        if (File.Exists(lockFile))
        {
            throw new GitRepositoryLockedException($"Repository {repoUrl} at {workingDirectory} is locked.");
        }
    }
}
