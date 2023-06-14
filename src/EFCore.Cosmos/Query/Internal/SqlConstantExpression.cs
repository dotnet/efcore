// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlConstantExpression : SqlExpression
{
    private readonly ConstantExpression _constantExpression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlConstantExpression(ConstantExpression constantExpression, CoreTypeMapping? typeMapping)
        : base(constantExpression.Type, typeMapping)
    {
        _constantExpression = constantExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? Value
        => _constantExpression.Value;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression ApplyTypeMapping(CoreTypeMapping? typeMapping)
        => new SqlConstantExpression(_constantExpression, typeMapping ?? TypeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void Print(ExpressionPrinter expressionPrinter)
        => Print(Value, expressionPrinter);

    private void Print(
        object? value,
        ExpressionPrinter expressionPrinter)
    {
        if (value is IEnumerable enumerable and not (string or byte[]))
        {
            var first = true;
            foreach (var item in enumerable)
            {
                if (!first)
                {
                    expressionPrinter.Append(", ");
                }

                first = false;
                Print(item, expressionPrinter);
            }
        }
        else
        {
            var jToken = GenerateJToken(Value, TypeMapping);

            expressionPrinter.Append(jToken == null ? "null" : jToken.ToString(Formatting.None));
        }
    }

    private JToken? GenerateJToken(object? value, CoreTypeMapping? typeMapping)
    {
        var mappingClrType = typeMapping?.ClrType.UnwrapNullableType() ?? Type;
        if (value?.GetType().IsInteger() == true
            && mappingClrType.IsEnum)
        {
            value = Enum.ToObject(mappingClrType, value);
        }

        var converter = typeMapping?.Converter;
        if (converter != null)
        {
            value = converter.ConvertToProvider(value);
        }

        if (value == null)
        {
            return null;
        }

        return (value as JToken) ?? JToken.FromObject(value, CosmosClientWrapper.Serializer);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlConstantExpression sqlConstantExpression
                && Equals(sqlConstantExpression));

    private bool Equals(SqlConstantExpression sqlConstantExpression)
        => base.Equals(sqlConstantExpression)
            && (Value?.Equals(sqlConstantExpression.Value) ?? sqlConstantExpression.Value == null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Value);
}
