// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Update.Internal
{
    public class SqliteModificationCommandBatchFactory : ModificationCommandBatchFactory
    {
        public SqliteModificationCommandBatchFactory(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] IUpdateSqlGenerator sqlGenerator)
            : base(commandBuilderFactory, sqlGenerator)
        {
        }

        public override ModificationCommandBatch Create(
            IDbContextOptions options,
            IRelationalAnnotationProvider annotationProvider)
            => new SingularModificationCommandBatch(CommandBuilderFactory, UpdateSqlGenerator);
    }
}
