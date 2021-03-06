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
        private Core.CommandConfig CommandConfig { get; }
        public NoteController(Core.Notes.INoteService noteService, Core.CommandConfig commandConfig)
        {
            NoteService = noteService;
            CommandConfig = commandConfig;
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
            var citations = BuildCitations(await NoteService.GetNotesAsync((new Core.Notes.NoteSearchQueryBuilder().Build()), CancellationToken.None));

            return notes.Select(note => ToResponse(note, citations));
        }

        private Dictionary<string, List<string>> BuildCitations(IEnumerable<Core.Notes.Note> notes)
        {
            var citations = new Dictionary<string, List<string>>();
            foreach (var note in notes)
            {
                foreach (var internalLink in note.InternalLinks)
                {
                    if (citations.TryGetValue(internalLink, out var citation))
                    {
                        citation.Add(note.RelativePath);
                    }
                    else
                    {
                        var addCitation = new List<string>();
                        addCitation.Add(note.RelativePath);
                        citations.Add(internalLink, addCitation);
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

        private MemoResponse ToResponse(Core.Notes.Note note, Dictionary<string, List<string>> citations)
        {
            var citation = (citations != null && citations.TryGetValue(note.RelativePath, out var cite)) ? cite : new string[0] as IEnumerable<string>;

            var noteRelativePath = note.RelativePath.Replace(CommandConfig.DirectorySeparator, System.IO.Path.DirectorySeparatorChar);
            return new MemoResponse()
            {
                Category = note.Category.Id.Value,
                Id = note.Id.Value,
                FilePath = System.IO.Path.Combine(CommandConfig.HomeDirectory.FullName, noteRelativePath),
                RelativePath = note.RelativePath,
                Title = note.Title.Value,
                Type = (note.Type is Core.Notes.Note.NoteType noteType) ? noteType.Value : string.Empty,
                InternalLinks = new string[0],
                Links = note.Links != null ? note.Links : new string[0],
                Citations = citation.Select(cite => System.IO.Path.Combine(CommandConfig.HomeDirectory.FullName, cite.Replace(CommandConfig.DirectorySeparator, System.IO.Path.DirectorySeparatorChar))),
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
            public string RelativePath { get; set; }
            public string Title { get; set; }
            public string Type { get; set; }
            public IEnumerable<string> Links { get; set; }
            public IEnumerable<string> InternalLinks { get; set; }
            public IEnumerable<string> Citations { get; set; }
        }
    }
}