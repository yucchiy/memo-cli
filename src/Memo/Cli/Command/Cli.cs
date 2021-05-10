using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.CommandLine.Builder;
using System.IO;
using Memo.Core;

namespace Memo
{
    public class Cli
    {
        public static readonly int SuccessExitCode = 0;
        public static readonly int FailedExitCode = 1;

        public RootCommand RootCommand { get; }
        public CommandConfig CommandConfig { get; }
        public TextWriter Output { get; }
        private IMemoCommand[] MemoCommands { get; }

        public Cli()
        {
            RootCommand = new RootCommand("memo");
            RootCommand.Description = "Memo Cli";
            RootCommand.AddGlobalOption(new Option<bool>(
                new string[] {"--no-color"},
                () => false,
                "Disable colorized output"
            ));

            CommandConfig = new CommandConfig();
            Output = System.Console.Out;

            MemoCommands = new IMemoCommand[]
            {
                new NewCommand(),
                new ConfigCommand(),
                new ListCommand(),
                new ListCategoryCommand(),
                new SaveCommand(),
            };

            foreach (var memoCommand in MemoCommands)
            {
                memoCommand.Setup(this);
            }
        }

        public async Task<int> Execute(string[] arguments)
        {
            return await RootCommand.InvokeAsync(arguments);
        }

        public static Parser CreateCommandLineParser()
        {
            var cli = new Cli();

            return new CommandLineBuilder(
                cli.RootCommand
            )
                .UseDefaults()
                .Build();
        }
    }
}