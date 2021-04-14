using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Memo
{
    public class ConfigCommand : CommandBase<ConfigCommand.Input>
    {
        public class Input : CommandInput
        {
            public string Key { get; set; }
        }

        protected override Command CreateCommand()
        {
            var command = new Command("config")
            {
                new Argument<string>(
                    "key",
                    "Config key"
                )
            };

            return command;
        }

        protected override async Task<int> ExecuteCommand(Input input, CancellationToken token)
        {
            switch (input.Key)
            {
                case "home":
                    await Output.WriteLineAsync(CommandConfig.HomeDirectory.FullName);
                    break;
                default:
                    using (var _ = new UseColor(System.ConsoleColor.Red))
                    {
                        await Output.WriteLineAsync(string.Format("{0}: No such config found.", input.Key));
                    }

                    return Cli.FailedExitCode;
            }


            return Cli.SuccessExitCode;
        }
    }
}