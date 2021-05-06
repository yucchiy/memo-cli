using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Memo.Core.Categories
{
    public interface ICategoryService :
        ICategoryServiceGetAll,
        ICategoryServiceFind
    {
    }

    public interface ICategoryServiceGetAll
    {
        Task<IEnumerable<Category>> GetAllAsync(CancellationToken token);
    }

    public interface ICategoryServiceFind
    {
        Task<Category?> FindAsync(CategoryId id, CancellationToken token);
    }
}