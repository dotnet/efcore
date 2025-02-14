// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlExpressionFactory(ITypeMappingSource typeMappingSource, IModel model)
    : ISqlExpressionFactory
{
    private readonly CoreTypeMapping _boolTypeMapping = typeMappingSource.FindMapping(typeof(bool), model)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull(nameof(sqlExpression))]
    public virtual SqlExpression? ApplyDefaultTypeMapping(SqlExpression? sqlExpression)
        => sqlExpression is not { TypeMapping: null }
            ? sqlExpression
            : ApplyTypeMapping(sqlExpression, typeMappingSource.FindMapping(sqlExpression.Type, model));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull(nameof(sqlExpression))]
    public virtual SqlExpression? ApplyTypeMapping(SqlExpression? sqlExpression, CoreTypeMapping? typeMapping)
        => sqlExpression switch
        {
            null or { TypeMapping: not null } => sqlExpression,

            ScalarSubqueryExpression e => e.ApplyTypeMapping(typeMapping),
            SqlConditionalExpression sqlConditionalExpression => ApplyTypeMappingOnSqlConditional(sqlConditionalExpression, typeMapping),
            SqlBinaryExpression sqlBinaryExpression => ApplyTypeMappingOnSqlBinary(sqlBinaryExpression, typeMapping),
            SqlUnaryExpression sqlUnaryExpression => ApplyTypeMappingOnSqlUnary(sqlUnaryExpression, typeMapping),
            SqlConstantExpression sqlConstantExpression => sqlConstantExpression.ApplyTypeMapping(typeMapping),
            SqlParameterExpression sqlParameterExpression => sqlParameterExpression.ApplyTypeMapping(typeMapping),
            SqlFunctionExpression sqlFunctionExpression => sqlFunctionExpression.ApplyTypeMapping(typeMapping),

            _ => sqlExpression
        };

    private SqlExpression ApplyTypeMappingOnSqlConditional(
        SqlConditionalExpression sqlConditionalExpression,
        CoreTypeMapping? typeMapping)
        => sqlConditionalExpression.Update(
            sqlConditionalExpression.Test,
            ApplyTypeMapping(sqlConditionalExpression.IfTrue, typeMapping),
            ApplyTypeMapping(sqlConditionalExpression.IfFalse, typeMapping));

    private SqlExpression ApplyTypeMappingOnSqlUnary(
        SqlUnaryExpression sqlUnaryExpression,
        CoreTypeMapping? typeMapping)
    {
        SqlExpression operand;
        Type resultType;
        CoreTypeMapping? resultTypeMapping;
        switch (sqlUnaryExpression.OperatorType)
        {
            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
            case ExpressionType.Not
                when sqlUnaryExpression.IsLogicalNot():
            {
                resultTypeMapping = _boolTypeMapping;
                resultType = typeof(bool);
                operand = ApplyDefaultTypeMapping(sqlUnaryExpression.Operand);
                break;
            }

            case ExpressionType.Convert:
                resultTypeMapping = typeMapping;
                // Since we are applying convert, resultTypeMapping decides the clrType
                resultType = resultTypeMapping?.ClrType ?? sqlUnaryExpression.Type;
                operand = ApplyDefaultTypeMapping(sqlUnaryExpression.Operand);
                break;

            case ExpressionType.Not:
            case ExpressionType.Negate:
                resultTypeMapping = typeMapping;
                // While Not is logical, negate is numeric hence we use clrType from TypeMapping
                resultType = resultTypeMapping?.ClrType ?? sqlUnaryExpression.Type;
                operand = ApplyTypeMapping(sqlUnaryExpression.Operand, typeMapping);
                break;

            default:
                throw new InvalidOperationException(
                    CosmosStrings.UnsupportedOperatorForSqlExpression(
                        sqlUnaryExpression.OperatorType, typeof(SqlUnaryExpression).ShortDisplayName()));
        }

        return new SqlUnaryExpression(sqlUnaryExpression.OperatorType, operand, resultType, resultTypeMapping);
    }

    private SqlExpression ApplyTypeMappingOnSqlBinary(
        SqlBinaryExpression sqlBinaryExpression,
        CoreTypeMapping? typeMapping)
    {
        var left = sqlBinaryExpression.Left;
        var right = sqlBinaryExpression.Right;

        Type resultType;
        CoreTypeMapping? resultTypeMapping;
        CoreTypeMapping? inferredTypeMapping;
        switch (sqlBinaryExpression.OperatorType)
        {
            case ExpressionType.Equal:
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
            case ExpressionType.NotEqual:
            {
                inferredTypeMapping = ExpressionExtensions.InferTypeMapping(left, right)
                    // We avoid object here since the result does not get typeMapping from outside.
                    ?? (left.Type != typeof(object)
                        ? typeMappingSource.FindMapping(left.Type, model)
                        : typeMappingSource.FindMapping(right.Type, model));
                resultType = typeof(bool);
                resultTypeMapping = _boolTypeMapping;
                break;
            }

            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
            {
                inferredTypeMapping = _boolTypeMapping;
                resultType = typeof(bool);
                resultTypeMapping = _boolTypeMapping;
                break;
            }

            case ExpressionType.Add:
            case ExpressionType.Subtract:
            case ExpressionType.Multiply:
            case ExpressionType.Divide:
            case ExpressionType.Modulo:
            case ExpressionType.LeftShift:
            case ExpressionType.RightShift:
            case ExpressionType.And:
            case ExpressionType.Or:
            case ExpressionType.ExclusiveOr:
            case ExpressionType.Coalesce:
            {
                inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);
                resultType = inferredTypeMapping?.ClrType ?? (left.Type != typeof(object) ? left.Type : right.Type);
                resultTypeMapping = inferredTypeMapping;
                break;
            }

            case ExpressionType.ArrayIndex:
            {
                // TODO: This infers based on the CLR type; need to properly infer based on the element type mapping
                // TODO: being applied here (e.g. WHERE @p[1] = c.PropertyWithValueConverter). #34026
                var arrayTypeMapping = left.TypeMapping
                    ?? (typeMapping is null
                        ? null
                        : typeMappingSource.FindMapping(typeof(IEnumerable<>).MakeGenericType(typeMapping.ClrType)));
                return new SqlBinaryExpression(
                    ExpressionType.ArrayIndex,
                    ApplyTypeMapping(left, arrayTypeMapping),
                    ApplyDefaultTypeMapping(right),
                    sqlBinaryExpression.Type,
                    typeMapping ?? sqlBinaryExpression.TypeMapping);
            }

            default:
                throw new InvalidOperationException(
                    CosmosStrings.UnsupportedOperatorForSqlExpression(
                        sqlBinaryExpression.OperatorType, typeof(SqlBinaryExpression).ShortDisplayName()));
        }

        return new SqlBinaryExpression(
            sqlBinaryExpression.OperatorType,
            ApplyTypeMapping(left, inferredTypeMapping),
            ApplyTypeMapping(right, inferredTypeMapping),
            resultType,
            resultTypeMapping);
    }

    private InExpression ApplyTypeMappingOnIn(InExpression inExpression)
    {
        var missingTypeMappingInValues = false;

        CoreTypeMapping? valuesTypeMapping = null;
        switch (inExpression)
        {
            case { ValuesParameter: SqlParameterExpression parameter }:
                valuesTypeMapping = parameter.TypeMapping;
                break;

            case { Values: IReadOnlyList<SqlExpression> values }:
                // Note: there could be conflicting type mappings inside the values; we take the first.
                foreach (var value in values)
                {
                    if (value.TypeMapping is null)
                    {
                        missingTypeMappingInValues = true;
                    }
                    else
                    {
                        valuesTypeMapping = value.TypeMapping;
                    }
                }

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        var item = ApplyTypeMapping(
            inExpression.Item,
            valuesTypeMapping ?? typeMappingSource.FindMapping(inExpression.Item.Type, model));

        switch (inExpression)
        {
            case { ValuesParameter: SqlParameterExpression parameter }:
                inExpression = inExpression.Update(item, (SqlParameterExpression)ApplyTypeMapping(parameter, item.TypeMapping));
                break;

            case { Values: IReadOnlyList<SqlExpression> values }:
                SqlExpression[]? newValues = null;

                if (missingTypeMappingInValues)
                {
                    newValues = new SqlExpression[values.Count];

                    for (var i = 0; i < newValues.Length; i++)
                    {
                        newValues[i] = ApplyTypeMapping(values[i], item.TypeMapping);
                    }
                }

                inExpression = inExpression.Update(item, newValues ?? values);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return inExpression.TypeMapping == _boolTypeMapping
            ? inExpression
            : inExpression.ApplyTypeMapping(_boolTypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public (SqlExpression, SqlExpression) ApplyTypeMappingsOnItemAndArray(SqlExpression itemExpression, SqlExpression arrayExpression)
    {
        // Attempt type inference either from the operand to the array or the other way around
        var arrayMapping = arrayExpression.TypeMapping;

        var itemMapping =
            itemExpression.TypeMapping
            // Unwrap convert-to-object nodes - these get added for object[].Contains(x)
            ?? (itemExpression is SqlUnaryExpression { OperatorType: ExpressionType.Convert } unary && unary.Type == typeof(object)
                ? unary.Operand.TypeMapping
                : null)
            // If we couldn't find a type mapping on the item, try inferring it from the array
            ?? arrayMapping?.ElementTypeMapping
            ?? typeMappingSource.FindMapping(itemExpression.Type, model);

        if (itemMapping is null)
        {
            throw new InvalidOperationException("Couldn't find element type mapping when applying item/array mappings");
        }

        // If the array's type mapping isn't provided (parameter/constant), attempt to infer it from the item.
        if (arrayMapping is null)
        {
            // Get a type mapping for the array from the item.
            // If the array CLR type is anything but an object[], just use that CLR type.
            // For object[], where the type mapping wouldn't be fine, construct an array/List CLR type based on the
            // items' CLR type.
            var arrayClrType = arrayExpression.Type switch
            {
                var t when t.TryGetSequenceType() != typeof(object) => t,
                { IsArray: true } => typeof(IEnumerable<>).MakeGenericType(itemExpression.Type),
                { IsConstructedGenericType: true, GenericTypeArguments.Length: 1 } t
                    => t.GetGenericTypeDefinition().MakeGenericType(itemExpression.Type),
                _ => throw new InvalidOperationException(
                    $"Can't construct generic primitive collection type for array type '{arrayExpression.Type}'")
            };

            arrayMapping = typeMappingSource.FindMapping(arrayClrType, model, itemMapping.ElementTypeMapping);

            if (arrayMapping is null)
            {
                throw new InvalidOperationException("Couldn't find array type mapping when applying item/array mappings");
            }
        }

        return (ApplyTypeMapping(itemExpression, itemMapping), ApplyTypeMapping(arrayExpression, arrayMapping));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? MakeBinary(
        ExpressionType operatorType,
        SqlExpression left,
        SqlExpression right,
        CoreTypeMapping? typeMapping,
        SqlExpression? existingExpression = null)
    {
        switch (operatorType)
        {
            case ExpressionType.AndAlso:
                return ApplyTypeMapping(AndAlso(left, right, existingExpression), typeMapping);
            case ExpressionType.OrElse:
                return ApplyTypeMapping(OrElse(left, right, existingExpression), typeMapping);
        }

        if (!SqlBinaryExpression.IsValidOperator(operatorType))
        {
            return null;
        }

        var returnType = left.Type;
        switch (operatorType)
        {
            case ExpressionType.Equal:
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
            case ExpressionType.NotEqual:
                returnType = typeof(bool);
                break;
        }

        return (SqlBinaryExpression)ApplyTypeMapping(
            new SqlBinaryExpression(operatorType, left, right, returnType, null), typeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Equal(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.Equal, left, right, null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression NotEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.NotEqual, left, right, null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Exists(SelectExpression subquery)
        => new ExistsExpression(subquery, _boolTypeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression GreaterThan(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.GreaterThan, left, right, null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.GreaterThanOrEqual, left, right, null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression LessThan(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.LessThan, left, right, null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression LessThanOrEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.LessThanOrEqual, left, right, null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression AndAlso(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.AndAlso, left, right, null)!;

    private SqlExpression AndAlso(SqlExpression left, SqlExpression right, SqlExpression? existingExpression)
    {
        // false && x -> false
        // x && true -> x
        // x && x -> x
        if (left is SqlConstantExpression { Value: false }
            || right is SqlConstantExpression { Value: true }
            || left.Equals(right))
        {
            return left;
        }

        // true && x -> x
        // x && false -> false
        if (left is SqlConstantExpression { Value: true } || right is SqlConstantExpression { Value: false })
        {
            return right;
        }

        // x is null && x is not null -> false
        // x is not null && x is null -> false
        if (left is SqlUnaryExpression { OperatorType: ExpressionType.Equal or ExpressionType.NotEqual } leftUnary
            && right is SqlUnaryExpression { OperatorType: ExpressionType.Equal or ExpressionType.NotEqual } rightUnary
            && leftUnary.Operand.Equals(rightUnary.Operand))
        {
            // the case in which left and right are the same expression is handled above
            return Constant(false);
        }

        if (existingExpression is SqlBinaryExpression { OperatorType: ExpressionType.AndAlso } binaryExpr
            && left == binaryExpr.Left
            && right == binaryExpr.Right)
        {
            return existingExpression;
        }

        return new SqlBinaryExpression(ExpressionType.AndAlso, left, right, typeof(bool), null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression OrElse(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.OrElse, left, right, null)!;

    private SqlExpression OrElse(SqlExpression left, SqlExpression right, SqlExpression? existingExpression)
    {
        // true || x -> true
        // x || false -> x
        // x || x -> x
        if (left is SqlConstantExpression { Value: true }
            || right is SqlConstantExpression { Value: false }
            || left.Equals(right))
        {
            return left;
        }

        // false || x -> x
        // x || true -> true
        if (left is SqlConstantExpression { Value: false }
            || right is SqlConstantExpression { Value: true })
        {
            return right;
        }

        // x is null || x is not null -> true
        // x is not null || x is null -> true
        if (left is SqlUnaryExpression { OperatorType: ExpressionType.Equal or ExpressionType.NotEqual } leftUnary
            && right is SqlUnaryExpression { OperatorType: ExpressionType.Equal or ExpressionType.NotEqual } rightUnary
            && leftUnary.Operand.Equals(rightUnary.Operand))
        {
            // the case in which left and right are the same expression is handled above
            return Constant(true);
        }

        if (existingExpression is SqlBinaryExpression { OperatorType: ExpressionType.OrElse } binaryExpr
            && left == binaryExpr.Left
            && right == binaryExpr.Right)
        {
            return existingExpression;
        }

        return new SqlBinaryExpression(ExpressionType.OrElse, left, right, typeof(bool), null);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Add(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Add, left, right, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Subtract(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Subtract, left, right, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Multiply(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Multiply, left, right, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Divide(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Divide, left, right, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Modulo(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Modulo, left, right, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression And(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.And, left, right, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Or(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Or, left, right, typeMapping)!;

    private SqlUnaryExpression? MakeUnary(
        ExpressionType operatorType,
        SqlExpression operand,
        Type type,
        CoreTypeMapping? typeMapping = null)
        => SqlUnaryExpression.IsValidOperator(operatorType)
            ? (SqlUnaryExpression)ApplyTypeMapping(new SqlUnaryExpression(operatorType, operand, type, null), typeMapping)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression CoalesceUndefined(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Coalesce, left, right, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression IsNull(SqlExpression operand)
        => Equal(operand, Constant(null, operand.Type));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression IsNotNull(SqlExpression operand)
        => NotEqual(operand, Constant(null, operand.Type));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression ArrayIndex(SqlExpression left, SqlExpression right, Type type, CoreTypeMapping? typeMapping = null)
        => new SqlBinaryExpression(ExpressionType.ArrayIndex, left, right, type, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Convert(SqlExpression operand, Type type, CoreTypeMapping? typeMapping = null)
        => MakeUnary(ExpressionType.Convert, operand, type, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Not(SqlExpression operand)
        => MakeUnary(ExpressionType.Not, operand, operand.Type, operand.TypeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Negate(SqlExpression operand)
        => MakeUnary(ExpressionType.Negate, operand, operand.Type, operand.TypeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Function(
        string functionName,
        IEnumerable<Expression> arguments,
        Type returnType,
        CoreTypeMapping? typeMapping = null)
    {
        var typeMappedArguments = new List<Expression>();

        foreach (var argument in arguments)
        {
            typeMappedArguments.Add(argument is SqlExpression sqlArgument ? ApplyDefaultTypeMapping(sqlArgument) : argument);
        }

        return new SqlFunctionExpression(
            functionName,
            typeMappedArguments,
            returnType,
            typeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Condition(SqlExpression test, SqlExpression ifTrue, SqlExpression ifFalse)
    {
        var typeMapping = ExpressionExtensions.InferTypeMapping(ifTrue, ifFalse);

        return new SqlConditionalExpression(
            ApplyTypeMapping(test, _boolTypeMapping),
            ApplyTypeMapping(ifTrue, typeMapping),
            ApplyTypeMapping(ifFalse, typeMapping));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression In(SqlExpression item, IReadOnlyList<SqlExpression> values)
        => values is [var singleValue]
            ? Equal(item, singleValue)
            : ApplyTypeMappingOnIn(new InExpression(item, values, _boolTypeMapping));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression In(SqlExpression item, SqlParameterExpression valuesParameter)
        => ApplyTypeMappingOnIn(new InExpression(item, valuesParameter, _boolTypeMapping));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Constant(object value, CoreTypeMapping? typeMapping = null)
        => new SqlConstantExpression(value, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression Constant(object? value, Type type, CoreTypeMapping? typeMapping = null)
        => new SqlConstantExpression(value, type, typeMapping);
}
