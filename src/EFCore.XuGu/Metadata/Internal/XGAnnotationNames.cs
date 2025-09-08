// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class XGAnnotationNames
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public const string Prefix = "XG:";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public const string ValueGenerationStrategy = Prefix + "ValueGenerationStrategy";
        public const string LegacyValueGeneratedOnAdd = Prefix + "ValueGeneratedOnAdd";
        public const string LegacyValueGeneratedOnAddOrUpdate = Prefix + "ValueGeneratedOnAddOrUpdate";
        public const string FullTextIndex = Prefix + "FullTextIndex";
        public const string FullTextParser = Prefix + "FullTextParser";
        public const string SpatialIndex = Prefix + "SpatialIndex";
        public const string CharSet = Prefix + "CharSet";
        public const string CharSetDelegation = Prefix + "CharSetDelegation";
        public const string CollationDelegation = Prefix + "CollationDelegation";
        public const string IndexPrefixLength = Prefix + "IndexPrefixLength";
        public const string SpatialReferenceSystemId = Prefix + "SpatialReferenceSystemId";
        public const string GuidCollation = Prefix + "GuidCollation";
        public const string StoreOptions = Prefix + "StoreOptions";

        [Obsolete("Use '" + nameof(RelationalAnnotationNames) + "." + nameof(RelationalAnnotationNames.Collation) + "' instead.")]
        public const string Collation = Prefix + "Collation";
    }
}
