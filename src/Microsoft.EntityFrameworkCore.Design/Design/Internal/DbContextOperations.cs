// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class DbContextOperations
    {
        private readonly IOperationReporter _reporter;
        private readonly Assembly _assembly;
        private readonly Assembly _startupAssembly;
        private readonly string _environment;
        private readonly string _contentRootPath;
        private readonly IServiceProvider _runtimeServices;

        public DbContextOperations(
            [NotNull] IOperationReporter reporter,
            [NotNull] Assembly assembly,
            [NotNull] Assembly startupAssembly,
            [CanBeNull] string environment,
            [NotNull] string contentRootPath)
        {
            Check.NotNull(reporter, nameof(reporter));
            Check.NotNull(assembly, nameof(assembly));
            Check.NotNull(startupAssembly, nameof(startupAssembly));
            Check.NotEmpty(contentRootPath, nameof(contentRootPath));

            _reporter = reporter;
            _assembly = assembly;
            _startupAssembly = startupAssembly;
            _environment = environment;
            _contentRootPath = contentRootPath;

            var startup = new StartupInvoker(reporter, startupAssembly, environment, contentRootPath);
            _runtimeServices = startup.ConfigureServices();
        }

        public virtual void DropDatabase([CanBeNull] string contextType)
        {
            using (var context = CreateContext(contextType))
            {
                var connection = context.Database.GetDbConnection();
                _reporter.WriteInformation(DesignStrings.LogDroppingDatabase(connection.Database));
                if (context.Database.EnsureDeleted())
                {
                    _reporter.WriteInformation(DesignStrings.LogDatabaseDropped(connection.Database));
                }
                else
                {
                    _reporter.WriteInformation(DesignStrings.LogNotExistDatabase(connection.Database));
                }
            }
        }

        public virtual DbContext CreateContext([CanBeNull] string contextType)
            => CreateContext(FindContextType(contextType).Value);

        private DbContext CreateContext(Func<DbContext> factory)
        {
            var context = factory();
            _reporter.WriteVerbose(DesignStrings.LogUseContext(context.GetType().ShortDisplayName()));

            var loggerFactory = context.GetService<ILoggerFactory>();
            loggerFactory.AddProvider(new LoggerProvider(categoryName => new OperationLogger(categoryName, _reporter)));

            return context;
        }

        public virtual IEnumerable<Type> GetContextTypes()
            => FindContextTypes().Keys;

        public virtual Type GetContextType([CanBeNull] string name)
            => FindContextType(name).Key;

        private IDictionary<Type, Func<DbContext>> FindContextTypes()
        {
            _reporter.WriteVerbose(DesignStrings.LogFindingContexts);

            var contexts = new Dictionary<Type, Func<DbContext>>();

            // Look for IDbContextFactory implementations
            var contextFactories = _startupAssembly.GetConstructableTypes()
                .Where(t => typeof(IDbContextFactory<DbContext>).GetTypeInfo().IsAssignableFrom(t));
            foreach (var factory in contextFactories)
            {
                var manufacturedContexts =
                    from i in factory.ImplementedInterfaces
                    where i.GetTypeInfo().IsGenericType
                          && i.GetGenericTypeDefinition() == typeof(IDbContextFactory<>)
                    select i.GenericTypeArguments[0];
                foreach (var context in manufacturedContexts)
                {
                    contexts.Add(
                        context,
                        () => ((IDbContextFactory<DbContext>)Activator.CreateInstance(factory.AsType())).Create(
                            CreateFactoryOptions()));
                }
            }

            // Look for DbContext classes registered in the service provider
            var registeredContexts = _runtimeServices.GetServices<DbContextOptions>()
                .Select(o => o.ContextType);
            foreach (var context in registeredContexts.Where(c => !contexts.ContainsKey(c)))
            {
                contexts.Add(
                    context,
                    FindContextFactory(context) ?? (() => (DbContext)ActivatorUtilities.CreateInstance(_runtimeServices, context)));
            }

            // Look for DbContext classes in assemblies
            var types = _startupAssembly.GetConstructableTypes()
                .Concat(_assembly.GetConstructableTypes())
                .Select(i => i.AsType())
                .ToList();
            var contextTypes = types.Where(t => typeof(DbContext).IsAssignableFrom(t))
                .Concat(
                    types.Where(t => typeof(Migration).IsAssignableFrom(t))
                        .Select(t => t.GetTypeInfo().GetCustomAttribute<DbContextAttribute>()?.ContextType)
                        .Where(t => t != null))
                .Distinct();
            foreach (var context in contextTypes.Where(c => !contexts.ContainsKey(c)))
            {
                contexts.Add(
                    context,
                    FindContextFactory(context) ?? (() =>
                        {
                            try
                            {
                                return (DbContext)Activator.CreateInstance(context);
                            }
                            catch (MissingMethodException ex)
                            {
                                throw new OperationException(DesignStrings.NoParameterlessConstructor(context.Name), ex);
                            }
                        }));
            }

            return contexts;
        }

        public virtual ContextInfo GetContextInfo([CanBeNull] string contextType)
        {
            using (var context = CreateContext(contextType))
            {
                var connection = context.Database.GetDbConnection();
                return new ContextInfo
                {
                    DatabaseName = connection.Database,
                    DataSource = connection.DataSource
                };
            }
        }

        private Func<DbContext> FindContextFactory(Type contextType)
        {
            var factoryInterface = typeof(IDbContextFactory<>).MakeGenericType(contextType).GetTypeInfo();
            var factory = contextType.GetTypeInfo().Assembly.GetConstructableTypes()
                .Where(t => factoryInterface.IsAssignableFrom(t))
                .Select(t => t.AsType())
                .FirstOrDefault();
            if (factory == null)
            {
                return null;
            }

            return () => ((IDbContextFactory<DbContext>)Activator.CreateInstance(factory)).Create(
                CreateFactoryOptions());
        }

        private KeyValuePair<Type, Func<DbContext>> FindContextType(string name)
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

            Debug.Assert(candidates.Count == 1, "candidates.Count is not 1.");

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

        private DbContextFactoryOptions CreateFactoryOptions()
            => new DbContextFactoryOptions
            {
#if NET451
                ApplicationBasePath = AppDomain.CurrentDomain.GetData("APP_CONTEXT_BASE_DIRECTORY") as string ?? AppDomain.CurrentDomain.BaseDirectory,
#else
                ApplicationBasePath = AppContext.BaseDirectory,
#endif
                ContentRootPath = _contentRootPath,
                EnvironmentName = !string.IsNullOrEmpty(_environment)
                    ? _environment
                    : "Development"
            };
    }
}
