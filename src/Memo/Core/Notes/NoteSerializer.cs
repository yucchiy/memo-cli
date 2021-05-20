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

        private class Options
        {
            public DirectoryInfo RootDirectory { get; }
            public char NoteDirectorySeparator { get; }

            public Options(DirectoryInfo rootDirectory, char noteDirectorySeparator)
            {
                RootDirectory = rootDirectory;
                NoteDirectorySeparator = noteDirectorySeparator;
            }
        }

        public NoteSerializer(INoteBuilder noteBuilder, Categories.ICategoryConfigStore categoryConfigStore, CommandConfig commandConfig)
        {
            NoteBuilder = noteBuilder;
            CategoryConfigStore = categoryConfigStore;
            MarkdownPipeline = new MarkdownPipelineBuilder()
                .UseYamlFrontMatter()
                .UseAdvancedExtensions()
                .Build();
            Option = new Options(commandConfig.HomeDirectory, commandConfig.DirectorySeparator);
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

            if (!TryParseId(fileInfo, out var ids))
            {
                return (false, default);
            }

            var (categoryId, noteTimestamp, noteSlug) = ids;
            var (frontMatters, links) = ParseContent(rawContent);

            var builder = (new NoteCreationParameterBuilder())
                .WithSlug(noteSlug)
                .WithTimestamp(noteTimestamp)
                .WithCategoryId(categoryId)
                .WithLinks(links.Where(link => link.Length > 4 && link.IndexOf("http") == 0))
                .WithInternalLinks(
                    links
                        .Select(link => TryParseLink(link, out var internalLink) ? internalLink : string.Empty)
                        .Where(link => !string.IsNullOrEmpty(link))
                );

            if (frontMatters.TryGetValue(kFrontMatterKeyTitle, out var noteTitle))
            {
                builder.WithTitle(noteTitle);
            }

            if (frontMatters.TryGetValue(kFrontMatterKeyType, out var noteType))
            {
                builder.WithType(noteType);
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

        private bool TryParseLink(string link, out string internalLink)
        {
            internalLink = default;
            if (link.Length > 4 && link.IndexOf("http") == 0)
            {
                return false;
            }

            var splitResult = link.Split(Option.NoteDirectorySeparator);
            if (splitResult.Length < 3)
            {
                return false;
            }

            internalLink = link;
            return true;
        }

        private bool TryParseId(FileInfo fileInfo, out (Categories.CategoryId CategoryId, Note.NoteTimestamp timestamp, Note.NoteSlug slug) ids)
        {
            var relativePath = Path.GetRelativePath(Option.RootDirectory.FullName, fileInfo.Directory.FullName).Replace(Path.PathSeparator, Option.NoteDirectorySeparator);
            if (AcceptedExtensions.Contains(fileInfo.Extension))
            {
                // /path/to/root/category1/category2/timestamp/slug.markdown
                // category id: category1/category2
                // note id: timestamp/slug
                var splitResult = relativePath.Split(Option.NoteDirectorySeparator);
                var categoryId = new Categories.CategoryId(
                    string.Join(Option.NoteDirectorySeparator, splitResult.SkipLast(1))
                );
                var slug = new Note.NoteSlug(Path.GetFileNameWithoutExtension(fileInfo.FullName));
                if (DateTime.TryParseExact(splitResult.TakeLast(1).First(), Note.NoteTimestamp.NoteTimestampFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var timestamp))
                {
                    ids = (categoryId, new Note.NoteTimestamp(timestamp), slug);
                    return true;
                }
            }

            ids = default;
            return false;
        }

        public async Task<(bool Success, string RawContent)> SerializeNoteAsync(Note note, CancellationToken token)
        {
            var config = CategoryConfigStore.GetConfig(note.Category.Id);
            var template = Scriban.Template.ParseLiquid(await GetTemplate(Option.RootDirectory.FullName, config, token));
            var options = new Dictionary<string, string>();

            var stringBuilder = new System.Text.StringBuilder();

            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine($"{kFrontMatterKeyTitle}: \"{note.Title.Value}\"");

            if (note.Type is Note.NoteType noteType)
            {
                stringBuilder.AppendLine($"{kFrontMatterKeyType}: {noteType.Value}");
            }

            stringBuilder.AppendLine($"{kFrontMatterKeyCreated}: {note.Timestamp.Value.ToString("yyyy-MM-ddTHH:mm:ss")}");

            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine("");
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