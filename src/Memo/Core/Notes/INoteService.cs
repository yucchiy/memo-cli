using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Memo.Core.Notes
{
    public interface INoteService
    {
        Task<Note?> CreateNoteAsync(NoteCreationParameter parameter, CancellationToken token);
        Task<Note?> FindNoteAsync(Categories.CategoryId categoryId, Note.NoteId id, CancellationToken token);
        Task<IEnumerable<Note>> GetNotesAsync(NoteSearchQuery query, CancellationToken token);
    }
}