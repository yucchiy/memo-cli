using System.Threading;
using System.Threading.Tasks;

namespace Memo.Core.Notes
{
    public class NoteBuilder : INoteBuilder
    {
        private static readonly char IdSeparator = '_';
        private static readonly string IdTimestampFormat = "yyyyMMddHHmmss";

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

        public Note.NoteId CreateId(in NoteCreationParameter parameter)
        {
            var timestamp = parameter.Options.Timestamp is System.DateTime ts ? ts : System.DateTime.Now;
            if (parameter.Options.Slug is Note.NoteSlug slug)
            {
                return new Note.NoteId($"{timestamp.ToString(IdTimestampFormat)}{IdSeparator}{slug.Value}");
            }

            return new Note.NoteId($"{timestamp.ToString(IdTimestampFormat)}");
        }

        private Note.NoteTitle CreateTitle(NoteCreationParameter parameter)
        {
            if (parameter.Options.Title is Note.NoteTitle title)
            {
                return title;
            }

            return new Note.NoteTitle("");
        }

        private Note BuildDefault(in NoteCreationParameter parameter)
        {
            return new Note(
                new Categories.Category(parameter.CategoryId),
                CreateId(parameter),
                CreateTitle(parameter),
                parameter.Options.Type,
                parameter.Options.Timestamp
            );
        }

        private Note BuildDaily(in NoteCreationParameter parameter)
        {
            var timestamp = parameter.Options.Timestamp is System.DateTime dt ? dt : System.DateTime.Now;
            var dailyTimestamp = new System.DateTime(timestamp.Year, timestamp.Month, timestamp.Day, System.Globalization.CultureInfo.CurrentCulture.Calendar);

            var builder = new NoteCreationParameterBuilder(in parameter)
                .WithCreationType(NoteCreationOptionParameter.NoteCreationType.Default)
                .WithTimestamp(dailyTimestamp);

            return BuildDefault(builder.Build());
        }

        private Note BuildWeekly(in NoteCreationParameter parameter)
        {
            var builder = new NoteCreationParameterBuilder(in parameter)
                .WithCreationType(NoteCreationOptionParameter.NoteCreationType.Default)
                .WithTimestamp(Utility.FirstDayOfWeek());

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