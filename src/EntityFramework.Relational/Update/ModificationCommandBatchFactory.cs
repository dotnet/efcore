// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Update
{
    public abstract class ModificationCommandBatchFactory : IModificationCommandBatchFactory
    {
        protected ModificationCommandBatchFactory(
            [NotNull] IUpdateSqlGenerator sqlGenerator,
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory)
        {
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));

            UpdateSqlGenerator = sqlGenerator;
            RelationalCommandBuilderFactory = commandBuilderFactory;
        }

        protected virtual IUpdateSqlGenerator UpdateSqlGenerator { get; }

        protected virtual IRelationalCommandBuilderFactory RelationalCommandBuilderFactory { get; }

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
