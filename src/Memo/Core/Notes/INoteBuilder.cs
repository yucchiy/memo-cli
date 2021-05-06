using System.Threading;
using System.Threading.Tasks;

namespace Memo.Core.Notes
{
    public interface INoteBuilder :
        INoteBuilderFromNoteCreationParameter
    {
    }

    public interface INoteBuilderFromNoteCreationParameter
    {
        Task<Note> BuildAsync(NoteCreationParameter parameter, CancellationToken token);
    }
}