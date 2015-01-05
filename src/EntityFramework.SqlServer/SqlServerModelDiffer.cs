// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerModelDiffer : ModelDiffer
    {
        public SqlServerModelDiffer(
            [NotNull] SqlServerMetadataExtensionProvider extensionProvider,
            [NotNull] SqlServerTypeMapper typeMapper,
            [NotNull] SqlServerMigrationOperationFactory operationFactory,
            [NotNull] SqlServerMigrationOperationProcessor operationProcessor)
            : base(
                extensionProvider,
                typeMapper,
                operationFactory,
                operationProcessor)
        {
        }

        public virtual new SqlServerMetadataExtensionProvider ExtensionProvider
        {
            get { return (SqlServerMetadataExtensionProvider)base.ExtensionProvider; }
        }

        public virtual new SqlServerTypeMapper TypeMapper
        {
            get { return (SqlServerTypeMapper)base.TypeMapper; }
        }

        public virtual new SqlServerMigrationOperationFactory OperationFactory
        {
            get { return (SqlServerMigrationOperationFactory)base.OperationFactory; }
        }

        public virtual new SqlServerMigrationOperationProcessor OperationProcessor
        {
            get { return (SqlServerMigrationOperationProcessor)base.OperationProcessor; }
        }

        protected override ISequence TryGetSequence(IProperty property)
        {
            return property.SqlServer().TryGetSequence();
        }

        protected override IReadOnlyList<ISequence> GetSequences(IModel model)
        {
            Check.NotNull(model, "model");

            return
                model.EntityTypes
                    .SelectMany(t => t.Properties)
                    .Select(TryGetSequence)
                    .Where(s => s != null)
                    .Distinct((x, y) => MatchSequenceNames(x, y) && MatchSequenceSchemas(x, y))
                    .ToList();
        }

        protected override bool EquivalentPrimaryKeys(IKey source, IKey target, IDictionary<IProperty, IProperty> columnMap)
        {
            return 
                base.EquivalentPrimaryKeys(source, target, columnMap)
                && ExtensionProvider.Extensions(source).IsClustered == ExtensionProvider.Extensions(target).IsClustered;
        }

        protected override bool EquivalentIndexes(IIndex source, IIndex target, IDictionary<IProperty, IProperty> columnMap)
        {
            return
                base.EquivalentIndexes(source, target, columnMap)
                && ExtensionProvider.Extensions(source).IsClustered == ExtensionProvider.Extensions(target).IsClustered;
        }
    }
}
