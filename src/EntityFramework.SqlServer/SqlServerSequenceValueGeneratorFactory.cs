// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerSequenceValueGeneratorFactory : IValueGeneratorFactory
    {
        private readonly SqlStatementExecutor _executor;

        public SqlServerSequenceValueGeneratorFactory([NotNull] SqlStatementExecutor executor)
        {
            Check.NotNull(executor, "executor");

            _executor = executor;
        }

        public virtual int GetBlockSize([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return property.FindAnnotationInHierarchy(
                Entity.Metadata.SqlServerMetadataExtensions.Annotations.SequenceBlockSize,
                Entity.Metadata.SqlServerMetadataExtensions.DefaultSequenceBlockSize);
        }

        public virtual string GetSequenceName([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return property.FindAnnotationInHierarchy(
                Entity.Metadata.SqlServerMetadataExtensions.Annotations.SequenceName,
                Entity.Metadata.SqlServerMetadataExtensions.GetDefaultSequenceName(property));
        }

        public virtual IValueGenerator Create(IProperty property)
        {
            Check.NotNull(property, "property");

            return new SqlServerSequenceValueGenerator(_executor, GetSequenceName(property), GetBlockSize(property));
        }

        public virtual int GetPoolSize(IProperty property)
        {
            Check.NotNull(property, "property");

            // TODO: Allow configuration without creation of derived factory type
            // Issue #778
            return 5;
        }

        public virtual string GetCacheKey(IProperty property)
        {
            Check.NotNull(property, "property");

            return GetSequenceName(property);
        }
    }
}
