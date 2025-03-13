// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class SqlExpressionFactory : ISqlExpressionFactory
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly RelationalTypeMapping _boolTypeMapping;

    /// <summary>
    ///     Creates a new instance of the <see cref="SqlExpressionFactory" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    public SqlExpressionFactory(SqlExpressionFactoryDependencies dependencies)
    {
        Dependencies = dependencies;
        _typeMappingSource = dependencies.TypeMappingSource;
        _boolTypeMapping = _typeMappingSource.FindMapping(typeof(bool), dependencies.Model)!;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual SqlExpressionFactoryDependencies Dependencies { get; }

    /// <inheritdoc />
    [return: NotNullIfNotNull(nameof(sqlExpression))]
    public virtual SqlExpression? ApplyDefaultTypeMapping(SqlExpression? sqlExpression)
        => sqlExpression is not { TypeMapping: null }
            ? sqlExpression
            : sqlExpression is SqlUnaryExpression { OperatorType: ExpressionType.Convert } sqlUnaryExpression
            && sqlUnaryExpression.Type == typeof(object)
                ? sqlUnaryExpression.Operand
                : ApplyTypeMapping(
                    sqlExpression, _typeMappingSource.FindMapping(sqlExpression.Type, Dependencies.Model));

    /// <inheritdoc />
    [return: NotNullIfNotNull(nameof(sqlExpression))]
    public virtual SqlExpression? ApplyTypeMapping(SqlExpression? sqlExpression, RelationalTypeMapping? typeMapping)
        => sqlExpression switch
        {
            null or { TypeMapping: not null } => sqlExpression,

            AtTimeZoneExpression e => ApplyTypeMappingOnAtTimeZone(e, typeMapping),
            CaseExpression e => ApplyTypeMappingOnCase(e, typeMapping),
            CollateExpression e => ApplyTypeMappingOnCollate(e, typeMapping),
            ColumnExpression e => e.ApplyTypeMapping(typeMapping),
            DistinctExpression e => ApplyTypeMappingOnDistinct(e, typeMapping),
            InExpression e => ApplyTypeMappingOnIn(e),

            // We only do type inference for JSON scalar expression which represent a single array indexing operation; we can infer the
            // array's mapping from the element or vice versa, allowing e.g. parameter primitive collections to get inferred when an
            // an indexer is used over them and then compared to a column.
            // But we can't infer anything for other Path forms of JsonScalarExpression (e.g. a property lookup).
            JsonScalarExpression { Path: [{ ArrayIndex: not null }] } e => ApplyTypeMappingOnJsonScalar(e, typeMapping),

            LikeExpression e => ApplyTypeMappingOnLike(e),
            ScalarSubqueryExpression e => e.ApplyTypeMapping(typeMapping),
            SqlBinaryExpression e => ApplyTypeMappingOnSqlBinary(e, typeMapping),
            SqlConstantExpression e => e.ApplyTypeMapping(typeMapping),
            SqlFragmentExpression e => e,
            SqlFunctionExpression e => e.ApplyTypeMapping(typeMapping),
            SqlParameterExpression e => e.ApplyTypeMapping(typeMapping),
            SqlUnaryExpression e => ApplyTypeMappingOnSqlUnary(e, typeMapping),

            _ => sqlExpression
        };

    private SqlExpression ApplyTypeMappingOnAtTimeZone(AtTimeZoneExpression atTimeZoneExpression, RelationalTypeMapping? typeMapping)
        => new AtTimeZoneExpression(atTimeZoneExpression.Operand, atTimeZoneExpression.TimeZone, atTimeZoneExpression.Type, typeMapping);

    private SqlExpression ApplyTypeMappingOnLike(LikeExpression likeExpression)
    {
        var inferredTypeMapping = (likeExpression.EscapeChar == null
                ? ExpressionExtensions.InferTypeMapping(
                    likeExpression.Match, likeExpression.Pattern)
                : ExpressionExtensions.InferTypeMapping(
                    likeExpression.Match, likeExpression.Pattern, likeExpression.EscapeChar))
            ?? _typeMappingSource.FindMapping(likeExpression.Match.Type, Dependencies.Model);

        return new LikeExpression(
            ApplyTypeMapping(likeExpression.Match, inferredTypeMapping),
            ApplyTypeMapping(likeExpression.Pattern, inferredTypeMapping),
            ApplyTypeMapping(likeExpression.EscapeChar, inferredTypeMapping),
            _boolTypeMapping);
    }

    private SqlExpression ApplyTypeMappingOnCase(
        CaseExpression caseExpression,
        RelationalTypeMapping? typeMapping)
    {
        var whenClauses = new List<CaseWhenClause>();
        foreach (var caseWhenClause in caseExpression.WhenClauses)
        {
            whenClauses.Add(
                new CaseWhenClause(
                    caseWhenClause.Test,
                    ApplyTypeMapping(caseWhenClause.Result, typeMapping)));
        }

        var elseResult = ApplyTypeMapping(caseExpression.ElseResult, typeMapping);

        return caseExpression.Update(caseExpression.Operand, whenClauses, elseResult);
    }

    private SqlExpression ApplyTypeMappingOnCollate(
        CollateExpression collateExpression,
        RelationalTypeMapping? typeMapping)
        => collateExpression.Update(ApplyTypeMapping(collateExpression.Operand, typeMapping));

    private SqlExpression ApplyTypeMappingOnDistinct(
        DistinctExpression distinctExpression,
        RelationalTypeMapping? typeMapping)
        => distinctExpression.Update(ApplyTypeMapping(distinctExpression.Operand, typeMapping));

    private SqlExpression ApplyTypeMappingOnSqlUnary(
        SqlUnaryExpression sqlUnaryExpression,
        RelationalTypeMapping? typeMapping)
    {
        SqlExpression operand;
        Type resultType;
        RelationalTypeMapping? resultTypeMapping;
        switch (sqlUnaryExpression.OperatorType)
        {
            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
            case ExpressionType.Not
                when sqlUnaryExpression.Type == typeof(bool):
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
            case ExpressionType.OnesComplement:
                resultTypeMapping = typeMapping;
                // While Not is logical, negate is numeric hence we use clrType from TypeMapping
                resultType = resultTypeMapping?.ClrType ?? sqlUnaryExpression.Type;
                operand = ApplyTypeMapping(sqlUnaryExpression.Operand, typeMapping);
                break;

            default:
                throw new InvalidOperationException(
                    RelationalStrings.UnsupportedOperatorForSqlExpression(
                        sqlUnaryExpression.OperatorType, typeof(SqlUnaryExpression).ShortDisplayName()));
        }

        return new SqlUnaryExpression(sqlUnaryExpression.OperatorType, operand, resultType, resultTypeMapping);
    }

    private SqlExpression ApplyTypeMappingOnSqlBinary(
        SqlBinaryExpression sqlBinaryExpression,
        RelationalTypeMapping? typeMapping)
    {
        var left = sqlBinaryExpression.Left;
        var right = sqlBinaryExpression.Right;

        Type resultType;
        RelationalTypeMapping? resultTypeMapping;
        RelationalTypeMapping? inferredTypeMapping;
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
                    ?? _typeMappingSource.FindMapping(
                        left.Type != typeof(object) ? left.Type : right.Type,
                        Dependencies.Model);
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

            case ExpressionType.Add when IsForString(left.TypeMapping) || IsForString(right.TypeMapping):
                inferredTypeMapping = typeMapping;

                if (inferredTypeMapping is null)
                {
                    var leftTypeMapping = left.TypeMapping;
                    var rightTypeMapping = right.TypeMapping;
                    if (leftTypeMapping != null || rightTypeMapping != null)
                    {
                        // Infer null size (nvarchar(max)) if either side has no size.
                        // Note that for constants, we could instead look at the value length; but that requires we know the type mappings
                        // which can have a size (string/byte[], maybe something else?).
                        var inferredSize = leftTypeMapping?.Size is { } leftSize && rightTypeMapping?.Size is { } rightSize
                            ? leftSize + rightSize
                            : (int?)null;

                        // Unless both sides are fixed length, the result isn't fixed length.
                        var inferredFixedLength = leftTypeMapping?.IsFixedLength is true && rightTypeMapping?.IsFixedLength is true;

                        // Default to Unicode unless both sides are non-unicode.
                        var inferredUnicode = !(leftTypeMapping?.IsUnicode is false && rightTypeMapping?.IsUnicode is false);
                        var baseTypeMapping = leftTypeMapping ?? rightTypeMapping!;

                        inferredTypeMapping = leftTypeMapping?.Size == inferredSize
                            && leftTypeMapping?.IsFixedLength == inferredFixedLength
                            && leftTypeMapping?.IsUnicode == inferredUnicode
                                ? leftTypeMapping
                                : rightTypeMapping?.Size == inferredSize
                                && rightTypeMapping?.IsFixedLength == inferredFixedLength
                                && rightTypeMapping?.IsUnicode == inferredUnicode
                                    ? rightTypeMapping
                                    : _typeMappingSource.FindMapping(
                                        baseTypeMapping.ClrType,
                                        storeTypeName: null,
                                        keyOrIndex: false,
                                        inferredUnicode,
                                        inferredSize,
                                        rowVersion: false,
                                        inferredFixedLength,
                                        baseTypeMapping.Precision,
                                        baseTypeMapping.Scale);
                    }
                }

                resultType = inferredTypeMapping?.ClrType ?? left.Type;
                resultTypeMapping = inferredTypeMapping;
                break;

            case ExpressionType.Add:
            case ExpressionType.Subtract:
            case ExpressionType.Multiply:
            case ExpressionType.Divide:
            case ExpressionType.Modulo:
            case ExpressionType.And:
            case ExpressionType.Or:
            case ExpressionType.ExclusiveOr:
            {
                inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);
                resultType = inferredTypeMapping?.ClrType ?? (left.Type != typeof(object) ? left.Type : right.Type);
                resultTypeMapping = inferredTypeMapping;
                break;
            }

            default:
                throw new InvalidOperationException(
                    RelationalStrings.UnsupportedOperatorForSqlExpression(
                        sqlBinaryExpression.OperatorType, typeof(SqlBinaryExpression).ShortDisplayName()));
        }

        return new SqlBinaryExpression(
            sqlBinaryExpression.OperatorType,
            ApplyTypeMapping(left, inferredTypeMapping),
            ApplyTypeMapping(right, inferredTypeMapping),
            resultType,
            resultTypeMapping);

        static bool IsForString(RelationalTypeMapping? typeMapping)
            => (typeMapping?.Converter?.ProviderClrType ?? typeMapping?.ClrType) == typeof(string);
    }

    private InExpression ApplyTypeMappingOnIn(InExpression inExpression)
    {
        var missingTypeMappingInValues = false;

        RelationalTypeMapping? valuesTypeMapping = null;
        switch (inExpression)
        {
            case { Subquery: SelectExpression subquery }:
                valuesTypeMapping = subquery.Projection[0].Expression.TypeMapping;
                break;

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
            valuesTypeMapping ?? Dependencies.TypeMappingSource.FindMapping(inExpression.Item.Type, Dependencies.Model));

        switch (inExpression)
        {
            case { Subquery: SelectExpression subquery }:
                inExpression = inExpression.Update(item, subquery);
                break;

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

    private SqlExpression ApplyTypeMappingOnJsonScalar(
        JsonScalarExpression jsonScalarExpression,
        RelationalTypeMapping? elementMapping)
    {
        if (jsonScalarExpression is not { Json: var array, Path: [{ ArrayIndex: { } index }] })
        {
            return jsonScalarExpression;
        }

        // The index expression isn't inferred and is always just an int. Apply the default type mapping to it.
        var indexWithTypeMapping = ApplyDefaultTypeMapping(index);
        var newPath = indexWithTypeMapping == index ? jsonScalarExpression.Path : [new PathSegment(indexWithTypeMapping)];

        // If a type mapping is being applied from the outside, it applies to the element resulting from the array indexing operation;
        // we can infer the array's type mapping from it.
        if (elementMapping is null)
        {
            return new JsonScalarExpression(
                array,
                newPath,
                jsonScalarExpression.Type,
                jsonScalarExpression.TypeMapping,
                jsonScalarExpression.IsNullable);
        }

        // Resolve the array type mapping for the given element mapping.
        if (_typeMappingSource.FindMapping(array.Type, Dependencies.Model, elementMapping) is not RelationalTypeMapping arrayMapping)
        {
            throw new UnreachableException($"Couldn't find collection type mapping for element type mapping {elementMapping.ClrType.Name}");
        }

        return new JsonScalarExpression(
            ApplyTypeMapping(array, arrayMapping),
            newPath,
            jsonScalarExpression.Type,
            elementMapping,
            jsonScalarExpression.IsNullable);
    }

    /// <inheritdoc />
    public virtual SqlExpression? MakeBinary(
        ExpressionType operatorType,
        SqlExpression left,
        SqlExpression right,
        RelationalTypeMapping? typeMapping,
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

        return ApplyTypeMapping(
            new SqlBinaryExpression(operatorType, left, right, returnType, null), typeMapping);
    }

    /// <inheritdoc />
    public virtual SqlExpression Equal(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.Equal, left, right, null)!;

    /// <inheritdoc />
    public virtual SqlExpression NotEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.NotEqual, left, right, null)!;

    /// <inheritdoc />
    public virtual SqlExpression GreaterThan(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.GreaterThan, left, right, null)!;

    /// <inheritdoc />
    public virtual SqlExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.GreaterThanOrEqual, left, right, null)!;

    /// <inheritdoc />
    public virtual SqlExpression LessThan(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.LessThan, left, right, null)!;

    /// <inheritdoc />
    public virtual SqlExpression LessThanOrEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.LessThanOrEqual, left, right, null)!;

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual SqlExpression Add(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Add, left, right, typeMapping)!;

    /// <inheritdoc />
    public virtual SqlExpression Subtract(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Subtract, left, right, typeMapping)!;

    /// <inheritdoc />
    public virtual SqlExpression Multiply(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Multiply, left, right, typeMapping)!;

    /// <inheritdoc />
    public virtual SqlExpression Divide(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Divide, left, right, typeMapping)!;

    /// <inheritdoc />
    public virtual SqlExpression Modulo(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Modulo, left, right, typeMapping)!;

    /// <inheritdoc />
    public virtual SqlExpression And(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.And, left, right, typeMapping)!;

    /// <inheritdoc />
    public virtual SqlExpression Or(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Or, left, right, typeMapping)!;

    /// <inheritdoc />
    public virtual SqlExpression Coalesce(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
    {
        var resultType = right.Type;
        var inferredTypeMapping = typeMapping
            ?? ExpressionExtensions.InferTypeMapping(left, right)
            ?? _typeMappingSource.FindMapping(resultType, Dependencies.Model);

        left = ApplyTypeMapping(left, inferredTypeMapping);
        right = ApplyTypeMapping(right, inferredTypeMapping);

        return left switch
        {
            SqlConstantExpression { Value: null } => right,

            SqlConstantExpression { Value: not null } or
                ColumnExpression { IsNullable: false } => left,

            _ => new SqlFunctionExpression(
                "COALESCE",
                [left, right],
                nullable: true,
                // COALESCE is handled separately since it's only nullable if *all* arguments are null
                argumentsPropagateNullability: Statics.FalseArrays[2],
                resultType,
                inferredTypeMapping)
        };
    }

    /// <inheritdoc />
    public virtual SqlExpression? MakeUnary(
        ExpressionType operatorType,
        SqlExpression operand,
        Type type,
        RelationalTypeMapping? typeMapping = null,
        SqlExpression? existingExpression = null)
        => operatorType switch
        {
            ExpressionType.Not => ApplyTypeMapping(Not(operand, existingExpression), typeMapping),
            _ when SqlUnaryExpression.IsValidOperator(operatorType)
                => ApplyTypeMapping(new SqlUnaryExpression(operatorType, operand, type, null), typeMapping),
            _ => null,
        };

    /// <inheritdoc />
    public virtual SqlExpression IsNull(SqlExpression operand)
        => MakeUnary(ExpressionType.Equal, operand, typeof(bool))!;

    /// <inheritdoc />
    public virtual SqlExpression IsNotNull(SqlExpression operand)
        => MakeUnary(ExpressionType.NotEqual, operand, typeof(bool))!;

    /// <inheritdoc />
    public virtual SqlExpression Convert(SqlExpression operand, Type type, RelationalTypeMapping? typeMapping = null)
        => MakeUnary(ExpressionType.Convert, operand, type.UnwrapNullableType(), typeMapping)!;

    /// <inheritdoc />
    public virtual SqlExpression Not(SqlExpression operand)
        => MakeUnary(ExpressionType.Not, operand, operand.Type, operand.TypeMapping)!;

    private SqlExpression Not(SqlExpression operand, SqlExpression? existingExpression)
        => operand switch
        {
            // !(null) -> null
            // ~(null) -> null (bitwise negation)
            SqlConstantExpression { Value: null } => operand,

            // !(true) -> false
            // !(false) -> true
            SqlConstantExpression { Value: bool boolValue } => Constant(!boolValue, operand.Type, operand.TypeMapping),

            // !(!a) -> a
            // ~(~a) -> a (bitwise negation)
            SqlUnaryExpression { OperatorType: ExpressionType.Not } unary => unary.Operand,

            // !(a IS NULL) -> a IS NOT NULL
            SqlUnaryExpression { OperatorType: ExpressionType.Equal } unary => IsNotNull(unary.Operand),

            // !(a IS NOT NULL) -> a IS NULL
            SqlUnaryExpression { OperatorType: ExpressionType.NotEqual } unary => IsNull(unary.Operand),

            // !(a AND b) -> !a OR !b (De Morgan)
            SqlBinaryExpression { OperatorType: ExpressionType.AndAlso } binary
                => OrElse(Not(binary.Left), Not(binary.Right)),

            // !(a OR b) -> !a AND !b (De Morgan)
            SqlBinaryExpression { OperatorType: ExpressionType.OrElse } binary
                => AndAlso(Not(binary.Left), Not(binary.Right)),

            SqlBinaryExpression
            {
                OperatorType: ExpressionType.Equal,
                Right: SqlConstantExpression { Value: bool },
                Left: SqlConstantExpression { Value: bool }
                    or SqlParameterExpression { IsNullable: false }
                    or ColumnExpression { IsNullable: false }
            } binary
                => Equal(binary.Left, Not(binary.Right)),

            SqlBinaryExpression
            {
                OperatorType: ExpressionType.Equal,
                Left: SqlConstantExpression { Value: bool },
                Right: SqlConstantExpression { Value: bool }
                    or SqlParameterExpression { IsNullable: false }
                    or ColumnExpression { IsNullable: false }
            } binary
                => Equal(Not(binary.Left), binary.Right),

            // !(a == b) -> a != b
            SqlBinaryExpression { OperatorType: ExpressionType.Equal } sqlBinaryOperand => NotEqual(
                sqlBinaryOperand.Left, sqlBinaryOperand.Right),

            // !(a != b) -> a == b
            SqlBinaryExpression { OperatorType: ExpressionType.NotEqual } sqlBinaryOperand => Equal(
                sqlBinaryOperand.Left, sqlBinaryOperand.Right),

            // !(CASE x WHEN t1 THEN r1 ... ELSE rN) -> CASE x WHEN t1 THEN !r1 ... ELSE !rN
            CaseExpression caseExpression
                when caseExpression.Type == typeof(bool)
                && caseExpression.ElseResult is null or SqlConstantExpression
                && caseExpression.WhenClauses.All(clause => clause.Result is SqlConstantExpression)
                => Case(
                    caseExpression.Operand,
                    [.. caseExpression.WhenClauses.Select(clause => new CaseWhenClause(clause.Test, Not(clause.Result)))],
                    caseExpression.ElseResult is null ? null : Not(caseExpression.ElseResult)),

            _ => existingExpression is SqlUnaryExpression { OperatorType: ExpressionType.Not } unaryExpr && unaryExpr.Operand == operand
                ? existingExpression
                : new SqlUnaryExpression(ExpressionType.Not, operand, operand.Type, null),
        };

    /// <inheritdoc />
    public virtual SqlExpression Negate(SqlExpression operand)
        => MakeUnary(ExpressionType.Negate, operand, operand.Type, operand.TypeMapping)!;

    /// <inheritdoc />
    public virtual SqlExpression Case(
        SqlExpression? operand,
        IReadOnlyList<CaseWhenClause> whenClauses,
        SqlExpression? elseResult,
        SqlExpression? existingExpression = null)
    {
        RelationalTypeMapping? testTypeMapping;
        if (operand == null)
        {
            testTypeMapping = _boolTypeMapping;
        }
        else
        {
            testTypeMapping = operand.TypeMapping
                ?? whenClauses.Select(wc => wc.Test.TypeMapping).FirstOrDefault(t => t != null)
                // Since we never look at type of Operand/Test after this place,
                // we need to find actual typeMapping based on non-object type.
                ?? new[] { operand.Type }.Concat(whenClauses.Select(wc => wc.Test.Type))
                    .Where(t => t != typeof(object)).Select(t => _typeMappingSource.FindMapping(t, Dependencies.Model))
                    .FirstOrDefault();

            operand = ApplyTypeMapping(operand, testTypeMapping);
        }

        var resultTypeMapping = elseResult?.TypeMapping
            ?? whenClauses.Select(wc => wc.Result.TypeMapping).FirstOrDefault(t => t != null);

        elseResult = ApplyTypeMapping(elseResult, resultTypeMapping);

        var typeMappedWhenClauses = new List<CaseWhenClause>();
        foreach (var caseWhenClause in whenClauses)
        {
            var test = caseWhenClause.Test;

            if (operand == null && test is CaseExpression { Operand: null, WhenClauses: [var nestedSingleClause] } testExpr)
            {
                if (nestedSingleClause.Result is SqlConstantExpression { Value: true }
                    && testExpr.ElseResult is null or SqlConstantExpression { Value: false or null })
                {
                    // WHEN CASE
                    //   WHEN x THEN TRUE
                    //   ELSE FALSE/NULL
                    // END THEN y
                    // simplifies to
                    // WHEN x THEN y
                    test = nestedSingleClause.Test;
                }
                else if (nestedSingleClause.Result is SqlConstantExpression { Value: false or null }
                         && testExpr.ElseResult is SqlConstantExpression { Value: true })
                {
                    // same for the negated results
                    test = Not(nestedSingleClause.Test);
                }
            }

            typeMappedWhenClauses.Add(
                new CaseWhenClause(
                    ApplyTypeMapping(test, testTypeMapping),
                    ApplyTypeMapping(caseWhenClause.Result, resultTypeMapping)));
        }

        if (operand is null && elseResult is CaseExpression { Operand: null } nestedCaseExpression)
        {
            typeMappedWhenClauses.AddRange(nestedCaseExpression.WhenClauses);
            elseResult = nestedCaseExpression.ElseResult;
        }

        typeMappedWhenClauses = typeMappedWhenClauses
            .Where(c => !IsSkipped(c))
            .TakeUpTo(IsMatched)
            .DistinctBy(c => c.Test)
            .ToList();

        // CASE
        //   ...
        //   WHEN TRUE THEN a
        //   ELSE b
        // END
        // simplifies to
        // CASE
        //   ...
        //   ELSE a
        // END
        if (typeMappedWhenClauses.Count > 0 && IsMatched(typeMappedWhenClauses[^1]))
        {
            elseResult = typeMappedWhenClauses[^1].Result;
            typeMappedWhenClauses.RemoveAt(typeMappedWhenClauses.Count - 1);
        }

        var nullResult = Constant(null, elseResult?.Type ?? whenClauses[0].Result.Type, resultTypeMapping);

        // if there are no whenClauses left (e.g. their tests evaluated to false):
        // - if there is Else block, return it
        // - if there is no Else block, return null
        if (typeMappedWhenClauses.Count == 0)
        {
            return elseResult ?? nullResult;
        }

        // omit `ELSE NULL` (this makes it easier to compare/reuse expressions)
        if (elseResult is SqlConstantExpression { Value: null })
        {
            elseResult = null;
        }

        // CASE
        //   ...
        //   WHEN x THEN CASE
        //     WHEN y THEN a
        //     ELSE b
        //   END
        //   ELSE b
        // END
        // simplifies to
        // CASE
        //   ...
        //   WHEN x AND y THEN a
        //   ELSE b
        // END
        if (operand == null
            && typeMappedWhenClauses[^1].Result is CaseExpression { Operand: null, WhenClauses: [var lastClause] } lastCase
            && Equals(elseResult, lastCase.ElseResult))
        {
            typeMappedWhenClauses[^1] = new CaseWhenClause(AndAlso(typeMappedWhenClauses[^1].Test, lastClause.Test), lastClause.Result);
            elseResult = lastCase.ElseResult;
        }

        return existingExpression is CaseExpression expr
            && operand == expr.Operand
            && typeMappedWhenClauses.SequenceEqual(expr.WhenClauses)
            && elseResult == expr.ElseResult
                ? expr
                : new CaseExpression(operand, typeMappedWhenClauses, elseResult);

        bool IsSkipped(CaseWhenClause clause)
            => operand is null && clause.Test is SqlConstantExpression { Value: false or null };

        bool IsMatched(CaseWhenClause clause)
            => operand is null && clause.Test is SqlConstantExpression { Value: true };
    }

    /// <inheritdoc />
    public virtual SqlExpression Case(IReadOnlyList<CaseWhenClause> whenClauses, SqlExpression? elseResult)
        => Case(operand: null, whenClauses, elseResult);

    /// <inheritdoc />
    public virtual SqlExpression Function(
        string name,
        IEnumerable<SqlExpression> arguments,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
    {
        var typeMappedArguments = new List<SqlExpression>();

        foreach (var argument in arguments)
        {
            typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
        }

        return new SqlFunctionExpression(name, typeMappedArguments, nullable, argumentsPropagateNullability, returnType, typeMapping);
    }

    /// <inheritdoc />
    public virtual SqlExpression Function(
        string? schema,
        string name,
        IEnumerable<SqlExpression> arguments,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
    {
        var typeMappedArguments = new List<SqlExpression>();
        foreach (var argument in arguments)
        {
            typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
        }

        return new SqlFunctionExpression(
            schema, name, typeMappedArguments, nullable, argumentsPropagateNullability, returnType, typeMapping);
    }

    /// <inheritdoc />
    public virtual SqlExpression Function(
        SqlExpression instance,
        string name,
        IEnumerable<SqlExpression> arguments,
        bool nullable,
        bool instancePropagatesNullability,
        IEnumerable<bool> argumentsPropagateNullability,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
    {
        instance = ApplyDefaultTypeMapping(instance);
        var typeMappedArguments = new List<SqlExpression>();
        foreach (var argument in arguments)
        {
            typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
        }

        return new SqlFunctionExpression(
            instance, name, typeMappedArguments, nullable, instancePropagatesNullability, argumentsPropagateNullability, returnType,
            typeMapping);
    }

    /// <inheritdoc />
    public virtual SqlExpression NiladicFunction(
        string name,
        bool nullable,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
        => new SqlFunctionExpression(name, nullable, returnType, typeMapping);

    /// <inheritdoc />
    public virtual SqlExpression NiladicFunction(
        string schema,
        string name,
        bool nullable,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
        => new SqlFunctionExpression(schema, name, nullable, returnType, typeMapping);

    /// <inheritdoc />
    public virtual SqlExpression NiladicFunction(
        SqlExpression instance,
        string name,
        bool nullable,
        bool instancePropagatesNullability,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
        => new SqlFunctionExpression(
            ApplyDefaultTypeMapping(instance), name, nullable, instancePropagatesNullability, returnType, typeMapping);

    /// <inheritdoc />
    public virtual SqlExpression Exists(SelectExpression subquery)
        => new ExistsExpression(subquery, _boolTypeMapping);

    /// <inheritdoc />
    public virtual SqlExpression In(SqlExpression item, SelectExpression subquery)
        => ApplyTypeMappingOnIn(new InExpression(item, subquery, _boolTypeMapping));

    /// <inheritdoc />
    public virtual SqlExpression In(SqlExpression item, IReadOnlyList<SqlExpression> values)
        => values is [var singleValue]
            ? Equal(item, singleValue)
            : ApplyTypeMappingOnIn(new InExpression(item, values, _boolTypeMapping));

    /// <inheritdoc />
    public virtual SqlExpression In(SqlExpression item, SqlParameterExpression valuesParameter)
        => ApplyTypeMappingOnIn(new InExpression(item, valuesParameter, _boolTypeMapping));

    /// <inheritdoc />
    public virtual SqlExpression Like(SqlExpression match, SqlExpression pattern, SqlExpression? escapeChar = null)
        => ApplyDefaultTypeMapping(new LikeExpression(match, pattern, escapeChar, null));

    /// <inheritdoc />
    public virtual SqlExpression Fragment(string sql, Type? type = null, RelationalTypeMapping? typeMapping = null)
        => new SqlFragmentExpression(sql, type, typeMapping);

    /// <inheritdoc />
    public virtual SqlExpression Constant(object value, RelationalTypeMapping? typeMapping = null)
        => new SqlConstantExpression(value, typeMapping);

    /// <inheritdoc />
    public virtual SqlExpression Constant(object? value, Type type, RelationalTypeMapping? typeMapping = null)
        => new SqlConstantExpression(value, type, typeMapping);

    /// <inheritdoc />
    public virtual SqlExpression Constant(object value, bool sensitive, RelationalTypeMapping? typeMapping = null)
        => new SqlConstantExpression(value, sensitive, typeMapping);

    /// <inheritdoc />
    public virtual SqlExpression Constant(object? value, Type type, bool sensitive, RelationalTypeMapping? typeMapping = null)
        => new SqlConstantExpression(value, type, sensitive, typeMapping);
}
