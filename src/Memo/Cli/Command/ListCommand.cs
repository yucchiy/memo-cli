using System.IO;
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
            var notes = new List<Note>();
            notes.AddRange(await Context.MemoManager.GetNotesAsync(Context.MemoManager.GetCategory(input.Category), input.Queries));
            notes.Sort(NoteCollector.CompareByModified);
            notes.Reverse();
            
            return await Show(notes, string.IsNullOrEmpty(input.Format) ? "{{ path }}" : input.Format, input.Relative);
        }

        private async Task<int> Show(List<Note> notes, string format, bool withRelativePath)
        {
            var template = Scriban.Template.ParseLiquid(format);
            foreach (var note in notes)
            {
                var path = withRelativePath ? ToRelativePath(note.File) : note.File.FullName;
                await Context.Output.WriteLineAsync(template.Render(new { Path = path, Category = note.Category.Name, Type = note.Meta?.Type, Created = note.Meta?.Created, Modified = note.Modified, Title = note.Meta?.Title }));
            }
            
            return Cli.SuccessExitCode;
        }

        private string ToRelativePath(FileInfo filePath) => Path.GetRelativePath(Context.CommandConfig.HomeDirectory.FullName, filePath.FullName);
    }
}