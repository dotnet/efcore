// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


#if ASPNET50 || ASPNETCORE50

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JB = JetBrains.Annotations;
////using Microsoft.Data.Entity.Commands.Utilities;
////using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.CodeGeneration;
using Microsoft.Framework.CodeGeneration.Templating;
using Microsoft.Framework.CodeGeneration.Templating.Compilation;
using Microsoft.Framework.Runtime.Common.CommandLine;

namespace Microsoft.Data.Entity.ReverseEngineering
{
    // TODO: Add verbose option
    public class Program
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationEnvironment _appEnv;
        private readonly ILibraryManager _libraryManager;
        private readonly ICodeGeneratorActionsService _codeGeneratorActionsService;
        private CommandLineApplication _app;

        public Program([JB.NotNull] IServiceProvider serviceProvider)
        {
            _serviceProvider = InitializeServices(serviceProvider);
            _appEnv = _serviceProvider.GetRequiredService<IApplicationEnvironment>();
            //_projectDir = appEnv.ApplicationBasePath;
            //_rootNamespace = appEnv.ApplicationName;
            _libraryManager = _serviceProvider.GetRequiredService<ILibraryManager>();
            _codeGeneratorActionsService = _serviceProvider.GetRequiredService<ICodeGeneratorActionsService>();
        }

