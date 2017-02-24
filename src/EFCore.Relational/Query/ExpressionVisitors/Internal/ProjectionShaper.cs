// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using System;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ProjectionShaper
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Shaper Create(
            [NotNull] IQuerySource querySource,
            [NotNull] Shaper originalShaper,
            [NotNull] Delegate materializer)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(originalShaper, nameof(originalShaper));
            Check.NotNull(materializer, nameof(materializer));

            var shaper
                = (Shaper)_createShaperMethodInfo
                    .MakeGenericMethod(
                        originalShaper.GetType(),
                        originalShaper.Type,
                        materializer.GetMethodInfo().ReturnType)
                    .Invoke(
                        null,
                        new object[]
                        {
                            querySource,
                            originalShaper,
                            materializer
                        });

            return shaper;
        }

        private static readonly MethodInfo _createShaperMethodInfo
            = typeof(ProjectionShaper).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateShaperMethod));

        [UsedImplicitly]
        private static TypedProjectionShaper<TShaper, TIn, TOut> CreateShaperMethod<TShaper, TIn, TOut>(
            IQuerySource querySource,
            TShaper shaper,
            Func<TIn, TOut> selector)
            where TShaper : Shaper, IShaper<TIn>
            => new TypedProjectionShaper<TShaper, TIn, TOut>(querySource, shaper, selector);

        private class TypedProjectionShaper<TShaper, TIn, TOut> : Shaper, IShaper<TOut>
            where TShaper : Shaper, IShaper<TIn>
        {
            private readonly TShaper _shaper;
            private readonly Func<TIn, TOut> _selector;

            public TypedProjectionShaper(
                IQuerySource querySource,
                TShaper shaper,
                Func<TIn, TOut> selector)
                : base(querySource)
            {
                _shaper = shaper;
                _selector = selector;
            }

            public override Type Type => typeof(TOut);

            public TOut Shape([NotNull] QueryContext queryContext, ValueBuffer valueBuffer)
                => _selector(_shaper.Shape(queryContext, valueBuffer));

            public override Shaper WithOffset(int offset)
                => new TypedProjectionShaper<TShaper, TIn, TOut>(
                    QuerySource,
                    _shaper,
                    _selector).AddOffset(offset);

            public override Shaper AddOffset(int offset)
            {
                _shaper.AddOffset(offset);
                return base.AddOffset(offset);
            }
        }
    }
}