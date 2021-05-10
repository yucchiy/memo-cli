using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
using Memo.Core;

namespace Memo
{
    public class SaveCommand : CommandBase<SaveCommand.Input>
    {
        public class Input : CommandInput
        {
            public bool NoSync { get; set; }
        }

        protected override Command CreateCommand()
        {
            var command = new Command("save")
            {
                new Option<bool>(
                    new string[] {"--no-sync"},
                    () => false,
                    "Without sync remote"
                ),
            };

            return command;
        }

        protected override async Task<int> ExecuteCommand(Input input, CancellationToken token)
        {
            using (var repository = new LibGit2Sharp.Repository(Context.CommandConfig.HomeDirectory.FullName))
            {
                if (!input.NoSync && !(await Pull(token))) return Cli.FailedExitCode;
                if (!(await AddAllAndCommit(repository, token))) return Cli.FailedExitCode;
                if (!input.NoSync && !(await Push(token))) return Cli.FailedExitCode;
            }

            return Cli.SuccessExitCode;
        }

        private async Task<bool> Pull(CancellationToken token)
        {
            using var git = new GitCommand(Context.CommandConfig, "pull");
            switch (await git.Execute(token))
            {
                case GitCommand.SuccessExitCode:
                    return true;
                default:
                    using (var _ = new UseColor(System.ConsoleColor.Red))
                    {
                        await Context.Output.WriteLineAsync("Failed to execute pull command following reason.");
                    }

                    using (var _ = new UseColor(System.ConsoleColor.Yellow))
                    {
                        await Context.Output.WriteLineAsync(await git.CollectStandardError());
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
                    await Context.Output.WriteLineAsync(item.FilePath);
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
            using var git = new GitCommand(Context.CommandConfig, $"config {property}");
            switch (await git.Execute(token))
            {
                case GitCommand.SuccessExitCode:
                    return await git.CollectStandardOutput();
                default:
                    using (var _ = new UseColor(System.ConsoleColor.Red))
                    {
                        await Context.Output.WriteLineAsync("Failed to execute config command following reason.");
                    }
                    using (var _ = new UseColor(System.ConsoleColor.Yellow))
                    {
                        await Context.Output.WriteLineAsync(await git.CollectStandardError());
                    }

                    throw new MemoCliException($"Failed to execute config command. property = {property}");
            }
        }

        private async Task<bool> Push(CancellationToken token)
        {
            using var git = new GitCommand(Context.CommandConfig, "push");
            switch (await git.Execute(token))
            {
                case GitCommand.SuccessExitCode:
                    return true;
                default:
                    using (var _ = new UseColor(System.ConsoleColor.Red))
                    {
                        await Context.Output.WriteLineAsync("Failed to execute push command following reason.");
                    }
                    using (var _ = new UseColor(System.ConsoleColor.Yellow))
                    {
                        await Context.Output.WriteLineAsync(await git.CollectStandardError());
                    }
                    return false;
            }
        }

        private class GitCommand : System.IDisposable
        {
            public const int SuccessExitCode = 0;
            public GitCommand(CommandConfig config, string arguments)
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