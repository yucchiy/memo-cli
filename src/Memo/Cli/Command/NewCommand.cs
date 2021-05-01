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
            public string Id { get; set; }
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
                new Option<string>(
                    "--id",
                    "A id of note."
                ),
                new Option<string[]>(
                    new string[] {"--options"},
                    "A options of creating note."
                )
            };
            command .AddAlias("n");

            return command;
        }

        protected override async Task<int> ExecuteCommand(Input input, CancellationToken token)
        {
            var parameter = await Context.MemoManager.CreateNoteCreationParameter(input.Category, input.Id, input.Options, token);
            var note = await Context.MemoManager.CreateNoteAsync(parameter, token);
            if (input.NoColor)
            {
                await Context.Output.WriteLineAsync($"{note.File.FullName}");
            }
            else
            {
                using (new UseColor(System.ConsoleColor.Green))
                {
                    await Context.Output.WriteLineAsync($"Created {note.File.FullName}");
                }
            }

            return Cli.SuccessExitCode;
        }
    }
}