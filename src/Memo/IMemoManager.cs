using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace Memo
{
    public interface IMemoManager
    {
        Category CreateCategory(string categoryName);
        Category GetCategory(string categoryName);
        Category[] GetCategories();
        Task<Note[]> GetNotesAsync(Category category, IEnumerable<string> queries);
        Task<Note> CreateNoteAsync(NoteCreateParameter input, CancellationToken token);
        Task<NoteCreateParameter> CreateNoteCreationParameter(string category, string id, IEnumerable<string> options, CancellationToken token);
        DirectoryInfo GetRoot();
    }
}