using System.Collections.Generic;

namespace Memo
{
    public class NoteCreateParameter
    {
        public string Category { get; set; }
        public string Id { get; set; }
        public Dictionary<string, string> Options { get; set; }
    }
}