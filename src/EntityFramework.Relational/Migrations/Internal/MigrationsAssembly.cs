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
        private readonly LazyRef<IReadOnlyList<Migration>> _migrations;
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
            _migrations = new LazyRef<IReadOnlyList<Migration>>(
                () => assembly.GetConstructibleTypes()
                    .Where(t => typeof(Migration).IsAssignableFrom(t.AsType())
                        && t.GetCustomAttribute<DbContextAttribute>()?.ContextType == contextType)
                    .Select(t => (Migration)Activator.CreateInstance(t.AsType()))
                    .OrderBy(m => m.Id)
                    .ToList());
            _modelSnapshot = new LazyRef<ModelSnapshot>(
                () => (
                        from t in assembly.GetConstructibleTypes()
                        where t.IsSubclassOf(typeof(ModelSnapshot))
                              && t.GetCustomAttribute<DbContextAttribute>()?.ContextType == contextType
                        select (ModelSnapshot)Activator.CreateInstance(t.AsType()))
                    .FirstOrDefault());
        }

        public virtual IReadOnlyList<Migration> Migrations => _migrations.Value;
        public virtual ModelSnapshot ModelSnapshot => _modelSnapshot.Value;

        public virtual Migration FindMigration(string nameOrId)
        {
            Check.NotEmpty(nameOrId, nameof(nameOrId));

            var candidates = _idGenerator.IsValidId(nameOrId)
                ? Migrations.Where(m => m.Id == nameOrId)
                    .Concat(Migrations.Where(m => string.Equals(m.Id, nameOrId, StringComparison.OrdinalIgnoreCase)))
                : Migrations.Where(m => _idGenerator.GetName(m.Id) == nameOrId)
                    .Concat(
                        Migrations.Where(
                            m => string.Equals(_idGenerator.GetName(m.Id), nameOrId, StringComparison.OrdinalIgnoreCase)));

            return candidates.FirstOrDefault();
        }
    }
}
