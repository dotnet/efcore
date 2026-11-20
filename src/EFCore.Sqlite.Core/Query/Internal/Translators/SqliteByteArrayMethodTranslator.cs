// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteByteArrayMethodTranslator(ISqlExpressionFactory sqlExpressionFactory) : IMethodCallTranslator
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.IsGenericMethod
            && method.DeclaringType == typeof(Enumerable)
            && method.Name == nameof(Enumerable.Contains)
            && arguments is [var source, var item]
            && source.Type == typeof(byte[]))
        {
            // A non-constant byte has to be turned into a one-byte BLOB. char() would produce TEXT (the UTF-8 encoding of
            // the code point), which makes instr() compare as text and gives wrong results for values above 0x7F.
            var value = item is SqlConstantExpression constantValue
                ? sqlExpressionFactory.Constant(new[] { (byte)constantValue.Value! }, source.TypeMapping)
                : sqlExpressionFactory.Function(
                    "unhex",
                    [
                        sqlExpressionFactory.Function(
                            "printf",
                            [sqlExpressionFactory.Constant("%02X"), item],
                            nullable: true,
                            argumentsPropagateNullability: Statics.FalseTrue,
                            typeof(string))
                    ],
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[1],
                    typeof(byte[]),
                    source.TypeMapping);

            return sqlExpressionFactory.GreaterThan(
                sqlExpressionFactory.Function(
                    "instr",
                    [source, value],
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[2],
                    typeof(int)),
                sqlExpressionFactory.Constant(0));
        }

        if (method.IsGenericMethod
            && method.DeclaringType == typeof(Enumerable)
            && method.Name == nameof(Enumerable.Concat)
            && arguments is [var first, var second]
            && IsBinary(first)
            && IsBinary(second)
            && (first.TypeMapping ?? second.TypeMapping) is { } typeMapping)
        {
            // || produces TEXT even over BLOBs, so the result is cast back to BLOB; otherwise e.g. length() would count characters
            // and comparisons with BLOBs would fail. The concatenation is typed as string (TEXT) to reflect that, which also
            // prevents the CAST from being removed as a no-op. The translation is typed as byte[] rather than as the method's
            // IEnumerable<byte>, so that byte array operations (ToArray, Length, SequenceEqual...) can be composed over it.
            return sqlExpressionFactory.Convert(
                new SqlBinaryExpression(
                    ExpressionType.Add,
                    sqlExpressionFactory.ApplyTypeMapping(first, typeMapping),
                    sqlExpressionFactory.ApplyTypeMapping(second, typeMapping),
                    typeof(string),
                    typeMapping: null),
                typeof(byte[]),
                typeMapping);
        }

        // ToArray over a byte array (e.g. over Concat above) doesn't change the value
        if (method.IsGenericMethod
            && method.DeclaringType == typeof(Enumerable)
            && method.Name == nameof(Enumerable.ToArray)
            && arguments is [var toArraySource]
            && toArraySource.Type == typeof(byte[]))
        {
            return toArraySource;
        }

        return method.IsGenericMethod
            && method.DeclaringType == typeof(Enumerable)
            && method.Name == nameof(Enumerable.Any)
            && arguments is [var anySource]
            && anySource.Type == typeof(byte[])
                ? sqlExpressionFactory.GreaterThan(
                    sqlExpressionFactory.Function(
                        "length",
                        [anySource],
                        nullable: true,
                        argumentsPropagateNullability: Statics.TrueArrays[1],
                        typeof(int)),
                    sqlExpressionFactory.Constant(0))
                : null;

        // A byte[] value converter could store something other than the bytes themselves, which can't be concatenated in SQL
        static bool IsBinary(SqlExpression expression)
            => expression is { Type: var type, TypeMapping: null or { Converter: null } } && type == typeof(byte[]);
    }

    // See issue#16428
    //if (method.IsGenericMethod
    //    && method.GetGenericMethodDefinition().Equals(EnumerableMethods.FirstWithoutPredicate)
    //    && arguments[0].Type == typeof(byte[]))
    //{
    //    return _sqlExpressionFactory.Function(
    //        "unicode",
    //        new SqlExpression[]
    //        {
    //            _sqlExpressionFactory.Function(
    //                "substr",
    //                new SqlExpression[]
    //                {
    //                    arguments[0],
    //                    _sqlExpressionFactory.Constant(1),
    //                    _sqlExpressionFactory.Constant(1)
    //                },
    //                nullable: true,
    //                argumentsPropagateNullability: new[] { true, true, true },
    //                typeof(byte[]))
    //        },
    //        nullable: true,
    //        argumentsPropagateNullability: new[] { true },
    //        method.ReturnType);
    //}
}
