using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Memo
{
    public class NoteCreationParameterBuilder : IDisposable
    {
        public NoteCreationParameterBuilder()
        {

        }

        public void Dispose()
        {

        }

        public async Task<NoteCreationParameter> Build(string category, string id, IEnumerable<string> options, CancellationToken token)
        {
            var parsedOptions = TryParseOptions(options, out var pso) ? pso : new Dictionary<string, string>();
            if (parsedOptions.TryGetValue("type", out var type))
            {
                switch (type)
                {
                    case "default":
                        return CreateDefaultParameter(category, id, parsedOptions);
                    case "daily":
                        return CreateDailyParameter(category, id, parsedOptions);
                    case "weekly":
                        return CreateWeeklyParameter(category, id, parsedOptions);
                    case "timestamp":
                        return CreateTimestampParameter(category, id, parsedOptions);
                    case "url":
                        return await CreateUrlParameter(category, id, parsedOptions, token);
                }
            }

            return CreateDefaultParameter(category, id, parsedOptions);
        }

        private NoteCreationParameter CreateDefaultParameter(string category, string id, Dictionary<string, string> options) => new NoteCreationParameter()
        {
            Category = category,
            Id = id,
            Options = options,
        };

        private NoteCreationParameter CreateDailyParameter(string category, string id, Dictionary<string, string> options)
        {
            var date = System.DateTime.Now;
            if (options.TryGetValue("title", out var title))
            {
                options["title"] = string.Format("{1} - {0}", date.ToString("yyyy/MM/dd"), title);
            }
            else
            {
                options["title"] = string.Format("{0} - {1}", date.ToString("yyyy/MM/dd"), category);
            }

            return new NoteCreationParameter()
            {
                Category = category,
                Id = string.IsNullOrEmpty(id) ? date.ToString("yyyyMMdd") : string.Format("{0}_{1}", date.ToString("yyyyMMdd"), id),
                Options = options,
            };
        }

        private NoteCreationParameter CreateWeeklyParameter(string category, string id, Dictionary<string, string> options)
        {
            var date = Utility.FirstDayOfWeek();
            if (options.TryGetValue("title", out var title))
            {
                options["title"] = string.Format("{1} - {0}", date.ToString("yyyy/MM/dd"), title);
            }
            else
            {
                options["title"] = string.Format("{0} - {1}", date.ToString("yyyy/MM/dd"), category);
            }

            return new NoteCreationParameter()
            {
                Category = category,
                Id = string.IsNullOrEmpty(id) ? date.ToString("yyyyMMdd") : string.Format("{0}_{1}", date.ToString("yyyyMMdd"), id),
                Options = options,
            };
        }

        private NoteCreationParameter CreateTimestampParameter(string category, string id, Dictionary<string, string> options)
        {
            var timestamp = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            if (options.TryGetValue("title", out var title))
            {
                options["title"] = string.Format("{0} - {1}", timestamp, title);
            }
            else
            {
                options["title"] = string.Format("{0} - {1}", timestamp, category);
            }

            return new NoteCreationParameter()
            {
                Category = category,
                Id = string.IsNullOrEmpty(id) ? timestamp : string.Format("{0}_{1}", timestamp, id),
                Options = options,
            };
        }

        private async Task<NoteCreationParameter> CreateUrlParameter(string category, string id, Dictionary<string, string> options, CancellationToken token)
        {
            if (!options.TryGetValue("url", out var url))
            {
                throw new MemoCliException("url not found in options.");
            }

            var uri = new System.Uri(url);
            using (var client = new System.Net.Http.HttpClient())
            {
                var response = await client.GetAsync(url, token);
                switch (response.StatusCode) 
                {
                    case System.Net.HttpStatusCode.OK:
                        id = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{uri.Scheme}://{uri.Host}/{uri.PathAndQuery}"));
                        if (Utility.TryParseTitle(await response.Content.ReadAsStringAsync(), out var title))
                        {
                            options["title"] = title;
                        }

                        return new NoteCreationParameter()
                        {
                            Category = category,
                            Id = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{uri.Scheme}://{uri.Host}/{uri.PathAndQuery}")),
                            Options = options,
                        };
                    default: 
                        return CreateDefaultParameter(category, id, options);
                }
            }
        }

        private bool TryParseOptions(IEnumerable<string> inputOptions, out Dictionary<string, string> outputOptions)
        {
            outputOptions = new Dictionary<string, string>();
            if (inputOptions == null) return false;

            foreach (var option in inputOptions)
            {
                var indexOfSeparator = option.IndexOf(':');
                if (indexOfSeparator >= 0)
                {
                    outputOptions.Add(option.Substring(0, indexOfSeparator), option.Substring(indexOfSeparator + 1));
                }
            }

            return true;
        }
    }
}