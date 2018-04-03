// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class UpdateEntryExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string BuildCurrentValuesString([NotNull] this IUpdateEntry entry, [NotNull] IEnumerable<IPropertyBase> properties)
            => "{" + string.Join(", ", properties.Select(p => p.Name + ": " + Convert.ToString(entry.GetCurrentValue(p), CultureInfo.InvariantCulture))) + "}";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string BuildOriginalValuesString([NotNull] this IUpdateEntry entry, [NotNull] IEnumerable<IPropertyBase> properties)
            => "{" + string.Join(", ", properties.Select(p => p.Name + ": " + Convert.ToString(entry.GetOriginalValue(p), CultureInfo.InvariantCulture))) + "}";
    }
}
