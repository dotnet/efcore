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
        ///     Clones the type mapping to update the name with size, precision,
        ///     and scale updated in the type name, as is common for some database
        ///     providers.
        /// </summary>
        /// <param name="mapping"> The mapping. </param>
        /// <param name="mappingInfo"> The mapping info containing the facets to use. </param>
        /// <returns> The cloned mapping, or the original mapping if no clone was needed. </returns>
        public static RelationalTypeMapping CloneWithFacetedName(
            [NotNull] this RelationalTypeMapping mapping,
            [NotNull] RelationalTypeMappingInfo mappingInfo)
        {
            Check.NotNull(mapping, nameof(mapping));
            Check.NotNull(mappingInfo, nameof(mappingInfo));

            var clone = false;

            var storeTypeName = mappingInfo.StoreTypeName;
            if (storeTypeName != null
                && !storeTypeName.Equals(mapping.StoreType, StringComparison.Ordinal))
            {
                clone = true;
            }
            else
            {
                storeTypeName = mapping.StoreType;
            }

            var hints = mappingInfo.ValueConverterInfo?.MappingHints;

            var size = mapping.Size == -1 ? -1 : (int?)null;
            if (size != -1)
            {
                size = mappingInfo.Size
                       ?? mapping.Size
                       ?? hints?.Size;

                if (size != mapping.Size)
                {
                    var typeNameBase = GetTypeNameBase(mappingInfo, storeTypeName, out var isMax);
                    if (!mappingInfo.StoreTypeNameSizeIsMax
                        && !isMax)
                    {
                        storeTypeName = typeNameBase + "(" + size + ")";
                    }

                    clone = true;
                }
            }

            if (mappingInfo.Precision != null
                && mappingInfo.Scale != null)
            {
                storeTypeName = GetTypeNameBase(mappingInfo, storeTypeName, out var _)
                                + "(" + mappingInfo.Precision + "," + mappingInfo.Scale + ")";
                clone = true;
            }

            if (clone)
            {
                mapping = mapping.Clone(storeTypeName, size);
            }

            return mapping;
        }

        private static string GetTypeNameBase(RelationalTypeMappingInfo mappingInfo, string storeTypeName, out bool isMax)
        {
            isMax = false;
            var typeNameBase = mappingInfo.StoreTypeNameBase;
            if (typeNameBase == null)
            {
                typeNameBase = storeTypeName;
                var openParen = typeNameBase.IndexOf("(", StringComparison.Ordinal);
                if (openParen > 0)
                {
                    if (typeNameBase.Substring(openParen + 1).Trim().StartsWith("max", StringComparison.OrdinalIgnoreCase))
                    {
                        isMax = true;
                    }

                    typeNameBase = typeNameBase.Substring(0, openParen);
                }
            }

            return typeNameBase;
        }
    }
}
