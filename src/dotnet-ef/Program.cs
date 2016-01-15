// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class Program
    {
        public static int Main([NotNull] string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);

            var app = new CommandLineApplication
            {
                Name = "dotnet ef",
                FullName = "Entity Framework Core Commands"
            };

            app.HelpOption("-h|--help");
            app.VersionOption("--version", GetVersion);

            app.Command("database", DatabaseCommand.Configure);
            app.Command("dbcontext", DbContextCommand.Configure);
            app.Command("migrations", MigrationsCommand.Configure);

            app.OnExecute(
                () =>
                {
                    WriteLogo();
                    app.ShowHelp();
                });


            int result;
            try
            {
                result = app.Execute(args);
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ex = ex.InnerException;
                }
                
                if (!(ex is OperationException))
                {
                    Reporter.Verbose.WriteLine(ex.ToString().Bold().Black());
                }

                Reporter.Error.WriteLine(ex.Message.Bold().Red());
                result = 1;
            }

            return result;
        }

        private static string GetVersion()
            => typeof(Program).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

        private static void WriteLogo()
        {
            const string Bold = "\x1b[1m";
            const string Normal = "\x1b[22m";
            const string Magenta = "\x1b[35m";
            const string White = "\x1b[37m";
            const string Default = "\x1b[39m";

            Reporter.Output.WriteLine();
            Reporter.Output.WriteLine(@"                     _/\__       ".Insert(21, Bold + White));
            Reporter.Output.WriteLine(@"               ---==/    \\      ");
            Reporter.Output.WriteLine(@"         ___  ___   |.    \|\    ".Insert(26, Bold).Insert(21, Normal).Insert(20, Bold + White).Insert(9, Normal + Magenta));
            Reporter.Output.WriteLine(@"        | __|| __|  |  )   \\\   ".Insert(20, Bold + White).Insert(8, Normal + Magenta));
            Reporter.Output.WriteLine(@"        | _| | _|   \_/ |  //|\\ ".Insert(20, Bold + White).Insert(8, Normal + Magenta));
            Reporter.Output.WriteLine(@"        |___||_|       /   \\\/\\".Insert(33, Normal + Default).Insert(23, Bold + White).Insert(8, Normal + Magenta));
            Reporter.Output.WriteLine();
        }
    }
}
