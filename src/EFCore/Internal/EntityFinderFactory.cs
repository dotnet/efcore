// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class EntityFinderFactory : IEntityFinderFactory
    {
        private readonly IEntityFinderSource _entityFinderSource;
        private readonly IStateManager _stateManager;
        private readonly IDbSetSource _setSource;
        private readonly IDbSetCache _setCache;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEntityFinder Create(IEntityType type)
            => _entityFinderSource.Create(_stateManager, _setSource, _setCache, type);
    }
}
