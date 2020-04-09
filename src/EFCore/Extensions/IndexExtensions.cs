// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class IndexExtensions
    {
        /// <summary>
        ///     <para>
        ///         Gets a factory for key values based on the index key values taken from various forms of entity data.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="index"> The index metadata. </param>
        /// <typeparam name="TKey"> The type of the index instance. </typeparam>
        /// <returns> The factory. </returns>
        public static IDependentKeyValueFactory<TKey> GetNullableValueFactory<TKey>([NotNull] this IIndex index)
            => index.AsIndex().GetNullableValueFactory<TKey>();
    }
}
