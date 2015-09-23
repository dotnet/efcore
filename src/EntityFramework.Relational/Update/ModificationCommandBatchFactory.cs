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
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] IUpdateSqlGenerator sqlGenerator)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            CommandBuilderFactory = commandBuilderFactory;
            UpdateSqlGenerator = sqlGenerator;
        }

        protected virtual IUpdateSqlGenerator UpdateSqlGenerator { get; }

        protected virtual IRelationalCommandBuilderFactory CommandBuilderFactory { get; }

        public abstract ModificationCommandBatch Create(
            IDbContextOptions options,
            IRelationalAnnotationProvider annotationProvider);

        public virtual bool AddCommand(
            ModificationCommandBatch modificationCommandBatch,
            ModificationCommand modificationCommand)
            => Check.NotNull(modificationCommandBatch, nameof(modificationCommandBatch))
                .AddCommand(Check.NotNull(modificationCommand, nameof(modificationCommand)));
    }
}
