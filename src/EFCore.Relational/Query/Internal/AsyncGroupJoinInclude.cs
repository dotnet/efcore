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
    public class AsyncGroupJoinInclude : GroupJoinIncludeBase
    {
        private readonly IReadOnlyList<Func<QueryContext, KeyValuePair<IncludeSpecification, IAsyncRelatedEntitiesLoader>>> _relatedEntitiesLoaderFactories;

        private AsyncGroupJoinInclude _previous;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public AsyncGroupJoinInclude(
            [NotNull] IncludeSpecification includeSpecification,
            [NotNull] IReadOnlyList<Func<QueryContext, KeyValuePair<IncludeSpecification, IAsyncRelatedEntitiesLoader>>> relatedEntitiesLoaderFactories,
            bool querySourceRequiresTracking)
            : base(includeSpecification, querySourceRequiresTracking)
        {
            _relatedEntitiesLoaderFactories = relatedEntitiesLoaderFactories;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual AsyncGroupJoinInclude WithEntityAccessor([NotNull] Delegate entityAccessor)
        {
            EntityAccessor = entityAccessor;

            return this;
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
        public virtual AsyncGroupJoinIncludeContext CreateIncludeContext([NotNull] RelationalQueryContext queryContext)
        {
            var asyncGroupJoinIncludeContext
                = new AsyncGroupJoinIncludeContext(
                    IncludeSpecification,
                    QuerySourceRequiresTracking,
                    queryContext,
                    _relatedEntitiesLoaderFactories);

            if (_previous != null)
            {
                asyncGroupJoinIncludeContext.SetPrevious(_previous.CreateIncludeContext(queryContext));
            }

            return asyncGroupJoinIncludeContext;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class AsyncGroupJoinIncludeContext : IDisposable
        {
            private readonly IncludeSpecification _includeSpecification;
            private readonly bool _querySourceRequiresTracking;
            private readonly RelationalQueryContext _queryContext;
            private readonly IReadOnlyDictionary<IncludeSpecification, IAsyncRelatedEntitiesLoader> _relatedEntitiesLoaders;

            private AsyncGroupJoinIncludeContext _previous;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public AsyncGroupJoinIncludeContext(
                [NotNull] IncludeSpecification includeSpecification,
                bool querySourceRequiresTracking,
                [NotNull] RelationalQueryContext queryContext,
                [NotNull] IReadOnlyList<Func<QueryContext, KeyValuePair<IncludeSpecification, IAsyncRelatedEntitiesLoader>>> relatedEntitiesLoaderFactories)
            {
                _includeSpecification = includeSpecification;
                _querySourceRequiresTracking = querySourceRequiresTracking;

                _queryContext = queryContext;
                _queryContext.BeginIncludeScope();

                _relatedEntitiesLoaders
                    = relatedEntitiesLoaderFactories.Select(f => f(queryContext)).ToDictionary(pair => pair.Key, pair => pair.Value);
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
                        _includeSpecification,
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

                    foreach (var relatedEntitiesLoader in _relatedEntitiesLoaders.Values)
                    {
                        relatedEntitiesLoader.Dispose();
                    }

                    _queryContext.EndIncludeScope();
                }
            }
        }
    }
}
