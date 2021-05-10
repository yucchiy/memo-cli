using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Memo.Core.Categories
{
    public class CategoryRepository : ICategoryRepository
    {
        Notes.INoteRepositoryGetAll NoteRepositoryGetAll { get; }

        public CategoryRepository(Notes.INoteRepositoryGetAll noteRepositoryGetAll)
        {
            NoteRepositoryGetAll = noteRepositoryGetAll;
        }

        public async Task<Category?> FindAsync(CategoryId id, CancellationToken token)
        {
            try
            {
                return (await GetAllAsync(token))
                    .First(category => category.Id.Equals(id));
            } catch (System.InvalidOperationException)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken token)
        {
            var notes = await NoteRepositoryGetAll.GetAllAsync(token);
            var categories = notes.Select(note => note.Category).Distinct();

            return notes
                .Select(note => note.Category)
                .Distinct();
        }
    }
}