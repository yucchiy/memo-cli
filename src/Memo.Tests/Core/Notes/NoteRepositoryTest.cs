using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Memo.Core.Notes.Tests
{
    public class NoteRepositoryTest
    {
        [Fact]
        public async void GetAll_Test()
        {
            var tokenSource = new CancellationTokenSource();

            var repository = new NoteRepository(
                new NoteStorageMock(new Note[]
                {
                }),
                new NoteQueryFilter()
            );

            Assert.Equal(await repository.GetAllAsync(tokenSource.Token), new Note[0]);

            var note = new Note(new Categories.Category(new Categories.CategoryId("test_category_1")), new Note.NoteId("id_1"), new Note.NoteTitle(""), null, null);

            await repository.SaveAsync(note, tokenSource.Token);

            Assert.Equal(await repository.GetAllAsync(tokenSource.Token), new Note[]{note});
        }

        [Fact]
        public async void SaveAsync_Test()
        {
            var tokenSource = new CancellationTokenSource();

            var repository = new NoteRepository(
                new NoteStorageMock(new Note[]
                {
                }),
                new NoteQueryFilter()
            );

            var note = new Note(new Categories.Category(new Categories.CategoryId("test_category_1")), new Note.NoteId("id_1"), new Note.NoteTitle(""), null, null);

            Assert.Equal(await repository.SaveAsync(note, tokenSource.Token), note);
        }

        [Fact]
        public async void FindAsync_Found()
        {
            var tokenSource = new CancellationTokenSource();

            var note = new Note(new Categories.Category(new Categories.CategoryId("test_category_1")), new Note.NoteId("id_1"), new Note.NoteTitle(""), null, null);

            var repository = new NoteRepository(
                new NoteStorageMock(new Note[]{note}),
                new NoteQueryFilter()
            );

            var actualNote = await repository.FindAsync(note.Category.Id, note.Id, tokenSource.Token);
            Assert.Equal(note, actualNote);
        }

        [Fact]
        public async void FindAsync_NotFound()
        {
            var tokenSource = new CancellationTokenSource();

            var note = new Note(new Categories.Category(new Categories.CategoryId("test_category_1")), new Note.NoteId("id_1"), new Note.NoteTitle(""), null, null);

            var repository = new NoteRepository(
                new NoteStorageMock(new Note[]{note}),
                new NoteQueryFilter()
            );

            Assert.False((await repository.FindAsync(note.Category.Id, new Note.NoteId("another_id"), tokenSource.Token)).HasValue);
            Assert.False((await repository.FindAsync(new Categories.CategoryId("another_category"), note.Id, tokenSource.Token)).HasValue);
        }

        public async void FindAsync_Theory(Note expectNote, Note[] notes)
        {
            var tokenSource = new CancellationTokenSource();

            var repository = new NoteRepository(
                new NoteStorageMock(notes),
                new NoteQueryFilter()
            );

            var actualNote = await repository.FindAsync(expectNote.Category.Id, expectNote.Id, tokenSource.Token);
            Assert.True(actualNote.HasValue);
            Assert.Equal(expectNote, actualNote.GetValueOrDefault());
        }

        public class NoteStorageMock : INoteStorage
        {
            public NoteStorageMock(Note[] notes)
            {
                Notes = new List<Note>(notes);
            }

            private List<Note> Notes { get; }

            public Task<IEnumerable<Note>> ReadAllAsync(CancellationToken token)
            {
                return Task.Run(() => Notes as IEnumerable<Note>, token);
            }

            public Task<bool> WriteAsync(Note note, CancellationToken token)
            {
                return Task.Run(() => {
                    Notes.Add(note);
                    return true;
                }, token);
            }
        }
    }
}