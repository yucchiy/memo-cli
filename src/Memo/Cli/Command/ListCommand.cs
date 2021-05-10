using System.Collections.Generic;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Memo
{
    public class ListCommand : CommandBase<ListCommand.Input>
    {
        public class Input : CommandInput
        {
            public string Category { get; set; }
            public IEnumerable<string> Queries { get; set; }
            public bool Relative { get; set; }
            public string Format { get; set; }
        }

        protected override Command CreateCommand()
        {
            var command = new Command("list")
            {
                new Option<string>(
                    new string[] {"--category", "-c"},
                    () => string.Empty,
                    "Filter list by category name with regular expression"
                ),
                new Option<IEnumerable<string>>(
                    new string[] {"--queries", "-q"},
                    () => new string[]{},
                    "Filter list by type with regular expression"
                ),
                new Option<bool>(
                    new string[] {"--relative", "-r"},
                    () => false,
                    "Show relative paths from $MEMO_CLI_HOME directory"
                ),
                new Option<string>(
                    new string[] {"--format", "-f"},
                    () => string.Empty,
                    "Specified output format"
                ),
            };
            command.AddAlias("ls");

            return command;
        }

        protected override async Task<int> ExecuteCommand(Input input, CancellationToken token)
        {
            var parameter = new Core.Notes.NoteSearchQueryBuilder();
            if (!string.IsNullOrEmpty(input.Category))
            {
                parameter.WithCategoryId(new Core.Categories.CategoryId(input.Category));
            }
            parameter.WithQueryStrings(input.Queries);

            var notes = await Context.NoteService.GetNotesAsync(parameter.Build(), token);
            
            return await Show(notes, string.IsNullOrEmpty(input.Format) ? "{{ path }}" : input.Format, input.Relative);
        }

        private async Task<int> Show(IEnumerable<Core.Notes.Note> notes, string format, bool withRelativePath)
        {
            var template = Scriban.Template.ParseLiquid(format);
            foreach (var note in notes)
            {
                var path = withRelativePath ? note.RelativePath : $"{Context.CommandConfig.HomeDirectory.FullName}/{note.RelativePath}";
                await Context.Output.WriteLineAsync(template.Render(new { Path = path, Category = note.Category.Id.Value, Type = note.Type?.Value, Created = note.Timestamp, Title = note.Title.Value }));
            }
            
            return Cli.SuccessExitCode;
        }
    }
}