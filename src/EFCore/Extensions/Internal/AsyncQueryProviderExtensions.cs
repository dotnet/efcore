// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Extensions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class AsyncQueryProviderExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ConstantExpression CreateEntityQueryableExpression(
            [NotNull] this IAsyncQueryProvider entityQueryProvider, [NotNull] Type type)
        {
            Check.NotNull(entityQueryProvider, nameof(entityQueryProvider));
            Check.NotNull(type, nameof(type));

            return Expression.Constant(
                _createEntityQueryableMethod
                    .MakeGenericMethod(type)
                    .Invoke(
                        null, new object[]
                            { entityQueryProvider }));
        }

        private static readonly MethodInfo _createEntityQueryableMethod
            = typeof(AsyncQueryProviderExtensions)
                .GetTypeInfo().GetDeclaredMethod(nameof(_CreateEntityQueryable));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static EntityQueryable<TResult> _CreateEntityQueryable<TResult>(IAsyncQueryProvider entityQueryProvider)
            => new EntityQueryable<TResult>(entityQueryProvider);
    }
}
