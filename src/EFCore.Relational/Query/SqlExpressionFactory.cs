// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
    [return: NotNullIfNotNull("sqlExpression")]
    public virtual SqlExpression? ApplyDefaultTypeMapping(SqlExpression? sqlExpression)
        => sqlExpression == null
            || sqlExpression.TypeMapping != null
                ? sqlExpression
                : sqlExpression is SqlUnaryExpression sqlUnaryExpression
                && sqlUnaryExpression.OperatorType == ExpressionType.Convert
                && sqlUnaryExpression.Type == typeof(object)
                    ? sqlUnaryExpression.Operand
                    : ApplyTypeMapping(
                        sqlExpression, _typeMappingSource.FindMapping(sqlExpression.Type, Dependencies.Model));

    /// <inheritdoc />
    [return: NotNullIfNotNull("sqlExpression")]
    public virtual SqlExpression? ApplyTypeMapping(SqlExpression? sqlExpression, RelationalTypeMapping? typeMapping)
    {
#pragma warning disable IDE0046 // Convert to conditional expression
        if (sqlExpression == null
#pragma warning restore IDE0046 // Convert to conditional expression
            || sqlExpression.TypeMapping != null)
        {
            return sqlExpression;
        }

        return sqlExpression switch
        {
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
    }

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

            case ExpressionType.Add:
            case ExpressionType.Subtract:
            case ExpressionType.Multiply:
            case ExpressionType.Divide:
            case ExpressionType.Modulo:
            case ExpressionType.And:
            case ExpressionType.Or:
            {
                inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);
                resultType = inferredTypeMapping?.ClrType ?? left.Type;
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
    }

    private SqlExpression ApplyTypeMappingOnIn(InExpression inExpression)
    {
        var itemTypeMapping = (inExpression.Values != null
                ? ExpressionExtensions.InferTypeMapping(inExpression.Item, inExpression.Values)
                : inExpression.Subquery != null
                    ? ExpressionExtensions.InferTypeMapping(inExpression.Item, inExpression.Subquery.Projection[0].Expression)
                    : inExpression.Item.TypeMapping)
            ?? _typeMappingSource.FindMapping(inExpression.Item.Type, Dependencies.Model);

        var item = ApplyTypeMapping(inExpression.Item, itemTypeMapping);
        if (inExpression.Values != null)
        {
            var values = ApplyTypeMapping(inExpression.Values, itemTypeMapping);

            return item != inExpression.Item || values != inExpression.Values || inExpression.TypeMapping != _boolTypeMapping
                ? new InExpression(item, values, inExpression.IsNegated, _boolTypeMapping)
                : inExpression;
        }

        return item != inExpression.Item || inExpression.TypeMapping != _boolTypeMapping
            ? new InExpression(item, inExpression.Subquery!, inExpression.IsNegated, _boolTypeMapping)
            : inExpression;
    }

    private SqlExpression ApplyTypeMappingOnJsonScalar(
        JsonScalarExpression jsonScalarExpression,
        RelationalTypeMapping? typeMapping)
    {
        if (jsonScalarExpression is not { Json: var array, Path: [{ ArrayIndex: { } index }] })
        {
            return jsonScalarExpression;
        }

        // The index expression isn't inferred and is always just an int. Apply the default type mapping to it.
        var indexWithTypeMapping = ApplyDefaultTypeMapping(index);
        var newPath = indexWithTypeMapping == index ? jsonScalarExpression.Path : new[] { new PathSegment(indexWithTypeMapping) };

        // If a type mapping is being applied from the outside, it applies to the element resulting from the array indexing operation;
        // we can infer the array's type mapping from it. Otherwise there's nothing to do but apply the default type mapping to the array.
        if (typeMapping is null)
        {
            return new JsonScalarExpression(
                ApplyDefaultTypeMapping(array),
                newPath,
                jsonScalarExpression.Type,
                _typeMappingSource.FindMapping(jsonScalarExpression.Type),
                jsonScalarExpression.IsNullable);
        }

        // TODO: blocked on #30730: we need to be able to construct a JSON collection type mapping based on the element's.
        // For now, hacking to apply the default type mapping instead.
        return new JsonScalarExpression(
            ApplyDefaultTypeMapping(array), // Hack, until #30730
            newPath,
            jsonScalarExpression.Type,
            typeMapping,
            jsonScalarExpression.IsNullable);
    }

    /// <inheritdoc />
    public virtual SqlBinaryExpression? MakeBinary(
        ExpressionType operatorType,
        SqlExpression left,
        SqlExpression right,
        RelationalTypeMapping? typeMapping)
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

    /// <inheritdoc />
    public virtual SqlBinaryExpression Equal(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.Equal, left, right, null)!;

    /// <inheritdoc />
    public virtual SqlBinaryExpression NotEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.NotEqual, left, right, null)!;

    /// <inheritdoc />
    public virtual SqlBinaryExpression GreaterThan(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.GreaterThan, left, right, null)!;

    /// <inheritdoc />
    public virtual SqlBinaryExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.GreaterThanOrEqual, left, right, null)!;

    /// <inheritdoc />
    public virtual SqlBinaryExpression LessThan(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.LessThan, left, right, null)!;

    /// <inheritdoc />
    public virtual SqlBinaryExpression LessThanOrEqual(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.LessThanOrEqual, left, right, null)!;

    /// <inheritdoc />
    public virtual SqlBinaryExpression AndAlso(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.AndAlso, left, right, null)!;

    /// <inheritdoc />
    public virtual SqlBinaryExpression OrElse(SqlExpression left, SqlExpression right)
        => MakeBinary(ExpressionType.OrElse, left, right, null)!;

    /// <inheritdoc />
    public virtual SqlBinaryExpression Add(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Add, left, right, typeMapping)!;

    /// <inheritdoc />
    public virtual SqlBinaryExpression Subtract(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Subtract, left, right, typeMapping)!;

    /// <inheritdoc />
    public virtual SqlBinaryExpression Multiply(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Multiply, left, right, typeMapping)!;

    /// <inheritdoc />
    public virtual SqlBinaryExpression Divide(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Divide, left, right, typeMapping)!;

    /// <inheritdoc />
    public virtual SqlBinaryExpression Modulo(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Modulo, left, right, typeMapping)!;

    /// <inheritdoc />
    public virtual SqlBinaryExpression And(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.And, left, right, typeMapping)!;

    /// <inheritdoc />
    public virtual SqlBinaryExpression Or(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
        => MakeBinary(ExpressionType.Or, left, right, typeMapping)!;

    /// <inheritdoc />
    public virtual SqlFunctionExpression Coalesce(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null)
    {
        var resultType = right.Type;
        var inferredTypeMapping = typeMapping
            ?? ExpressionExtensions.InferTypeMapping(left, right)
            ?? _typeMappingSource.FindMapping(resultType, Dependencies.Model);

        var typeMappedArguments = new List<SqlExpression>
        {
            ApplyTypeMapping(left, inferredTypeMapping), ApplyTypeMapping(right, inferredTypeMapping)
        };

        return new SqlFunctionExpression(
            "COALESCE",
            typeMappedArguments,
            nullable: true,
            // COALESCE is handled separately since it's only nullable if *all* arguments are null
            argumentsPropagateNullability: new[] { false, false },
            resultType,
            inferredTypeMapping);
    }

    /// <inheritdoc />
    public virtual SqlUnaryExpression? MakeUnary(
        ExpressionType operatorType,
        SqlExpression operand,
        Type type,
        RelationalTypeMapping? typeMapping = null)
        => !SqlUnaryExpression.IsValidOperator(operatorType)
            ? null
            : (SqlUnaryExpression)ApplyTypeMapping(new SqlUnaryExpression(operatorType, operand, type, null), typeMapping);

    /// <inheritdoc />
    public virtual SqlUnaryExpression IsNull(SqlExpression operand)
        => MakeUnary(ExpressionType.Equal, operand, typeof(bool))!;

    /// <inheritdoc />
    public virtual SqlUnaryExpression IsNotNull(SqlExpression operand)
        => MakeUnary(ExpressionType.NotEqual, operand, typeof(bool))!;

    /// <inheritdoc />
    public virtual SqlUnaryExpression Convert(SqlExpression operand, Type type, RelationalTypeMapping? typeMapping = null)
        => MakeUnary(ExpressionType.Convert, operand, type.UnwrapNullableType(), typeMapping)!;

    /// <inheritdoc />
    public virtual SqlUnaryExpression Not(SqlExpression operand)
        => MakeUnary(ExpressionType.Not, operand, operand.Type, operand.TypeMapping)!;

    /// <inheritdoc />
    public virtual SqlUnaryExpression Negate(SqlExpression operand)
        => MakeUnary(ExpressionType.Negate, operand, operand.Type, operand.TypeMapping)!;

    /// <inheritdoc />
    public virtual CaseExpression Case(SqlExpression? operand, IReadOnlyList<CaseWhenClause> whenClauses, SqlExpression? elseResult)
    {
        var operandTypeMapping = operand!.TypeMapping
            ?? whenClauses.Select(wc => wc.Test.TypeMapping).FirstOrDefault(t => t != null)
            // Since we never look at type of Operand/Test after this place,
            // we need to find actual typeMapping based on non-object type.
            ?? new[] { operand.Type }.Concat(whenClauses.Select(wc => wc.Test.Type))
                .Where(t => t != typeof(object)).Select(t => _typeMappingSource.FindMapping(t, Dependencies.Model))
                .FirstOrDefault();

        var resultTypeMapping = elseResult?.TypeMapping
            ?? whenClauses.Select(wc => wc.Result.TypeMapping).FirstOrDefault(t => t != null);

        operand = ApplyTypeMapping(operand, operandTypeMapping);
        elseResult = ApplyTypeMapping(elseResult, resultTypeMapping);

        var typeMappedWhenClauses = new List<CaseWhenClause>();
        foreach (var caseWhenClause in whenClauses)
        {
            typeMappedWhenClauses.Add(
                new CaseWhenClause(
                    ApplyTypeMapping(caseWhenClause.Test, operandTypeMapping),
                    ApplyTypeMapping(caseWhenClause.Result, resultTypeMapping)));
        }

        return new CaseExpression(operand, typeMappedWhenClauses, elseResult);
    }

    /// <inheritdoc />
    public virtual CaseExpression Case(IReadOnlyList<CaseWhenClause> whenClauses, SqlExpression? elseResult)
    {
        var resultTypeMapping = elseResult?.TypeMapping
            ?? whenClauses.Select(wc => wc.Result.TypeMapping).FirstOrDefault(t => t != null);

        var typeMappedWhenClauses = new List<CaseWhenClause>();
        foreach (var caseWhenClause in whenClauses)
        {
            typeMappedWhenClauses.Add(
                new CaseWhenClause(
                    ApplyTypeMapping(caseWhenClause.Test, _boolTypeMapping),
                    ApplyTypeMapping(caseWhenClause.Result, resultTypeMapping)));
        }

        elseResult = ApplyTypeMapping(elseResult, resultTypeMapping);

        return new CaseExpression(typeMappedWhenClauses, elseResult);
    }

    /// <inheritdoc />
    public virtual SqlFunctionExpression Function(
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
    public virtual SqlFunctionExpression Function(
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
    public virtual SqlFunctionExpression Function(
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
    public virtual SqlFunctionExpression NiladicFunction(
        string name,
        bool nullable,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
        => new(name, nullable, returnType, typeMapping);

    /// <inheritdoc />
    public virtual SqlFunctionExpression NiladicFunction(
        string schema,
        string name,
        bool nullable,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
        => new(schema, name, nullable, returnType, typeMapping);

    /// <inheritdoc />
    public virtual SqlFunctionExpression NiladicFunction(
        SqlExpression instance,
        string name,
        bool nullable,
        bool instancePropagatesNullability,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
        => new(
            ApplyDefaultTypeMapping(instance), name, nullable, instancePropagatesNullability, returnType, typeMapping);

    /// <inheritdoc />
    public virtual ExistsExpression Exists(SelectExpression subquery, bool negated)
        => new(subquery, negated, _boolTypeMapping);

    /// <inheritdoc />
    public virtual InExpression In(SqlExpression item, SqlExpression values, bool negated)
    {
        var typeMapping = item.TypeMapping ?? _typeMappingSource.FindMapping(item.Type, Dependencies.Model);

        item = ApplyTypeMapping(item, typeMapping);
        values = ApplyTypeMapping(values, typeMapping);

        return new InExpression(item, values, negated, _boolTypeMapping);
    }

    /// <inheritdoc />
    public virtual InExpression In(SqlExpression item, SelectExpression subquery, bool negated)
    {
        var sqlExpression = subquery.Projection.Single().Expression;
        var typeMapping = sqlExpression.TypeMapping;

        item = ApplyTypeMapping(item, typeMapping);
        return new InExpression(item, subquery, negated, _boolTypeMapping);
    }

    /// <inheritdoc />
    public virtual LikeExpression Like(SqlExpression match, SqlExpression pattern, SqlExpression? escapeChar = null)
        => (LikeExpression)ApplyDefaultTypeMapping(new LikeExpression(match, pattern, escapeChar, null));

    /// <inheritdoc />
    public virtual SqlFragmentExpression Fragment(string sql)
        => new(sql);

    /// <inheritdoc />
    public virtual SqlConstantExpression Constant(object? value, RelationalTypeMapping? typeMapping = null)
        => new(Expression.Constant(value), typeMapping);

    /// <inheritdoc />
    public virtual SqlConstantExpression Constant(object? value, Type type, RelationalTypeMapping? typeMapping = null)
        => new(Expression.Constant(value, type), typeMapping);

    /// <inheritdoc />
    public virtual SelectExpression Select(SqlExpression? projection)
        => new(projection);

    /// <inheritdoc />
    public virtual SelectExpression Select(IEntityType entityType)
    {
        var selectExpression = new SelectExpression(entityType, this);
        AddConditions(selectExpression, entityType);

        return selectExpression;
    }

    /// <inheritdoc />
    public virtual SelectExpression Select(IEntityType entityType, TableExpressionBase tableExpressionBase)
    {
        var selectExpression = new SelectExpression(entityType, tableExpressionBase);
        AddConditions(selectExpression, entityType);

        return selectExpression;
    }

    /***
     * We need to add additional conditions on basic SelectExpression for certain cases
     * - If we are selecting from TPH then we need to add condition for discriminator if mapping is incomplete
     * - When we are selecting optional dependent sharing table, we need to add condition to figure out existence
     *  ** Optional Dependent **
     *  - Only root type can be the dependent
     *  - Dependents will have a non-principal-non-PK-shared required property
     *  - Principal can be any type in TPH/TPT or leaf type in TPC
     *  - Dependent side can be TPH or TPT but not TPC
     ***/
    private void AddConditions(SelectExpression selectExpression, IEntityType entityType)
    {
        // First add condition for discriminator mapping
        var discriminatorProperty = entityType.FindDiscriminatorProperty();
        if (discriminatorProperty != null
            && (!entityType.GetRootType().GetIsDiscriminatorMappingComplete()
                || !entityType.GetAllBaseTypesInclusiveAscending()
                    .All(e => (e == entityType || e.IsAbstract()) && !HasSiblings(e))))
        {
            var discriminatorColumn = GetMappedEntityProjectionExpression(selectExpression).BindProperty(discriminatorProperty);
            var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToList();
            var predicate = concreteEntityTypes.Count == 1
                ? (SqlExpression)Equal(discriminatorColumn, Constant(concreteEntityTypes[0].GetDiscriminatorValue()))
                : In(discriminatorColumn, Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()), negated: false);

            selectExpression.ApplyPredicate(predicate);

            // If discriminator predicate is added then it will also serve as condition for existence of dependents in table sharing
            return;
        }

        // Keyless entities cannot be table sharing
        if (entityType.FindPrimaryKey() == null)
        {
            return;
        }

        // Add conditions if this is optional dependent with table sharing
        if (entityType.GetRootType() != entityType // Non-root cannot be dependent
            || entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy) // Dependent cannot be TPC
        {
            return;
        }

        var table = (selectExpression.Tables[0] as ITableBasedExpression)?.Table;
        Check.DebugAssert(table is not null, "SelectExpression with unexpected missing table");

        if (table.IsOptional(entityType))
        {
            SqlExpression? predicate = null;
            var entityProjectionExpression = GetMappedEntityProjectionExpression(selectExpression);
            var requiredNonPkProperties = entityType.GetProperties().Where(p => !p.IsNullable && !p.IsPrimaryKey()).ToList();
            if (requiredNonPkProperties.Count > 0)
            {
                predicate = requiredNonPkProperties.Select(e => IsNotNull(e, entityProjectionExpression))
                    .Aggregate((l, r) => AndAlso(l, r));
            }

            var allNonSharedNonPkProperties = entityType.GetNonPrincipalSharedNonPkProperties(table);
            // We don't need condition for nullable property if there exist at least one required property which is non shared.
            if (allNonSharedNonPkProperties.Count != 0
                && allNonSharedNonPkProperties.All(p => p.IsNullable))
            {
                var atLeastOneNonNullValueInNullablePropertyCondition = allNonSharedNonPkProperties
                    .Select(e => IsNotNull(e, entityProjectionExpression))
                    .Aggregate((a, b) => OrElse(a, b));

                predicate = predicate == null
                    ? atLeastOneNonNullValueInNullablePropertyCondition
                    : AndAlso(predicate, atLeastOneNonNullValueInNullablePropertyCondition);
            }

            if (predicate != null)
            {
                selectExpression.ApplyPredicate(predicate);
            }
        }

        bool HasSiblings(IEntityType entityType)
            => entityType.BaseType?.GetDirectlyDerivedTypes().Any(i => i != entityType) == true;
    }

    private static EntityProjectionExpression GetMappedEntityProjectionExpression(SelectExpression selectExpression)
        => (EntityProjectionExpression)selectExpression.GetProjection(
            new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(ValueBuffer)));

    private SqlExpression IsNotNull(IProperty property, EntityProjectionExpression entityProjection)
        => IsNotNull(entityProjection.BindProperty(property));
}
