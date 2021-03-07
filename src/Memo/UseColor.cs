using System;

namespace Memo
{
    public struct UseColor : System.IDisposable
    {
        public static bool NoColor = false; 

        private ConsoleColor OriginalColor { get; }

        public UseColor(ConsoleColor color)
        {
            OriginalColor = Console.ForegroundColor;
            ChangeColor(color);
        }

        public void Dispose()
        {
            ChangeColor(OriginalColor);
        }

        private void ChangeColor(ConsoleColor color)
        {
            if (!NoColor)
            {
                Console.ForegroundColor = color;
            }
        }
    }
}