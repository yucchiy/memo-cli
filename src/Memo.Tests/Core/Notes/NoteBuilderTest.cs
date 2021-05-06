using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Memo.Core.Notes.Tests
{
    public class NoteBuilderTest
    {
        [Fact]
        public async void Build_FullParameter()
        {
            var builder = new NoteBuilder();

            var category = new Categories.Category(new Categories.CategoryId("test1/test2"));
            var title = new Note.NoteTitle("testtitle");
            var slug = new Note.NoteSlug("testslug");
            var type = new Note.NoteType("testtype");
            var timestamp = System.DateTime.Now;

            var parameter = (new NoteCreationParameterBuilder())
                .WithCategoryId(category.Id)
                .WithTitle(title)
                .WithSlug(slug)
                .WithType(type)
                .WithTimestamp(timestamp)
                .Build();

            var note = await builder.BuildAsync(parameter, CancellationToken.None);

            Assert.Equal(note.Id, new Note.NoteId($"{timestamp.ToString("yyyyMMddHHmmss")}_{slug.Value}"));
            Assert.Equal(note.Category, category);
            Assert.Equal(note.Title, title);
        }

        [Fact]
        public async void Build_Daily()
        {
            var builder = new NoteBuilder();

            var category = new Categories.Category(new Categories.CategoryId("test1/test2"));
            var title = new Note.NoteTitle("testtitle");
            var slug = new Note.NoteSlug("testslug");
            var type = new Note.NoteType("testtype");
            var timestamp = System.DateTime.Now;

            var parameter = (new NoteCreationParameterBuilder())
                .WithCategoryId(category.Id)
                .WithTitle(title)
                .WithSlug(slug)
                .WithType(type)
                .WithCreationType(NoteCreationOptionParameter.NoteCreationType.Daily)
                .WithTimestamp(timestamp)
                .Build();

            var note = await builder.BuildAsync(parameter, CancellationToken.None);

            Assert.Equal(note.Id, new Note.NoteId($"{timestamp.ToString("yyyyMMdd000000")}_{slug.Value}"));
            Assert.Equal(note.Category, category);
            Assert.Equal(note.Title, title);
        }
    }
}