using System.Threading;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Memo
{
    public class SaveCommand : Command
    {
        public class Input
        {
            public bool NoSync { get; set; }
        }

        public SaveCommand() : base("save")
        {
            AddOption(new Option<bool>(
                new string[] {"--no-sync"},
                () => false,
                "Without Sync"
            ));
        }


        public class CommandHandler : ICommandHandler
        {
            public SaveCommand.Input Input { get; set; }
            public Core.CommandConfig CommandConfig { get; }

            public CommandHandler(Core.CommandConfig commandConfig)
            {
                CommandConfig = commandConfig;
            }

            public async Task<int> InvokeAsync(InvocationContext context)
            {
                var token = context.GetCancellationToken();
                using (var repository = new LibGit2Sharp.Repository(CommandConfig.HomeDirectory.FullName))
                {
                    if (!Input.NoSync && !(await Pull(token))) return Cli.FailedExitCode;
                    if (!(await AddAllAndCommit(repository, token))) return Cli.FailedExitCode;
                    if (!Input.NoSync && !(await Push(token))) return Cli.FailedExitCode;
                }

                return Cli.SuccessExitCode;
            }

            private async Task<bool> Pull(CancellationToken token)
            {
                using var git = new GitCommand(CommandConfig, "pull");
                switch (await git.Execute(token))
                {
                    case GitCommand.SuccessExitCode:
                        return true;
                    default:
                        using (var _ = new UseColor(System.ConsoleColor.Red))
                        {
                            await System.Console.Out.WriteLineAsync("Failed to execute pull command following reason.");
                        }

                        using (var _ = new UseColor(System.ConsoleColor.Yellow))
                        {
                            await System.Console.Out.WriteLineAsync(await git.CollectStandardError());
                        }
                        return false;
                }
            }

            private async Task<bool> AddAllAndCommit(LibGit2Sharp.Repository repository, CancellationToken token)
            {
                var author = new LibGit2Sharp.Signature(await GetGitConfig("user.name", token), await GetGitConfig("user.email", token), System.DateTime.Now);
                var committer = author;

                var status = repository.RetrieveStatus(new LibGit2Sharp.StatusOptions(){
                    // only supported above formats
                    PathSpec = new string[] {
                        "*.json",
                        "*.markdown",
                        "*.md",
                        "*.png",
                        "*.jpg",
                        "*.gif",
                    },
                });

                var count = 0;
                using (var _ = new UseColor(System.ConsoleColor.Green))
                {
                    foreach (var item in status)
                    {
                        await System.Console.Out.WriteLineAsync(item.FilePath);
                        count++;
                    }

                    if (count > 0)
                    {
                        LibGit2Sharp.Commands.Stage(repository, "*");

                        var commit = repository.Commit(
                            "commit: " + System.DateTime.Now.ToString("r"),
                            author, committer,
                            new LibGit2Sharp.CommitOptions()
                        );
                    }
                }

                return true;
            }

            private async Task<string> GetGitConfig(string property, CancellationToken token)
            {
                using var git = new GitCommand(CommandConfig, $"config {property}");
                switch (await git.Execute(token))
                {
                    case GitCommand.SuccessExitCode:
                        return await git.CollectStandardOutput();
                    default:
                        using (var _ = new UseColor(System.ConsoleColor.Red))
                        {
                            await System.Console.Out.WriteLineAsync("Failed to execute config command following reason.");
                        }
                        using (var _ = new UseColor(System.ConsoleColor.Yellow))
                        {
                            await System.Console.Out.WriteLineAsync(await git.CollectStandardError());
                        }

                        throw new MemoCliException($"Failed to execute config command. property = {property}");
                }
            }

            private async Task<bool> Push(CancellationToken token)
            {
                using var git = new GitCommand(CommandConfig, "push");
                switch (await git.Execute(token))
                {
                    case GitCommand.SuccessExitCode:
                        return true;
                    default:
                        using (var _ = new UseColor(System.ConsoleColor.Red))
                        {
                            await System.Console.Out.WriteLineAsync("Failed to execute push command following reason.");
                        }
                        using (var _ = new UseColor(System.ConsoleColor.Yellow))
                        {
                            await System.Console.Out.WriteLineAsync(await git.CollectStandardError());
                        }
                        return false;
                }
            }

            private class GitCommand : System.IDisposable
            {
                public const int SuccessExitCode = 0;
                public GitCommand(Core.CommandConfig config, string arguments)
                {
                    _process = new System.Diagnostics.Process();
                    _process.StartInfo.WorkingDirectory = config.HomeDirectory.FullName;
                    _process.StartInfo.FileName = config.GitPath;
                    _process.StartInfo.Arguments = arguments;
                    _process.StartInfo.CreateNoWindow = true;
                    _process.StartInfo.UseShellExecute = false;
                    _process.StartInfo.RedirectStandardInput = true;
                    _process.StartInfo.RedirectStandardOutput = true;
                    _process.StartInfo.RedirectStandardError = true;
                }

                public async Task<int> Execute(CancellationToken token)
                {
                    _process.Start();
                    await _process.WaitForExitAsync(token);
                    return _process.ExitCode;
                }

                public async Task<string> CollectStandardError()
                {
                    return await _process.StandardError.ReadToEndAsync();
                }

                public async Task<string> CollectStandardOutput()
                {
                    return await _process.StandardOutput.ReadToEndAsync();
                }

                public void Dispose()
                {
                    _process?.Dispose();
                }

                private System.Diagnostics.Process _process = null;
            }
        }
    }
}