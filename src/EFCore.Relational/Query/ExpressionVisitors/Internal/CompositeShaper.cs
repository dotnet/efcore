// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class CompositeShaper
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Shaper Create(
            [NotNull] IQuerySource querySource,
            [NotNull] Shaper outerShaper,
            [NotNull] Shaper innerShaper,
            [NotNull] LambdaExpression materializer,
            bool storeMaterializerExpression)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(outerShaper, nameof(outerShaper));
            Check.NotNull(innerShaper, nameof(innerShaper));
            Check.NotNull(materializer, nameof(materializer));

            return (Shaper)_createCompositeShaperMethodInfo
                    .MakeGenericMethod(
                        outerShaper.GetType(),
                        outerShaper.Type,
                        innerShaper.GetType(),
                        innerShaper.Type,
                        materializer.ReturnType)
                    .Invoke(
                        null,
                        new object[]
                        {
                            querySource,
                            outerShaper,
                            innerShaper,
                            materializer.Compile(),
                            storeMaterializerExpression ? materializer : null
                        });
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Obsolete]
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
                        outerShaper.GetType(),
                        outerShaper.Type,
                        innerShaper.GetType(),
                        innerShaper.Type,
                        materializer.GetMethodInfo().ReturnType)
                    .Invoke(
                        null,
                        new object[]
                        {
                            querySource,
                            outerShaper,
                            innerShaper,
                            materializer,
                            null
                        });

            return compositeShaper;
        }

        private static readonly MethodInfo _createCompositeShaperMethodInfo
            = typeof(CompositeShaper).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateCompositeShaperMethod));

        [UsedImplicitly]
        private static TypedCompositeShaper<TOuterShaper, TOuter, TInnerShaper, TInner, TResult>
            CreateCompositeShaperMethod<TOuterShaper, TOuter, TInnerShaper, TInner, TResult>(
                IQuerySource querySource,
                TOuterShaper outerShaper,
                TInnerShaper innerShaper,
                Func<TOuter, TInner, TResult> materializer,
                Expression materializerExpression)
            where TOuterShaper : Shaper, IShaper<TOuter>
            where TInnerShaper : Shaper, IShaper<TInner>
            => new TypedCompositeShaper<TOuterShaper, TOuter, TInnerShaper, TInner, TResult>(
                querySource, outerShaper, innerShaper, materializer, materializerExpression);

        private class TypedCompositeShaper<TOuterShaper, TOuter, TInnerShaper, TInner, TResult>
            : Shaper, IShaper<TResult>
            where TOuterShaper : Shaper, IShaper<TOuter>
            where TInnerShaper : Shaper, IShaper<TInner>
        {
            private readonly TOuterShaper _outerShaper;
            private readonly TInnerShaper _innerShaper;
            private readonly Func<TOuter, TInner, TResult> _materializer;

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public TypedCompositeShaper(
                IQuerySource querySource,
                TOuterShaper outerShaper,
                TInnerShaper innerShaper,
                Func<TOuter, TInner, TResult> materializer,
                Expression materializerExpression)
                : base(querySource, materializerExpression)
            {
                _outerShaper = outerShaper;
                _innerShaper = innerShaper;
                _materializer = materializer;
            }

            public TResult Shape(QueryContext queryContext, in ValueBuffer valueBuffer)
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

            public override Shaper WithOffset(int offset)
                => new TypedCompositeShaper<TOuterShaper, TOuter, TInnerShaper, TInner, TResult>(
                    QuerySource,
                    _outerShaper,
                    _innerShaper,
                    _materializer,
                    MaterializerExpression).AddOffset(offset);

            public override Shaper AddOffset(int offset)
            {
                _outerShaper.AddOffset(offset);
                _innerShaper.AddOffset(offset);

                return base.AddOffset(offset);
            }

            public override Shaper Unwrap(IQuerySource querySource)
                => _outerShaper.Unwrap(querySource)
                       ?? _innerShaper.Unwrap(querySource)
                       ?? base.Unwrap(querySource);
        }
    }
}
