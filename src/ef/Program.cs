// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
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
