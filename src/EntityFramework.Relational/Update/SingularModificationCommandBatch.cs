// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Update
{
    public abstract class SingularModificationCommandBatch : ReaderModificationCommandBatch
    {
        protected SingularModificationCommandBatch(
            [NotNull] ISqlGenerator sqlGenerator)
            : base(sqlGenerator)
        {
        }

        protected override bool CanAddCommand(ModificationCommand modificationCommand)
        {
            return ModificationCommands.Count == 0;
        }

        protected override bool IsCommandTextValid()
        {
            return true;
        }
    }
}
