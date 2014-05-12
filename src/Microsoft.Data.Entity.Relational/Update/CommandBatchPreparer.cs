// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class CommandBatchPreparer
    {
        private readonly ParameterNameGeneratorFactory _parameterNameGeneratorFactory;

        public CommandBatchPreparer([NotNull] ParameterNameGeneratorFactory parameterNameGeneratorFactory)
        {
            Check.NotNull(parameterNameGeneratorFactory, "parameterNameGeneratorFactory");

            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
        }

        public virtual IEnumerable<ModificationCommandBatch> BatchCommands([NotNull] IReadOnlyList<StateEntry> stateEntries)
        {
            Check.NotNull(stateEntries, "stateEntries");

            var parameterNameGenerator = _parameterNameGeneratorFactory.Create();

            // TODO: Use topological sort for ordering
            // TODO: Handle multiple state entries that update the same row
            // TODO: Note that the code below appears to do batching, but it doesn't really do it because
            // it always creates a new batch for each insert, update, or delete operation.
            return stateEntries.Select(e => new ModificationCommandBatch(
                new[] { new ModificationCommand(e.EntityType.StorageName, parameterNameGenerator).AddStateEntry(e) }));
        }
    }
}
