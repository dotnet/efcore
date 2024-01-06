// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
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
public class QuerySqlGenerator : SqlExpressionVisitor
{
    private readonly ITypeMappingSource _typeMappingSource;
    private readonly IndentedStringBuilder _sqlBuilder = new();
    private IReadOnlyDictionary<string, object> _parameterValues;
    private List<SqlParameter> _sqlParameters;
    private bool _useValueProjection;
    private ParameterNameGenerator _parameterNameGenerator;

    private readonly IDictionary<ExpressionType, string> _operatorMap = new Dictionary<ExpressionType, string>
    {
        // Arithmetic
        { ExpressionType.Add, " + " },
        { ExpressionType.Subtract, " - " },
        { ExpressionType.Multiply, " * " },
        { ExpressionType.Divide, " / " },
        { ExpressionType.Modulo, " % " },

        // Bitwise >>> (zero-fill right shift) not available in C#
        { ExpressionType.Or, " | " },
        { ExpressionType.And, " & " },
        { ExpressionType.ExclusiveOr, " ^ " },
        { ExpressionType.LeftShift, " << " },
        { ExpressionType.RightShift, " >> " },

        // Logical
        { ExpressionType.AndAlso, " AND " },
        { ExpressionType.OrElse, " OR " },

        // Comparison
        { ExpressionType.Equal, " = " },
        { ExpressionType.NotEqual, " != " },
        { ExpressionType.GreaterThan, " > " },
        { ExpressionType.GreaterThanOrEqual, " >= " },
        { ExpressionType.LessThan, " < " },
        { ExpressionType.LessThanOrEqual, " <= " },

        // Unary
        { ExpressionType.UnaryPlus, "+" },
        { ExpressionType.Negate, "-" },
        { ExpressionType.Not, "~" }
    };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public QuerySqlGenerator(ITypeMappingSource typeMappingSource)
    {
        _typeMappingSource = typeMappingSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosSqlQuery GetSqlQuery(
        SelectExpression selectExpression,
        IReadOnlyDictionary<string, object> parameterValues)
    {
        _sqlBuilder.Clear();
        _parameterValues = parameterValues;
        _sqlParameters = [];
        _parameterNameGenerator = new ParameterNameGenerator();

        Visit(selectExpression);

        return new CosmosSqlQuery(_sqlBuilder.ToString(), _sqlParameters);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitEntityProjection(EntityProjectionExpression entityProjectionExpression)
    {
        Visit(entityProjectionExpression.AccessExpression);

        return entityProjectionExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitObjectArrayProjection(ObjectArrayProjectionExpression objectArrayProjectionExpression)
    {
        _sqlBuilder.Append(objectArrayProjectionExpression.ToString());

        return objectArrayProjectionExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitKeyAccess(KeyAccessExpression keyAccessExpression)
    {
        _sqlBuilder.Append(keyAccessExpression.ToString());

        return keyAccessExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitObjectAccess(ObjectAccessExpression objectAccessExpression)
    {
        _sqlBuilder.Append(objectAccessExpression.ToString());

        return objectAccessExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitProjection(ProjectionExpression projectionExpression)
    {
        if (_useValueProjection)
        {
            _sqlBuilder.Append('"').Append(projectionExpression.Alias).Append("\" : ");
        }

        Visit(projectionExpression.Expression);

        if (!_useValueProjection
            && !string.IsNullOrEmpty(projectionExpression.Alias)
            && projectionExpression.Alias != projectionExpression.Name)
        {
            _sqlBuilder.Append(" AS " + projectionExpression.Alias);
        }

        return projectionExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitRootReference(RootReferenceExpression rootReferenceExpression)
    {
        _sqlBuilder.Append(rootReferenceExpression.ToString());

        return rootReferenceExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSelect(SelectExpression selectExpression)
    {
        _sqlBuilder.Append("SELECT ");

        if (selectExpression.IsDistinct)
        {
            _sqlBuilder.Append("DISTINCT ");
        }

        if (selectExpression.Projection.Count > 0)
        {
            if (selectExpression.Projection.Any(p => !string.IsNullOrEmpty(p.Alias) && p.Alias != p.Name)
                && !selectExpression.Projection.Any(p => p.Expression is SqlFunctionExpression)) // Aggregates are not allowed
            {
                _useValueProjection = true;
                _sqlBuilder.Append("VALUE {");
                GenerateList(selectExpression.Projection, e => Visit(e));
                _sqlBuilder.Append('}');
                _useValueProjection = false;
            }
            else
            {
                GenerateList(selectExpression.Projection, e => Visit(e));
            }
        }
        else
        {
            _sqlBuilder.Append('1');
        }

        _sqlBuilder.AppendLine();

        _sqlBuilder.Append(selectExpression.FromExpression is FromSqlExpression ? "FROM " : "FROM root ");

        Visit(selectExpression.FromExpression);

        if (selectExpression.Predicate != null)
        {
            _sqlBuilder.AppendLine().Append("WHERE ");
            Visit(selectExpression.Predicate);
        }

        if (selectExpression.Orderings.Any())
        {
            _sqlBuilder.AppendLine().Append("ORDER BY ");

            GenerateList(selectExpression.Orderings, e => Visit(e));
        }

        if (selectExpression.Offset != null
            || selectExpression.Limit != null)
        {
            _sqlBuilder.AppendLine().Append("OFFSET ");

            if (selectExpression.Offset != null)
            {
                Visit(selectExpression.Offset);
            }
            else
            {
                _sqlBuilder.Append('0');
            }

            _sqlBuilder.Append(" LIMIT ");

            if (selectExpression.Limit != null)
            {
                Visit(selectExpression.Limit);
            }
            else
            {
                // TODO: See Issue#18923
                throw new InvalidOperationException(CosmosStrings.OffsetRequiresLimit);
            }
        }

        return selectExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
    {
        var sql = fromSqlExpression.Sql;

        string[] substitutions;

        switch (fromSqlExpression.Arguments)
        {
            case ParameterExpression { Name: not null } parameterExpression
                when _parameterValues.TryGetValue(parameterExpression.Name, out var parameterValue)
                && parameterValue is object[] parameterValues:
            {
                substitutions = new string[parameterValues.Length];
                for (var i = 0; i < parameterValues.Length; i++)
                {
                    var parameterName = _parameterNameGenerator.GenerateNext();
                    _sqlParameters.Add(new SqlParameter(parameterName, parameterValues[i]));
                    substitutions[i] = parameterName;
                }

                break;
            }

            case ConstantExpression { Value: object[] constantValues }:
            {
                substitutions = new string[constantValues.Length];
                for (var i = 0; i < constantValues.Length; i++)
                {
                    var value = constantValues[i];
                    substitutions[i] = GenerateConstant(value, _typeMappingSource.FindMapping(value.GetType()));
                }

                break;
            }

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(fromSqlExpression),
                    fromSqlExpression.Arguments,
                    CosmosStrings.InvalidFromSqlArguments(
                        fromSqlExpression.Arguments.GetType(),
                        fromSqlExpression.Arguments is ConstantExpression constantExpression
                            ? constantExpression.Value?.GetType()
                            : null));
        }

        // ReSharper disable once CoVariantArrayConversion
        // InvariantCulture not needed since substitutions are all strings
        sql = string.Format(sql, substitutions);

        _sqlBuilder.AppendLine("(");

        using (_sqlBuilder.Indent())
        {
            _sqlBuilder.AppendLines(sql);
        }

        _sqlBuilder
            .Append(") ")
            .Append(fromSqlExpression.Alias);

        return fromSqlExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitOrdering(OrderingExpression orderingExpression)
    {
        Visit(orderingExpression.Expression);

        if (!orderingExpression.IsAscending)
        {
            _sqlBuilder.Append(" DESC");
        }

        return orderingExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
    {
        var op = _operatorMap[sqlBinaryExpression.OperatorType];
        _sqlBuilder.Append('(');
        Visit(sqlBinaryExpression.Left);

        if (sqlBinaryExpression.OperatorType == ExpressionType.Add
            && sqlBinaryExpression.Left.Type == typeof(string))
        {
            op = " || ";
        }

        _sqlBuilder.Append(op);

        Visit(sqlBinaryExpression.Right);
        _sqlBuilder.Append(')');

        return sqlBinaryExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
    {
        var op = _operatorMap[sqlUnaryExpression.OperatorType];

        if (sqlUnaryExpression.OperatorType == ExpressionType.Not
            && sqlUnaryExpression.Operand.Type == typeof(bool))
        {
            if (sqlUnaryExpression.Operand is InExpression inExpression)
            {
                GenerateIn(inExpression, negated: true);

                return sqlUnaryExpression;
            }

            op = "NOT";
        }

        _sqlBuilder.Append(op);

        _sqlBuilder.Append('(');
        Visit(sqlUnaryExpression.Operand);
        _sqlBuilder.Append(')');

        return sqlUnaryExpression;
    }

    private void GenerateList<T>(
        IReadOnlyList<T> items,
        Action<T> generationAction,
        Action<IndentedStringBuilder> joinAction = null)
    {
        joinAction ??= (isb => isb.Append(", "));

        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                joinAction(_sqlBuilder);
            }

            generationAction(items[i]);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
    {
        _sqlBuilder.Append(GenerateConstant(sqlConstantExpression.Value, sqlConstantExpression.TypeMapping));

        return sqlConstantExpression;
    }

    private static string GenerateConstant(object value, CoreTypeMapping typeMapping)
    {
        var jToken = GenerateJToken(value, typeMapping);

        return jToken is null ? "null" : jToken.ToString(Formatting.None);
    }

    private static JToken GenerateJToken(object value, CoreTypeMapping typeMapping)
    {
        if (value?.GetType().IsInteger() == true)
        {
            var unwrappedType = typeMapping.ClrType.UnwrapNullableType();
            value = unwrappedType.IsEnum
                ? Enum.ToObject(unwrappedType, value)
                : unwrappedType == typeof(char)
                    ? Convert.ChangeType(value, unwrappedType)
                    : value;
        }

        var converter = typeMapping.Converter;
        if (converter != null)
        {
            value = converter.ConvertToProvider(value);
        }

        return value == null
            ? null
            : (value as JToken) ?? JToken.FromObject(value, CosmosClientWrapper.Serializer);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlConditional(SqlConditionalExpression sqlConditionalExpression)
    {
        _sqlBuilder.Append('(');
        Visit(sqlConditionalExpression.Test);
        _sqlBuilder.Append(" ? ");
        Visit(sqlConditionalExpression.IfTrue);
        _sqlBuilder.Append(" : ");
        Visit(sqlConditionalExpression.IfFalse);
        _sqlBuilder.Append(')');

        return sqlConditionalExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
    {
        var parameterName = $"@{sqlParameterExpression.Name}";

        if (_sqlParameters.All(sp => sp.Name != parameterName))
        {
            var jToken = GenerateJToken(_parameterValues[sqlParameterExpression.Name], sqlParameterExpression.TypeMapping);
            _sqlParameters.Add(new SqlParameter(parameterName, jToken));
        }

        _sqlBuilder.Append(parameterName);

        return sqlParameterExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected sealed override Expression VisitIn(InExpression inExpression)
    {
        GenerateIn(inExpression, negated: false);

        return inExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void GenerateIn(InExpression inExpression, bool negated)
    {
        Check.DebugAssert(
            inExpression.ValuesParameter is null,
            "InExpression.ValuesParameter must have been expanded to constants before SQL generation (in "
            + "InExpressionValuesExpandingExpressionVisitor)");
        Check.DebugAssert(inExpression.Values is not null, "Missing Values on InExpression");

        Visit(inExpression.Item);
        _sqlBuilder.Append(negated ? " NOT IN (" : " IN (");
        GenerateList(inExpression.Values, e => Visit(e));
        _sqlBuilder.Append(')');
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
    {
        _sqlBuilder.Append(sqlFunctionExpression.Name);
        _sqlBuilder.Append('(');
        GenerateList(sqlFunctionExpression.Arguments, e => Visit(e));
        _sqlBuilder.Append(')');

        return sqlFunctionExpression;
    }

    private sealed class ParameterNameGenerator
    {
        private int _count;

        public string GenerateNext()
            => "@p" + _count++;
    }
}
