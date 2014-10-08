// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Utilities;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public class MigrationAssembly
    {
        private readonly DbContextConfiguration _contextConfiguration;

        private IReadOnlyList<IMigrationMetadata> _migrations;
        private IModel _model;

        public MigrationAssembly([NotNull] DbContextConfiguration contextConfiguration)
        {
            Check.NotNull(contextConfiguration, "contextConfiguration");

            _contextConfiguration = contextConfiguration;
        }

        protected virtual DbContextConfiguration ContextConfiguration
        {
            get { return _contextConfiguration; }
        }

        public virtual Assembly Assembly
        {
            get { return ContextConfiguration.GetMigrationAssembly(); }
        }

        public virtual IReadOnlyList<IMigrationMetadata> Migrations
        {
            get { return _migrations ?? (_migrations = LoadMigrations()); }
        }

        public virtual IModel Model
        {
            get { return _model ?? (_model = LoadModel()); }
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

        public static IEnumerable<IMigrationMetadata> LoadMigartions(
            [NotNull] IEnumerable<Type> migrationTypes,
            [CanBeNull] Type contextType)
        {
            Check.NotNull(migrationTypes, "migrationTypes");

            return migrationTypes
                .Where(t => TryGetContextType(t) == contextType)
                .Select(t => (IMigrationMetadata)Activator.CreateInstance(t))
                .OrderBy(m => m.MigrationId);
        }

        protected virtual IReadOnlyList<IMigrationMetadata> LoadMigrations()
        {
            var contextType = ContextConfiguration.Context.GetType();
            return LoadMigartions(GetMigrationTypes(Assembly), contextType)
                .ToArray();
        }

        protected virtual IModel LoadModel()
        {
            var contextType = ContextConfiguration.Context.GetType();
            var modelSnapshotType = Assembly.GetAccessibleTypes().SingleOrDefault(
                t => t.GetTypeInfo().IsSubclassOf(typeof(ModelSnapshot))
                     && t.GetPublicConstructor() != null
                     && !t.GetTypeInfo().IsAbstract
                     && !t.GetTypeInfo().IsGenericType
                     && TryGetContextType(t) == contextType);

            return modelSnapshotType != null
                ? ((ModelSnapshot)Activator.CreateInstance(modelSnapshotType)).Model
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
