using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Memo.Core.Notes
{
    public class NoteService : INoteService
    {
        private INoteRepository Repository { get; }
        private INoteBuilder NoteBuilder { get; }

        public NoteService(INoteRepository repository, INoteBuilder noteBuilder)
        {
            Repository = repository;
            NoteBuilder = noteBuilder;
        }

        public async Task<Note> CreateNoteAsync(NoteCreationParameter parameter, CancellationToken token)
        {
            var note = await NoteBuilder.BuildAsync(parameter, token);
            return await Repository.SaveAsync(note, token);
        }

        public async Task<Note?> FindNoteAsync(Categories.CategoryId categoryId, Note.NoteId id, CancellationToken token)
        {
            return await Repository.FindAsync(categoryId, id, token);
        }

        public async Task<IEnumerable<Note>> GetNotesAsync(NoteSearchQuery query, CancellationToken token)
        {
            return await Repository.SearchAsync(query, token);
        }
    }
}