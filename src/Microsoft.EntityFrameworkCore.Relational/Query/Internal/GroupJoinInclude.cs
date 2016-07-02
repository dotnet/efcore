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
    public class GroupJoinInclude : IDisposable
    {
        private readonly IncludeSpecification _includeSpecification;
        private readonly IReadOnlyDictionary<IncludeSpecification, Func<QueryContext, IRelatedEntitiesLoader>> _relatedEntitiesLoaderFactories;
        private readonly bool _querySourceRequiresTracking;

        private RelationalQueryContext _queryContext;
        private IReadOnlyDictionary<IncludeSpecification, IRelatedEntitiesLoader> _relatedEntitiesLoaders;
        private GroupJoinInclude _previous;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public GroupJoinInclude(
            [NotNull] IncludeSpecification includeSpecification,
            [NotNull] IReadOnlyDictionary<IncludeSpecification, Func<QueryContext, IRelatedEntitiesLoader>> relatedEntitiesLoaderFactories,
            bool querySourceRequiresTracking)
        {
            _includeSpecification = includeSpecification;
            _relatedEntitiesLoaderFactories = relatedEntitiesLoaderFactories;
            _querySourceRequiresTracking = querySourceRequiresTracking;
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
        public virtual void Initialize([NotNull] RelationalQueryContext queryContext)
        {
            _queryContext = queryContext;
            _queryContext.BeginIncludeScope();

            _relatedEntitiesLoaders
                = _relatedEntitiesLoaderFactories.ToDictionary(l => l.Key, l => l.Value(queryContext));

            _previous?.Initialize(queryContext);
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

                foreach (var relatedEntitiesLoader in _relatedEntitiesLoaders)
                {
                    relatedEntitiesLoader.Value.Dispose();
                }

                _queryContext.EndIncludeScope();
            }
        }
    }
}
