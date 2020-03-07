// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public static class RelationalPropertyExtensions
    {
        /// <summary>
        ///     Creates a comma-separated list of property names.
        /// </summary>
        /// <param name="properties"> The properties to format. </param>
        /// <returns> A comma-separated list of property names. </returns>
        public static string FormatColumns([NotNull] this IEnumerable<IProperty> properties)
            => "{" + string.Join(", ", properties.Select(p => "'" + p.GetColumnName() + "'")) + "}";
    }
}
