using System;
using System.IO;

namespace Memo.Tests
{
    public class TemporaryDirectory : System.IDisposable
    {
        private static readonly Random _random = new Random();

        public DirectoryInfo Dir { get; private set; }

        public TemporaryDirectory(string prefix)
        {
            string directoryName;
            lock(_random)
            {
                directoryName = prefix + _random.Next(1000000000);
            }

            Dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), directoryName));
        }

        public void Dispose()
        {
            Directory.Delete(Dir.FullName, true);
        }
    }
}