// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Contains extension methods on <see cref="DbFunctions" /> for the Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite library.
/// </summary>
public static class SqlServerNetTopologySuiteDbFunctionsExtensions
{
    /// <summary>
    ///     Maps to the SQL Server <c>STCurveToLine</c> function which returns a polygonal approximation of an instance that contains circular arc
    ///     segments.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="geometry">The instance containing circular arc segments.</param>
    /// <returns>The polygonal approximation.</returns>
    public static Geometry CurveToLine(this DbFunctions _, Geometry geometry)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(CurveToLine)));
}
