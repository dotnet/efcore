// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ChangeTrackerFactory : IChangeTrackerFactory
    {
        private readonly IStateManager _stateManager;
        private readonly IChangeDetector _changeDetector;
        private readonly IEntityEntryGraphIterator _graphIterator;
        private readonly DbContext _context;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ChangeTracker Create()
            => new ChangeTracker(_stateManager, _changeDetector, _graphIterator, _context);
    }
}
