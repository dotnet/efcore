// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Commands
{
    public class Program
    {
        public static int Main([NotNull] string[] args)
        {
            HandleVerboseOption(ref args);
            DebugHelper.HandleDebugSwitch(ref args);

            var app = new CommandLineApplication
            {
                Name = "dotnet ef",
                FullName = "Entity Framework .NET Core CLI Commands"
            };

            app.HelpOption();
            app.VerboseOption();
            app.VersionOption(GetVersion);

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

                if ((ex as OperationErrorException)?.Type != OperationErrorException.OperationException)
                {
                    Reporter.Error.WriteLine(ex.ToString());
                }

                Reporter.Error.WriteLine(ex.Message.Bold().Red());
                result = 1;
            }

            return result;
        }

        private static void HandleVerboseOption(ref string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-v" || args[i] == "--verbose")
                {
                    Environment.SetEnvironmentVariable(CommandContext.Variables.Verbose, bool.TrueString);
                    args = args.Take(i).Concat(args.Skip(i + 1)).ToArray();

                    return;
                }
            }
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
