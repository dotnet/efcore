// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteStringTypeMapping : StringTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new SqliteStringTypeMapping Default { get; } = new(SqliteTypeMappingSource.TextTypeName);

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqliteStringTypeMapping" /> class.
    /// </summary>
    /// <param name="storeType">The name of the database type.</param>
    /// <param name="dbType">The <see cref="DbType" /> to be used.</param>
    /// <param name="unicode">A value indicating whether the type should handle Unicode data or not.</param>
    /// <param name="size">The size of data the property is configured to store, or null if no size is configured.</param>
    public SqliteStringTypeMapping(
        string storeType,
        DbType? dbType = null,
        bool unicode = false,
        int? size = null)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(string), jsonValueReaderWriter: JsonStringReaderWriter.Instance), storeType, StoreTypePostfix.None, dbType,
                unicode, size))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqliteStringTypeMapping" /> class.
    /// </summary>
    /// <param name="parameters">Parameter object for <see cref="RelationalTypeMapping" />.</param>
    protected SqliteStringTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqliteStringTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var stringValue = (string)value;
        var builder = new StringBuilder();

        var start = 0;
        int i;
        int length;
        var openApostrophe = false;
        var lengths = new List<int>();
        var startIndexes = new List<int> { 0 };
        for (i = 0; i < stringValue.Length; i++)
        {
            var lineFeed = stringValue[i] == '\n';
            var carriageReturn = stringValue[i] == '\r';
            var apostrophe = stringValue[i] == '\'';
            if (lineFeed || carriageReturn || apostrophe)
            {
                length = i - start;
                if (length != 0)
                {
                    if (!openApostrophe)
                    {
                        if (builder.Length != 0)
                        {
                            lengths.Add(builder.Length - startIndexes[^1]);
                            startIndexes.Add(builder.Length);
                        }

                        builder.Append('\'');
                        openApostrophe = true;
                    }

                    builder.Append(stringValue.AsSpan().Slice(start, length));
                }

                if (lineFeed || carriageReturn)
                {
                    if (openApostrophe)
                    {
                        builder.Append('\'');
                        openApostrophe = false;
                    }

                    if (builder.Length != 0)
                    {
                        lengths.Add(builder.Length - startIndexes[^1]);
                        startIndexes.Add(builder.Length);
                    }

                    builder
                        .Append("CHAR(")
                        .Append(lineFeed ? "10" : "13")
                        .Append(')');
                }
                else if (apostrophe)
                {
                    if (!openApostrophe)
                    {
                        if (builder.Length != 0)
                        {
                            lengths.Add(builder.Length - startIndexes[^1]);
                            startIndexes.Add(builder.Length);
                        }

                        builder.Append('\'');
                        openApostrophe = true;
                    }

                    builder.Append("''");
                }

                start = i + 1;
            }
        }

        length = i - start;
        if (length != 0)
        {
            if (!openApostrophe)
            {
                if (builder.Length != 0)
                {
                    lengths.Add(builder.Length - startIndexes[^1]);
                    startIndexes.Add(builder.Length);
                }

                builder.Append('\'');
                openApostrophe = true;
            }

            builder.Append(stringValue.AsSpan().Slice(start, length));
        }

        if (openApostrophe)
        {
            builder.Append('\'');
        }

        if (builder.Length != 0)
        {
            lengths.Add(builder.Length - startIndexes[^1]);
        }

        if (lengths.Count == 0
            && builder.Length == 0)
        {
            return "''";
        }

        var newBuilder = new StringBuilder();
        GenerateBalancedTree(0, lengths.Count);

        return newBuilder.ToString();

        void GenerateBalancedTree(int start, int end)
        {
            var count = end - start;
            if (count < 1)
            {
                return;
            }

            if (count == 1)
            {
                newBuilder.Append(builder, startIndexes[start], lengths[start]);
                return;
            }

            var mid = start + count / 2;
            newBuilder.Append('(');
            GenerateBalancedTree(start, mid);
            newBuilder.Append(" || ");
            GenerateBalancedTree(mid, end);
            newBuilder.Append(')');
        }
    }
}
