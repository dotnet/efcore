// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Update
{
    public abstract class ModificationCommandBatchFactory : IModificationCommandBatchFactory
    {
        protected ModificationCommandBatchFactory(
            [NotNull] IUpdateSqlGenerator sqlGenerator)
        {
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            UpdateSqlGenerator = sqlGenerator;
        }

        protected IUpdateSqlGenerator UpdateSqlGenerator { get; }

        public abstract ModificationCommandBatch Create(
            IDbContextOptions options,
            IRelationalMetadataExtensionProvider metadataExtensionProvider);

        public virtual bool AddCommand(
            ModificationCommandBatch modificationCommandBatch,
            ModificationCommand modificationCommand)
            => Check.NotNull(modificationCommandBatch, nameof(modificationCommandBatch))
                .AddCommand(Check.NotNull(modificationCommand, nameof(modificationCommand)));
    }
}
