// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    public class MigrationAssembly : IMigrationAssembly
    {
        private readonly LazyRef<IReadOnlyList<Migration>> _migrations;
        private readonly LazyRef<ModelSnapshot> _modelSnapshot;

        public MigrationAssembly([NotNull] DbContext context, [NotNull] IDbContextOptions options)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(options, nameof(options));

            var contextType = context.GetType();

            var assemblyName = RelationalOptionsExtension.Extract(options)?.MigrationsAssembly;
            var assembly = assemblyName == null
                ? contextType.GetTypeInfo().Assembly
                : Assembly.Load(new AssemblyName(assemblyName));

            _migrations = new LazyRef<IReadOnlyList<Migration>>(
                () => GetMigrationTypes(assembly)
                    .Where(t => TryGetContextType(t) == contextType)
                    .Select(t => (Migration)Activator.CreateInstance(t.AsType()))
                    .OrderBy(m => m.Id)
                    .ToList());
            _modelSnapshot = new LazyRef<ModelSnapshot>(
                () => (
                    from t in GetTypes(assembly)
                    where t.IsSubclassOf(typeof(ModelSnapshot))
                          && TryGetContextType(t) == contextType
                    select (ModelSnapshot)Activator.CreateInstance(t.AsType()))
                    .FirstOrDefault());
        }

        public virtual IReadOnlyList<Migration> Migrations => _migrations.Value;
        public virtual ModelSnapshot ModelSnapshot => _modelSnapshot.Value;

        public static IEnumerable<TypeInfo> GetMigrationTypes([NotNull] Assembly assembly) =>
            GetTypes(assembly).Where(ti => ti.IsSubclassOf(typeof(Migration)));

        public static Type TryGetContextType([NotNull] TypeInfo migrationType) =>
            migrationType.GetCustomAttribute<ContextTypeAttribute>()?.ContextType;

        protected static IEnumerable<TypeInfo> GetTypes([NotNull] Assembly assembly) =>
            assembly.DefinedTypes.Where(
                ti => ti.DeclaredConstructors.Any(c => c.IsPublic && c.GetParameters().Length == 0)
                      && !ti.IsAbstract
                      && !ti.IsGenericType);
    }
}
