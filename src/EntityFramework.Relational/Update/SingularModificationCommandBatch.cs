// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class SingularModificationCommandBatch : ReaderModificationCommandBatch
    {
        protected override bool CanAddCommand(ModificationCommand modificationCommand, string newSql)
        {
            return !ModificationCommands.Any();
        }
    }
}
