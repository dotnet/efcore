// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Relational.Model;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Update
{
    public class CommandBatchPreparer
    {
        public virtual IEnumerable<ModificationCommandBatch> BatchCommands(
            [NotNull] IEnumerable<StateEntry> stateEntries, [NotNull] Database database)
        {
            Check.NotNull(stateEntries, "database");
            Check.NotNull(database, "stateEntries");

            return
                stateEntries.Select(
                    e => new ModificationCommandBatch(new[]
                        {
                            new ModificationCommand(e, database.GetTable(e.EntityType.StorageName))
                        }));
        }
    }
}
