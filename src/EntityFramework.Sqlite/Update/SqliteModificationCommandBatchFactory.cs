// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Update;

namespace Microsoft.Data.Entity.Sqlite.Update
{
    public class SqliteModificationCommandBatchFactory : ModificationCommandBatchFactory
    {
        public SqliteModificationCommandBatchFactory([NotNull] ISqlGenerator sqlGenerator)
            : base(sqlGenerator)
        {
        }

        public override ModificationCommandBatch Create(
            IEntityOptions options,
            IRelationalMetadataExtensionProvider metadataExtensionProvider)
            => new SingularModificationCommandBatch(SqlGenerator, metadataExtensionProvider);
    }
}
