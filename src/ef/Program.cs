// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Commands;

namespace Microsoft.EntityFrameworkCore.Tools;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (Console.IsOutputRedirected)
        {
            Console.OutputEncoding = Encoding.UTF8;
        }

        // Redirect Console.Out to stderr so that any user-configured logging providers
        // (e.g. ConsoleLogger) don't pollute stdout with diagnostic messages.
        // Actual data output uses the original stdout saved by Reporter.
        Reporter.SetStdOut(Console.Out);
        Console.SetOut(Console.Error);

        var app = new CommandLineApplication { Name = "ef" };

        new RootCommand().Configure(app);

        try
        {
            return app.Execute(args);
        }
        catch (Exception ex)
        {
            var wrappedException = ex as WrappedException;
            if (ex is CommandException
                || ex is CommandParsingException
                || (wrappedException?.Type == "Microsoft.EntityFrameworkCore.Design.OperationException"))
            {
                Reporter.WriteVerbose(ex.ToString());
            }
            else
            {
                Reporter.WriteInformation(ex.ToString());
            }

            Reporter.WriteError(ex.Message);

            return 1;
        }
    }
}
