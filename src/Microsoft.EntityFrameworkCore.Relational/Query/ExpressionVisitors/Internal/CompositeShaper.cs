// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CompositeShaper
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Shaper Create(
            [NotNull] IQuerySource querySource,
            [NotNull] Shaper outerShaper,
            [NotNull] Shaper innerShaper,
            [NotNull] Delegate materializer)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(outerShaper, nameof(outerShaper));
            Check.NotNull(innerShaper, nameof(innerShaper));
            Check.NotNull(materializer, nameof(materializer));

            var compositeShaper
                = (Shaper)_createCompositeShaperMethodInfo
                    .MakeGenericMethod(
                        outerShaper.Type,
                        innerShaper.Type,
                        materializer.GetMethodInfo().ReturnType)
                    .Invoke(
                        null,
                        new object[]
                        {
                                querySource,
                                outerShaper,
                                innerShaper,
                                materializer
                        });

            return compositeShaper;
        }

        private static readonly MethodInfo _createCompositeShaperMethodInfo
            = typeof(CompositeShaper).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateCompositeShaperMethod));

        [UsedImplicitly]
        private static TypedCompositeShaper<TOuter, TInner, TResult> CreateCompositeShaperMethod<TOuter, TInner, TResult>(
            IQuerySource querySource,
            IShaper<TOuter> outerShaper,
            IShaper<TInner> innerShaper,
            Func<TOuter, TInner, TResult> materializer)
            => new TypedCompositeShaper<TOuter, TInner, TResult>(
                querySource, outerShaper, innerShaper, materializer);

        private class TypedCompositeShaper<TOuter, TInner, TResult> : Shaper, IShaper<TResult>
        {
            private readonly IShaper<TOuter> _outerShaper;
            private readonly IShaper<TInner> _innerShaper;
            private readonly Func<TOuter, TInner, TResult> _materializer;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public TypedCompositeShaper(
                IQuerySource querySource,
                IShaper<TOuter> outerShaper,
                IShaper<TInner> innerShaper,
                Func<TOuter, TInner, TResult> materializer)
                : base(querySource)
            {
                _outerShaper = outerShaper;
                _innerShaper = innerShaper;
                _materializer = materializer;
            }

            public TResult Shape(QueryContext queryContext, ValueBuffer valueBuffer)
                => _materializer(
                    _outerShaper.Shape(queryContext, valueBuffer),
                    _innerShaper.Shape(queryContext, valueBuffer));

            public override Type Type => typeof(TResult);

            public override bool IsShaperForQuerySource(IQuerySource querySource)
                => base.IsShaperForQuerySource(querySource)
                    || _outerShaper.IsShaperForQuerySource(querySource)
                    || _innerShaper.IsShaperForQuerySource(querySource);

            public override void SaveAccessorExpression(QuerySourceMapping querySourceMapping)
            {
                _outerShaper.SaveAccessorExpression(querySourceMapping);
                _innerShaper.SaveAccessorExpression(querySourceMapping);
            }

            public override Expression GetAccessorExpression(IQuerySource querySource)
                => _outerShaper.GetAccessorExpression(querySource)
                    ?? _innerShaper.GetAccessorExpression(querySource);
        }
    }
}
