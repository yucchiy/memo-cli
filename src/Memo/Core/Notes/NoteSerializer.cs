using Markdig;
using Markdig.Extensions.Yaml;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System;

namespace Memo.Core.Notes
{
    public class NoteParser : INoteSerializer
    {
        private static readonly HashSet<string> AcceptedExtensions = new HashSet<string>(new string[]{".md", ".markdown"});
        private static readonly char FrontMatterSeparator = ':';

        private const string kFrontMatterKeyTitle = "title";
        private const string kFrontMatterKeyType = "type";
        private const string kFrontMatterKeyCreated = "created";

        private Categories.ICategoryConfigStore CategoryConfigStore { get; }
        private MarkdownPipeline FrontMatterPipeline { get; }
        private Options Option { get; }

        public class Options
        {
            public DirectoryInfo RootDirectory { get; }
            public char NoteDirectorySeparator { get; }

            public Options(DirectoryInfo rootDirectory, char noteDirectorySeparator)
            {
                RootDirectory = rootDirectory;
                NoteDirectorySeparator = noteDirectorySeparator;
            }
        }

        public NoteParser(Categories.ICategoryConfigStore categoryConfigStore, Options option)
        {
            CategoryConfigStore = categoryConfigStore;
            FrontMatterPipeline = new MarkdownPipelineBuilder()
                .UseYamlFrontMatter()
                .Build();
            Option = option;
        }

        public NoteParser(Categories.ICategoryConfigStore categoryConfigStore, MarkdownPipeline frontMatterPipeline, Options option)
        {
            CategoryConfigStore = categoryConfigStore;
            FrontMatterPipeline = frontMatterPipeline;
            Option = option;
        }

        // (result, note)
        public async Task<(bool, Note)> DeserializeNoteAsync(FileInfo fileInfo, CancellationToken token)
        {
            // check extension
            if (!AcceptedExtensions.Contains(fileInfo.Extension.ToLower()))
            {
                return (false, default);
            }

            var rawContent = await File.ReadAllTextAsync(fileInfo.FullName);

            var frontMatters = ParseFrontMatter(rawContent);

            var noteTitle = new Note.NoteTitle(frontMatters.TryGetValue(kFrontMatterKeyTitle, out var title) ? title : string.Empty);
            System.Nullable<Note.NoteType> noteType = frontMatters.TryGetValue(kFrontMatterKeyType, out var type) ? new Note.NoteType(type) : null;
            System.Nullable<System.DateTime> noteCreated = (frontMatters.TryGetValue(kFrontMatterKeyCreated, out var createdContent) && System.DateTime.TryParse(createdContent, out var created)) ? created : null;

            var (categoryId, noteId) = ParseId(fileInfo);

            return (true, new Note(new Categories.Category(categoryId), noteId, noteTitle, noteType, noteCreated));
        }

        private Dictionary<string, string> ParseFrontMatter(string rawContent)
        {
            var result = new Dictionary<string, string>();
            var document = Markdown.Parse(rawContent, FrontMatterPipeline);

            foreach (var block in document)
            {
                if (block is YamlFrontMatterBlock yamlFrontMatterBlock)
                {
                    foreach (var line in yamlFrontMatterBlock.Lines.Lines)
                    {
                        var content = line.ToString();

                        var splitResult = content.Split(FrontMatterSeparator);
                        if (splitResult.Length != 2)
                        {
                            continue;
                        }

                        var frontMatterKey = splitResult[0].Trim();
                        // ` "this is a test"` => `"this is a test"` => `this is a test`
                        var frontMatterValue = splitResult[1].Trim().Trim('"');

                        result.Add(frontMatterKey, frontMatterValue);
                    }
                }
            }

            return result;
        }

        private (Categories.CategoryId CategoryId, Note.NoteId NoteId) ParseId(FileInfo fileInfo)
        {
            var relativePath = Path.GetRelativePath(Option.RootDirectory.FullName, fileInfo.Directory.FullName).Replace(Path.PathSeparator, Option.NoteDirectorySeparator);
            if (fileInfo.Name.IndexOf("index") == 0 && AcceptedExtensions.Contains(fileInfo.Extension))
            {
                // /path/to/root/category1/category2/test_note/index.markdown
                // category id: category1/category2
                // note id: test_note
                var splitResult = relativePath.Split(Option.NoteDirectorySeparator);
                var categoryId = new Categories.CategoryId(
                    string.Join(Option.NoteDirectorySeparator, splitResult.SkipLast(1))
                );
                var noteId = new Note.NoteId(splitResult.TakeLast(1).First());

                return (categoryId, noteId);
            }
            else
            {
                // /path/to/root/category1/category2/test_note.markdown
                // category id: category1/category2
                // note id: test_note
 
                var categoryId = new Categories.CategoryId(
                    relativePath
                );

                var noteId = new Note.NoteId(fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf(fileInfo.Extension)));

                return (categoryId, noteId);
            }
        }

        public async Task<(bool Success, string RawContent)> SerializeNoteAsync(Note note, CancellationToken token)
        {
            var config = CategoryConfigStore.GetConfig(note.Category);
            var template = Scriban.Template.ParseLiquid(await GetTemplate(Option.RootDirectory.FullName, config, token));
            var options = new Dictionary<string, string>();

            var stringBuilder = new System.Text.StringBuilder();

            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine($"{kFrontMatterKeyTitle}: {note.Title.Value}");

            if (note.Type is Note.NoteType noteType)
            {
                stringBuilder.AppendLine($"{kFrontMatterKeyType}: {noteType.Value}");
            }

            if (note.Created is DateTime created)
            {
                stringBuilder.AppendLine($"{kFrontMatterKeyCreated}: {created.ToString("yyyy-MM-ddTHH:mm:ss")}");
            }

            stringBuilder.AppendLine("---");
            stringBuilder.Append(template.RenderAsync(options));

            return (true, stringBuilder.ToString());
       }

        private async Task<string> GetTemplate(string rootDirectory, MemoConfig.CategoryConfig categoryConfig, CancellationToken token)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = string.IsNullOrEmpty(categoryConfig.MemoTemplateFilePath) ?
                new StreamReader(assembly.GetManifestResourceStream($"Memo.res.Default.md")) :
                new StreamReader(System.IO.File.OpenRead(Path.Combine(rootDirectory, categoryConfig.MemoTemplateFilePath)));

            return await stream.ReadToEndAsync();
        }
    }
}