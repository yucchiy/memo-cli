using Xunit;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Memo.Core.Categories.Test
{
    public class CategoryRepositoryTest
    {
        [Fact]
        public async void GetAllAsync()
        {
            var tokenSource = new CancellationTokenSource();
            var categoryRepository = new CategoryRepository(new NoteRositoryGetAllMock());

            Assert.Equal(await categoryRepository.GetAllAsync(tokenSource.Token), new Categories.Category[]
            {
                new Category(new CategoryId("test1")),
                new Category(new CategoryId("test2")),
                new Category(new CategoryId("test3")),
                new Category(new CategoryId("test2/test12")),
            });
        }

        [Fact]
        public async void FindAsync_Found()
        {
            var tokenSource = new CancellationTokenSource();
            var categoryRepository = new CategoryRepository(new NoteRositoryGetAllMock());

            var categoryId = new CategoryId("test2");
            var category = (await categoryRepository.FindAsync(categoryId, tokenSource.Token));

            var hasCategory = category is Category expectedCategory;
            Assert.True(hasCategory);

            Assert.Equal(categoryId, expectedCategory.Id);
        }

        [Fact]
        public async void FindAsync_NotFound()
        {
            var tokenSource = new CancellationTokenSource();
            var categoryRepository = new CategoryRepository(new NoteRositoryGetAllMock());

            var categoryId = new CategoryId("no_such_category");
            var category = (await categoryRepository.FindAsync(categoryId, tokenSource.Token));

            Assert.False(category.HasValue);
        }

        public class NoteRositoryGetAllMock : Notes.INoteRepositoryGetAll
        {
            Task<IEnumerable<Notes.Note>> Notes.INoteRepositoryGetAll.GetAllAsync(CancellationToken token)
            {
                return Task.Run(() => new Notes.Note[]
                {
                    new Notes.Note(new Category(new CategoryId("test1")), new Notes.Note.NoteId("id1"), new Notes.Note.NoteTitle(""), null, null),
                    new Notes.Note(new Category(new CategoryId("test2")), new Notes.Note.NoteId("id1"), new Notes.Note.NoteTitle(""), null, null),
                    new Notes.Note(new Category(new CategoryId("test3")), new Notes.Note.NoteId("id1"), new Notes.Note.NoteTitle(""), null, null),
                    new Notes.Note(new Category(new CategoryId("test1")), new Notes.Note.NoteId("id2"), new Notes.Note.NoteTitle(""), null, null),
                    new Notes.Note(new Category(new CategoryId("test2/test12")), new Notes.Note.NoteId("id1"), new Notes.Note.NoteTitle(""), null, null),
                    new Notes.Note(new Category(new CategoryId("test2")), new Notes.Note.NoteId("id2"), new Notes.Note.NoteTitle(""), null, null)
                } as IEnumerable<Notes.Note>);
            }
        }
    }
}