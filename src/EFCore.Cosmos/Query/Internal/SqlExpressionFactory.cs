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
public class SqlExpressionFactory : ISqlExpressionFactory
{
    private readonly ITypeMappingSource _typeMappingSource;
    private readonly IModel _model;
    private readonly CoreTypeMapping _boolTypeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlExpressionFactory(ITypeMappingSource typeMappingSource, IModel model)
    {
        _typeMappingSource = typeMappingSource;
        _model = model;
        _boolTypeMapping = typeMappingSource.FindMapping(typeof(bool), model)!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull("sqlExpression")]
    public virtual SqlExpression? ApplyDefaultTypeMapping(SqlExpression? sqlExpression)
        => sqlExpression is not { TypeMapping: null }
            ? sqlExpression
            : ApplyTypeMapping(sqlExpression, _typeMappingSource.FindMapping(sqlExpression.Type, _model));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull("sqlExpression")]
    public virtual SqlExpression? ApplyTypeMapping(SqlExpression? sqlExpression, CoreTypeMapping? typeMapping)
        => sqlExpression switch
        {
            null or { TypeMapping: not null } => sqlExpression,

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
                        ? _typeMappingSource.FindMapping(left.Type, _model)
                        : _typeMappingSource.FindMapping(right.Type, _model));
                resultType = typeof(bool);
                resultTypeMapping = _boolTypeMapping;
            }
                break;

            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
            {
                inferredTypeMapping = _boolTypeMapping;
                resultType = typeof(bool);
                resultTypeMapping = _boolTypeMapping;
            }
                break;

            case ExpressionType.Add:
            case ExpressionType.Subtract:
            case ExpressionType.Multiply:
            case ExpressionType.Divide:
            case ExpressionType.Modulo:
            case ExpressionType.LeftShift:
            case ExpressionType.RightShift:
            case ExpressionType.And:
            case ExpressionType.Or:
            {
                inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);
                resultType = inferredTypeMapping?.ClrType ?? left.Type;
                resultTypeMapping = inferredTypeMapping;
            }
                break;

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
            valuesTypeMapping ?? _typeMappingSource.FindMapping(inExpression.Item.Type, _model));

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
    public virtual SqlBinaryExpression MakeBinary(
        ExpressionType operatorType,
        SqlExpression left,
        SqlExpression right,
        CoreTypeMapping? typeMapping)
    {
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
        => MakeBinary(ExpressionType.Equal, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression NotEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.NotEqual, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression GreaterThan(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.GreaterThan, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.GreaterThanOrEqual, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression LessThan(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.LessThan, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression LessThanOrEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.LessThanOrEqual, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression AndAlso(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.AndAlso, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression OrElse(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.OrElse, left, right, null);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression Add(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Add, left, right, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression Subtract(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Subtract, left, right, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression Multiply(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Multiply, left, right, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression Divide(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Divide, left, right, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression Modulo(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Modulo, left, right, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression And(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.And, left, right, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlBinaryExpression Or(SqlExpression left, SqlExpression right, CoreTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Or, left, right, typeMapping);

    private SqlUnaryExpression MakeUnary(
        ExpressionType operatorType,
        SqlExpression operand,
        Type type,
        CoreTypeMapping? typeMapping = null)
        => (SqlUnaryExpression)ApplyTypeMapping(new SqlUnaryExpression(operatorType, operand, type, null), typeMapping);

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
    public virtual SqlUnaryExpression Convert(SqlExpression operand, Type type, CoreTypeMapping? typeMapping = null)
        => MakeUnary(ExpressionType.Convert, operand, type, typeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlUnaryExpression Not(SqlExpression operand)
        => MakeUnary(ExpressionType.Not, operand, operand.Type, operand.TypeMapping);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlUnaryExpression Negate(SqlExpression operand)
        => MakeUnary(ExpressionType.Negate, operand, operand.Type, operand.TypeMapping);

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
            var discriminatorColumn = ((EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember()))
                .BindProperty(concreteEntityTypes[0].FindDiscriminatorProperty(), clientEval: false);

            selectExpression.ApplyPredicate(
                In((SqlExpression)discriminatorColumn, concreteEntityTypes.Select(et => Constant(et.GetDiscriminatorValue())).ToArray()));
        }
    }
}
