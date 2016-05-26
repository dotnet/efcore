// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class ExecuteCommand
    {
        private static readonly Assembly ThisAssembly = typeof(ExecuteCommand).GetTypeInfo().Assembly;
        private static readonly string AssemblyVersion = ThisAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? ThisAssembly.GetName().Version.ToString();
        
        private const string DispatcherVersionArgumentName = "--dispatcher-version";
        private const string AssemblyOptionTemplate = "--assembly";
        private const string DataDirectoryOptionTemplate = "--data-dir";
        private const string ProjectDirectoryOptionTemplate = "--project-dir";
        private const string RootNamespaceOptionTemplate = "--root-namespace";
        private const string VerboseOptionTemplate = "--verbose";

        public static IEnumerable<string> CreateArgs(
            [NotNull] string assembly,
            [NotNull] string dataDir,
            [NotNull] string projectDir,
            [NotNull] string rootNamespace,
            bool verbose)
            => new[]
            {
                DispatcherVersionArgumentName, AssemblyVersion,
                AssemblyOptionTemplate, Check.NotEmpty(assembly, nameof(assembly)),
                DataDirectoryOptionTemplate, Check.NotEmpty(dataDir, nameof(dataDir)),
                ProjectDirectoryOptionTemplate, Check.NotEmpty(projectDir, nameof(projectDir)),
                RootNamespaceOptionTemplate, Check.NotEmpty(rootNamespace, nameof(rootNamespace)),
                verbose ? VerboseOptionTemplate : string.Empty
            };
            
        private static void EnsureValidDispatchRecipient(ref string[] args)
        {
            if (!args.Contains(DispatcherVersionArgumentName, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            var dispatcherArgumentIndex = Array.FindIndex(
                args,
                (value) => string.Equals(value, DispatcherVersionArgumentName, StringComparison.OrdinalIgnoreCase));
            var dispatcherArgumentValueIndex = dispatcherArgumentIndex + 1;
            if (dispatcherArgumentValueIndex < args.Length)
            {
                var dispatcherVersion = args[dispatcherArgumentValueIndex];

                if (string.Equals(dispatcherVersion, AssemblyVersion, StringComparison.Ordinal))
                {
                    // Remove dispatcher arguments from
                    var preDispatcherArgument = args.Take(dispatcherArgumentIndex);
                    var postDispatcherArgument = args.Skip(dispatcherArgumentIndex + 2);
                    var newProgramArguments = preDispatcherArgument.Concat(postDispatcherArgument);
                    args = newProgramArguments.ToArray();
                    return;
                }
            }

            // Could not validate the dispatchers version.
            throw new InvalidOperationException(
                $"Could not invoke tool {GetToolName()}. Ensure it has matching versions in the project.json's 'dependencies' and 'tools' sections.");
        }

        public static CommandLineApplication Create(ref string[] args)
        {
            EnsureValidDispatchRecipient(ref args);
            
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
            CommonCommandOptions commonOptions = null;

            if (args.Contains(AssemblyOptionTemplate))
            {
                // hidden parameters. Only add these to app for parsing.
                commonOptions = new CommonCommandOptions
                {
                    Assembly = app.Option(AssemblyOptionTemplate + " <ASSEMBLY>",
                        "The assembly file to load"),
                    DataDirectory = app.Option(DataDirectoryOptionTemplate + " <DIR>",
                        "The folder to use as the data directory. If not specified, the runtime output folder is used."),
                    ProjectDirectory = app.Option(ProjectDirectoryOptionTemplate + " <DIR>",
                        "The folder to use as the project directory. If not specified, the current working is used."),
                    RootNamespace = app.Option(RootNamespaceOptionTemplate + " <NAMESPACE>",
                        "The root namespace of the current project. If not specified, the project name is used.")
                };
            }

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

            ConsoleCommandLogger.Output("");
            ConsoleCommandLogger.Output(@"                     _/\__       ".Insert(21, Bold + White));
            ConsoleCommandLogger.Output(@"               ---==/    \\      ");
            ConsoleCommandLogger.Output(@"         ___  ___   |.    \|\    ".Insert(26, Bold).Insert(21, Normal).Insert(20, Bold + White).Insert(9, Normal + Magenta));
            ConsoleCommandLogger.Output(@"        | __|| __|  |  )   \\\   ".Insert(20, Bold + White).Insert(8, Normal + Magenta));
            ConsoleCommandLogger.Output(@"        | _| | _|   \_/ |  //|\\ ".Insert(20, Bold + White).Insert(8, Normal + Magenta));
            ConsoleCommandLogger.Output(@"        |___||_|       /   \\\/\\".Insert(33, Normal + Default).Insert(23, Bold + White).Insert(8, Normal + Magenta));
            ConsoleCommandLogger.Output("");
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