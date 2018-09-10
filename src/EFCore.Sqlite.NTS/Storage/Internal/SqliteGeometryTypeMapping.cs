// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.ValueConversion.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.IO;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteGeometryTypeMapping : RelationalTypeMapping
    {
        private readonly GaiaGeoReader _reader;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqliteGeometryTypeMapping(Type clrType, GaiaGeoReader reader, string storeType)
            : base(
                  new RelationalTypeMappingParameters(
                      new CoreTypeMappingParameters(
                          clrType,
                          new GeometryValueConverter(clrType, reader),
                          new GeometryValueComparer(clrType)),
                      storeType))
        {
            _reader = reader;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected SqliteGeometryTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new SqliteGeometryTypeMapping(parameters);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GenerateNonNullSqlLiteral(object value)
        {
            // TODO: Avoid converting in the first place
            var geometry = _reader.Read((byte[])value);
            var srid = geometry.SRID;

            // TODO: This won't emit M (see NetTopologySuite/NetTopologySuite#156)
            var text = "'" + geometry.AsText() + "'";

            return srid > 0
                ? $"GeomFromText({text}, {srid})"
                : $"GeomFromText({text})";
        }
    }
}
