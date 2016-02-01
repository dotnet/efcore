// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class InMemoryQueryContextFactory : QueryContextFactory
    {
        private readonly IInMemoryDatabase _database;

        public InMemoryQueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IConcurrencyDetector concurrencyDetector,
            [NotNull] IInMemoryDatabase database,
            [NotNull] IChangeDetector changeDetector)
            : base(stateManager, concurrencyDetector, changeDetector)
        {
            Check.NotNull(database, nameof(database));

            _database = database;
        }

        public override QueryContext Create()
            => new InMemoryQueryContext(CreateQueryBuffer, _database.Store, StateManager, ConcurrencyDetector);
    }
}
