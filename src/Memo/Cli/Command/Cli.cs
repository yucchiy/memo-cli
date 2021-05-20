using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Memo.Core;

namespace Memo
{
    public class Cli
    {
        public static readonly int SuccessExitCode = 0;
        public static readonly int FailedExitCode = 1;

        private RootCommand RootCommand { get; }

        public Cli()
        {
            RootCommand = new RootCommand("memo");
            RootCommand.Description = "Memo Cli";
            RootCommand.AddGlobalOption(new Option<bool>(
                new string[] {"--no-color"},
                () => false,
                "Disable colorized output"
            ));

            RootCommand.AddCommand(new NewCommand());
            RootCommand.AddCommand(new ConfigCommand());
            RootCommand.AddCommand(new ListCommand());
            RootCommand.AddCommand(new ListCategoryCommand());
            RootCommand.AddCommand(new SaveCommand());
        }

        public async Task<int> ExecuteAsync(string[] arguments)
        {
            var builder = new CommandLineBuilder(RootCommand);
            builder.UseDefaults();
            builder.UseHost(host => 
            {
                host.ConfigureServices((context, services) =>
                {
                    services.AddSingleton<CommandConfig>();
                    services.AddSingleton<Core.Categories.ICategoryConfigStore, Core.Categories.CategoryConfigStore>();
                    services.AddSingleton<Core.Notes.INoteBuilder, Core.Notes.NoteBuilder>();
                    services.AddSingleton<Core.Notes.INoteSerializer, Core.Notes.NoteSerializer>();
                    services.AddSingleton<Core.Notes.INoteQueryFilter, Core.Notes.NoteQueryFilter>();
                    services.AddSingleton<Core.Notes.INoteStorage, Core.Notes.NoteStorageFileSystemImpl>();
                    services.AddSingleton<Core.Notes.INoteRepository, Core.Notes.NoteRepository>();
                    services.AddSingleton<Core.Notes.INoteService, Core.Notes.NoteService>();
                    services.AddSingleton<Core.Categories.ICategoryRepository, Core.Categories.CategoryRepository>();
                    services.AddSingleton<Core.Categories.ICategoryService, Core.Categories.CategoryService>();
                });

                host.UseCommandHandler<NewCommand, NewCommand.CommandHandler>();
                host.UseCommandHandler<ConfigCommand, ConfigCommand.CommandHandler>();
                host.UseCommandHandler<ListCommand, ListCommand.CommandHandler>();
                host.UseCommandHandler<ListCategoryCommand, ListCategoryCommand.CommandHandler>();
                host.UseCommandHandler<SaveCommand, SaveCommand.CommandHandler>();
            });

            var parser = builder.Build();

            try
            {
                return await parser.InvokeAsync(arguments);
            }
            catch (MemoCliException memoCliException)
            {
                using (var _ = new UseColor(System.ConsoleColor.Red))
                {
                    await System.Console.Out.WriteLineAsync(memoCliException.Message);
                }
            }
            catch (System.Exception unhandledException)
            {
                using (var _ = new UseColor(System.ConsoleColor.Red))
                {
                    await System.Console.Out.WriteLineAsync(string.Format("Internal Error: {0}({1})", unhandledException.GetType().ToString(), unhandledException.Message));
                    await System.Console.Out.WriteLineAsync(string.Format("{0}", unhandledException.StackTrace));
                }
            }

            return Cli.FailedExitCode;
        }
    }
}