using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markdig.Extensions.Yaml;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Memo.Core.Notes
{
    public class NoteSerializer : INoteSerializer
    {
        private static readonly HashSet<string> AcceptedExtensions = new HashSet<string>(new string[]{".md", ".markdown"});
        private static readonly char FrontMatterSeparator = ':';

        private const string kFrontMatterKeyTitle = "title";
        private const string kFrontMatterKeyType = "type";
        private const string kFrontMatterKeyCreated = "created";

        private INoteBuilder NoteBuilder { get; }
        private Categories.ICategoryConfigStore CategoryConfigStore { get; }
        private MarkdownPipeline MarkdownPipeline { get; }
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

        public NoteSerializer(INoteBuilder noteBuilder, Categories.ICategoryConfigStore categoryConfigStore, Options option)
        {
            NoteBuilder = noteBuilder;
            CategoryConfigStore = categoryConfigStore;
            MarkdownPipeline = new MarkdownPipelineBuilder()
                .UseYamlFrontMatter()
                .UseAdvancedExtensions()
                .Build();
            Option = option;
        }

        public NoteSerializer(INoteBuilder noteBuilder, Categories.ICategoryConfigStore categoryConfigStore, MarkdownPipeline markdownPipeline, Options option)
        {
            NoteBuilder = noteBuilder;
            CategoryConfigStore = categoryConfigStore;
            MarkdownPipeline = markdownPipeline;
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

            var (categoryId, noteId) = ParseId(fileInfo);
            var (frontMatters, links) = ParseContent(rawContent);

            var builder = (new NoteCreationParameterBuilder())
                .WithCategoryId(categoryId)
                .WithId(noteId)
                .WithLinks(links.Where(link => link.Length > 4 && link.IndexOf("http") == 0))
                .WithInternalLinks(
                    links
                        .Select(link => TryParseLink(link, out var cid, out var nid) ? (cid, nid) : default)
                        .Where(link => !string.IsNullOrEmpty(link.cid.Value) && !string.IsNullOrEmpty(link.nid.Value))
                );

            if (frontMatters.TryGetValue(kFrontMatterKeyTitle, out var noteTitle))
            {
                builder.WithTitle(noteTitle);
            }

            if (frontMatters.TryGetValue(kFrontMatterKeyType, out var noteType))
            {
                builder.WithType(noteType);
            }

            if (frontMatters.TryGetValue(kFrontMatterKeyCreated, out var timestampContent) && System.DateTime.TryParse(timestampContent, out var noteTimestamp))
            {
                builder.WithTimestamp(noteTimestamp);
            }

            return (true, await NoteBuilder.BuildAsync(builder.Build(), token));
        }

        private (Dictionary<string, string> frontMatters, List<string>) ParseContent(string rawContent)
        {
            var frontMatters = new Dictionary<string, string>();
            var links = new List<string>();

            var document = Markdown.Parse(rawContent, MarkdownPipeline);
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

                        frontMatters.Add(frontMatterKey, frontMatterValue);
                    }
                }
                else if (block is LinkReferenceDefinition linkReferenceDefinition)
                {
                    links.Add(linkReferenceDefinition.Url);
                }
                else if (block is LinkReferenceDefinitionGroup linkReferenceDefinitionGroup)
                {
                    foreach (var link in linkReferenceDefinitionGroup.Links)
                    {
                        links.Add(link.Value.Url);
                    }
                }
                else if (block is ParagraphBlock paragraphBlock)
                {
                    foreach (var inline in paragraphBlock.Inline)
                    {
                        if (inline is LinkInline linkInline)
                        {
                            links.Add(linkInline.Url);
                        }
                    }
                }
            }

            links = links.Where(link => !string.IsNullOrEmpty(link)).ToList();

            return (frontMatters, links);
        }

        private bool TryParseLink(string link, out Categories.CategoryId categoryId, out Note.NoteId noteId)
        {
            categoryId = default;
            noteId = default;

            if (link.Length > 4 && link.IndexOf("http") == 0)
            {
                return false;
            }

            var splitResult = link.Split(Option.NoteDirectorySeparator);
            if (splitResult.Length < 3)
            {
                return false;
            }

            categoryId = new Categories.CategoryId(
                string.Join(Option.NoteDirectorySeparator, splitResult.SkipLast(2))
            );
            noteId = new Note.NoteId(splitResult.TakeLast(2).First());

            return true;
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