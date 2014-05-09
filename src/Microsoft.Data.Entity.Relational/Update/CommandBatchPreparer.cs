// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class CommandBatchPreparer
    {
        public virtual IEnumerable<ModificationCommandBatch> BatchCommands(
            [NotNull] IEnumerable<StateEntry> stateEntries, [NotNull] DatabaseModel database)
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
