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

        public NoteCollector()
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
        }

        public async Task<Note[]> Collect(Category category, string type = null)
        {
            var notes = new List<Note>();
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

                        var note = new Note(file, contentDocument, category);
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

                        if (!string.IsNullOrEmpty(type))
                        {
                            if (note.Meta == null || string.IsNullOrEmpty(note.Meta.Type)) continue;
                            if (!Regex.IsMatch(note.Meta.Type, type)) continue;
                        }

                        notes.Add(note);
                    }
                }
            }

            return notes.ToArray();
        }

        public async Task<Note> Find(string filePath, Category category, string type)
        {
            var notes = await Collect(category, type);
            return notes
                .Where(note => note.File.FullName == filePath)
                .FirstOrDefault();
        }

        public static int CompareByModified(Note a, Note b) => DateTime.Compare(a.Modified, b.Modified);
    }
}