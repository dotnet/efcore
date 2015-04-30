// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public abstract class ModificationCommandBatchFactory : IModificationCommandBatchFactory
    {
        protected ModificationCommandBatchFactory(
            [NotNull] ISqlGenerator sqlGenerator)
        {
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            SqlGenerator = sqlGenerator;
        }

        protected ISqlGenerator SqlGenerator { get; }

        public abstract ModificationCommandBatch Create(IDbContextOptions options);

        public virtual bool AddCommand(
            ModificationCommandBatch modificationCommandBatch,
            ModificationCommand modificationCommand)
            => Check.NotNull(modificationCommandBatch, nameof(modificationCommandBatch))
                .AddCommand(Check.NotNull(modificationCommand, nameof(modificationCommand)));
    }
}
