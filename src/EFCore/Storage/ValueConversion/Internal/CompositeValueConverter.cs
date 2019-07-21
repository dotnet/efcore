// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CompositeValueConverter<TModel, TMiddle, TProvider> : ValueConverter<TModel, TProvider>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CompositeValueConverter(
            [NotNull] ValueConverter converter1,
            [NotNull] ValueConverter converter2,
            [CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                Compose(
                    (Expression<Func<TModel, TMiddle>>)converter1.ConvertToProviderExpression,
                    (Expression<Func<TMiddle, TProvider>>)converter2.ConvertToProviderExpression),
                Compose(
                    (Expression<Func<TProvider, TMiddle>>)converter2.ConvertFromProviderExpression,
                    (Expression<Func<TMiddle, TModel>>)converter1.ConvertFromProviderExpression),
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
