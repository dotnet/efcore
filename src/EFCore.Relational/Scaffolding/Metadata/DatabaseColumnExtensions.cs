// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata
{
    /// <summary>
    ///     Extension methods for <see cref="DatabaseColumn" />.
    /// </summary>
    public static class DatabaseColumnExtensions
    {
        /// <summary>
        ///     Gets the underlying store/database type for the given column.
        /// </summary>
        /// <param name="column"> The column. </param>
        /// <returns> The database/store type, or <c>null</c> if none has been set. </returns>
        [Obsolete("Set storetype directly on DatabaseColumn.StoreType.")]
        public static string GetUnderlyingStoreType([NotNull] this DatabaseColumn column)
            => (string)Check.NotNull(column, nameof(column))[ScaffoldingAnnotationNames.UnderlyingStoreType];

        /// <summary>
        ///     Sets the underlying store/database type for the given column.
        /// </summary>
        /// <param name="column"> The column. </param>
        /// <param name="value"> The database/store type, or <c>null</c> if none. </param>
        [Obsolete("Set storetype directly on DatabaseColumn.StoreType.")]
        public static void SetUnderlyingStoreType([NotNull] this DatabaseColumn column, [CanBeNull] string value)
            => Check.NotNull(column, nameof(column))[ScaffoldingAnnotationNames.UnderlyingStoreType] = value;
    }
}
