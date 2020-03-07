// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Tools.Commands;

namespace Microsoft.EntityFrameworkCore.Tools
{
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
}
