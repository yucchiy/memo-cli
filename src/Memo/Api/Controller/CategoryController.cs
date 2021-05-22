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
        private Core.Notes.INoteService NoteService { get; }
        private Core.Categories.ICategoryConfigStore CategoryConfigStore { get; }

        public CategoryController(
            Core.Categories.ICategoryService categoryService,
            Core.Notes.INoteService noteService,
            Core.Categories.ICategoryConfigStore categoryConfigStore)
        {
            CategoryService = categoryService;
            NoteService = noteService;
            CategoryConfigStore = categoryConfigStore;
        }

        [HttpGet]
        public async Task<CategoriesResponse> GetAsync()
        {
            var token = CancellationToken.None;

            var categories = await CategoryService.GetAllAsync(token);
            var notes = await NoteService.GetNotesAsync((new Core.Notes.NoteSearchQueryBuilder()).Build(), token);
            var noteTypes = new Dictionary<Core.Categories.CategoryId, string[]>();
            foreach (var category in categories)
            {
                noteTypes.Add(
                    category.Id,
                    notes.Where(n => n.Category.Id.Equals(category.Id))
                        .Select(n => n.Type is Core.Notes.Note.NoteType noteType ? noteType.Value : string.Empty)
                        .Where(t => !string.IsNullOrEmpty(t))
                        .Distinct()
                        .ToArray()
                );
            }

            return new CategoriesResponse(categories
                .Select(c => new CategoriesResponse.Category(
                    c.Id.Value,
                    CategoryConfigStore.GetConfig(c.Id).MemoCreationType,
                    noteTypes.TryGetValue(c.Id, out var types) ? types: new string[0]
                ))
            );
        }

        public class CategoriesResponse
        {
            public IEnumerable<Category> Categories { get; }

            public CategoriesResponse(IEnumerable<Category> categories)
            {
                Categories = categories;
            }

            public class Category
            {
                public string Name { get; }
                public string CreationType { get; }
                public IEnumerable<string> Types { get; }

                public Category(string name, Core.CreationType creationType, IEnumerable<string> types)
                {
                    Name = name;
                    CreationType = creationType.ToString();
                    Types = types;
                }
            }
        }
    }
}