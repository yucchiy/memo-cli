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

        public readonly Note.NoteId ? Id { get; }
        public readonly Note.NoteSlug? Slug { get; }
        public readonly Note.NoteTitle? Title { get; }
        public readonly Note.NoteType? Type { get; }
        public readonly DateTime? Timestamp { get; }
        public readonly NoteCreationType? CreationType { get; }
        public readonly string? Url { get; }
        public readonly IEnumerable<string> Links { get; }
        public readonly IEnumerable<(Categories.CategoryId, Note.NoteId)> InternalLinks { get; }

        public NoteCreationOptionParameter(
            Note.NoteId? id,
            Note.NoteSlug? slug,
            Note.NoteTitle? title,
            Note.NoteType? type,
            DateTime? timestamp,
            NoteCreationType? creationType,
            string? url,
            IEnumerable<string> links,
            IEnumerable<(Categories.CategoryId, Note.NoteId)> internalLinks)
        {
            Id = id;
            Slug = slug;
            Title = title;
            Type = type;
            Timestamp = timestamp;
            CreationType = creationType;
            Url = url;
            Links = links;
            InternalLinks = internalLinks;
        }
    }
}