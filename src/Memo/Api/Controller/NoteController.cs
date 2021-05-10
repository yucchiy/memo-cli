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

            var notes = await NoteService.GetNotesAsync(builder.Build(), CancellationToken.None);
            var citations = BuildCitations(notes);

            return notes.Select(note => ToResponse(note, citations));
        }

        private Dictionary<(Core.Categories.CategoryId, Core.Notes.Note.NoteId), List<(Core.Categories.CategoryId, Core.Notes.Note.NoteId)>> BuildCitations(IEnumerable<Core.Notes.Note> notes)
        {
            var citations = new Dictionary<(Core.Categories.CategoryId, Core.Notes.Note.NoteId), List<(Core.Categories.CategoryId, Core.Notes.Note.NoteId)>>();
            foreach (var note in notes)
            {
                foreach (var internalLink in note.InternalLinks)
                {
                    if (citations.TryGetValue((internalLink.CategoryId, internalLink.NoteId), out var citation))
                    {
                        citation.Add((note.Category.Id, note.Id));
                    }
                    else
                    {
                        var addCitation = new List<(Core.Categories.CategoryId, Core.Notes.Note.NoteId)>();
                        addCitation.Add((note.Category.Id, note.Id));
                        citations.Add((internalLink.CategoryId, internalLink.NoteId), addCitation);
                    }
                }
           }

            return citations;
        }

        [HttpPost]
        public async Task<MemoResponse> CreateAsync(
            [FromBody] MemoCreationRequest request)
        {
            var builder = new Core.Notes.NoteCreationParameterBuilder();
            builder.WithCategoryId(request.Category);
            builder.WithQueryStrings(request.Options);

            var note = await NoteService.CreateNoteAsync(builder.Build(), CancellationToken.None);
            return ToResponse(note, null);
        }

        private MemoResponse ToResponse(Core.Notes.Note note, Dictionary<(Core.Categories.CategoryId, Core.Notes.Note.NoteId), List<(Core.Categories.CategoryId, Core.Notes.Note.NoteId)>> citations)
        {
            var citation = (citations != null && citations.TryGetValue((note.Category.Id, note.Id), out var cite)) ?
                cite.Select(c => $"{Manager.GetRoot().FullName}/{c.Item1.Value}/{c.Item2.Value}/{c.Item2.Value}.markdown") :
                new string[0] as IEnumerable<string>;

            return new MemoResponse()
            {
                Category = note.Category.Id.Value,
                Id = note.Id.Value,
                FilePath = $"{Manager.GetRoot().FullName}/{note.RelativePath}",
                Title = note.Title.Value,
                Type = (note.Type is Core.Notes.Note.NoteType noteType) ? noteType.Value : string.Empty,
                InternalLinks = note.InternalLinks != null ?
                    note.InternalLinks.Select(c => $"{Manager.GetRoot().FullName}/{c.Item1.Value}/{c.Item2.Value}/{c.Item2.Value}.markdown") :
                    new string[0],
                Links = note.Links != null ? note.Links : new string[0],
                Citations = citation,
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
            public IEnumerable<string> Links { get; set; }
            public IEnumerable<string> InternalLinks { get; set; }
            public IEnumerable<string> Citations { get; set; }
        }
    }
}