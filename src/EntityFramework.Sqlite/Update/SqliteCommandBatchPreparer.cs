// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Update;

namespace Microsoft.Data.Entity.Sqlite.Update
{
    public class SqliteCommandBatchPreparer : CommandBatchPreparer
    {
        public SqliteCommandBatchPreparer(
            [NotNull] IModificationCommandBatchFactory modificationCommandBatchFactory,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IComparer<ModificationCommand> modificationCommandComparer,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
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
