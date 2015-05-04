// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Query;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Update
{
    public class SqlServerCommandBatchPreparer : CommandBatchPreparer
    {
        public SqlServerCommandBatchPreparer(
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

        public override IRelationalPropertyExtensions GetPropertyExtensions(IProperty property) 
            => Check.NotNull(property, nameof(property)).SqlServer();

        public override IRelationalEntityTypeExtensions GetEntityTypeExtensions(IEntityType entityType) 
            => Check.NotNull(entityType, nameof(entityType)).SqlServer();
    }
}
