// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Sqlite.Query;

namespace Microsoft.Data.Entity.Sqlite.Update
{
    public class SqliteCommandBatchPreparer : CommandBatchPreparer, ISqliteCommandBatchPreparer
    {
        public SqliteCommandBatchPreparer(
            [NotNull] ISqliteModificationCommandBatchFactory modificationCommandBatchFactory,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IComparer<ModificationCommand> modificationCommandComparer,
            [NotNull] ISqliteValueBufferFactoryFactory valueBufferFactoryFactory)
            : base(
                  modificationCommandBatchFactory,
                  parameterNameGeneratorFactory,
                  modificationCommandComparer,
                  valueBufferFactoryFactory)
        {
        }

        public override IRelationalEntityTypeExtensions GetEntityTypeExtensions(IEntityType entityType) =>
            entityType.Relational();
        public override IRelationalPropertyExtensions GetPropertyExtensions(IProperty property) => property.Relational();
    }
}