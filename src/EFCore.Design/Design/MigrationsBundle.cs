// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Tools;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Defines the entry point for Migations bundles.
    /// </summary>
    public static class MigrationsBundle
    {
        /// <summary>
        ///     The entry point for Migrations bundles.
        /// </summary>
        /// <param name="context"> The DbContext to use. </param>
        /// <param name="assembly"> The Migrations assembly. </param>
        /// <param name="startupAssembly"> The startup assembly. </param>
        /// <param name="args"> The command-line arguemnts. </param>
        /// <returns> Zero if the command succeedes; otherwise, one. </returns>
        public static int Execute(string? context, Assembly assembly, Assembly startupAssembly, string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "bundle",
                FullName = DesignStrings.BundleFullName,
                HandleResponseFiles = true
            };

            var migration = app.Argument("<MIGRATION>", DesignStrings.MigrationDescription);
            var connection = app.Option("--connection <CONNECTION>", DesignStrings.ConnectionDescription);

            app.VersionOption("--version", ProductInfo.GetVersion);
            app.HelpOption("-h|--help");
            var verbose = app.Option("-v|--verbose", DesignStrings.VerboseDescription);
            var noColor = app.Option("--no-color", DesignStrings.NoColorDescription);
            var prefixOutput = app.Option("--prefix-output", DesignStrings.PrefixDescription);

            app.OnExecute(
                args =>
                {
                    Reporter.IsVerbose = verbose.HasValue();
                    Reporter.NoColor = noColor.HasValue();
                    Reporter.PrefixOutput = prefixOutput.HasValue();

                    ExecuteInternal(context, assembly, startupAssembly, args, migration.Value, connection.Value());

                    return 0;
                });

            try
            {
                return app.Execute(args);
            }
            catch (Exception ex)
            {
                if (ex is CommandParsingException
                    || ex is OperationException)
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

        private static void ExecuteInternal(
            string? context,
            Assembly assembly,
            Assembly startupAssembly,
            string[] args,
            string? migration,
            string? connection)
        {
            new MigrationsOperations(
                new OperationReporter(
                    new OperationReportHandler(
                        Reporter.WriteError,
                        Reporter.WriteWarning,
                        Reporter.WriteInformation,
                        Reporter.WriteVerbose)),
                assembly,
                startupAssembly,
                projectDir: string.Empty,
                rootNamespace: null,
                language: null,
                nullable: false,
                args)
                .UpdateDatabase(migration, connection, context);
        }
    }
}
