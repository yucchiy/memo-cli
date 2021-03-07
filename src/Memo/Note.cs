using System;
using System.IO;
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
    }
}