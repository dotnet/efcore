// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class DbContextOperations
    {
        private readonly IOperationReporter _reporter;
        private readonly Assembly _assembly;
        private readonly Assembly _startupAssembly;
        private readonly string _projectDir;
        private readonly string? _rootNamespace;
        private readonly string? _language;
        private readonly bool _nullable;
        private readonly string[] _args;
        private readonly AppServiceProviderFactory _appServicesFactory;
        private readonly DesignTimeServicesBuilder _servicesBuilder;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public DbContextOperations(
            IOperationReporter reporter,
            Assembly assembly,
            Assembly startupAssembly,
            string projectDir,
            string? rootNamespace,
            string? language,
            bool nullable,
            string[]? args)
            : this(reporter,
                  assembly,
                  startupAssembly,
                  projectDir,
                  rootNamespace,
                  language,
                  nullable,
                  args,
                  new AppServiceProviderFactory(startupAssembly, reporter))
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected DbContextOperations(
            IOperationReporter reporter,
            Assembly assembly,
            Assembly startupAssembly,
            string projectDir,
            string? rootNamespace,
            string? language,
            bool nullable,
            string[]? args,
            AppServiceProviderFactory appServicesFactory)
        {
            Check.NotNull(reporter, nameof(reporter));
            Check.NotNull(assembly, nameof(assembly));
            Check.NotNull(startupAssembly, nameof(startupAssembly));
            Check.NotNull(projectDir, nameof(projectDir));

            _reporter = reporter;
            _assembly = assembly;
            _startupAssembly = startupAssembly;
            _projectDir = projectDir;
            _rootNamespace = rootNamespace;
            _language = language;
            _nullable = nullable;
            _args = args ?? Array.Empty<string>();
            _appServicesFactory = appServicesFactory;
            _servicesBuilder = new DesignTimeServicesBuilder(assembly, startupAssembly, reporter, _args);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void DropDatabase(string? contextType)
        {
            using var context = CreateContext(contextType);
            var connection = context.Database.GetDbConnection();
            _reporter.WriteInformation(DesignStrings.DroppingDatabase(connection.Database, connection.DataSource));
            if (context.Database.EnsureDeleted())
            {
                _reporter.WriteInformation(DesignStrings.DatabaseDropped(connection.Database));
            }
            else
            {
                _reporter.WriteInformation(DesignStrings.NotExistDatabase(connection.Database));
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string ScriptDbContext(string? contextType)
        {
            using var context = CreateContext(contextType);
            return context.Database.GenerateCreateScript();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Optimize(string? outputDir, string? modelNamespace, string? contextType)
        {
            using var context = CreateContext(contextType);

            var services = _servicesBuilder.Build(context);
            var scaffolder = services.GetRequiredService<ICompiledModelScaffolder>();

            outputDir = outputDir != null
                ? Path.GetFullPath(Path.Combine(_projectDir, outputDir))
                : _projectDir;

            var finalModelNamespace = modelNamespace ?? GetNamespaceFromOutputPath(outputDir) ?? "";

            scaffolder.ScaffoldModel(
                context.GetService<IDesignTimeModel>().Model,
                outputDir,
                new CompiledModelCodeGenerationOptions
                {
                    ContextType = context.GetType(),
                    ModelNamespace = finalModelNamespace,
                    Language = _language,
                    UseNullableReferenceTypes = _nullable
                });

            var fullName = context.GetType().ShortDisplayName() + "Model";
            if (!string.IsNullOrEmpty(modelNamespace))
            {
                fullName = modelNamespace + "." + fullName;
            }

            _reporter.WriteInformation(DesignStrings.CompiledModelGenerated($"options.UseModel({fullName}.Instance)"));

            var cacheKeyFactory = context.GetService<IModelCacheKeyFactory>();
            if (!(cacheKeyFactory is ModelCacheKeyFactory))
            {
                _reporter.WriteWarning(DesignStrings.CompiledModelCustomCacheKeyFactory(cacheKeyFactory.GetType().ShortDisplayName()));
            }
        }

        private string? GetNamespaceFromOutputPath(string directoryPath)
        {
            var subNamespace = SubnamespaceFromOutputPath(_projectDir, directoryPath);
            return string.IsNullOrEmpty(subNamespace)
                ? _rootNamespace
                : string.IsNullOrEmpty(_rootNamespace)
                    ? subNamespace
                    : _rootNamespace + "." + subNamespace;
        }

        // if outputDir is a subfolder of projectDir, then use each subfolder as a subnamespace
        // --output-dir $(projectFolder)/A/B/C
        // => "namespace $(rootnamespace).A.B.C"
        private static string? SubnamespaceFromOutputPath(string projectDir, string outputDir)
        {
            if (!outputDir.StartsWith(projectDir, StringComparison.Ordinal))
            {
                return null;
            }

            var subPath = outputDir.Substring(projectDir.Length);

            return !string.IsNullOrWhiteSpace(subPath)
                ? string.Join(
                    ".",
                    subPath.Split(
                        new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries))
                : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DbContext CreateContext(string? contextType)
            => CreateContext(FindContextType(contextType).Value);

        private DbContext CreateContext(Func<DbContext> factory)
        {
            var context = factory();
            _reporter.WriteVerbose(DesignStrings.UseContext(context.GetType().ShortDisplayName()));

            var loggerFactory = context.GetService<ILoggerFactory>();
            loggerFactory.AddProvider(new OperationLoggerProvider(_reporter));

            return context;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Type> GetContextTypes()
            => FindContextTypes().Keys;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Type GetContextType(string? name)
            => FindContextType(name).Key;

        private IDictionary<Type, Func<DbContext>> FindContextTypes()
        {
            _reporter.WriteVerbose(DesignStrings.FindingContexts);

            var contexts = new Dictionary<Type, Func<DbContext>>();

            // Look for IDesignTimeDbContextFactory implementations
            _reporter.WriteVerbose(DesignStrings.FindingContextFactories);
            var contextFactories = _startupAssembly.GetConstructibleTypes()
                .Where(t => typeof(IDesignTimeDbContextFactory<DbContext>).IsAssignableFrom(t));
            foreach (var factory in contextFactories)
            {
                _reporter.WriteVerbose(DesignStrings.FoundContextFactory(factory.ShortDisplayName()));
                var manufacturedContexts =
                    from i in factory.ImplementedInterfaces
                    where i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IDesignTimeDbContextFactory<>)
                    select i.GenericTypeArguments[0];
                foreach (var context in manufacturedContexts)
                {
                    _reporter.WriteVerbose(DesignStrings.FoundDbContext(context.ShortDisplayName()));
                    contexts.Add(
                        context,
                        () => CreateContextFromFactory(factory.AsType(), context));
                }
            }

            // Look for DbContext classes registered in the service provider
            var appServices = _appServicesFactory.Create(_args);
            var registeredContexts = appServices.GetServices<DbContextOptions>()
                .Select(o => o.ContextType);
            foreach (var context in registeredContexts.Where(c => !contexts.ContainsKey(c)))
            {
                _reporter.WriteVerbose(DesignStrings.FoundDbContext(context.ShortDisplayName()));
                contexts.Add(
                    context,
                    FindContextFactory(context)
                    ?? FindContextFromRuntimeDbContextFactory(appServices, context)
                    ?? (() => (DbContext)ActivatorUtilities.GetServiceOrCreateInstance(appServices, context)));
            }

            // Look for DbContext classes in assemblies
            _reporter.WriteVerbose(DesignStrings.FindingReferencedContexts);
            var types = _startupAssembly.GetConstructibleTypes()
                .Concat(_assembly.GetConstructibleTypes())
                .ToList();

            var contextTypes = types.Where(t => typeof(DbContext).IsAssignableFrom(t)).Select(
                    t => t.AsType())
                .Concat(
                    types.Where(t => typeof(Migration).IsAssignableFrom(t))
                        .Select(t => t.GetCustomAttribute<DbContextAttribute>()?.ContextType)
                        .Where(t => t != null)
                        .Cast<Type>())
                .Distinct();

            foreach (var context in contextTypes.Where(c => !contexts.ContainsKey(c)))
            {
                _reporter.WriteVerbose(DesignStrings.FoundDbContext(context.ShortDisplayName()));
                contexts.Add(
                    context,
                    FindContextFactory(context)
                    ?? (() =>
                    {
                        try
                        {
                            return (DbContext)ActivatorUtilities.GetServiceOrCreateInstance(appServices, context);
                        }
                        catch (Exception ex)
                        {
                            throw new OperationException(DesignStrings.NoParameterlessConstructor(context.Name), ex);
                        }
                    }));
            }

            return contexts;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ContextInfo GetContextInfo(string? contextType)
        {
            using var context = CreateContext(contextType);
            var info = new ContextInfo
            {
                Type = context.GetType().FullName!
            };

            var provider = context.GetService<IDatabaseProvider>();
            info.ProviderName = provider.Name;

            if (((IDatabaseFacadeDependenciesAccessor)context.Database).Dependencies is IRelationalDatabaseFacadeDependencies)
            {
                try
                {
                    var connection = context.Database.GetDbConnection();
                    info.DataSource = connection.DataSource;
                    info.DatabaseName = connection.Database;
                }
                catch (Exception exception)
                {
                    info.DataSource = info.DatabaseName = DesignStrings.BadConnection(exception.Message);
                }
            }
            else
            {
                info.DataSource = info.DatabaseName = DesignStrings.NoRelationalConnection;
            }

            var options = context.GetService<IDbContextOptions>();
            info.Options = options.BuildOptionsFragment().Trim();

            return info;
        }

        private Func<DbContext>? FindContextFromRuntimeDbContextFactory(IServiceProvider appServices, Type contextType)
        {
            var factoryInterface = typeof(IDbContextFactory<>).MakeGenericType(contextType);
            var service = appServices.GetService(factoryInterface);
            return service == null
                ? (Func<DbContext>?)null
                : () => (DbContext)factoryInterface
                    .GetMethod(nameof(IDbContextFactory<DbContext>.CreateDbContext))
                    !.Invoke(service, null)!;
        }

        private Func<DbContext>? FindContextFactory(Type contextType)
        {
            var factoryInterface = typeof(IDesignTimeDbContextFactory<>).MakeGenericType(contextType);
            var factory = contextType.Assembly.GetConstructibleTypes()
                .FirstOrDefault(t => factoryInterface.IsAssignableFrom(t));
            return factory == null ? (Func<DbContext>?)null : (() => CreateContextFromFactory(factory.AsType(), contextType));
        }

        private DbContext CreateContextFromFactory(Type factory, Type contextType)
        {
            _reporter.WriteVerbose(DesignStrings.UsingDbContextFactory(factory.ShortDisplayName()));

            return (DbContext)typeof(IDesignTimeDbContextFactory<>).MakeGenericType(contextType)
                .GetMethod(nameof(IDesignTimeDbContextFactory<DbContext>.CreateDbContext), new[] { typeof(string[]) })!
                .Invoke(Activator.CreateInstance(factory), new object[] { _args })!;
        }

        private KeyValuePair<Type, Func<DbContext>> FindContextType(string? name)
        {
            var types = FindContextTypes();

            if (string.IsNullOrEmpty(name))
            {
                if (types.Count == 0)
                {
                    throw new OperationException(DesignStrings.NoContext(_assembly.GetName().Name));
                }

                if (types.Count == 1)
                {
                    return types.First();
                }

                throw new OperationException(DesignStrings.MultipleContexts);
            }

            var candidates = FilterTypes(types, name, ignoreCase: true);
            if (candidates.Count == 0)
            {
                throw new OperationException(DesignStrings.NoContextWithName(name));
            }

            if (candidates.Count == 1)
            {
                return candidates.First();
            }

            // Disambiguate using case
            candidates = FilterTypes(candidates, name);
            if (candidates.Count == 0)
            {
                throw new OperationException(DesignStrings.MultipleContextsWithName(name));
            }

            if (candidates.Count == 1)
            {
                return candidates.First();
            }

            // Allow selecting types in the default namespace
            candidates = candidates.Where(t => t.Key.Namespace == null).ToDictionary(t => t.Key, t => t.Value);
            if (candidates.Count == 0)
            {
                throw new OperationException(DesignStrings.MultipleContextsWithQualifiedName(name));
            }

            Check.DebugAssert(candidates.Count == 1, $"candidates.Count is {candidates.Count}");

            return candidates.First();
        }

        private static IDictionary<Type, Func<DbContext>> FilterTypes(
            IDictionary<Type, Func<DbContext>> types,
            string name,
            bool ignoreCase = false)
        {
            var comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return types
                .Where(
                    t => string.Equals(t.Key.Name, name, comparisonType)
                        || string.Equals(t.Key.FullName, name, comparisonType)
                        || string.Equals(t.Key.AssemblyQualifiedName, name, comparisonType))
                .ToDictionary(t => t.Key, t => t.Value);
        }
    }
}
