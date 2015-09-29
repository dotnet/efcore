// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Design.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Design
{
    public class DbContextOperations
    {
        private readonly ILoggerProvider _loggerProvider;
        private readonly string _assemblyName;
        private readonly string _startupAssemblyName;
        private readonly LazyRef<ILogger> _logger;
        private readonly IServiceProvider _runtimeServices;

        public DbContextOperations(
            [NotNull] ILoggerProvider loggerProvider,
            [NotNull] string assemblyName,
            [NotNull] string startupAssemblyName,
            [CanBeNull] string environment,
            [CanBeNull] IServiceProvider dnxServices = null)
        {
            Check.NotNull(loggerProvider, nameof(loggerProvider));
            Check.NotEmpty(assemblyName, nameof(assemblyName));
            Check.NotEmpty(startupAssemblyName, nameof(startupAssemblyName));

            _loggerProvider = loggerProvider;
            _assemblyName = assemblyName;
            _startupAssemblyName = startupAssemblyName;
            _logger = new LazyRef<ILogger>(() => _loggerProvider.CreateCommandsLogger());

            var startup = new StartupInvoker(startupAssemblyName, environment, dnxServices);
            _runtimeServices = startup.ConfigureServices();
        }

        public virtual DbContext CreateContext([CanBeNull] string contextType)
        {
            var context = FindContextType(contextType).Value();
            _logger.Value.LogVerbose(CommandsStrings.LogUseContext(context.GetType().Name));

            var loggerFactory = context.GetService<ILoggerFactory>();
            loggerFactory.AddProvider(_loggerProvider);

            return context;
        }

        public virtual IEnumerable<Type> GetContextTypes()
            => FindContextTypes().Keys;

        public virtual Type GetContextType([CanBeNull] string name)
            => FindContextType(name).Key;

        private IDictionary<Type, Func<DbContext>> FindContextTypes()
        {
            // TODO: Look for IDbContextFactory implementations
            var contexts = new Dictionary<Type, Func<DbContext>>();

            // Look for DbContext classes registered in the service provider
            var registeredContexts = _runtimeServices.GetServices<DbContextOptions>()
                .Select(o => o.GetType().GenericTypeArguments[0]);
            foreach (var context in registeredContexts)
            {
                contexts.Add(context, () => (DbContext)_runtimeServices.GetRequiredService(context));
            }

            // Look for DbContext classes in assemblies
            var startupAssembly = Assembly.Load(new AssemblyName(_startupAssemblyName));

            Assembly assembly;
            try
            {
                assembly = Assembly.Load(new AssemblyName(_assemblyName));
            }
            catch (Exception ex)
            {
                throw new OperationException(CommandsStrings.UnreferencedAssembly(_assemblyName, _startupAssemblyName), ex);
            }

            var types = startupAssembly.GetConstructibleTypes()
                .Concat(assembly.GetConstructibleTypes())
                .Select(i => i.AsType());
            var contextTypes = types.Where(t => typeof(DbContext).IsAssignableFrom(t))
                .Concat(
                    types.Where(t => typeof(Migration).IsAssignableFrom(t))
                        .Select(t => t.GetTypeInfo().GetCustomAttribute<DbContextAttribute>()?.ContextType)
                        .Where(t => t != null))
                .Distinct();
            foreach (var context in contextTypes)
            {
                contexts.Add(context, () => (DbContext)Activator.CreateInstance(context));
            }

            return contexts;
        }

        private KeyValuePair<Type, Func<DbContext>> FindContextType(string name)
        {
            var types = FindContextTypes();

            if (string.IsNullOrEmpty(name))
            {
                if (types.Count == 0)
                {
                    throw new OperationException(CommandsStrings.NoContext);
                }
                if (types.Count == 1)
                {
                    return types.First();
                }

                throw new OperationException(CommandsStrings.MultipleContexts);
            }

            var candidates = FilterTypes(types, name, ignoreCase: true);
            if (candidates.Count == 0)
            {
                throw new OperationException(CommandsStrings.NoContextWithName(name));
            }
            if (candidates.Count == 1)
            {
                return candidates.First();
            }

            // Disambiguate using case
            candidates = FilterTypes(candidates, name);
            if (candidates.Count == 0)
            {
                throw new OperationException(CommandsStrings.MultipleContextsWithName(name));
            }
            if (candidates.Count == 1)
            {
                return candidates.First();
            }

            // Allow selecting types in the default namespace
            candidates = candidates.Where(t => t.Key.Namespace == null).ToDictionary(t => t.Key, t => t.Value);
            if (candidates.Count == 0)
            {
                throw new OperationException(CommandsStrings.MultipleContextsWithQualifiedName(name));
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
    }
}