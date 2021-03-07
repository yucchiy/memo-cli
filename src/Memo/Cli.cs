using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace Memo
{
    public class Cli
    {
        public static readonly int SuccessExitCode = 0;
        public static readonly int FailedExitCode = 1;

        public RootCommand RootCommand { get; }
        public CommandConfig CommandConfig { get; }
        public Category[] Categories { get; }
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

            Categories = new CategoryCollector().Collect(CommandConfig.HomeDirectory);

            MemoCommands = new IMemoCommand[]
            {
                new NewCommand(),
                new ListCommand(),
                new ListCategoryCommand(),
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
    }
}