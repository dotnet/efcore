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
            case ExpressionType.Coalesce:
            {
                inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);
                resultType = inferredTypeMapping?.ClrType ?? left.Type;
                resultTypeMapping = inferredTypeMapping;
                break;
            }

            case ExpressionType.ArrayIndex:
            {
                // TODO: This infers based on the CLR type; need to properly infer based on the element type mapping
                // TODO: being applied here (e.g. WHERE @p[1] = c.PropertyWithValueConverter)
                var arrayTypeMapping = left.TypeMapping
                    ?? (typeMapping is null ? null : typeMappingSource.FindMapping(typeMapping.ClrType.MakeArrayType()));
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
                { IsArray: true } => itemExpression.Type.MakeArrayType(),
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
    public virtual SqlBinaryExpression? MakeBinary(
        ExpressionType operatorType,
        SqlExpression left,
        SqlExpression right,
        CoreTypeMapping? typeMapping)
    {
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
            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
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
    public virtual SqlBinaryExpression Equal(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.Equal, left, right, null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression NotEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.NotEqual, left, right, null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ExistsExpression Exists(SelectExpression subquery)
        => new(subquery, _boolTypeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression GreaterThan(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.GreaterThan, left, right, null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.GreaterThanOrEqual, left, right, null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression LessThan(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.LessThan, left, right, null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression LessThanOrEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.LessThanOrEqual, left, right, null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression AndAlso(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.AndAlso, left, right, null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression OrElse(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.OrElse, left, right, null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression Add(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Add, left, right, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression Subtract(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Subtract, left, right, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression Multiply(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Multiply, left, right, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression Divide(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Divide, left, right, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression Modulo(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Modulo, left, right, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression And(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.And, left, right, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression Or(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
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
    public virtual SqlBinaryExpression IsNull(SqlExpression operand)
        => Equal(operand, Constant(null));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression IsNotNull(SqlExpression operand)
        => NotEqual(operand, Constant(null));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression ArrayIndex(SqlExpression left, SqlExpression right, Type type, CoreTypeMapping? typeMapping = null)
        => new(ExpressionType.ArrayIndex, left, right, type, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlUnaryExpression Convert(SqlExpression operand, Type type, CoreTypeMapping? typeMapping = null)
        => MakeUnary(ExpressionType.Convert, operand, type, typeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlUnaryExpression Not(SqlExpression operand)
        => MakeUnary(ExpressionType.Not, operand, operand.Type, operand.TypeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlUnaryExpression Negate(SqlExpression operand)
        => MakeUnary(ExpressionType.Negate, operand, operand.Type, operand.TypeMapping)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlFunctionExpression Function(
        string functionName,
        IEnumerable<SqlExpression> arguments,
        Type returnType,
        CoreTypeMapping? typeMapping = null)
    {
        var typeMappedArguments = new List<SqlExpression>();

        foreach (var argument in arguments)
        {
            typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
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
    public virtual SqlConditionalExpression Condition(SqlExpression test, SqlExpression ifTrue, SqlExpression ifFalse)
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
    public virtual InExpression In(SqlExpression item, IReadOnlyList<SqlExpression> values)
        => ApplyTypeMappingOnIn(new InExpression(item, values, _boolTypeMapping));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InExpression In(SqlExpression item, SqlParameterExpression valuesParameter)
        => ApplyTypeMappingOnIn(new InExpression(item, valuesParameter, _boolTypeMapping));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlConstantExpression Constant(object? value, CoreTypeMapping? typeMapping = null)
        => new(Expression.Constant(value), typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual FragmentExpression Fragment(string fragment)
        => new(fragment);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SelectExpression Select(IEntityType entityType)
    {
        var selectExpression = new SelectExpression(entityType);
        AddDiscriminator(selectExpression, entityType);

        return selectExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SelectExpression ReadItem(IEntityType entityType, ReadItemInfo readItemInfo)
    {
        var selectExpression = new SelectExpression(entityType, readItemInfo);
        AddDiscriminator(selectExpression, entityType);

        return selectExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SelectExpression Select(IEntityType entityType, string sql, Expression argument)
        => new(entityType, sql, argument);

    private void AddDiscriminator(SelectExpression selectExpression, IEntityType entityType)
    {
        var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToList();

        if (concreteEntityTypes.Count == 1)
        {
            var concreteEntityType = concreteEntityTypes[0];
            var discriminatorProperty = concreteEntityType.FindDiscriminatorProperty();
            if (discriminatorProperty != null)
            {
                var discriminatorColumn = ((EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember()))
                    .BindProperty(discriminatorProperty, clientEval: false);

                selectExpression.ApplyPredicate(
                    Equal((SqlExpression)discriminatorColumn, Constant(concreteEntityType.GetDiscriminatorValue())));
            }
        }
        else
        {
            var discriminatorProperty = concreteEntityTypes[0].FindDiscriminatorProperty();
            Check.DebugAssert(discriminatorProperty is not null, "Missing discriminator property in hierarchy");
            var discriminatorColumn = ((EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember()))
                .BindProperty(discriminatorProperty, clientEval: false);

            selectExpression.ApplyPredicate(
                In((SqlExpression)discriminatorColumn, concreteEntityTypes.Select(et => Constant(et.GetDiscriminatorValue())).ToArray()));
        }
    }
}
