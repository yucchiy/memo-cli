namespace Memo
{
    [System.Serializable]
    public class MemoCliException : System.Exception
    {
        public MemoCliException() { }
        public MemoCliException(string message) : base(message) { }
        public MemoCliException(string message, System.Exception inner) : base(message, inner) { }
        protected MemoCliException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}