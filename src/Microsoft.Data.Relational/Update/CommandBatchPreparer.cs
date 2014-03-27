// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Relational.Update
{
    internal class CommandBatchPreparer
    {
        public IEnumerable<ModificationCommandBatch> BatchCommands([NotNull] IEnumerable<StateEntry> stateEntries, [NotNull] Database database)
        {
            return
                stateEntries.Select(
                    e => new ModificationCommandBatch(new[]
                        {
                            new ModificationCommand(e, database.GetTable(e.EntityType.StorageName))
                        }));
        }
    }
}
