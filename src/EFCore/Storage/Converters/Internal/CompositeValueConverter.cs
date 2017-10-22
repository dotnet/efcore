// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Microsoft.EntityFrameworkCore.Storage.Converters.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CompositeValueConverter<TModel, TMiddle, TStore> : ValueConverter<TModel, TStore>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CompositeValueConverter(
            [NotNull] ValueConverter converter1,
            [NotNull] ValueConverter converter2,
            ConverterMappingHints mappingHints = default)
            : base(
                Compose(
                    (Expression<Func<TModel, TMiddle>>)converter1.ConvertToStoreExpression,
                    (Expression<Func<TMiddle, TStore>>)converter2.ConvertToStoreExpression),
                Compose(
                    (Expression<Func<TStore, TMiddle>>)converter2.ConvertFromStoreExpression,
                    (Expression<Func<TMiddle, TModel>>)converter1.ConvertFromStoreExpression),
                mappingHints)
        {
        }

        private static Expression<Func<TIn, TOut>> Compose<TIn, TOut>(
            Expression<Func<TIn, TMiddle>> upper,
            Expression<Func<TMiddle, TOut>> lower)
            => Expression.Lambda<Func<TIn, TOut>>(
                ReplacingExpressionVisitor.Replace(
                    lower.Parameters.Single(),
                    upper.Body,
                    lower.Body),
                upper.Parameters.Single());
    }
}
