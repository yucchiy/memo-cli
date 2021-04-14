using System.IO;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

namespace Memo
{
    public abstract class CommandBase<TInputType> : IMemoCommand
        where TInputType : CommandInput
    {
        public class MemoContext
        {
            public MemoManager MemoManager { get; }
            public CommandConfig CommandConfig { get; }
            public TextWriter Output { get; }
            public IConsole Console { get; }

            public MemoContext(MemoManager memoManager, CommandConfig commandConfig, TextWriter output)
            {
                MemoManager = memoManager;
                CommandConfig = commandConfig;
                Output = output;
            }
        }

        public MemoContext Context { get; private set; }

        public void Setup(Cli cli)
        {
            var command = CreateCommand();
            command.Handler = CommandHandler.Create(async (TInputType input, CancellationToken token) => await Execute(input, token));
            cli.RootCommand.AddCommand(command);

            Context = new MemoContext(
                new MemoManager(new MemoManager.Configuration()
                {
                    HomeDirectory = cli.CommandConfig.HomeDirectory
                }),
                cli.CommandConfig,
                cli.Output
            );
        }

        public async Task<int> Execute(TInputType input, CancellationToken token)
        {
            if (input.NoColor)
            {
                UseColor.NoColor = true;
            }

            try
            {
                return await ExecuteCommand(input, token);
            }
            catch (MemoCliException memoCliException)
            {
                using (var _ = new UseColor(System.ConsoleColor.Red))
                {
                    await Context.Output.WriteLineAsync(memoCliException.Message);
                }
            }
            catch (System.Exception unhandledException)
            {
                using (var _ = new UseColor(System.ConsoleColor.Red))
                {
                    await Context.Output.WriteLineAsync(string.Format("Internal Error: {0}({1})", unhandledException.GetType().ToString(), unhandledException.Message));
                    await Context.Output.WriteLineAsync(string.Format("{0}", unhandledException.StackTrace));
                }
            }

            return Cli.FailedExitCode;
        }

        protected abstract Command CreateCommand();
        protected abstract Task<int> ExecuteCommand(TInputType input, CancellationToken token);

        protected void ConsoleError(string message)
        {
            using (var alertColor = new UseColor(System.ConsoleColor.Red))
            {
                Context.Output.WriteLine(message);
            }
        }
    }
}
