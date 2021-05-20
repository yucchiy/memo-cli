using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Memo
{
    public class NewCommand : Command
    {
        public class Input
        {
            public bool NoColor { get; set; }
            public string Category { get; set; }
            public string[] Options { get; set; }
        }

        public NewCommand() : base("new")
        {
            AddArgument(new Argument<string>(
                "category",
                "Category of note. Note must belong to one category"
            ));
            AddOption(new Option<string[]>(
                new string[] {"--options"},
                () => new string[0],
                "A options of creating note."
            ));

            AddAlias("n");
        }

        public class CommandHandler : ICommandHandler
        {
            public NewCommand.Input Input { get; set; }
            private Core.CommandConfig CommandConfig { get; }
            private Core.Notes.INoteService NoteService { get; }

            public CommandHandler(Core.CommandConfig commandConfig, Core.Notes.INoteService noteService)
            {
                CommandConfig = commandConfig;
                NoteService = noteService;
            }

            public async Task<int> InvokeAsync(InvocationContext context)
            {
                var token = context.GetCancellationToken();
                var builder = new Core.Notes.NoteCreationParameterBuilder();
                builder.WithCategoryId(Input.Category);
                builder.WithQueryStrings(Input.Options);

                if (await NoteService.CreateNoteAsync(builder.Build(), token) is Core.Notes.Note note)
                {
                    if (Input.NoColor)
                    {
                        await System.Console.Out.WriteLineAsync($"{CommandConfig.HomeDirectory.FullName}/{note.RelativePath}");
                    }
                    else
                    {
                        using (new UseColor(System.ConsoleColor.Green))
                        {
                            await System.Console.Out.WriteLineAsync($"{CommandConfig.HomeDirectory.FullName}/{note.RelativePath}");
                        }
                    }

                    return Cli.SuccessExitCode;
                }

                throw new MemoCliException("Failed to create note");
            }
        }
    }
}