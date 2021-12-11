// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class SqlExpressionFactory : ISqlExpressionFactory
{
    private readonly RelationalTypeMapping _boolTypeMapping;

    /// <summary>
    ///     Creates a new instance of the <see cref="SqlExpressionFactory" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    public SqlExpressionFactory(SqlExpressionFactoryDependencies dependencies)
    {
        Dependencies = dependencies;
        _boolTypeMapping = dependencies.TypeMappingSource.FindMapping(typeof(bool), dependencies.Model)!;
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
                        sqlExpression, Dependencies.TypeMappingSource.FindMapping(sqlExpression.Type, Dependencies.Model));

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
            CaseExpression e => ApplyTypeMappingOnCase(e, typeMapping),
            CollateExpression e => ApplyTypeMappingOnCollate(e, typeMapping),
            DistinctExpression e => ApplyTypeMappingOnDistinct(e, typeMapping),
            LikeExpression e => ApplyTypeMappingOnLike(e),
            SqlBinaryExpression e => ApplyTypeMappingOnSqlBinary(e, typeMapping),
            SqlUnaryExpression e => ApplyTypeMappingOnSqlUnary(e, typeMapping),
            SqlConstantExpression e => e.ApplyTypeMapping(typeMapping),
            SqlFragmentExpression e => e,
            SqlFunctionExpression e => e.ApplyTypeMapping(typeMapping),
            SqlParameterExpression e => e.ApplyTypeMapping(typeMapping),
            InExpression e => ApplyTypeMappingOnIn(e),
            RowValueExpression e => e,
            _ => sqlExpression
        };
    }

    private SqlExpression ApplyTypeMappingOnLike(LikeExpression likeExpression)
    {
        var inferredTypeMapping = (likeExpression.EscapeChar == null
                ? ExpressionExtensions.InferTypeMapping(
                    likeExpression.Match, likeExpression.Pattern)
                : ExpressionExtensions.InferTypeMapping(
                    likeExpression.Match, likeExpression.Pattern, likeExpression.EscapeChar))
            ?? Dependencies.TypeMappingSource.FindMapping(likeExpression.Match.Type, Dependencies.Model);

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
                    ?? (left.Type != typeof(object)
                        ? Dependencies.TypeMappingSource.FindMapping(left.Type, Dependencies.Model)
                        : Dependencies.TypeMappingSource.FindMapping(right.Type, Dependencies.Model));
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
            ?? Dependencies.TypeMappingSource.FindMapping(inExpression.Item.Type, Dependencies.Model);

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
            ?? Dependencies.TypeMappingSource.FindMapping(resultType, Dependencies.Model);

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
                .Where(t => t != typeof(object)).Select(t => Dependencies.TypeMappingSource.FindMapping(t, Dependencies.Model))
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
        var typeMapping = item.TypeMapping ?? Dependencies.TypeMappingSource.FindMapping(item.Type, Dependencies.Model);

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
    public virtual RowValueExpression RowValue(
        ExpressionType operatorType,
        IReadOnlyList<SqlExpression> columns,
        IReadOnlyList<object> values)
    {
        var valuesTypeMappings = values
            .Select(x => Dependencies.TypeMappingSource.FindMapping(x.GetType(), Dependencies.Model)!)
            .ToList();
        return new(operatorType, columns, values, valuesTypeMappings, _boolTypeMapping);
    }

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

    private void AddSelfConditions(SelectExpression selectExpression, IEntityType entityType, ITableBase? table = null)
    {
        // Add conditions if TPH
        var discriminatorAdded = AddDiscriminatorCondition(selectExpression, entityType);
        if (entityType.FindPrimaryKey() == null)
        {
            return;
        }

        // Add conditions if dependent sharing table with principal
        table ??= entityType.GetViewOrTableMappings().FirstOrDefault()?.Table;
        if (table != null
            && table.IsOptional(entityType)
            && !discriminatorAdded)
        {
            AddOptionalDependentConditions(selectExpression, entityType, table);
        }
    }

    private void AddConditions(SelectExpression selectExpression, IEntityType entityType, ITableBase? table = null)
    {
        AddSelfConditions(selectExpression, entityType, table);
        // Add inner join to principal if table sharing
        table ??= entityType.GetViewOrTableMappings().FirstOrDefault()?.Table;
        if (table != null)
        {
            var linkingFks = table.GetRowInternalForeignKeys(entityType);
            var first = true;
            foreach (var foreignKey in linkingFks)
            {
                if (first)
                {
                    AddInnerJoin(selectExpression, foreignKey, table);
                    first = false;
                }
                else
                {
                    var dependentSelectExpression = new SelectExpression(entityType, this);
                    AddSelfConditions(dependentSelectExpression, entityType, table);
                    AddInnerJoin(dependentSelectExpression, foreignKey, table);
                    selectExpression.ApplyUnion(dependentSelectExpression, distinct: true);
                }
            }
        }
    }

    private void AddInnerJoin(SelectExpression selectExpression, IForeignKey foreignKey, ITableBase? table)
    {
        var outerEntityProjection = GetMappedEntityProjectionExpression(selectExpression);
        var outerIsPrincipal = foreignKey.PrincipalEntityType.IsAssignableFrom(outerEntityProjection.EntityType);

        var innerSelect = outerIsPrincipal
            ? new SelectExpression(foreignKey.DeclaringEntityType, this)
            : new SelectExpression(foreignKey.PrincipalEntityType, this);

        if (outerIsPrincipal)
        {
            AddSelfConditions(innerSelect, foreignKey.DeclaringEntityType, table);
        }
        else
        {
            AddConditions(innerSelect, foreignKey.PrincipalEntityType, table);
        }

        var innerEntityProjection = GetMappedEntityProjectionExpression(innerSelect);

        var outerKey = (outerIsPrincipal ? foreignKey.PrincipalKey.Properties : foreignKey.Properties)
            .Select(p => outerEntityProjection.BindProperty(p));
        var innerKey = (outerIsPrincipal ? foreignKey.Properties : foreignKey.PrincipalKey.Properties)
            .Select(p => innerEntityProjection.BindProperty(p));

        var joinPredicate = outerKey.Zip(innerKey, Equal).Aggregate(AndAlso);

        selectExpression.AddInnerJoin(innerSelect, joinPredicate);
    }

    private bool AddDiscriminatorCondition(SelectExpression selectExpression, IEntityType entityType)
    {
        var discriminatorProperty = entityType.FindDiscriminatorProperty();
        if (discriminatorProperty == null
            || (entityType.GetRootType().GetIsDiscriminatorMappingComplete()
                && entityType.GetAllBaseTypesInclusiveAscending()
                    .All(e => (e == entityType || e.IsAbstract()) && !HasSiblings(e))))
        {
            return false;
        }

        var discriminatorColumn = GetMappedEntityProjectionExpression(selectExpression).BindProperty(discriminatorProperty);
        var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToList();
        var predicate = concreteEntityTypes.Count == 1
            ? (SqlExpression)Equal(discriminatorColumn, Constant(concreteEntityTypes[0].GetDiscriminatorValue()))
            : In(discriminatorColumn, Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()), negated: false);

        selectExpression.ApplyPredicate(predicate);

        return true;

        bool HasSiblings(IEntityType entityType)
            => entityType.BaseType?.GetDirectlyDerivedTypes().Any(i => i != entityType) == true;
    }

    private void AddOptionalDependentConditions(
        SelectExpression selectExpression,
        IEntityType entityType,
        ITableBase table)
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

    private static EntityProjectionExpression GetMappedEntityProjectionExpression(SelectExpression selectExpression)
        => (EntityProjectionExpression)selectExpression.GetProjection(
            new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(ValueBuffer)));

    private SqlExpression IsNotNull(IProperty property, EntityProjectionExpression entityProjection)
        => IsNotNull(entityProjection.BindProperty(property));
}