        public virtual int Main([JB.NotNull] string[] args)
        {
            // Check.NotNull(args, "args");

            // TODO: Enable subcommands in help
            _app = new CommandLineApplication { Name = "ef" };
            _app.VersionOption(
                "-v|--version",
                typeof(Program).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion);
            _app.HelpOption("-h|--help");
            _app.Command(
                "re",
                re =>
                {
                    re.Description = "Command to reverse engineer code from a database";
                    re.HelpOption("-h|--help");
                    var providerAssemblyName = re.Argument("[providerAssemblyName]", "The name of the provider assembly which will interpret data from the database");
                    var connectionString = re.Argument("[connectionString]", "The connection string of the database");
                    var outputPath = re.Option(
                        "-o|--outputPath <output_path>",
                        "The path in which to place the generated code",
                        CommandOptionType.SingleValue);
                    var codeNamespace = re.Option(
                        "-n|--namespace <namespace>",
                        "The namespace to use in the generated code",
                        CommandOptionType.SingleValue);
                    var contextClassName = re.Option(
                        "-c|--contextClassName <class_name>",
                        "The name of the class to use for the generated DbContext class",
                        CommandOptionType.SingleValue);
                    var filters = re.Option(
                        "-f|--filters <comma_separated_list>",
                        "The name of the class to use for the generated DbContext class",
                        CommandOptionType.SingleValue);

                    re.OnExecute(() => ReverseEngineer(providerAssemblyName.Value, connectionString.Value, outputPath.Value(),
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


        public IServiceProvider InitializeServices(IServiceProvider serviceProvider)
        {
            var fallbackServiceProvider = new FallbackServiceProvider(serviceProvider);
            fallbackServiceProvider.Add(typeof(IServiceProvider), fallbackServiceProvider);


            //Ordering of services is important here
            var filesLocator = new FilesLocator();
            fallbackServiceProvider.Add(typeof(IFilesLocator), filesLocator);

            var libraryManager =
                serviceProvider.GetRequiredService<ILibraryManager>();
            var applicationEnvironment =
                serviceProvider.GetRequiredService<IApplicationEnvironment>();
            var assemblyLoadContextAccessor =
                serviceProvider.GetRequiredService<IAssemblyLoadContextAccessor>();
            var compilationService =
                new RoslynCompilationService(applicationEnvironment, assemblyLoadContextAccessor, libraryManager);
            fallbackServiceProvider.Add(typeof(ICompilationService), compilationService);

            var templating = new RazorTemplating(compilationService);
            fallbackServiceProvider.Add(typeof(ITemplating), templating);

            var codeGeneratorActionsService = new CodeGeneratorActionsService(templating, filesLocator);
            fallbackServiceProvider.Add(typeof(ICodeGeneratorActionsService), codeGeneratorActionsService);

            return fallbackServiceProvider;
        }

        public virtual int ReverseEngineer(string providerAssemblyName, string connectionString,
            string outputPath, string codeNamespace, string contextClassName, string filters)
        {
            var providerAssembly = GetCandidateAssembly(providerAssemblyName);
            if (providerAssembly == null)
            {
                Console.WriteLine("No provider assembly was found with name " + providerAssemblyName);
                return 1;
            }

            var type = providerAssembly.GetExportedTypes()
                .FirstOrDefault(t => typeof(IDatabaseMetadataModelProvider).IsAssignableFrom(t));
            if (type == null)
            {
                Console.WriteLine("In assembly " + providerAssemblyName + 
                    " no type was found which extends " + typeof(IDatabaseMetadataModelProvider).FullName);
                return 2;
            }

            Console.WriteLine("Args: providerAssemblyName: " + providerAssemblyName);
            Console.WriteLine("Args: connectionString: " + connectionString);
            Console.WriteLine("Args: outputPath: " + outputPath);
            Console.WriteLine("Args: codeNamespace: " + codeNamespace);
            Console.WriteLine("Args: contextClassName: " + contextClassName);
            Console.WriteLine("Args: filters: " + filters);

            IDatabaseMetadataModelProvider metadataModelProvider = null;
            try
            {
                metadataModelProvider = (IDatabaseMetadataModelProvider)Activator.CreateInstance(type);
            }
            catch (Exception)
            {
                Console.WriteLine("In assembly " + providerAssemblyName +
                    " no type was found which extends " + typeof(IDatabaseMetadataModelProvider).FullName);
                return 3;
            }


            var commandLineModel = new ReverseEngineeringGeneratorModel()
                {
                ProviderAssembly = providerAssembly,
                ConnectionString = connectionString,
                OutputPath = outputPath,
                Namespace = codeNamespace,
                ContextClassName = contextClassName,
                Filters = filters
            };

            if (_codeGeneratorActionsService == null)
            {
                Console.WriteLine("_codeGeneratorActionsService == null");
                return 3;
            }

            var libraryManager = _serviceProvider.GetRequiredService<ILibraryManager>();
            var appEnv = _serviceProvider.GetRequiredService<IApplicationEnvironment>();
            var codeGeneratorActionsService = _serviceProvider.GetRequiredService<ICodeGeneratorActionsService>();
            var templatingService = _serviceProvider.GetRequiredService<ITemplating>();
            var generator = new ReverseEngineeringGenerator(libraryManager, appEnv, codeGeneratorActionsService, templatingService);

            // generator.GenerateFromTemplate(commandLineModel, metadataModelProvider).Wait();
            generator.GenerateFromTemplateResource(commandLineModel,
                metadataModelProvider, "ContextTemplate.cshtml", "PocoTemplate.cshtml").Wait();

            return 0;
        }


        private Assembly GetCandidateAssembly(string providerAssemblyName)
        {
            //var assembly = Assembly.LoadFrom(assemblyFilePath);
            //return new List<Assembly>() { assembly };
            ////var refLibs = _libraryManager.GetReferencingLibraries("EntityFramework.ReverseEngineering");
            ////Console.WriteLine("AppName = " + _appEnv.ApplicationName);
            ////Console.WriteLine("Reflibs = " + string.Join(":::", refLibs.Select(l => l.Name)));
            ////return null;

            var libraryManager = _serviceProvider.GetRequiredService<ILibraryManager>();

            return libraryManager.GetReferencingLibraries("EntityFramework.ReverseEngineering")
                .Distinct()
                .Where(l => l.Name == providerAssemblyName)
                .SelectMany(l => l.LoadableAssemblies)
                .Select((assemblyName, assembly) => Assembly.Load(assemblyName))
                .FirstOrDefault();
        }

        //public virtual int ListContexts()
        //{
        //    var contexts = _migrationTool.GetContextTypes();
        //    var any = false;
        //    foreach (var context in contexts)
        //    {
        //        // TODO: Show simple names
        //        Console.WriteLine(context.FullName);
        //        any = true;
        //    }

        //    if (!any)
        //    {
        //        Console.WriteLine("No DbContext was found.");
        //    }

        //    return 0;
        //}

        //public virtual int AddMigration([NotNull] string name, [CanBeNull] string context)
        //{
        //    Check.NotEmpty(name, "name");

        //    var migration = _migrationTool.AddMigration(name, _rootNamespace, context);
        //    _migrationTool.WriteMigration(_projectDir, migration).ToArray();

        //    return 0;
        //}

        //public virtual int ApplyMigration([CanBeNull] string migration, [CanBeNull] string context)
        //{
        //    _migrationTool.ApplyMigration(migration, context);

        //    return 0;
        //}

        //public virtual int ListMigrations([CanBeNull] string context)
        //{
        //    var migrations = _migrationTool.GetMigrations(context);
        //    var any = false;
        //    foreach (var migration in migrations)
        //    {
        //        // TODO: Show simple names
        //        Console.WriteLine(migration.GetMigrationId());
        //        any = true;
        //    }

        //    if (!any)
        //    {
        //        Console.WriteLine("No migrations were found.");
        //    }

        //    return 0;
        //}

        //public virtual int ScriptMigration(
        //    [CanBeNull] string from,
        //    [CanBeNull] string to,
        //    bool idempotent,
        //    [CanBeNull] string context)
        //{
        //    var sql = _migrationTool.ScriptMigration(from, to, idempotent, context);

        //    // TODO: Write to file?
        //    Console.WriteLine(sql);

        //    return 0;
        //}

        public virtual int ShowHelp(string command)
        {
            _app.ShowHelp(command);
            return 0;
        }
    }

    public class FallbackServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        private readonly IServiceProvider _fallbackServiceProvider;

        public FallbackServiceProvider(IServiceProvider fallbackServiceProvider)
        {
            _instances[typeof(IServiceProvider)] = this;
            _fallbackServiceProvider = fallbackServiceProvider;
        }

        public void Add(Type type, object instance)
        {
            _instances[type] = instance;
        }

        public object GetService(Type serviceType)
        {
            object instance;
            if (_instances.TryGetValue(serviceType, out instance))
            {
                return instance;
            }

            if (_fallbackServiceProvider != null)
            {
                return _fallbackServiceProvider.GetService(serviceType);
            }

            return null;
        }
    }
}

#endif
