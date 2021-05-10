using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Memo
{
    [ApiController]
    [Route("[Controller]")]
    public class CategoryController : ControllerBase
    {
        private Core.Categories.ICategoryService CategoryService { get; }
        private IMemoManager Manager { get; }

        public CategoryController(Core.Categories.ICategoryService categoryService, IMemoManager manager)
        {
            CategoryService = categoryService;
            Manager = manager;
        }

        [HttpGet]
        public async Task<CategoryResponse> GetAsync()
        {
            var categories = await CategoryService.GetAllAsync(CancellationToken.None);

            return new CategoryResponse()
            {
                Categories = categories.Select(c => c.Id.Value)
            };
        }

        public class CategoryResponse
        {
            public IEnumerable<string> Categories { get; set; }
        }
    }
}