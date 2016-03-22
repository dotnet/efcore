// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class ChangeTrackerFactory : IChangeTrackerFactory
    {
        private readonly IStateManager _stateManager;
        private readonly IChangeDetector _changeDetector;
        private readonly IEntityEntryGraphIterator _graphIterator;
        private readonly DbContext _context;

        public ChangeTrackerFactory(
            [NotNull] IStateManager stateManager,
            [NotNull] IChangeDetector changeDetector,
            [NotNull] IEntityEntryGraphIterator graphIterator,
            [NotNull] ICurrentDbContext currentContext)
        {
            _stateManager = stateManager;
            _changeDetector = changeDetector;
            _graphIterator = graphIterator;
            _context = currentContext.Context;
        }

        public virtual ChangeTracker Create()
            => new ChangeTracker(_stateManager, _changeDetector, _graphIterator, _context);
    }
}
