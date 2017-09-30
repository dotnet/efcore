// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityFinderFactory : IEntityFinderFactory
    {
        private readonly IEntityFinderSource _entityFinderSource;
        private readonly IStateManager _stateManager;
        private readonly IDbSetSource _setSource;
        private readonly IDbSetCache _setCache;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityFinderFactory(
            [NotNull] IEntityFinderSource entityFinderSource,
            [NotNull] IStateManager stateManager,
            [NotNull] IDbSetSource setSource,
            [NotNull] IDbSetCache setCache)
        {
            _entityFinderSource = entityFinderSource;
            _stateManager = stateManager;
            _setSource = setSource;
            _setCache = setCache;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEntityFinder Create(IEntityType type)
            => _entityFinderSource.Create(_stateManager, _setSource, _setCache, type);
    }
}
