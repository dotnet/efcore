// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlTypes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.ValueConversion.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class GeometryValueConverter<TGeometry> : ValueConverter<TGeometry, SqlBytes>
        where TGeometry : Geometry
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public GeometryValueConverter(SqlServerBytesReader reader, SqlServerBytesWriter writer)
            : base(
                g => new SqlBytes(writer.Write(g)),
                b => (TGeometry)reader.Read(b.Value))
        {
        }
    }
}
