// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Extension methods for the <see cref="RelationalTypeMapping" /> class.
    /// </summary>
    public static class RelationalTypeMappingExtensions
    {
        /// <summary>
        ///     Clones the type mapping to update the name and size from the
        ///     mapping info, if needed.
        /// </summary>
        /// <param name="mapping"> The mapping. </param>
        /// <param name="mappingInfo"> The mapping info containing the facets to use. </param>
        /// <returns> The cloned mapping, or the original mapping if no clone was needed. </returns>
        public static RelationalTypeMapping Clone(
            [NotNull] this RelationalTypeMapping mapping,
            [NotNull] RelationalTypeMappingInfo mappingInfo)
        {
            Check.NotNull(mapping, nameof(mapping));
            Check.NotNull(mappingInfo, nameof(mappingInfo));

            return (mappingInfo.Size != null
                    && mappingInfo.Size != mapping.Size)
                   || (mappingInfo.StoreTypeName != null
                       && !string.Equals(mappingInfo.StoreTypeName, mapping.StoreType, StringComparison.Ordinal))
                ? mapping.Clone(
                    mappingInfo.StoreTypeName ?? mapping.StoreType,
                    mappingInfo.Size ?? mapping.Size)
                : mapping;
        }
    }
}
