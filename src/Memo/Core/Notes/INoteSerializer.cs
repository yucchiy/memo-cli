using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Memo.Core.Notes
{
    public interface INoteSerializer :
        INoteSerializerDeserialize,
        INoteSerializerSerialize
    {
    }

    public interface INoteSerializerSerialize
    {
        Task<(bool Success, string RawContent)> SerializeNoteAsync(Note note, CancellationToken token);
    }

    public interface INoteSerializerDeserialize
    {
        Task<(bool Success, Note Note)> DeserializeNoteAsync(FileInfo fileInfo, CancellationToken token);
    }
}