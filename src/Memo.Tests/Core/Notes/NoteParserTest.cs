using Xunit;
using System.Threading;
using Markdig;
using System.IO;

namespace Memo.Core.Notes
{
    public class NoteParserTest
    {
        [Fact]
        public async void ParseNoteAsync_FileNameIsIndexMarkdown()
        {
            using (var root = new Memo.Tests.TemporaryDirectory("a"))
            {
                var noteDirectoryPath  = $"{root.Dir.FullName}/category1/category11/20210101000000_test_note";
                var filePath = $"{noteDirectoryPath}/index.markdown";

                Directory.CreateDirectory(noteDirectoryPath);
                using (var writer = File.CreateText(filePath))
                {
                    await writer.WriteLineAsync("---");
                    await writer.WriteLineAsync("title: \"this is a test title\"");
                    await writer.WriteLineAsync("type: inbox");
                    await writer.WriteLineAsync("---");
                    await writer.WriteLineAsync("");
                    await writer.WriteLineAsync("This is a content line 1");
                    await writer.WriteLineAsync("This is a content line 2");
                    await writer.WriteLineAsync("This is a content line 3");
                    await writer.WriteLineAsync("This is a content line 4");
                }

                var tokenSource = new CancellationTokenSource();
                var pipeline = new MarkdownPipelineBuilder()
                    .UseYamlFrontMatter()
                    .Build();
                var option = new NoteParser.Options(root.Dir, '/');
                var parser = new NoteParser(new NoteBuilder(), new Categories.CategoryConfigStore(new MemoConfig.CategoryConfig[0]), pipeline, option);

                var (result, note) = await parser.DeserializeNoteAsync(new FileInfo(filePath), tokenSource.Token);

                Assert.True(result);
                Assert.Equal(note.Category.Id, new Categories.CategoryId("category1/category11"));
                Assert.Equal(note.Id, new Note.NoteId("20210101000000_test_note"));
                Assert.Equal(note.Type, new Note.NoteType("inbox"));
                Assert.Equal(note.Title, new Note.NoteTitle("this is a test title"));
                Assert.False(note.Created.HasValue);
            }
        }

        [Fact]
        public async void ParseNoteAsync_FileNameIsNotIndexMarkdown()
        {
            using (var root = new Memo.Tests.TemporaryDirectory("a"))
            {
                var noteDirectoryPath  = $"{root.Dir.FullName}/category1/category11";
                var filePath = $"{noteDirectoryPath}/20210101000000_test_note.markdown";

                Directory.CreateDirectory(noteDirectoryPath);
                using (var writer = File.CreateText(filePath))
                {
                    await writer.WriteLineAsync("---");
                    await writer.WriteLineAsync("title: \"this is a test title\"");
                    await writer.WriteLineAsync("type: inbox");
                    await writer.WriteLineAsync("---");
                    await writer.WriteLineAsync("");
                    await writer.WriteLineAsync("This is a content line 1");
                    await writer.WriteLineAsync("This is a content line 2");
                    await writer.WriteLineAsync("This is a content line 3");
                    await writer.WriteLineAsync("This is a content line 4");
                }

                var tokenSource = new CancellationTokenSource();
                var pipeline = new MarkdownPipelineBuilder()
                    .UseYamlFrontMatter()
                    .Build();
                var option = new NoteParser.Options(root.Dir, '/');
                var parser = new NoteParser(new NoteBuilder(), new Categories.CategoryConfigStore(new MemoConfig.CategoryConfig[0]), pipeline, option);

                var (result, note) = await parser.DeserializeNoteAsync(new FileInfo(filePath), tokenSource.Token);

                Assert.True(result);
                Assert.Equal(note.Category.Id, new Categories.CategoryId("category1/category11"));
                Assert.Equal(note.Id, new Note.NoteId("20210101000000_test_note"));
                Assert.Equal(note.Type, new Note.NoteType("inbox"));
                Assert.Equal(note.Title, new Note.NoteTitle("this is a test title"));
                Assert.False(note.Created.HasValue);
             }
        }
    }
}