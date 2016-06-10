// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.EntityFrameworkCore.Tools.Cli
{
    public class ExecuteCommand
    {
        private const string DispatcherVersionArgumentName = "--dispatcher-version";
        private const string AssemblyOptionTemplate = "--assembly";
        private const string StartupAssemblyOptionTemplate = "--startup-assembly";
        private const string DataDirectoryOptionTemplate = "--data-dir";
        private const string ProjectDirectoryOptionTemplate = "--project-dir";
        private const string ContentRootPathOptionTemplate = "--content-root-path";
        private const string RootNamespaceOptionTemplate = "--root-namespace";

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

                if (string.Equals(dispatcherVersion, GetVersion(), StringComparison.Ordinal))
                {
                    // Remove dispatcher arguments from
                    var preDispatcherArgument = args.Take(dispatcherArgumentIndex);
                    var postDispatcherArgument = args.Skip(dispatcherArgumentIndex + 2);
                    var newProgramArguments = preDispatcherArgument.Concat(postDispatcherArgument);
                    args = newProgramArguments.ToArray();
                    return;
                }

                ConsoleCommandLogger.Verbose("Expected dispatch version " + GetVersion() + " but received " + dispatcherVersion);
            }

            // Could not validate the dispatcher version.
            throw new OperationException(
                "Could not invoke command. Ensure project.json has matching versions of 'Microsoft.EntityFrameworkCore.Design' in the 'dependencies' section and 'Microsoft.EntityFrameworkCore.Tools' in the 'tools' section.");
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

            var commonOptions = new CommonCommandOptions
            {
                // required
                Assembly = app.Option(AssemblyOptionTemplate + " <ASSEMBLY>",
                     "The assembly file to load."),

                // optional
                StartupAssembly = app.Option(StartupAssemblyOptionTemplate + " <ASSEMBLY>",
                     "The assembly file containing the startup class."),
                DataDirectory = app.Option(DataDirectoryOptionTemplate + " <DIR>",
                    "The folder used as the data directory (defaults to current working directory)."),
                ProjectDirectory = app.Option(ProjectDirectoryOptionTemplate + " <DIR>",
                    "The folder used as the project directory (defaults to current working directory)."),
                ContentRootPath = app.Option(ContentRootPathOptionTemplate + " <DIR>",
                    "The folder used as the content root path for the application (defaults to application base directory)."),
                RootNamespace = app.Option(RootNamespaceOptionTemplate + " <NAMESPACE>",
                    "The root namespace of the target project (defaults to the project assembly name).")
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

            ConsoleCommandLogger.Output("");
            ConsoleCommandLogger.Output(@"                     _/\__       ".Insert(21, Bold + White));
            ConsoleCommandLogger.Output(@"               ---==/    \\      ".Insert(20, Bold + White));
            ConsoleCommandLogger.Output(@"         ___  ___   |.    \|\    ".Insert(26, Bold).Insert(21, Normal).Insert(20, Bold + White).Insert(9, Normal + Magenta));
            ConsoleCommandLogger.Output(@"        | __|| __|  |  )   \\\   ".Insert(20, Bold + White).Insert(8, Normal + Magenta));
            ConsoleCommandLogger.Output(@"        | _| | _|   \_/ |  //|\\ ".Insert(20, Bold + White).Insert(8, Normal + Magenta));
            ConsoleCommandLogger.Output(@"        |___||_|       /   \\\/\\".Insert(33, Normal + Default).Insert(23, Bold + White).Insert(8, Normal + Magenta));
            ConsoleCommandLogger.Output("");
        }

        private static readonly Assembly ThisAssembly = typeof(ExecuteCommand).GetTypeInfo().Assembly;

        private static string GetVersion()
            => ThisAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? ThisAssembly.GetName().Version.ToString();

        public static string GetToolName()
            => typeof(ExecuteCommand).GetTypeInfo().Assembly.GetName().Name;
    }
}