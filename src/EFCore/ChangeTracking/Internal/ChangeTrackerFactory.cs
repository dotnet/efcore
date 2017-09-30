// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ChangeTrackerFactory : IChangeTrackerFactory
    {
        private readonly DbContext _context;
        private readonly IStateManager _stateManager;
        private readonly IChangeDetector _changeDetector;
        private readonly IModel _model;
        private readonly IEntityEntryGraphIterator _graphIterator;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ChangeTrackerFactory(
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IStateManager stateManager,
            [NotNull] IChangeDetector changeDetector,
            [NotNull] IModel model,
            [NotNull] IEntityEntryGraphIterator graphIterator)
        {
            _context = currentContext.Context;
            _stateManager = stateManager;
            _changeDetector = changeDetector;
            _model = model;
            _graphIterator = graphIterator;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ChangeTracker Create()
            => new ChangeTracker(_context, _stateManager, _changeDetector, _model, _graphIterator);
    }
}
