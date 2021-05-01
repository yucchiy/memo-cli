using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Memo
{
    [ApiController]
    [Route("[Controller]")]
    public class NoteController : ControllerBase
    {
        private IMemoManager _memoManager;
        public NoteController(IMemoManager memoManager)
        {
            _memoManager = memoManager;
        }

        [HttpGet]
        public async Task<IEnumerable<MemoResponse>> GetAsync(
            [FromQuery(Name = "category")] string inputCategory,
            [FromQuery(Name = "queries")] string[] queries)
        {
            var category = _memoManager.GetCategory(inputCategory);
            if (category == null) return new List<MemoResponse>();

            return (await _memoManager.GetNotesAsync(category, queries))
                .Select(note => new MemoResponse()
                {
                    Category = note.Category.Name,
                    Id = note.Id,
                    FilePath = note.File.FullName,
                    Title = note.Meta.Title,
                    Type = note.Meta.Type ?? string.Empty,
                })
                .ToArray();
        }

        [HttpPost]
        public async Task<MemoResponse> CreateAsync(
            [FromBody] MemoCreationRequest request)
        {
            var parameter = await _memoManager.CreateNoteCreationParameter(
                request.Category,
                request.Id,
                request.Options,
                CancellationToken.None
            );
            var note = await _memoManager.CreateNoteAsync(parameter, CancellationToken.None);

            return new MemoResponse()
            {
                Category = note.Category.Name,
                Id = note.Id,
                FilePath = note.File.FullName,
                Title = note.Meta.Title,
                Type = note.Meta.Type ?? string.Empty,
            };
        }

        public class MemoCreationRequest
        {
            [Required]
            public string Category { get; set; }
            public string Id { get ; set; }
            public IEnumerable<string> Options { get ; set; }
        }

        public class MemoResponse
        {
            public string Category { get; set; }
            public string Id { get; set; }
            public string FilePath { get; set; }
            public string Title { get; set; }
            public string Type { get; set; }
        }
    }
}