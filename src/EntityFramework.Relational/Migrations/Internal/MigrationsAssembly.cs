// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Internal
{
    public class MigrationsAssembly : IMigrationsAssembly
    {
        private readonly IMigrationsIdGenerator _idGenerator;
        private readonly LazyRef<IReadOnlyDictionary<string, TypeInfo>> _migrations;
        private readonly LazyRef<ModelSnapshot> _modelSnapshot;

        public MigrationsAssembly(
            [NotNull] DbContext context,
            [NotNull] IDbContextOptions options,
            [NotNull] IMigrationsIdGenerator idGenerator)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(options, nameof(options));
            Check.NotNull(idGenerator, nameof(idGenerator));

            var contextType = context.GetType();

            var assemblyName = RelationalOptionsExtension.Extract(options)?.MigrationsAssembly;
            var assembly = assemblyName == null
                ? contextType.GetTypeInfo().Assembly
                : Assembly.Load(new AssemblyName(assemblyName));

            _idGenerator = idGenerator;
            _migrations = new LazyRef<IReadOnlyDictionary<string, TypeInfo>>(
                () => (
                        from t in assembly.GetConstructibleTypes()
                        where t.IsSubclassOf(typeof(Migration))
                            && t.GetCustomAttribute<DbContextAttribute>()?.ContextType == contextType
                        let id = t.GetCustomAttribute<MigrationAttribute>()?.Id
                        orderby id
                        select new { Key = id, Element = t })
                    .ToDictionary(i => i.Key, i => i.Element));
            _modelSnapshot = new LazyRef<ModelSnapshot>(
                () => (
                        from t in assembly.GetConstructibleTypes()
                        where t.IsSubclassOf(typeof(ModelSnapshot))
                              && t.GetCustomAttribute<DbContextAttribute>()?.ContextType == contextType
                        select (ModelSnapshot)Activator.CreateInstance(t.AsType()))
                    .FirstOrDefault());
        }

        public virtual IReadOnlyDictionary<string, TypeInfo> Migrations => _migrations.Value;
        public virtual ModelSnapshot ModelSnapshot => _modelSnapshot.Value;

        public virtual string FindMigrationId(string nameOrId)
            => Migrations.Keys
                .Where(
                    _idGenerator.IsValidId(nameOrId)
                        ? (Func<string, bool>)(id => string.Equals(id, nameOrId, StringComparison.OrdinalIgnoreCase))
                        : id => string.Equals(_idGenerator.GetName(id), nameOrId, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

        public virtual Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
        {
            Check.NotNull(activeProvider, nameof(activeProvider));

            var migration = (Migration)Activator.CreateInstance(migrationClass.AsType());
            migration.ActiveProvider = activeProvider;

            return migration;
        }
    }
}
