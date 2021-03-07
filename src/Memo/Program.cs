namespace Memo
{
    class Program
    {
        static int Main(string[] args)
        {
            var cli = new Cli();
            return cli.Execute(args).Result;
        }
    }
}