// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class GroupJoinInclude : GroupJoinIncludeBase
    {
        private readonly IReadOnlyList<Func<QueryContext, KeyValuePair<IncludeSpecification, IRelatedEntitiesLoader>>> _relatedEntitiesLoaderFactories;

        private GroupJoinInclude _previous;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public GroupJoinInclude(
            [NotNull] IncludeSpecification includeSpecification,
            [NotNull] IReadOnlyList<Func<QueryContext, KeyValuePair<IncludeSpecification, IRelatedEntitiesLoader>>> relatedEntitiesLoaderFactories,
            bool querySourceRequiresTracking)
            : base(includeSpecification, querySourceRequiresTracking)
        {
            _relatedEntitiesLoaderFactories = relatedEntitiesLoaderFactories;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual GroupJoinInclude WithEntityAccessor([NotNull] Delegate entityAccessor)
        {
            EntityAccessor = entityAccessor;

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetPrevious([NotNull] GroupJoinInclude previous)
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
        public virtual GroupJoinIncludeContext CreateIncludeContext([NotNull] RelationalQueryContext queryContext)
        {
            var groupJoinIncludeContext
                = new GroupJoinIncludeContext(
                    IncludeSpecification,
                    QuerySourceRequiresTracking,
                    queryContext,
                    _relatedEntitiesLoaderFactories);

            if (_previous != null)
            {
                groupJoinIncludeContext.SetPrevious(_previous.CreateIncludeContext(queryContext));
            }

            return groupJoinIncludeContext;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class GroupJoinIncludeContext : IDisposable
        {
            private readonly IncludeSpecification _includeSpecification;
            private readonly bool _querySourceRequiresTracking;
            private readonly RelationalQueryContext _queryContext;
            private readonly IReadOnlyDictionary<IncludeSpecification, IRelatedEntitiesLoader> _relatedEntitiesLoaders;

            private GroupJoinIncludeContext _previous;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public GroupJoinIncludeContext(
                [NotNull] IncludeSpecification includeSpecification,
                bool querySourceRequiresTracking,
                [NotNull] RelationalQueryContext queryContext,
                [NotNull] IReadOnlyList<Func<QueryContext, KeyValuePair<IncludeSpecification, IRelatedEntitiesLoader>>> relatedEntitiesLoaderFactories)
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
            public virtual void SetPrevious([NotNull] GroupJoinIncludeContext previous)
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
            public virtual void Include([CanBeNull] object entity)
            {
                _previous?.Include(entity);

                _queryContext.QueryBuffer
                    .Include(
                        _queryContext,
                        entity,
                        _includeSpecification,
                        _relatedEntitiesLoaders,
                        _querySourceRequiresTracking);
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