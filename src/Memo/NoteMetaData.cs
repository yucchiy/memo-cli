using System;
using System.Text;
using YamlDotNet.Serialization;

namespace Memo
{
    public class NoteMetaData
    {
        [YamlMember(Alias = "title")]
        public string Title;
        [YamlMember(Alias = "category")]
        public string Category;
        [YamlMember(Alias = "type")]
        public string Type;
        [YamlMember(Alias = "created")]
        public DateTime Created;

        public string ToText()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("---");
            stringBuilder.AppendLine($"title: {Title}");
            stringBuilder.AppendLine($"category: {Category}");
            if (!string.IsNullOrWhiteSpace(Type)) stringBuilder.AppendLine($"type: {Type}");
            stringBuilder.AppendLine($"created: {Created}");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("---");

            return stringBuilder.ToString();
        }
    }
}