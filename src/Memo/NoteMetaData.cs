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
    }
}