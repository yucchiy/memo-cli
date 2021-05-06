using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Memo.Core.Notes
{
    public interface INoteRepository :
        INoteRepositorySave,
        INoteRepositoryFind,
        INoteRepositorySearch,
        INoteRepositoryGetAll
    {
    }

    public interface INoteRepositorySave
    {
        Task<Note> SaveAsync(Note note, CancellationToken token);
    }

    public interface INoteRepositoryFind
    {
        Task<Note?> FindAsync(Categories.CategoryId categoryId, Note.NoteId id, CancellationToken token);
    }

    public interface INoteRepositorySearch
    {
        Task<IEnumerable<Note>> SearchAsync(NoteSearchQuery query, CancellationToken token);
    }

    public interface INoteRepositoryGetAll
    {
        Task<IEnumerable<Note>> GetAllAsync(CancellationToken token);
    }
}