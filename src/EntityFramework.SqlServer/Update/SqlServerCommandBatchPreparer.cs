// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Query;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Update
{
    public class SqlServerCommandBatchPreparer : CommandBatchPreparer, ISqlServerCommandBatchPreparer
    {
        public SqlServerCommandBatchPreparer(
            [NotNull] ISqlServerModificationCommandBatchFactory modificationCommandBatchFactory,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IComparer<ModificationCommand> modificationCommandComparer,
            [NotNull] IBoxedValueReaderSource boxedValueReaderSource,
            [NotNull] ISqlServerValueReaderFactoryFactory valueReaderFactoryFactory)
            : base(
                  modificationCommandBatchFactory, 
                  parameterNameGeneratorFactory, 
                  modificationCommandComparer, 
                  boxedValueReaderSource,
                  valueReaderFactoryFactory)
        {
        }

        public override IRelationalPropertyExtensions GetPropertyExtensions(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.SqlServer();
        }

        public override IRelationalEntityTypeExtensions GetEntityTypeExtensions(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.SqlServer();
        }
    }
}
