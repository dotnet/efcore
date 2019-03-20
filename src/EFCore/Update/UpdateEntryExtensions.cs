// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     Extension methods for <see cref="IUpdateEntry" />.
    /// </summary>
    public static class UpdateEntryExtensions
    {
        /// <summary>
        ///     Creates a formatted string representation of the given properties and their current
        ///     values such as is useful when throwing exceptions about keys, indexes, etc. that use
        ///     the properties.
        /// </summary>
        /// <param name="entry"> The entry from which values will be obtained. </param>
        /// <param name="properties"> The properties to format. </param>
        /// <returns> The string representation. </returns>
        public static string BuildCurrentValuesString(
            [NotNull] this IUpdateEntry entry,
            [NotNull] IEnumerable<IPropertyBase> properties)
            => "{" + string.Join(", ", properties.Select(p => p.Name + ": " + Convert.ToString(entry.GetCurrentValue(p), CultureInfo.InvariantCulture))) + "}";

        /// <summary>
        ///     Creates a formatted string representation of the given properties and their original
        ///     values such as is useful when throwing exceptions about keys, indexes, etc. that use
        ///     the properties.
        /// </summary>
        /// <param name="entry"> The entry from which values will be obtained. </param>
        /// <param name="properties"> The properties to format. </param>
        /// <returns> The string representation. </returns>
        public static string BuildOriginalValuesString(
            [NotNull] this IUpdateEntry entry,
            [NotNull] IEnumerable<IPropertyBase> properties)
            => "{" + string.Join(", ", properties.Select(p => p.Name + ": " + Convert.ToString(entry.GetOriginalValue(p), CultureInfo.InvariantCulture))) + "}";
    }
}
