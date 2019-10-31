// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class MigrationsAssembly : IMigrationsAssembly
    {
        private readonly IMigrationsIdGenerator _idGenerator;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Migrations> _logger;
        private IReadOnlyDictionary<string, TypeInfo> _migrations;
        private ModelSnapshot _modelSnapshot;
        private readonly Type _contextType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public MigrationsAssembly(
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IDbContextOptions options,
            [NotNull] IMigrationsIdGenerator idGenerator,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
        {
            Check.NotNull(currentContext, nameof(currentContext));
            Check.NotNull(options, nameof(options));
            Check.NotNull(idGenerator, nameof(idGenerator));
            Check.NotNull(logger, nameof(logger));

            _contextType = currentContext.Context.GetType();

            var assemblyName = RelationalOptionsExtension.Extract(options)?.MigrationsAssembly;
            Assembly = assemblyName == null
                ? _contextType.GetTypeInfo().Assembly
                : Assembly.Load(new AssemblyName(assemblyName));

            _idGenerator = idGenerator;
            _logger = logger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyDictionary<string, TypeInfo> Migrations
        {
            get
            {
                IReadOnlyDictionary<string, TypeInfo> Create()
                {
                    var result = new SortedList<string, TypeInfo>();

                    var items
                        = from t in Assembly.GetConstructibleTypes()
                          where t.IsSubclassOf(typeof(Migration))
                              && t.GetCustomAttribute<DbContextAttribute>()?.ContextType == _contextType
                          let id = t.GetCustomAttribute<MigrationAttribute>()?.Id
                          orderby id
                          select (id, t);

                    foreach (var (id, t) in items)
                    {
                        if (id == null)
                        {
                            _logger.MigrationAttributeMissingWarning(t);

                            continue;
                        }

                        result.Add(id, t);
                    }

                    return result;
                }

                return _migrations ??= Create();
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ModelSnapshot ModelSnapshot
            => _modelSnapshot
                ??= (from t in Assembly.GetConstructibleTypes()
                     where t.IsSubclassOf(typeof(ModelSnapshot))
                         && t.GetCustomAttribute<DbContextAttribute>()?.ContextType == _contextType
                     select (ModelSnapshot)Activator.CreateInstance(t.AsType()))
                .FirstOrDefault();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Assembly Assembly { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string FindMigrationId(string nameOrId)
            => Migrations.Keys
                .Where(
                    _idGenerator.IsValidId(nameOrId)
                        // ReSharper disable once ImplicitlyCapturedClosure
                        ? (Func<string, bool>)(id => string.Equals(id, nameOrId, StringComparison.OrdinalIgnoreCase))
                        : id => string.Equals(_idGenerator.GetName(id), nameOrId, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
        {
            Check.NotNull(activeProvider, nameof(activeProvider));

            var migration = (Migration)Activator.CreateInstance(migrationClass.AsType());
            migration.ActiveProvider = activeProvider;

            return migration;
        }
    }
}
