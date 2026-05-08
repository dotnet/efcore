// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerVectorTypeMapping : RelationalTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SqlServerVectorTypeMapping Default { get; } = new(dimensions: null);

    private static readonly VectorComparer _comparerInstance = new();

    // Note that dimensions is mandatory with SQL Server vector.
    // However, our scaffolder looks up each type mapping without the facets, to find out whether the scaffolded
    // facet happens to be the default (and therefore can be omitted). So we allow constructing a SqlServerVectorTypeMapping
    // without dimensions, and validate against it in SqlServerModelValidator.

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerVectorTypeMapping(int? dimensions)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(SqlVector<float>), comparer: _comparerInstance),
                "vector",
                StoreTypePostfix.Size,
                size: dimensions))
    {
        if (dimensions is <= 0)
        {
            throw new InvalidOperationException(SqlServerStrings.VectorDimensionsInvalid);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlServerVectorTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
        if (parameters.Size is <= 0)
        {
            throw new InvalidOperationException(SqlServerStrings.VectorDimensionsInvalid);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqlServerVectorTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        Check.DebugAssert(Size is not null);

        var sqlVector = (SqlVector<float>)value;

        if (sqlVector.IsNull)
        {
            return "NULL";
        }

        // SQL Server has an implicit cast from JSON arrays (as strings or as the json type) to vector -
        // that's the literal representation (though use-cases are probably mostly contrived/testing-only).
        var builder = new StringBuilder();
        var floats = sqlVector.Memory.Span;

        builder.Append("CAST('[");

        for (var i = 0; i < floats.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(',');
            }

            builder.Append(floats[i]);
        }

        builder
            .Append("]' AS VECTOR(")
            .Append(Size)
            .Append("))");

        return builder.ToString();
    }

    private sealed class VectorComparer() : ValueComparer<SqlVector<float>>(
        (x, y) => CalculateEquality(x, y),
        v => CalculateHashCode(v),
        v => v)
    {
        // Note that we do not perform value comparison here, only checking that the SqlVector wraps the same memory.
        // This is because vectors are basically immutable, and it's better to have more efficient change tracking
        // equality checks.
        private static bool CalculateEquality(SqlVector<float>? x, SqlVector<float>? y)
            => x is { } v1 && y is { } v2
                ? v1.IsNull
                    ? v2.IsNull
                    : !v2.IsNull && v1.Memory.Span == v2.Memory.Span
                : x is null && y is null;

        private static int CalculateHashCode(SqlVector<float> vector)
        {
            if (vector.IsNull)
            {
                return 0;
            }

            var hash = new HashCode();

            foreach (var value in vector.Memory.Span)
            {
                hash.Add(value);
            }

            return hash.ToHashCode();
        }
    }
}
