// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class CompiledQueryCacheKeyGenerator : ICompiledQueryCacheKeyGenerator
    {
        private readonly IModel _model;
        private readonly DbContext _context;

        public CompiledQueryCacheKeyGenerator([NotNull] IModel model, [NotNull] ICurrentDbContext currentContext)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(currentContext, nameof(currentContext));

            _model = model;
            _context = currentContext.Context;
        }

        public virtual object GenerateCacheKey(Expression query, bool async)
            => GenerateCacheKeyCore(query, async);

        protected CompiledQueryCacheKey GenerateCacheKeyCore([NotNull] Expression query, bool async)
            => new CompiledQueryCacheKey(
                Check.NotNull(query, nameof(query)),
                _model,
                _context.ChangeTracker.QueryTrackingBehavior,
                async);

        protected struct CompiledQueryCacheKey
        {
            private static readonly ExpressionEqualityComparer _expressionEqualityComparer
                = new ExpressionEqualityComparer();

            private readonly Expression _query;
            private readonly IModel _model;
            private readonly QueryTrackingBehavior _queryTrackingBehavior;
            private readonly bool _async;

            public CompiledQueryCacheKey(
                [NotNull] Expression query,
                [NotNull] IModel model,
                QueryTrackingBehavior queryTrackingBehavior,
                bool async)
            {
                _query = query;
                _model = model;
                _queryTrackingBehavior = queryTrackingBehavior;
                _async = async;
            }

            public override bool Equals(object obj)
            {
                var other = (CompiledQueryCacheKey)obj;

                return ReferenceEquals(_model, other._model)
                       && _queryTrackingBehavior == other._queryTrackingBehavior
                       && _async == other._async
                       && _expressionEqualityComparer.Equals(_query, other._query);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = _expressionEqualityComparer.GetHashCode(_query);
                    hashCode = (hashCode * 397) ^ _model.GetHashCode();
                    hashCode = (hashCode * 397) ^ (int)_queryTrackingBehavior;
                    hashCode = (hashCode * 397) ^ _async.GetHashCode();
                    return hashCode;
                }
            }
        }
    }
}
