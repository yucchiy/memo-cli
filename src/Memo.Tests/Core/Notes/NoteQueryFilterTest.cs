using Xunit;

namespace Memo.Core.Notes.Tests
{
    public class NoteQueryFilterTest
    {
        [Fact]
        public void Filter_Id()
        {
        }

        [Fact]
        public void Filter_CategoryName()
        {
            var queryFilter = new NoteQueryFilter();

            var query1 = (new NoteSearchQueryBuilder())
                .WithCategoryId("test_category_1")
                .Build();
            var query2 = (new NoteSearchQueryBuilder())
                .WithCategoryId("test_category_2")
                .Build();

            var note1 = new Note(new Categories.Category(new Categories.CategoryId("test_category_1")), new Note.NoteId("id"), new Note.NoteTitle(""), null, null);
            var note2 = new Note(new Categories.Category(new Categories.CategoryId("test_category_2")), new Note.NoteId("id"), new Note.NoteTitle(""), null, null);

            Assert.True(queryFilter.FilterNote(note1, query1));
            Assert.False(queryFilter.FilterNote(note1, query2));
            Assert.True(queryFilter.FilterNote(note2, query2));
            Assert.False(queryFilter.FilterNote(note2, query1));
        }

        [Fact]
        public void Filter_Type()
        {
            var queryFilter = new NoteQueryFilter();

            var type1 = new Note.NoteType("type_1");
            var type2 = new Note.NoteType("type_2");

            var query1 = (new NoteSearchQueryBuilder())
                .WithType(type1)
                .Build();
            var query2 = (new NoteSearchQueryBuilder())
                .WithType(type2)
                .Build();

            var note1 = new Note(new Categories.Category(new Categories.CategoryId("test_category_1")), new Note.NoteId("id"), new Note.NoteTitle(""), type1, null);
            var note2 = new Note(new Categories.Category(new Categories.CategoryId("test_category_2")), new Note.NoteId("id"), new Note.NoteTitle(""), type2, null);

            Assert.True(queryFilter.FilterNote(note1, query1));
            Assert.False(queryFilter.FilterNote(note1, query2));
            Assert.True(queryFilter.FilterNote(note2, query2));
            Assert.False(queryFilter.FilterNote(note2, query1));
        }
    }
}