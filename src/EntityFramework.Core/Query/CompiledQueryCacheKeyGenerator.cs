// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class CompiledQueryCacheKeyGenerator : ICompiledQueryCacheKeyGenerator
    {
        private readonly IModel _model;
        private readonly DbContext _context;

        public CompiledQueryCacheKeyGenerator([NotNull] IModel model, [NotNull] DbContext context)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(context, nameof(context));

            _model = model;
            _context = context;
        }

        public virtual object GenerateCacheKey(Expression query, bool async)
            => GenerateCacheKeyCore(query, async);

        protected CompiledQueryCacheKey GenerateCacheKeyCore([NotNull] Expression query, bool async)
            => new CompiledQueryCacheKey(
                new ExpressionStringBuilder().Build(Check.NotNull(query, nameof(query))),
                _model,
                _context.ChangeTracker.QueryTrackingBehavior,
                async);

        protected struct CompiledQueryCacheKey
        {
            private readonly string _query;
            private readonly IModel _model;
            private readonly QueryTrackingBehavior _queryTrackingBehavior;
            private readonly bool _async;

            public CompiledQueryCacheKey(
                [NotNull] string query, 
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
                => !ReferenceEquals(null, obj)
                   && (obj is CompiledQueryCacheKey && Equals((CompiledQueryCacheKey)obj));

            private bool Equals(CompiledQueryCacheKey other)
                => string.Equals(_query, other._query)
                   && _model.Equals(other._model)
                   && _queryTrackingBehavior == other._queryTrackingBehavior
                   && _async == other._async;

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = _query.GetHashCode();
                    hashCode = (hashCode * 397) ^ _model.GetHashCode();
                    hashCode = (hashCode * 397) ^ _queryTrackingBehavior.GetHashCode();
                    hashCode = (hashCode * 397) ^ _async.GetHashCode();
                    return hashCode;
                }
            }
        }
    }
}
