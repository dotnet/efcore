// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Sqlite.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteCommandBatchPreparer : CommandBatchPreparer
    {
        public SqliteCommandBatchPreparer(
            [NotNull] SqliteModificationCommandBatchFactory modificationCommandBatchFactory,
            [NotNull] ParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] GraphFactory graphFactory,
            [NotNull] ModificationCommandComparer modificationCommandComparer)
            : base(modificationCommandBatchFactory, parameterNameGeneratorFactory, graphFactory, modificationCommandComparer)
        {
        }

        public override IRelationalPropertyExtensions GetPropertyExtensions(IProperty property)
        {
            Check.NotNull(property, "property");

            // TODO: Use SQLite-specific extensions
            // Issue #875
            return property.Relational();
        }

        public override IRelationalEntityTypeExtensions GetEntityTypeExtensions(IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            // TODO: Use SQLite-specific extensions
            // Issue #875
            return entityType.Relational();
        }
    }
}
