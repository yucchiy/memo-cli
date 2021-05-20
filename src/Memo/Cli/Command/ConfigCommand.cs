using System.CommandLine;
using System.Threading.Tasks;
using System.CommandLine.Invocation;

namespace Memo
{
    public class ConfigCommand : Command
    {
        public class Input
        {
            public string Key { get; set; }
        }

        public ConfigCommand()
            : base("config")
        {
            AddArgument(new Argument<string>(
                "key",
                "Config key"
            ));
        }

        public class CommandHandler : ICommandHandler
        {
            public ConfigCommand.Input Input { get; set; }
            private Core.CommandConfig CommandConfig { get; }

            public CommandHandler(Core.CommandConfig commandConfig)
            {
                CommandConfig = commandConfig;
            }

            public async Task<int> InvokeAsync(InvocationContext context)
            {
                switch (Input.Key)
                {
                    case "home":
                        await System.Console.Out.WriteLineAsync(CommandConfig.HomeDirectory.FullName);
                        break;
                    default:
                        using (var _ = new UseColor(System.ConsoleColor.Red))
                        {
                            await System.Console.Out.WriteLineAsync(string.Format("{0}: No such config found.", Input.Key));
                        }

                        return Cli.FailedExitCode;
                }

                return Cli.SuccessExitCode;
            }
        }
    }
}