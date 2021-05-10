#nullable enable
namespace Memo.Core.Notes
{
    public class NoteQueryFilter : INoteQueryFilter
    {
        public bool FilterNote(Note note, in NoteSearchQuery query)
        {
            if (query.Id is Note.NoteId id) 
            {
                if (!id.Equals(note.Id))
                {
                    return false;
                }
            }

            if (query.Type is Note.NoteType type)
            {
                if (!type.Equals(note.Type))
                {
                    return false;
                }
            }

            if (query.CategoryId is Categories.CategoryId categoryId)
            {
                if (!categoryId.Equals(note.Category.Id))
                {
                    return false;
                }
            }

            return true;
        }
    }
}