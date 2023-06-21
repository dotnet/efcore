// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.DotNet.Cli.CommandLine;

namespace Microsoft.EntityFrameworkCore.Tools;

internal static class Program
{
    private static int Main(string[] args)
    {
        var app = new CommandLineApplication(throwOnUnexpectedArg: false) { Name = "dotnet ef" };

        new RootCommand().Configure(app);

        try
        {
            return app.Execute(args);
        }
        catch (Exception ex)
        {
            if (ex is CommandException or CommandParsingException)
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
