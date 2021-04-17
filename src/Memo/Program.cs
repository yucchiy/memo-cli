﻿using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Memo
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length == 0) return await Cli.CreateCommandLineParser().InvokeAsync(args);
            else if (args[0] == "api")
            {
                try
                {
                    await Api.CreateWebHost().RunAsync();
                }
                catch (System.Exception e)
                {
                    System.Console.Error.WriteLine(e.Message);
                    return Cli.FailedExitCode;
                }
                return Cli.SuccessExitCode;
            }
            else return await Cli.CreateCommandLineParser().InvokeAsync(args);
        }
    }
}