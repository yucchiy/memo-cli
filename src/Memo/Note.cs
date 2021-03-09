using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using Markdig.Syntax;

namespace Memo
{
    public class Note
    {
        public Category Category { get; }
        public FileInfo File { get; }
        public MarkdownDocument Content { get; }
        public NoteMetaData Meta { get; set; }

        public string ContentTitle
        {
            get
            {
                var heading = Content.Select(block => block as HeadingBlock)
                    .Where(block => block != null && block.Level == 1);
                if (heading.Any()) return heading.First().Inline.FirstChild.ToString();

                return string.Empty;
            }
        }

        public DateTime Modified { get => File.LastWriteTime; }

        public Note(FileInfo filePath, MarkdownDocument content, Category category)
        {
            File = filePath;
            Content = content;
            Category = category;
        }

        public static async Task<string> GetTemplate(MemoConfig.CategoryConfig categoryConfig)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = string.IsNullOrEmpty(categoryConfig.MemoTemplateFilePath) ?
                new System.IO.StreamReader(assembly.GetManifestResourceStream($"Memo.res.{categoryConfig.MemoCreationType.ToString()}.md")) :
                new System.IO.StreamReader(System.IO.File.OpenRead(categoryConfig.MemoTemplateFilePath));

            return await stream.ReadToEndAsync();
        }
    }
}