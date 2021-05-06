using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Memo
{
    public class NewCommand : CommandBase<NewCommand.Input>
    {
        public class Input : CommandInput
        {
            public string Category { get; set; }
            public string[] Options { get; set; }
        }

        protected override Command CreateCommand()
        {
            var command = new Command("new")
            {
                new Argument<string>(
                    "category",
                    "Category of note. Note must belong to one category"
                ),
               new Option<string[]>(
                    new string[] {"--options"},
                    () => new string[0],
                    "A options of creating note."
                )
            };
            command .AddAlias("n");

            return command;
        }

        protected override async Task<int> ExecuteCommand(Input input, CancellationToken token)
        {
            var builder = new Core.Notes.NoteCreationParameterBuilder();
            builder.WithCategoryId(input.Category);
            builder.WithQueryStrings(input.Options);

            if (await Context.NoteService.CreateNoteAsync(builder.Build(), token) is Core.Notes.Note note)
            {
                if (input.NoColor)
                {
                    await Context.Output.WriteLineAsync($"{Context.CommandConfig.HomeDirectory.FullName}/{note.RelativePath}");
                }
                else
                {
                    using (new UseColor(System.ConsoleColor.Green))
                    {
                        await Context.Output.WriteLineAsync($"{Context.CommandConfig.HomeDirectory.FullName}/{note.RelativePath}");
                    }
                }

                return Cli.SuccessExitCode;
            }

            throw new MemoCliException("Failed to create note");
        }
    }
}