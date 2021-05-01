using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.Yaml;
using System.Linq;
using YamlDotNet.Serialization;
using System.Threading.Tasks;

namespace Memo
{
    public class NoteCollector
    {
        private MarkdownPipeline DefaultFrontMatterPipeline { get; }
        private MarkdownPipeline YamlFrontMatterPipeline { get; }
        private IDeserializer YamlDeserializer { get; }
        private CategoryCollector CategoryCollector { get; }

        public NoteCollector(CategoryCollector categoryCollector)
        {
            DefaultFrontMatterPipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseYamlFrontMatter()
                .Build();
            YamlFrontMatterPipeline = new MarkdownPipelineBuilder()
                .UseYamlFrontMatter()
                .Build();
            YamlDeserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();
            CategoryCollector = categoryCollector;
        }

        public async Task<Note[]> Collect(Category filterCategory, IEnumerable<string> queries)
        {
            var notes = new List<Note>();
            var queryMap = TryParseQueries(queries);
            foreach (var category in CategoryCollector.Collect(filterCategory.Path))
            {
                foreach (var file in category.Path.GetFiles())
                {
                    if (file.Name.StartsWith('.')) continue;

                    if (Path.GetExtension(file.FullName) is string extension)
                    {
                        extension = extension.ToLower();
                        if (extension == ".md" || extension == ".markdown")
                        {
                            var noteText = await File.ReadAllTextAsync(file.FullName);
                            var contentDocument = Markdown.Parse(noteText, DefaultFrontMatterPipeline);

                            var document = Markdown.Parse(noteText, YamlFrontMatterPipeline);

                            var yamlFrontMatterBlocks = document
                                .Where(block => block is YamlFrontMatterBlock)
                                .Select(block => block as YamlFrontMatterBlock)
                                .ToArray();

                            var note = new Note(file, contentDocument, Path.GetFileNameWithoutExtension(file.FullName) == "index" ? (category.ParentCategory == null ? category : category.ParentCategory) : category);
                            if (yamlFrontMatterBlocks.Length > 0)
                            {
                                var yamlText = yamlFrontMatterBlocks[0]
                                    .Lines
                                    .Lines
                                    .Where(x => x.Slice.Length > 0)
                                    .Select(x => $"{x}\n")
                                    .Aggregate((line, aggrigation) => aggrigation + line);
                                try
                                {
                                    note.Meta = YamlDeserializer.Deserialize<NoteMetaData>(yamlText);
                                }
                                catch (Exception)
                                {
                                }
                            }

                            if (note.Meta == null)
                            {
                                note.Meta = new NoteMetaData();
                                note.Meta.Category = note.Category.Name;
                                note.Meta.Title = note.ContentTitle;
                                note.Meta.Created = DateTime.Now;
                                note.Meta.Type = string.Empty;
                            }

                            if (queryMap.TryGetValue("id", out var id))
                            {
                                if (note.Category.Name != filterCategory.Name || note.Id != id) continue;
                            }

                            if (queryMap.TryGetValue("type", out var type))
                            {
                                if (note.Meta == null || string.IsNullOrEmpty(note.Meta.Type)) continue;
                                if (!Regex.IsMatch(note.Meta.Type, type)) continue;
                            }

                            notes.Add(note);
                        }
                    }
                }
            }

            return notes.ToArray();
        }

        private Dictionary<string, string> TryParseQueries(IEnumerable<string> queries)
        {
            var result = new Dictionary<string, string>();
            foreach (var query in queries)
            {
                if (TryParseQuery(query, out var kv))
                {
                    result.Add(kv.Key, kv.Value);
                }
            }

            return result;
        }

        private bool TryParseQuery(string query, out KeyValuePair<string, string> result)
        {
            var indexOf = query.IndexOf(":");
            if (indexOf > 0)
            {
                result = new KeyValuePair<string, string>(query.Substring(0, indexOf), query.Substring(indexOf + 1));
                return true;
            }

            result = default;
            return false;
        }

        public async Task<Note> Find(Category category, string id)
        {
            var notes = await Collect(category, new string[] {$"id:{id}"});
            return notes.FirstOrDefault();
        }

        public static int CompareByModified(Note a, Note b) => DateTime.Compare(a.Modified, b.Modified);
    }
}