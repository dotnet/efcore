// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Common.CommandLine;

namespace Microsoft.Data.Entity.ReverseEngineering
{
    // TODO: Add verbose option
    public class Program
    {
        private readonly IServiceProvider _serviceProvider;
        private CommandLineApplication _app;

        public Program(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public virtual int Main(string[] args)
        {
            // TODO: Enable subcommands in help
            _app = new CommandLineApplication { Name = "ef" };
            _app.VersionOption(
                "-v|--version",
                typeof(Program).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion);
            _app.HelpOption("-h|--help");
            _app.Command(
                "fromDB",
                fromDb =>
                {
                    fromDb.Description = "Command to reverse engineer code from a database";
                    fromDb.HelpOption("-h|--help");

                    var connectionString = fromDb.Argument(
                            "[connectionString]",
                            "The connection string of the database");
                    var providerAssemblyName = fromDb.Argument(
                            "[providerAssemblyName]",
                            "The name of the provider assembly which will interpret data from the database");

                    var outputPath = fromDb.Option(
                        "-o|--outputPath <output_path>",
                        "The path of the directory in which to place the generated code",
                        CommandOptionType.SingleValue);
                    var codeNamespace = fromDb.Option(
                        "-n|--namespace <namespace>",
                        "The namespace to use in the generated code",
                        CommandOptionType.SingleValue);
                    var contextClassName = fromDb.Option(
                        "-c|--contextClassName <class_name>",
                        "The name of the class to use for the generated DbContext class",
                        CommandOptionType.SingleValue);
                    var filters = fromDb.Option(
                        "-f|--filters <comma_separated_list>",
                        "The name of the class to use for the generated DbContext class",
                        CommandOptionType.SingleValue);

                    fromDb.OnExecute(() => ReverseEngineerFromDatabase(
                        connectionString.Value, providerAssemblyName.Value, outputPath.Value(),
                        codeNamespace.Value(), contextClassName.Value(), filters.Value()));
                },
                addHelpCommand: false);

            _app.Command(
                "help",
                help =>
                {
                    help.Description = "Show help information";
                    var command = help.Argument("[command]", "Command that help information explains");
                    help.OnExecute(() => ShowHelp(command.Value));
                },
                addHelpCommand: false);
            _app.OnExecute(() => ShowHelp(command: null));

            return _app.Execute(args);
        }

        public virtual int ShowHelp(string command)
        {
            _app.ShowHelp(command);
            return 0;
        }

        public virtual int ReverseEngineerFromDatabase(
            string connectionString, string providerAssemblyName, string outputPath,
            string codeNamespace, string contextClassName, string filters)
        {
            var providerAssembly = GetCandidateAssembly(providerAssemblyName);
            if (providerAssembly == null)
            {
                Console.WriteLine("No provider assembly was found with name " + providerAssemblyName);
                return 1;
            }

            Console.WriteLine("Args: providerAssemblyName: " + providerAssemblyName);
            Console.WriteLine("Args: connectionString: " + connectionString);
            Console.WriteLine("Args: outputPath: " + outputPath);
            Console.WriteLine("Args: codeNamespace: " + codeNamespace);
            Console.WriteLine("Args: contextClassName: " + contextClassName);
            Console.WriteLine("Args: filters: " + filters);

            var configuration = new ReverseEngineeringConfiguration()
            {
                ProviderAssembly = providerAssembly,
                ConnectionString = connectionString,
                OutputPath = outputPath,
                Namespace = codeNamespace,
                ContextClassName = contextClassName,
                Filters = filters
            };

            var generator = new ReverseEngineeringGenerator();
            generator.Generate(configuration).Wait();

            return 0;
        }


        private Assembly GetCandidateAssembly(string providerAssemblyName)
        {
            var libraryManager = _serviceProvider.GetRequiredService<ILibraryManager>();

            return libraryManager.GetReferencingLibraries("EntityFramework.ReverseEngineering")
                .Distinct()
                .Where(l => l.Name == providerAssemblyName)
                .SelectMany(l => l.LoadableAssemblies)
                .Select((assemblyName, assembly) => Assembly.Load(assemblyName))
                .FirstOrDefault();
        }
    }
}
