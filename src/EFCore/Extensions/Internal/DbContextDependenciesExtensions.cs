// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class DbContextDependenciesExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        /// <remarks>
        ///     This should only be called from <see cref="InternalDbSet{TEntity}" /> as it is created
        ///     before the context is initialized
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDbContextDependencies GetDependencies([NotNull] this IDbContextDependencies context)
            => context;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDbContextDependencies GetDependencies([NotNull] this ICurrentDbContext context)
            => context.Context;
    }
}
