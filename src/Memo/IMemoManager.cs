using System;
using System.Threading;
using System.Threading.Tasks;

namespace Memo
{
    public interface IMemoManager
    {
        Category CreateCategory(string categoryName);
        Category GetCategory(string categoryName);
        Category[] GetCategories();
        Task<Note[]> GetNotesAsync(Category category, string type);
        Task<Note> CreateNoteAsync(NoteCreationParameter input, CancellationToken token);
    }

}