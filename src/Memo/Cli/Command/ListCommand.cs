using System.Collections.Generic;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Memo
{
    public class ListCommand : Command
    {
        public class Input
        {
            public string Category { get; set; }
            public IEnumerable<string> Queries { get; set; }
            public bool Relative { get; set; }
            public string Format { get; set; }
        }

        public ListCommand() : base("list")
        {
            AddOption(new Option<string>(
                new string[] {"--category", "-c"},
                () => string.Empty,
                "Filter list by category name with regular expression"
            ));
            AddOption(new Option<IEnumerable<string>>(
                new string[] {"--queries", "-q"},
                () => new string[]{},
                "Filter list by type with regular expression"
            ));
            AddOption(new Option<bool>(
                new string[] {"--relative", "-r"},
                () => false,
                "Show relative paths from $MEMO_CLI_HOME directory"
            ));
            AddOption(new Option<string>(
                new string[] {"--format", "-f"},
                () => string.Empty,
                "Specified output format"
            ));
            AddAlias("ls");
        }

        public class CommandHandler : ICommandHandler
        {
            // From DI
            public ListCommand.Input Input { get; set; }
            private Core.CommandConfig CommandConfig { get; }
            private Core.Notes.INoteService NoteService { get; }

            public CommandHandler(Core.CommandConfig commandConfig, Core.Notes.INoteService noteService)
            {
                NoteService = noteService;
                CommandConfig = commandConfig;
            }

            public async Task<int> InvokeAsync(InvocationContext context)
            {
                var token = context.GetCancellationToken();
                var parameter = new Core.Notes.NoteSearchQueryBuilder();
                if (!string.IsNullOrEmpty(Input.Category))
                {
                    parameter.WithCategoryId(new Core.Categories.CategoryId(Input.Category));
                }
                parameter.WithQueryStrings(Input.Queries);

                var notes = await NoteService.GetNotesAsync(parameter.Build(), token);
                
                return await Show(notes, string.IsNullOrEmpty(Input.Format) ? "{{ path }}" : Input.Format, Input.Relative);
            }

            private async Task<int> Show(IEnumerable<Core.Notes.Note> notes, string format, bool withRelativePath)
            {
                var template = Scriban.Template.ParseLiquid(format);
                foreach (var note in notes)
                {
                    var noteRelativePath = note.RelativePath.Replace(CommandConfig.DirectorySeparator, System.IO.Path.DirectorySeparatorChar);
                    var path = withRelativePath ? noteRelativePath: System.IO.Path.Combine(CommandConfig.HomeDirectory.FullName, noteRelativePath);
                    await System.Console.Out.WriteLineAsync(template.Render(new { Path = path, Category = note.Category.Id.Value, Type = note.Type?.Value, Created = note.Timestamp, Title = note.Title.Value }));
                }
                
                return Cli.SuccessExitCode;
            }
        }
    }
}