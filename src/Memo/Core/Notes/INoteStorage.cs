using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Memo.Core.Notes
{
    public interface INoteStorage
    {
        Task<IEnumerable<Note>> ReadAllAsync(CancellationToken token);
        Task<bool> WriteAsync(Note note, CancellationToken token);
    }
}