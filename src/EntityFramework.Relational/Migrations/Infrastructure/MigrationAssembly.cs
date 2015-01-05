// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Migrations.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    public class MigrationAssembly
    {
        private readonly DbContextService<DbContext> _context;
        private readonly DbContextService<IDbContextOptions> _options;

        private IReadOnlyList<Migration> _migrations;
        private ModelSnapshot _modelSnapshot;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected MigrationAssembly()
        {
        }

        public MigrationAssembly([NotNull] DbContextService<DbContext> context, [NotNull] DbContextService<IDbContextOptions> options)
        {
            Check.NotNull(context, "context");
            Check.NotNull(options, "options");

            _context = context;
            _options = options;
        }

        public virtual Assembly Assembly
        {
            get
            {
                var extension = MigrationsOptionsExtension.Extract(_options.Service);
                return extension != null && extension.MigrationAssembly != null
                    ? extension.MigrationAssembly
                    : _context.Service.GetType().GetTypeInfo().Assembly;
            }
        }

        public virtual IReadOnlyList<Migration> Migrations
        {
            get { return _migrations ?? (_migrations = LoadMigrations()); }
        }

        public virtual ModelSnapshot ModelSnapshot
        {
            get { return _modelSnapshot ?? (_modelSnapshot = LoadModelSnapshot()); }
        }

        public static IEnumerable<Type> GetMigrationTypes([NotNull] Assembly assembly)
        {
            Check.NotNull(assembly, "assembly");

            return assembly.GetAccessibleTypes()
                .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(Migration))
                            && t.GetPublicConstructor() != null
                            && !t.GetTypeInfo().IsAbstract
                            && !t.GetTypeInfo().IsGenericType);
        }

        public static IEnumerable<Migration> LoadMigrations(
            [NotNull] IEnumerable<Type> migrationTypes,
            [CanBeNull] Type contextType)
        {
            Check.NotNull(migrationTypes, "migrationTypes");

            return migrationTypes
                .Where(t => TryGetContextType(t) == contextType)
                .Select(t => (Migration)Activator.CreateInstance(t))
                .OrderBy(m => m.GetMigrationId());
        }

        protected virtual IReadOnlyList<Migration> LoadMigrations()
        {
            var contextType = _context.Service.GetType();
            return LoadMigrations(GetMigrationTypes(Assembly), contextType)
                .ToArray();
        }

        protected virtual ModelSnapshot LoadModelSnapshot()
        {
            var contextType = _context.Service.GetType();
            var modelSnapshotType = Assembly.GetAccessibleTypes().SingleOrDefault(
                t => t.GetTypeInfo().IsSubclassOf(typeof(ModelSnapshot))
                     && t.GetPublicConstructor() != null
                     && !t.GetTypeInfo().IsAbstract
                     && !t.GetTypeInfo().IsGenericType
                     && TryGetContextType(t) == contextType);

            return modelSnapshotType != null
                ? (ModelSnapshot)Activator.CreateInstance(modelSnapshotType)
                : null;
        }

        public static Type TryGetContextType([NotNull] Type type)
        {
            Check.NotNull(type, "type");

            var contextTypeAttribute = type.GetTypeInfo().GetCustomAttribute<ContextTypeAttribute>(inherit: true);

            return contextTypeAttribute != null ? contextTypeAttribute.ContextType : null;
        }
    }
}
