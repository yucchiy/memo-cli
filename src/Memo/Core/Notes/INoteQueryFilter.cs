namespace Memo.Core.Notes
{
    public interface INoteQueryFilter
    {
        bool FilterNote(Note note, in NoteSearchQuery query);
    }
}