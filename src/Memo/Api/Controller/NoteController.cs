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
            [FromQuery(Name = "category")] string categoryString,
            [FromQuery(Name = "type")] string typeString)
        {
            var response = new List<MemoResponse>();

            var category = _memoManager.GetCategories().Where(c => c.Name == categoryString).FirstOrDefault();
            if (category == null) return response;

            foreach (var note in await _memoManager.GetNotesAsync(category, typeString))
            {
                response.Add(new MemoResponse()
                {
                    Category = note.Category.Name,
                    FilePath = note.File.FullName,
                    Title = note.Meta.Title,
                    Type = note.Meta.Type ?? string.Empty,
                });
            }

            return response;
        }

        [HttpPost]
        public async Task<MemoResponse> CreateAsync(
            [FromBody] MemoCreationRequest request)
        {
            var note = await _memoManager.CreateNoteAsync(new NoteCreationParameter()
            {
                Category = request.Category,
                Title = request.Title,
                Filename = request.FileName,
                Url = request.Url,
            }, CancellationToken.None);

            return new MemoResponse()
            {
                Category = note.Category.Name,
                FilePath = note.File.FullName,
                Title = note.Meta.Title,
                Type = note.Meta.Type ?? string.Empty,
            };
        }

        public class MemoCreationRequest
        {
            [Required]
            public string Category { get; set; }
            public string Title { get ; set; }
            public string FileName { get ; set; }
            public string Url { get ; set; }
        }

        public class MemoResponse
        {
            public string Category { get; set; }
            public string FilePath { get; set; }
            public string Title { get; set; }
            public string Type { get; set; }
        }
    }
}