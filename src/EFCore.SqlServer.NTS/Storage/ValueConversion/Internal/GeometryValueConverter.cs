// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.SqlTypes;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.ValueConversion.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class GeometryValueConverter<TGeometry> : ValueConverter<TGeometry, SqlBytes>
    where TGeometry : Geometry
{
    // The IsValid flag is at bit 2 (0x04) of the Properties byte at offset 5 in the SQL Server
    // geography/geometry binary format (MS-SSCLRT). SqlServerBytesWriter sets this flag based on
    // NTS's Geometry.IsValid, but NTS and SQL Server use different validation rules. We always set
    // this flag to true to avoid NTS validation rules incorrectly marking geometries as invalid
    // in SQL Server. See https://github.com/dotnet/efcore/issues/37416
    private const int PropertiesByteIndex = 5;
    private const byte IsValidFlag = 0x04;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public GeometryValueConverter(SqlServerBytesReader reader, SqlServerBytesWriter writer)
        : base(
            g => new SqlBytes(SetIsValidFlag(writer.Write(g))),
            b => (TGeometry)reader.Read(b.Value))
    {
    }

    private static byte[] SetIsValidFlag(byte[] bytes)
    {
        if (bytes.Length > PropertiesByteIndex)
        {
            bytes[PropertiesByteIndex] |= IsValidFlag;
        }

        return bytes;
    }
}
