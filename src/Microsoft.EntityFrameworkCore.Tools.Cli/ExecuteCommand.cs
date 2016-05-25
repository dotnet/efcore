// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class ExecuteCommand
    {
        private const string FrameworkOptionTemplate = "--framework";
        private const string ConfigOptionTemplate = "--configuration";
        private const string VerboseOptionTemplate = "--verbose";

        public static IEnumerable<string> CreateArgs(
            [NotNull] NuGetFramework framework,
            [NotNull] string configuration,
            bool verbose)
            => new[]
            {
                FrameworkOptionTemplate, framework.GetShortFolderName(),
                ConfigOptionTemplate, configuration,
                verbose ? VerboseOptionTemplate : string.Empty
            };

        public static CommandLineApplication Create()
        {
            var app = new CommandLineApplication()
            {
                // Technically, the real "dotnet-ef.dll" is in Microsoft.EntityFrameworkCore.Tools,
                // but this name is what the help usage displays
                Name = "dotnet ef",
                FullName = "Entity Framework .NET Core CLI Commands"
            };

            app.HelpOption();
            app.VerboseOption();
            app.VersionOption(GetVersion);

            var commonOptions = new CommonCommandOptions
            {
                Framework = app.Option(FrameworkOptionTemplate + " <FRAMEWORK>",
                    "Target framework to load",
                    CommandOptionType.SingleValue),
                Configuration = app.Option(ConfigOptionTemplate + " <CONFIGURATION>",
                    "Configuration under which to load",
                    CommandOptionType.SingleValue)
            };

            app.Command("database", c => DatabaseCommand.Configure(c, commonOptions));
            app.Command("dbcontext", c => DbContextCommand.Configure(c, commonOptions));
            app.Command("migrations", c => MigrationsCommand.Configure(c, commonOptions));

            app.OnExecute(
                () =>
                {
                    WriteLogo();
                    app.ShowHelp();
                });

            return app;
        }

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

        private static string GetVersion()
            => typeof(ExecuteCommand)
                .GetTypeInfo()
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

        public static string GetToolName()
            => typeof(ExecuteCommand).GetTypeInfo().Assembly.GetName().Name;
    }
}