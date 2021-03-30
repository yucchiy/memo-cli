using System.IO;
using System.Collections.Generic;
using System.CommandLine;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Memo
{
    public class ListCommand : CommandBase<ListCommand.Input>
    {
        public class Input : CommandInput
        {
            public string Category { get; set; }
            public string Type { get; set; }
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
                new Option<string>(
                    new string[] {"--type", "-t"},
                    () => string.Empty,
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
            var notes = await CollectNotes(input, token);

            notes.Sort(NoteCollector.CompareByModified);
            notes.Reverse();
            
            return await Show(notes, string.IsNullOrEmpty(input.Format) ? "{{ path }}" : input.Format, input.Relative);
        }

        private async Task<List<Note>> CollectNotes(Input input, CancellationToken token)
        {
            var notes = new List<Note>();
            var noteCollector = new NoteCollector();
            foreach (var category in Categories)
            {
                if (!string.IsNullOrEmpty(input.Category))
                {
                    if (Regex.IsMatch(category.Name, input.Category))
                    {
                        notes.AddRange(await noteCollector.Collect(category, input.Type));
                    }
                }
                else
                {
                    notes.AddRange(await noteCollector.Collect(category, input.Type));
                }
            }

            return notes;
        }

        private async Task<int> Show(List<Note> notes, string format, bool withRelativePath)
        {
            var template = Scriban.Template.ParseLiquid(format);
            foreach (var note in notes)
            {
                var path = withRelativePath ? ToRelativePath(note.File) : note.File.FullName;
                await Output.WriteLineAsync(template.Render(new { Path = path, Category = note.Category.Name, Type = note.Meta?.Type, Created = note.Meta?.Created, Modified = note.Modified, Title = note.Meta?.Title }));
            }
            
            return Cli.SuccessExitCode;
        }

        private string ToRelativePath(FileInfo filePath) => Path.GetRelativePath(CommandConfig.HomeDirectory.FullName, filePath.FullName);
    }
}