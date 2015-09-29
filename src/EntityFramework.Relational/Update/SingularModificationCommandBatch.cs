// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Update
{
    public class SingularModificationCommandBatch : AffectedCountModificationCommandBatch
    {
        public SingularModificationCommandBatch(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerator sqlGenerator,
            [NotNull] IUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
            : base(commandBuilderFactory, sqlGenerator, updateSqlGenerator, valueBufferFactoryFactory)
        {
        }

        protected override bool CanAddCommand(ModificationCommand modificationCommand)
            => ModificationCommands.Count == 0;

        protected override bool IsCommandTextValid()
            => true;
    }
}
