// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Extension methods for <see cref="ValueComparer" />.
    /// </summary>
    public static class ValueComparerExtensions
    {
        /// <summary>
        ///     Returns <see langword="true" /> if the given <see cref="ValueComparer" /> is a default EF Core implementation.
        /// </summary>
        /// <param name="valueComparer"> The value comparer. </param>
        /// <returns> <see langword="true" /> if the value comparer is the default; <see langword="false" /> otherwise. </returns>
        public static bool IsDefault([NotNull] this ValueComparer valueComparer)
            => valueComparer.GetType().IsGenericType
                && valueComparer.GetType().GetGenericTypeDefinition() == typeof(ValueComparer.DefaultValueComparer<>);
    }
}
