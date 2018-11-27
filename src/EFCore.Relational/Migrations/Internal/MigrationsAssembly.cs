// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MigrationsAssembly : IMigrationsAssembly
    {
        private readonly IMigrationsIdGenerator _idGenerator;
        private readonly LazyRef<IReadOnlyDictionary<string, TypeInfo>> _migrations;
        private readonly LazyRef<ModelSnapshot> _modelSnapshot;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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

            var contextType = currentContext.Context.GetType();

            var assemblyName = RelationalOptionsExtension.Extract(options)?.MigrationsAssembly;
            Assembly = assemblyName == null
                ? contextType.GetTypeInfo().Assembly
                : Assembly.Load(new AssemblyName(assemblyName));

            _idGenerator = idGenerator;
            _migrations = new LazyRef<IReadOnlyDictionary<string, TypeInfo>>(
                () =>
                {
                    var result = new SortedList<string, TypeInfo>();
                    var items =
                        from t in Assembly.GetConstructibleTypes()
                        where t.IsSubclassOf(typeof(Migration))
                              && t.GetCustomAttribute<DbContextAttribute>()?.ContextType == contextType
                        let id = t.GetCustomAttribute<MigrationAttribute>()?.Id
                        orderby id
                        select (id, t);
                    foreach (var (id, t) in items)
                    {
                        if (id == null)
                        {
                            logger.MigrationAttributeMissingWarning(t);

                            continue;
                        }

                        result.Add(id, t);
                    }

                    return result;
                });
            _modelSnapshot = new LazyRef<ModelSnapshot>(
                () => (
                        from t in Assembly.GetConstructibleTypes()
                        where t.IsSubclassOf(typeof(ModelSnapshot))
                              && t.GetCustomAttribute<DbContextAttribute>()?.ContextType == contextType
                        select (ModelSnapshot)Activator.CreateInstance(t.AsType()))
                    .FirstOrDefault());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyDictionary<string, TypeInfo> Migrations => _migrations.Value;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ModelSnapshot ModelSnapshot => _modelSnapshot.Value;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Assembly Assembly { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
