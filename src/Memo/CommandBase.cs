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
        protected Cli Cli { get; set; }
        protected Category[] Categories { get => Cli.Categories; }
        protected CommandConfig CommandConfig { get => Cli.CommandConfig; }
        protected TextWriter Output { get => Cli.Output; }
        protected IConsole Console { get; set; }

        public void Setup(Cli cli)
        {
            Cli = cli;
            var command = CreateCommand();
            command.Handler = CommandHandler.Create(async (TInputType input, IConsole console, CancellationToken token) => await Execute(input, console, token));
            Cli.RootCommand.AddCommand(command);
        }

        public async Task<int> Execute(TInputType input, IConsole console, CancellationToken token)
        {
            Console = console;
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
                    await Output.WriteLineAsync(memoCliException.Message);
                }
            }
            catch (System.Exception unhandledException)
            {
                using (var _ = new UseColor(System.ConsoleColor.Red))
                {
                    await Output.WriteLineAsync(string.Format("Internal Error: {0}({1})", unhandledException.GetType().ToString(), unhandledException.Message));
                    await Output.WriteLineAsync(string.Format("{0}", unhandledException.StackTrace));
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
                Output.WriteLine(message);
            }
        }
    }
}
