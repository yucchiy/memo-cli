using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Memo.Core.Categories
{
    public class CategoryService : ICategoryService
    {
        private ICategoryRepository Repository { get; }
        public CategoryService(ICategoryRepository repository)
        {
            Repository = repository;
        }

        public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken token)
        {
            return await Repository.GetAllAsync(token);
        }

        public async Task<Category?> FindAsync(CategoryId id, CancellationToken token)
        {
            return await Repository.FindAsync(id, token);
        }
    }
}