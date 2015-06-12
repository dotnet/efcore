// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class SingularModificationCommandBatch : ReaderModificationCommandBatch
    {
        public SingularModificationCommandBatch(
            [NotNull] ISqlGenerator sqlGenerator)
            : base(sqlGenerator)
        {
        }

        protected override bool CanAddCommand(ModificationCommand modificationCommand)
            => ModificationCommands.Count == 0;

        protected override bool IsCommandTextValid()
            => true;
    }
}
