using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Memo
{
    public class NewCommand : CommandBase<NewCommand.Input>
    {
        public class Input : CommandInput
        {
            public string Title { get; set; }
            public string Category { get; set; }
            public string Filename { get; set; }
            public string Url { get; set; }
        }

        protected override Command CreateCommand()
        {
            var command = new Command("new")
            {
                new Option<string>(
                    new string[] {"--title", "-t"},
                    "Title of note."
                ),
                new Option<string>(
                    new string[] {"--category", "-c"},
                    "Category of note. Note must belong to one category"
                ),
                new Option<string>(
                    new string[] {"--filename", "-f"},
                    "File name of note. It automatically adds '.markdown' file extension if omitted"
                ),
                new Option<string>(
                    new string[] {"--url"},
                    "Collect title and filename from url. If this option specificated, title and filename will be override."
                ),
            };
            command .AddAlias("n");

            return command;
        }

        protected override async Task<int> ExecuteCommand(Input input, CancellationToken token)
        {
            var note = await Context.MemoManager.CreateNoteAsync(new NoteCreationParameter()
            {
                Title = input.Title,
                Category = input.Category,
                Filename = input.Filename,
                Url = input.Url,
            },
            token);

            return Cli.SuccessExitCode;
        }
    }
}