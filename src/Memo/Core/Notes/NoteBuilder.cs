using System;
using System.Threading;
using System.Threading.Tasks;

namespace Memo.Core.Notes
{
    public class NoteBuilder : INoteBuilder
    {
        private static readonly string HumanReadableTimestampFormat = "yyyy/MM/dd";

        public NoteBuilder()
        {
        }

        public async Task<Note> BuildAsync(NoteCreationParameter parameter, CancellationToken token)
        {
            if (parameter.Options.CreationType is NoteCreationOptionParameter.NoteCreationType creationType)
            {
                switch (creationType)
                {
                    case NoteCreationOptionParameter.NoteCreationType.Default:
                        return BuildDefault(in parameter);
                    case NoteCreationOptionParameter.NoteCreationType.Daily:
                        return BuildDaily(in parameter);
                    case NoteCreationOptionParameter.NoteCreationType.Weekly:
                        return BuildWeekly(in parameter);
                    case NoteCreationOptionParameter.NoteCreationType.Url:
                        return await BuildUrlAsync(parameter, token);
                }
           }

            return BuildDefault(in parameter);
        }

        private Note.NoteTitle CreateTitle(NoteCreationParameter parameter)
        {
            if (parameter.Options.Title is Note.NoteTitle title)
            {
                return title;
            }

            return new Note.NoteTitle("");
        }

        private Note.NoteTimestamp CreateTimestamp(NoteCreationParameter parameter)
        {
            if (parameter.Options.Timestamp is Note.NoteTimestamp timestamp)
            {
                return timestamp;
            }

            return new Note.NoteTimestamp(System.DateTime.Now);
        }

        private Note.NoteSlug CreateSlug(NoteCreationParameter parameter)
        {
            if (parameter.Options.Slug is Note.NoteSlug slug)
            {
                return slug;
            }

            return new Note.NoteSlug("index");
        }

        private Note BuildDefault(in NoteCreationParameter parameter)
        {
            return new Note(
                new Categories.Category(parameter.CategoryId),
                CreateTimestamp(parameter),
                CreateSlug(parameter),
                CreateTitle(parameter),
                parameter.Options.Type,
                parameter.Options.Links,
                parameter.Options.InternalLinks
            );
        }

        private Note BuildDaily(in NoteCreationParameter parameter)
        {
            var timestamp = parameter.Options.Timestamp is Note.NoteTimestamp ts ? ts : new Note.NoteTimestamp(System.DateTime.Now);
            var dailyTimestamp = new Note.NoteTimestamp(new System.DateTime(timestamp.Value.Year, timestamp.Value.Month, timestamp.Value.Day, System.Globalization.CultureInfo.CurrentCulture.Calendar));

           var builder = new NoteCreationParameterBuilder(in parameter)
                .WithCreationType(NoteCreationOptionParameter.NoteCreationType.Default)
                .WithTimestamp(dailyTimestamp);

            if (parameter.Options.Title is Note.NoteTitle title)
            {
                builder.WithTitle($"{title.Value} - {dailyTimestamp.Value.ToString(HumanReadableTimestampFormat)}");
            }
            else
            {
                builder.WithTitle($"{dailyTimestamp.Value.ToString(HumanReadableTimestampFormat)} - {parameter.CategoryId.Value}");
            }

            return BuildDefault(builder.Build());
        }

        private Note BuildWeekly(in NoteCreationParameter parameter)
        {
            var timestamp = new Note.NoteTimestamp(Utility.FirstDayOfWeek());
            var builder = new NoteCreationParameterBuilder(in parameter)
                .WithCreationType(NoteCreationOptionParameter.NoteCreationType.Default)
                .WithTimestamp(timestamp);

            if (parameter.Options.Title is Note.NoteTitle title)
            {
                builder.WithTitle($"{title.Value} - {timestamp.Value.ToString(HumanReadableTimestampFormat)}");
            }
            else
            {
                builder.WithTitle($"{timestamp.Value.ToString(HumanReadableTimestampFormat)} - {parameter.CategoryId.Value}");
            }

            return BuildDefault(builder.Build());
        }

        private async Task<Note> BuildUrlAsync(NoteCreationParameter parameter, CancellationToken token)
        {
            if (string.IsNullOrEmpty(parameter.Options.Url))
            {
                throw new MemoCliException("Url not set");
            }

            var builder = new NoteCreationParameterBuilder(in parameter);

            var uri = new System.Uri(parameter.Options.Url);
            using (var client = new System.Net.Http.HttpClient())
            {
                var response = await client.GetAsync(parameter.Options.Url, token);
                switch (response.StatusCode) 
                {
                    case System.Net.HttpStatusCode.OK:
                        builder.WithSlug(System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{uri.Scheme}://{uri.Host}/{uri.PathAndQuery}")));
                        if (Utility.TryParseTitle(await response.Content.ReadAsStringAsync(), out var title))
                        {
                            builder.WithTitle(title);
                        }
                        break;
                }
            }

            builder.WithCreationType(NoteCreationOptionParameter.NoteCreationType.Default);
            return await BuildAsync(builder.Build(), token);
        }
    }
}