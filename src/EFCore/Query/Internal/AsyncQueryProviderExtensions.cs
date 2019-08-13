// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class AsyncQueryProviderExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                        null, new object[] { entityQueryProvider }));
        }

        private static readonly MethodInfo _createEntityQueryableMethod
            = typeof(AsyncQueryProviderExtensions)
                .GetTypeInfo().GetDeclaredMethod(nameof(CreateEntityQueryable));

        [UsedImplicitly]
        private static EntityQueryable<TResult> CreateEntityQueryable<TResult>(IAsyncQueryProvider entityQueryProvider)
            => new EntityQueryable<TResult>(entityQueryProvider);
    }
}
