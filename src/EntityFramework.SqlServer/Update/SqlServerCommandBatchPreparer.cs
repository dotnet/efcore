// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Update
{
    public class SqlServerCommandBatchPreparer : CommandBatchPreparer
    {
        public SqlServerCommandBatchPreparer(
            [NotNull] SqlServerModificationCommandBatchFactory modificationCommandBatchFactory,
            [NotNull] ParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] GraphFactory graphFactory,
            [NotNull] ModificationCommandComparer modificationCommandComparer)
            : base(modificationCommandBatchFactory, parameterNameGeneratorFactory, graphFactory, modificationCommandComparer)
        {
        }

        public override IRelationalPropertyExtensions GetPropertyExtensions(IProperty property)
        {
            Check.NotNull(property, "property");

            return property.SqlServer();
        }

        public override IRelationalEntityTypeExtensions GetEntityTypeExtensions(IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return entityType.SqlServer();
        }
    }
}
