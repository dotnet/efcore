// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class AsyncGroupJoinInclude : IDisposable
    {
        private readonly IReadOnlyList<INavigation> _navigationPath;
        private readonly IReadOnlyList<Func<QueryContext, IAsyncRelatedEntitiesLoader>> _relatedEntitiesLoaderFactories;
        private readonly bool _querySourceRequiresTracking;

        private AsyncGroupJoinInclude _previous;
        private AsyncGroupJoinIncludeContext _currentContext;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public AsyncGroupJoinInclude(
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            [NotNull] IReadOnlyList<Func<QueryContext, IAsyncRelatedEntitiesLoader>> relatedEntitiesLoaderFactories,
            bool querySourceRequiresTracking)
        {
            _navigationPath = navigationPath;
            _relatedEntitiesLoaderFactories = relatedEntitiesLoaderFactories;
            _querySourceRequiresTracking = querySourceRequiresTracking;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetPrevious([NotNull] AsyncGroupJoinInclude previous)
        {
            if (_previous != null)
            {
                _previous.SetPrevious(previous);
            }
            else
            {
                _previous = previous;
            }
        }
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Obsolete(
            "This method is obsolete and will be removed in the 1.1.0 release. Use CreateIncludeContext instead.",
             error: true)]
        public virtual void Initialize([NotNull] RelationalQueryContext queryContext)
            => CreateIncludeContext(queryContext);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual AsyncGroupJoinIncludeContext CreateIncludeContext([NotNull] RelationalQueryContext queryContext)
        {
            var asyncGroupJoinIncludeContext
                = new AsyncGroupJoinIncludeContext(
                    _navigationPath,
                    _querySourceRequiresTracking,
                    queryContext,
                    _relatedEntitiesLoaderFactories);

            if (_previous != null)
            {
                asyncGroupJoinIncludeContext.SetPrevious(_previous.CreateIncludeContext(queryContext));
            }

            return _currentContext = asyncGroupJoinIncludeContext;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Obsolete(
            "This method is obsolete and will be removed in the 1.1.0 release. Use IncludeAsync on the object returned by CreateIncludeContext instead.",
             error: true)]
        public virtual Task IncludeAsync([CanBeNull] object entity, CancellationToken cancellationToken)
            => _currentContext.IncludeAsync(entity, cancellationToken);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Dispose()
        {
            _currentContext?.Dispose();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class AsyncGroupJoinIncludeContext : IDisposable
        {
            private readonly IReadOnlyList<INavigation> _navigationPath;
            private readonly bool _querySourceRequiresTracking;
            private readonly RelationalQueryContext _queryContext;
            private readonly IAsyncRelatedEntitiesLoader[] _relatedEntitiesLoaders;

            private AsyncGroupJoinIncludeContext _previous;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public AsyncGroupJoinIncludeContext(
                [NotNull] IReadOnlyList<INavigation> navigationPath,
                bool querySourceRequiresTracking,
                [NotNull] RelationalQueryContext queryContext,
                [NotNull] IReadOnlyList<Func<QueryContext, IAsyncRelatedEntitiesLoader>> relatedEntitiesLoaderFactories)
            {
                _navigationPath = navigationPath;
                _querySourceRequiresTracking = querySourceRequiresTracking;

                _queryContext = queryContext;
                _queryContext.BeginIncludeScope();

                _relatedEntitiesLoaders
                    = relatedEntitiesLoaderFactories.Select(f => f(queryContext))
                        .ToArray();
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual void SetPrevious([NotNull] AsyncGroupJoinIncludeContext previous)
            {
                if (_previous != null)
                {
                    _previous.SetPrevious(previous);
                }
                else
                {
                    _previous = previous;
                }
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual async Task IncludeAsync([CanBeNull] object entity, CancellationToken cancellationToken)
            {
                if (_previous != null)
                {
                    await _previous.IncludeAsync(entity, cancellationToken);
                }

                await _queryContext.QueryBuffer
                    .IncludeAsync(
                        _queryContext,
                        entity,
                        _navigationPath,
                        _relatedEntitiesLoaders,
                        _querySourceRequiresTracking,
                        cancellationToken);
            }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual void Dispose()
            {
                if (_queryContext != null)
                {
                    _previous?.Dispose();

                    foreach (var relatedEntitiesLoader in _relatedEntitiesLoaders)
                    {
                        relatedEntitiesLoader.Dispose();
                    }

                    _queryContext.EndIncludeScope();
                }
            }
        }
    }
}
