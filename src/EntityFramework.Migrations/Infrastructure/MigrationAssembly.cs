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

        public virtual string Namespace
        {
            get { return ContextConfiguration.GetMigrationNamespace(); }
        }

        public virtual IReadOnlyList<IMigrationMetadata> Migrations
        {
            get { return _migrations ?? (_migrations = LoadMigrations()); }
        }

        public virtual IModel Model
        {
            get { return _model ?? (_model = LoadModel()); }
        }

        protected virtual IReadOnlyList<IMigrationMetadata> LoadMigrations()
        {
            return Assembly.GetAccessibleTypes()
                .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(Migration))
                            && t.GetPublicConstructor() != null
                            && !t.GetTypeInfo().IsAbstract
                            && !t.GetTypeInfo().IsGenericType
                            && t.Namespace == Namespace)
                .Select(t => (IMigrationMetadata)Activator.CreateInstance(t))
                .OrderBy(m => m, MigrationMetadataComparer.Instance)
                .ToArray();
        }

        protected virtual IModel LoadModel()
        {
            var modelSnapshotType = Assembly.GetAccessibleTypes().SingleOrDefault(
                t => t.GetTypeInfo().IsSubclassOf(typeof(ModelSnapshot))
                     && t.GetPublicConstructor() != null
                     && !t.GetTypeInfo().IsAbstract
                     && !t.GetTypeInfo().IsGenericType
                     && t.Namespace == Namespace);

            return modelSnapshotType != null
                ? ((ModelSnapshot)Activator.CreateInstance(modelSnapshotType)).Model
                : null;
        }
    }
}
