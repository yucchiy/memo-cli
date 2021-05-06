#nullable enable
using System;

namespace Memo.Core.Notes
{
    public readonly struct NoteCreationOptionParameter
    {
        public enum NoteCreationType
        {
            Default,
            Daily,
            Weekly,
            Url,
        }

        public readonly Note.NoteSlug? Slug { get; }
        public readonly Note.NoteTitle? Title { get; }
        public readonly Note.NoteType? Type { get; }
        public readonly DateTime? Timestamp { get; }
        public readonly NoteCreationType? CreationType { get; }
        public readonly string? Url { get; }

        public NoteCreationOptionParameter(
            Note.NoteSlug? slug,
            Note.NoteTitle? title,
            Note.NoteType? type,
            DateTime? timestamp,
            NoteCreationType? creationType,
            string? url
        )
        {
            Slug = slug;
            Title = title;
            Type = type;
            Timestamp = timestamp;
            CreationType = creationType;
            Url = url;
        }
    }
}