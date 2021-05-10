#nullable enable
using System;
using System.Collections.Generic;

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
        public readonly Note.NoteTimestamp Timestamp { get; }
        public readonly Note.NoteSlug Slug { get; }
        public readonly Note.NoteTitle? Title { get; }
        public readonly Note.NoteType? Type { get; }
        public readonly NoteCreationType? CreationType { get; }
        public readonly string? Url { get; }
        public readonly IEnumerable<string> Links { get; }
        public readonly IEnumerable<string> InternalLinks { get; }

        public NoteCreationOptionParameter(
            Note.NoteTimestamp timestamp,
            Note.NoteSlug slug,
            Note.NoteTitle? title,
            Note.NoteType? type,
            NoteCreationType? creationType,
            string? url,
            IEnumerable<string> links,
            IEnumerable<string> internalLinks)
        {
            Timestamp = timestamp;
            Slug = slug;
            Title = title;
            Type = type;
            CreationType = creationType;
            Url = url;
            Links = links;
            InternalLinks = internalLinks;
        }
    }
}