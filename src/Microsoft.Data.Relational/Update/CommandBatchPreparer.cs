// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;

namespace Microsoft.Data.Relational.Update
{
    internal class CommandBatchPreparer
    {
        public IEnumerable<ModificationCommandBatch> BatchCommands(IEnumerable<StateEntry> stateEntries)
        {
            return
                stateEntries.Select(
                    e => new ModificationCommandBatch(new[] { new ModificationCommand(e) }));
        }
    }
}
