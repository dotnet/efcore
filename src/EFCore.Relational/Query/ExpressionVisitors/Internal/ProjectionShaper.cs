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
    public static class ProjectionShaper
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Shaper Create(
            [NotNull] Shaper originalShaper,
            [NotNull] LambdaExpression materializer,
            bool storeMaterializerExpression)
        {
            Check.NotNull(originalShaper, nameof(originalShaper));
            Check.NotNull(materializer, nameof(materializer));

            materializer
                = Expression.Lambda(
                    materializer.Body,
                    EntityQueryModelVisitor.QueryContextParameter,
                    materializer.Parameters[0]);

            var shaper
                = (Shaper)_createShaperMethodInfo
                    .MakeGenericMethod(
                        originalShaper.GetType(),
                        originalShaper.Type,
                        materializer.ReturnType)
                    .Invoke(
                        null,
                        new object[]
                        {
                            originalShaper,
                            materializer.Compile(),
                            storeMaterializerExpression ? materializer : null
                        });

            return shaper;
        }

        private static readonly MethodInfo _createShaperMethodInfo
            = typeof(ProjectionShaper).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateShaperMethod));

        [UsedImplicitly]
        private static TypedProjectionShaper<TShaper, TIn, TOut> CreateShaperMethod<TShaper, TIn, TOut>(
            TShaper shaper,
            Func<QueryContext, TIn, TOut> selector,
            Expression materializerExpression)
            where TShaper : Shaper, IShaper<TIn>
            => new TypedProjectionShaper<TShaper, TIn, TOut>(shaper, selector, materializerExpression);

        private class TypedProjectionShaper<TShaper, TIn, TOut> : Shaper, IShaper<TOut>
            where TShaper : Shaper, IShaper<TIn>
        {
            private readonly TShaper _shaper;
            private readonly Func<QueryContext, TIn, TOut> _selector;

            public TypedProjectionShaper(
                TShaper shaper,
                Func<QueryContext, TIn, TOut> selector,
                Expression materializerExpression)
                : base(shaper.QuerySource, materializerExpression)
            {
                _shaper = shaper;
                _selector = selector;
            }

            public override Expression GetAccessorExpression(IQuerySource querySource)
                => _shaper.GetAccessorExpression(querySource);

            public override void UpdateQuerySource(IQuerySource querySource)
                => _shaper.UpdateQuerySource(querySource);

            public override bool IsShaperForQuerySource(IQuerySource querySource)
                => _shaper.IsShaperForQuerySource(querySource);

            public override void SaveAccessorExpression(QuerySourceMapping querySourceMapping)
                => _shaper.SaveAccessorExpression(querySourceMapping);

            public override IQuerySource QuerySource => _shaper.QuerySource;

            public override Type Type => typeof(TOut);

            public TOut Shape(QueryContext queryContext, in ValueBuffer valueBuffer)
                => _selector(queryContext, _shaper.Shape(queryContext, valueBuffer.WithOffset(ValueBufferOffset)));

            public override Shaper WithOffset(int offset)
                => new TypedProjectionShaper<TShaper, TIn, TOut>(
                    _shaper,
                    _selector,
                    MaterializerExpression).AddOffset(offset);

            public override Shaper AddOffset(int offset)
            {
                _shaper.AddOffset(offset);
                return base.AddOffset(offset);
            }
        }
    }
}
