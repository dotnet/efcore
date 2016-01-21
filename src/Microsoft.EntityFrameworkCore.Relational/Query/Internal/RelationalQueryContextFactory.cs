// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class RelationalQueryContextFactory : QueryContextFactory
    {
        private readonly IRelationalConnection _connection;

        public RelationalQueryContextFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IConcurrencyDetector concurrencyDetector,
            [NotNull] IRelationalConnection connection,
            [NotNull] IChangeDetector changeDetector)
            : base(stateManager, concurrencyDetector, changeDetector)
        {
            _connection = connection;
        }

        public override QueryContext Create()
            => new RelationalQueryContext(CreateQueryBuffer, _connection, StateManager, ConcurrencyDetector);
    }
}
