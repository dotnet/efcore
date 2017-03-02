// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Update
{
    public class SingularModificationCommandBatch : AffectedCountModificationCommandBatch
    {
        public SingularModificationCommandBatch(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
            : base(
                commandBuilderFactory,
                sqlGenerationHelper,
                updateSqlGenerator,
                valueBufferFactoryFactory)
        {
        }

        protected override bool CanAddCommand(ModificationCommand modificationCommand)
            => ModificationCommands.Count == 0;

        protected override bool IsCommandTextValid()
            => true;
    }
}
