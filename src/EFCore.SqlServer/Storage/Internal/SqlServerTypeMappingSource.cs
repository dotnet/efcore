// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerTypeMappingSource : FallbackRelationalTypeMappingSource
    {
        private readonly RelationalTypeMapping _sqlVariant = new SqlServerSqlVariantTypeMapping("sql_variant");
        private readonly FloatTypeMapping _real = new SqlServerFloatTypeMapping("real");

        private readonly IReadOnlyDictionary<string, Func<Type, RelationalTypeMapping>> _namedClrMappings
            = new Dictionary<string, Func<Type, RelationalTypeMapping>>(StringComparer.Ordinal)
            {
                { "Microsoft.SqlServer.Types.SqlHierarchyId", t => new SqlServerUdtTypeMapping("hierarchyid", t) },
                { "Microsoft.SqlServer.Types.SqlGeography", t => new SqlServerUdtTypeMapping("geography", t) },
                { "Microsoft.SqlServer.Types.SqlGeometry", t => new SqlServerUdtTypeMapping("geometry", t) }
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerTypeMappingSource(
            [NotNull] TypeMappingSourceDependencies dependencies,
            [NotNull] RelationalTypeMappingSourceDependencies relationalDependencies,
            [NotNull] IRelationalTypeMapper typeMapper)
            : base(dependencies, relationalDependencies, typeMapper)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping FindMapping(RelationalTypeMappingInfo mappingInfo)
        {
            var targetClrType = mappingInfo.ProviderClrType;

            if (targetClrType != null
                && _namedClrMappings.TryGetValue(targetClrType.FullName, out var mappingFunc))
            {
                return mappingFunc(targetClrType);
            }

            var storeTypeName = mappingInfo.StoreTypeName;

            if (storeTypeName != null)
            {
                if (_sqlVariant.StoreType.Equals(storeTypeName, StringComparison.OrdinalIgnoreCase)
                    && targetClrType == typeof(object))
                {
                    return _sqlVariant.Clone(storeTypeName, null);
                }

                if ((storeTypeName.StartsWith("float", StringComparison.OrdinalIgnoreCase)
                     || storeTypeName.StartsWith("double precision", StringComparison.OrdinalIgnoreCase))
                    && TryParseScale(storeTypeName, out var scale)
                    && scale <= 24)
                {
                    return _real.Clone(storeTypeName, null);
                }
            }

            return base.FindMapping(mappingInfo);
        }

        private bool TryParseScale(string storeTypeName, out int scale)
        {
            var openParen = storeTypeName.IndexOf("(", StringComparison.Ordinal);
            if (openParen > 0)
            {
                var closeParen = storeTypeName.IndexOf(")", openParen + 1, StringComparison.Ordinal);
                if (closeParen > openParen)
                {
                    return int.TryParse(storeTypeName.Substring(openParen + 1, closeParen - openParen - 1), out scale);
                }
            }

            scale = 0;
            return false;
        }
    }
}
