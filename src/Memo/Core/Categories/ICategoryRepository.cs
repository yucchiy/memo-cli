using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Memo.Core.Categories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Categories.Category>> GetAllAsync(CancellationToken token);
        Task<Categories.Category?> FindAsync(Categories.CategoryId id, CancellationToken token);
    }
}