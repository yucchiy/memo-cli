#nullable enable

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Memo.Core.Notes
{
    public class NoteRepository : INoteRepository
    {
        private INoteStorage Storage { get; }
        private INoteQueryFilter QueryFilter { get; }

        public NoteRepository(INoteStorage storage, INoteQueryFilter queryFilter)
        {
            Storage = storage;
            QueryFilter = queryFilter;
        }

        public async Task<Note> SaveAsync(Note note, CancellationToken token)
        {
            try
            {
                if (await FindAsync(note.Category.Id, note.Id, token) is Note result)
                {
                    return result;
                }
            }
            catch(MemoCliException)
            {
                if (await Storage.WriteAsync(note, token))
                {
                    return note;
                }
            }

            throw new MemoCliException("Failed to save note");
        }

        public async Task<Note> FindAsync(Categories.CategoryId categoryId, Note.NoteId id, CancellationToken token)
        {
            try
            {
                return (await GetAllAsync(token))
                    .First(note => note.Category.Id.Equals(categoryId) && note.Id.Equals(id));
            }
            catch (System.InvalidOperationException)
            {
                throw new MemoCliException("No such note found.");
            }
        }

        public async Task<IEnumerable<Note>> GetAllAsync(CancellationToken token)
        {
            return (await Storage.ReadAllAsync(token));
        }
 
        public async Task<IEnumerable<Note>> SearchAsync(NoteSearchQuery query, CancellationToken token)
        {
            return (await GetAllAsync(token))
                .Where(note => QueryFilter.FilterNote(note, in query));
        }
    }
}