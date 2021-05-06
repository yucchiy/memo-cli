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
        private Core.Notes.INoteService NoteService { get; }
        private IMemoManager Manager { get; }
        public NoteController(Core.Notes.INoteService noteService, IMemoManager manager)
        {
            NoteService = noteService;
            Manager = manager;
        }

        [HttpGet]
        public async Task<IEnumerable<MemoResponse>> GetAsync(
            [FromQuery(Name = "category")] string inputCategory,
            [FromQuery(Name = "queries")] string[] queries)
        {

            var builder = new Core.Notes.NoteSearchQueryBuilder();
            builder.WithCategoryId(inputCategory);
            builder.WithQueryStrings(queries);

            return (await NoteService.GetNotesAsync(builder.Build(), CancellationToken.None))
                .Select(ToResponse);
        }

        [HttpPost]
        public async Task<MemoResponse> CreateAsync(
            [FromBody] MemoCreationRequest request)
        {
            var builder = new Core.Notes.NoteCreationParameterBuilder();
            builder.WithCategoryId(request.Id);
            builder.WithQueryStrings(request.Options);

            var note = await NoteService.CreateNoteAsync(builder.Build(), CancellationToken.None);
            return ToResponse(note);
        }

        private MemoResponse ToResponse(Core.Notes.Note note)
        {
            return new MemoResponse()
            {
                Category = note.Category.Id.Value,
                Id = note.Id.Value,
                FilePath = $"{Manager.GetRoot().FullName}/{note.RelativePath}",
                Title = note.Title.Value,
                Type = (note.Type is Core.Notes.Note.NoteType noteType) ? noteType.Value : string.Empty,
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