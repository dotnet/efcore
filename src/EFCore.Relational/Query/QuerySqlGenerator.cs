// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         A query SQL generator to get <see cref="IRelationalCommand" /> for given <see cref="SelectExpression" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class QuerySqlGenerator : SqlExpressionVisitor
{
    private static readonly Dictionary<ExpressionType, string> OperatorMap = new()
    {
        { ExpressionType.Equal, " = " },
        { ExpressionType.NotEqual, " <> " },
        { ExpressionType.GreaterThan, " > " },
        { ExpressionType.GreaterThanOrEqual, " >= " },
        { ExpressionType.LessThan, " < " },
        { ExpressionType.LessThanOrEqual, " <= " },
        { ExpressionType.AndAlso, " AND " },
        { ExpressionType.OrElse, " OR " },
        { ExpressionType.Add, " + " },
        { ExpressionType.Subtract, " - " },
        { ExpressionType.Multiply, " * " },
        { ExpressionType.Divide, " / " },
        { ExpressionType.Modulo, " % " },
        { ExpressionType.And, " & " },
        { ExpressionType.Or, " | " }
    };

    private readonly IRelationalCommandBuilderFactory _relationalCommandBuilderFactory;
    private readonly ISqlGenerationHelper _sqlGenerationHelper;
    private IRelationalCommandBuilder _relationalCommandBuilder;
    private Dictionary<string, int>? _repeatedParameterCounts;

    /// <summary>
    ///     Creates a new instance of the <see cref="QuerySqlGenerator" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    public QuerySqlGenerator(QuerySqlGeneratorDependencies dependencies)
    {
        Dependencies = dependencies;

        _relationalCommandBuilderFactory = dependencies.RelationalCommandBuilderFactory;
        _sqlGenerationHelper = dependencies.SqlGenerationHelper;
        _relationalCommandBuilder = default!;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual QuerySqlGeneratorDependencies Dependencies { get; }

    /// <summary>
    ///     Gets a relational command for a query expression.
    /// </summary>
    /// <param name="queryExpression">A query expression to print in command text.</param>
    /// <returns>A relational command with a SQL represented by the query expression.</returns>
    public virtual IRelationalCommand GetCommand(Expression queryExpression)
    {
        _relationalCommandBuilder = _relationalCommandBuilderFactory.Create();

        GenerateRootCommand(queryExpression);

        return _relationalCommandBuilder.Build();
    }

    /// <summary>
    ///     Generates the command for the given top-level query expression. This allows providers to intercept if an expression
    ///     requires different processing when it is at top-level.
    /// </summary>
    /// <param name="queryExpression">A query expression to print in command.</param>
    protected virtual void GenerateRootCommand(Expression queryExpression)
    {
        switch (queryExpression)
        {
            case SelectExpression selectExpression:
                GenerateTagsHeaderComment(selectExpression.Tags);

                if (selectExpression.IsNonComposedFromSql())
                {
                    GenerateFromSql((FromSqlExpression)selectExpression.Tables[0]);
                }
                else
                {
                    VisitSelect(selectExpression);
                }

                break;

            case UpdateExpression updateExpression:
                GenerateTagsHeaderComment(updateExpression.Tags);
                VisitUpdate(updateExpression);
                break;

            case DeleteExpression deleteExpression:
                GenerateTagsHeaderComment(deleteExpression.Tags);
                VisitDelete(deleteExpression);
                break;

            default:
                base.Visit(queryExpression);
                break;
        }
    }

    /// <summary>
    ///     The default alias separator.
    /// </summary>
    protected virtual string AliasSeparator
        => " AS ";

    /// <summary>
    ///     The current SQL command builder.
    /// </summary>
    protected virtual IRelationalCommandBuilder Sql
        => _relationalCommandBuilder;

    /// <summary>
    ///     Generates the head comment for tags.
    /// </summary>
    /// <param name="selectExpression">A select expression to generate tags for.</param>
    [Obsolete("Use the method which takes tags instead.")]
    protected virtual void GenerateTagsHeaderComment(SelectExpression selectExpression)
    {
        if (selectExpression.Tags.Count > 0)
        {
            foreach (var tag in selectExpression.Tags)
            {
                _relationalCommandBuilder.AppendLines(_sqlGenerationHelper.GenerateComment(tag));
            }

            _relationalCommandBuilder.AppendLine();
        }
    }

    /// <summary>
    ///     Generates the head comment for tags.
    /// </summary>
    /// <param name="tags">A set of tags to print as comment.</param>
    protected virtual void GenerateTagsHeaderComment(ISet<string> tags)
    {
        if (tags.Count > 0)
        {
            foreach (var tag in tags)
            {
                _relationalCommandBuilder.AppendLines(_sqlGenerationHelper.GenerateComment(tag));
            }

            _relationalCommandBuilder.AppendLine();
        }
    }

    /// <inheritdoc />
    protected override Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression)
    {
        _relationalCommandBuilder.Append(sqlFragmentExpression.Sql);

        return sqlFragmentExpression;
    }

    private static bool IsNonComposedSetOperation(SelectExpression selectExpression)
        => selectExpression is
            {
                Tables: [SetOperationBase setOperation],
                Predicate: null,
                Orderings: [],
                Offset: null,
                Limit: null,
                IsDistinct: false,
                Having: null,
                GroupBy: []
            }
            && selectExpression.Projection.Count == setOperation.Source1.Projection.Count
            && selectExpression.Projection.Select(
                    (pe, index) => pe.Expression is ColumnExpression column
                        && column.TableAlias == setOperation.Alias
                        && column.Name == setOperation.Source1.Projection[index].Alias)
                .All(e => e);

    /// <inheritdoc />
    protected override Expression VisitDelete(DeleteExpression deleteExpression)
    {
        var selectExpression = deleteExpression.SelectExpression;

        if (selectExpression is
            {
                Tables: [var table],
                GroupBy: [],
                Having: null,
                Projection: [],
                Orderings: [],
                Offset: null,
                Limit: null
            }
            && table.Equals(deleteExpression.Table))
        {
            _relationalCommandBuilder.Append("DELETE FROM ");
            Visit(deleteExpression.Table);

            if (selectExpression.Predicate != null)
            {
                _relationalCommandBuilder.AppendLine().Append("WHERE ");
                Visit(selectExpression.Predicate);
            }

            return deleteExpression;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(nameof(RelationalQueryableExtensions.ExecuteDelete)));
    }

    /// <inheritdoc />
    protected override Expression VisitSelect(SelectExpression selectExpression)
    {
        IDisposable? subQueryIndent = null;
        if (selectExpression.Alias != null)
        {
            _relationalCommandBuilder.AppendLine("(");
            subQueryIndent = _relationalCommandBuilder.Indent();
        }

        if (!TryGenerateWithoutWrappingSelect(selectExpression))
        {
            _relationalCommandBuilder.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                _relationalCommandBuilder.Append("DISTINCT ");
            }

            GenerateTop(selectExpression);
            GenerateProjection(selectExpression);
            GenerateTables(selectExpression);

            if (selectExpression.Predicate != null)
            {
                _relationalCommandBuilder.AppendLine().Append("WHERE ");

                Visit(selectExpression.Predicate);
            }

            if (selectExpression.GroupBy.Count > 0)
            {
                _relationalCommandBuilder.AppendLine().Append("GROUP BY ");

                GenerateList(selectExpression.GroupBy, e => Visit(e));
            }

            if (selectExpression.Having != null)
            {
                _relationalCommandBuilder.AppendLine().Append("HAVING ");

                Visit(selectExpression.Having);
            }

            GenerateOrderings(selectExpression);
            GenerateLimitOffset(selectExpression);
        }

        if (selectExpression.Alias != null)
        {
            subQueryIndent!.Dispose();

            _relationalCommandBuilder.AppendLine()
                .Append(")")
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(selectExpression.Alias));
        }

        return selectExpression;
    }

    /// <summary>
    ///     If possible, generates the expression contained within the provided <paramref name="selectExpression" /> without the wrapping
    ///     SELECT. This can be done for set operations and VALUES, which can appear as top-level statements without needing to be wrapped
    ///     in SELECT.
    /// </summary>
    protected virtual bool TryGenerateWithoutWrappingSelect(SelectExpression selectExpression)
    {
        if (IsNonComposedSetOperation(selectExpression))
        {
            GenerateSetOperation((SetOperationBase)selectExpression.Tables[0]);
            return true;
        }

        if (selectExpression is
            {
                Tables: [ValuesExpression valuesExpression],
                Offset: null,
                Limit: null,
                IsDistinct: false,
                Predicate: null,
                Having: null,
                Orderings.Count: 0,
                GroupBy.Count: 0,
            }
            && selectExpression.Projection.Count == valuesExpression.ColumnNames.Count
            && selectExpression.Projection.Select(
                    (pe, index) => pe.Expression is ColumnExpression column
                        && column.Name == valuesExpression.ColumnNames[index])
                .All(e => e))
        {
            GenerateValues(valuesExpression);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Generates a pseudo FROM clause. Required by some providers when a query has no actual FROM clause.
    /// </summary>
    protected virtual void GeneratePseudoFromClause()
    {
    }

    /// <summary>
    ///     Generates empty projection for a SelectExpression.
    /// </summary>
    /// <param name="selectExpression">SelectExpression for which the empty projection will be generated.</param>
    protected virtual void GenerateEmptyProjection(SelectExpression selectExpression)
        => _relationalCommandBuilder.Append("1");

    /// <inheritdoc />
    protected override Expression VisitProjection(ProjectionExpression projectionExpression)
    {
        Visit(projectionExpression.Expression);

        if (projectionExpression.Alias != string.Empty
            && !(projectionExpression.Expression is ColumnExpression column && column.Name == projectionExpression.Alias))
        {
            _relationalCommandBuilder
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(projectionExpression.Alias));
        }

        return projectionExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
    {
        if (sqlFunctionExpression.IsBuiltIn)
        {
            if (sqlFunctionExpression.Instance != null)
            {
                Visit(sqlFunctionExpression.Instance);
                _relationalCommandBuilder.Append(".");
            }

            _relationalCommandBuilder.Append(sqlFunctionExpression.Name);
        }
        else
        {
            if (!string.IsNullOrEmpty(sqlFunctionExpression.Schema))
            {
                _relationalCommandBuilder
                    .Append(_sqlGenerationHelper.DelimitIdentifier(sqlFunctionExpression.Schema))
                    .Append(".");
            }

            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.DelimitIdentifier(sqlFunctionExpression.Name));
        }

        if (!sqlFunctionExpression.IsNiladic)
        {
            _relationalCommandBuilder.Append("(");
            GenerateList(sqlFunctionExpression.Arguments, e => Visit(e));
            _relationalCommandBuilder.Append(")");
        }

        return sqlFunctionExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitTableValuedFunction(TableValuedFunctionExpression tableValuedFunctionExpression)
    {
        if (!string.IsNullOrEmpty(tableValuedFunctionExpression.Schema))
        {
            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableValuedFunctionExpression.Schema))
                .Append(".");
        }

        var name = tableValuedFunctionExpression.IsBuiltIn
            ? tableValuedFunctionExpression.Name
            : _sqlGenerationHelper.DelimitIdentifier(tableValuedFunctionExpression.Name);

        _relationalCommandBuilder
            .Append(name)
            .Append("(");

        GenerateList(tableValuedFunctionExpression.Arguments, e => Visit(e));

        _relationalCommandBuilder
            .Append(")")
            .Append(AliasSeparator)
            .Append(_sqlGenerationHelper.DelimitIdentifier(tableValuedFunctionExpression.Alias));

        return tableValuedFunctionExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitColumn(ColumnExpression columnExpression)
    {
        _relationalCommandBuilder
            .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.TableAlias))
            .Append(".")
            .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Name));

        return columnExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitTable(TableExpression tableExpression)
    {
        _relationalCommandBuilder
            .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Name, tableExpression.Schema))
            .Append(AliasSeparator)
            .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));

        return tableExpression;
    }

    private void GenerateFromSql(FromSqlExpression fromSqlExpression)
    {
        var sql = fromSqlExpression.Sql;
        string[]? substitutions;

        switch (fromSqlExpression.Arguments)
        {
            case ConstantExpression { Value: CompositeRelationalParameter compositeRelationalParameter }:
            {
                var subParameters = compositeRelationalParameter.RelationalParameters;
                substitutions = new string[subParameters.Count];
                for (var i = 0; i < subParameters.Count; i++)
                {
                    substitutions[i] = _sqlGenerationHelper.GenerateParameterNamePlaceholder(subParameters[i].InvariantName);
                }

                _relationalCommandBuilder.AddParameter(compositeRelationalParameter);

                break;
            }

            case ConstantExpression { Value: object[] constantValues }:
            {
                substitutions = new string[constantValues.Length];
                for (var i = 0; i < constantValues.Length; i++)
                {
                    var value = constantValues[i];
                    if (value is RawRelationalParameter rawRelationalParameter)
                    {
                        substitutions[i] = _sqlGenerationHelper.GenerateParameterNamePlaceholder(rawRelationalParameter.InvariantName);
                        _relationalCommandBuilder.AddParameter(rawRelationalParameter);
                    }
                    else if (value is SqlConstantExpression sqlConstantExpression)
                    {
                        substitutions[i] = sqlConstantExpression.TypeMapping!.GenerateSqlLiteral(sqlConstantExpression.Value);
                    }
                }

                break;
            }

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(fromSqlExpression),
                    fromSqlExpression.Arguments,
                    RelationalStrings.InvalidFromSqlArguments(
                        fromSqlExpression.Arguments.GetType(),
                        fromSqlExpression.Arguments is ConstantExpression constantExpression
                            ? constantExpression.Value?.GetType()
                            : null));
        }

        // ReSharper disable once CoVariantArrayConversion
        // InvariantCulture not needed since substitutions are all strings
        sql = string.Format(sql, substitutions);

        _relationalCommandBuilder.AppendLines(sql);
    }

    /// <inheritdoc />
    protected override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
    {
        _relationalCommandBuilder.AppendLine("(");

        CheckComposableSql(fromSqlExpression.Sql);

        using (_relationalCommandBuilder.Indent())
        {
            GenerateFromSql(fromSqlExpression);
        }

        _relationalCommandBuilder.Append(")")
            .Append(AliasSeparator)
            .Append(_sqlGenerationHelper.DelimitIdentifier(fromSqlExpression.Alias));

        return fromSqlExpression;
    }

    /// <summary>
    ///     Checks whether a given SQL string is composable, i.e. can be embedded as a subquery within a
    ///     larger SQL query.
    /// </summary>
    /// <param name="sql">An SQL string to be checked for composability.</param>
    /// <exception cref="InvalidOperationException">The given SQL isn't composable.</exception>
    protected virtual void CheckComposableSql(string sql)
    {
        var span = sql.AsSpan().TrimStart();

        while (true)
        {
            // SQL -- comment
            if (span.StartsWith("--"))
            {
                var i = span.IndexOf('\n');
                span = i > 0
                    ? span[(i + 1)..].TrimStart()
                    : throw new InvalidOperationException(RelationalStrings.FromSqlNonComposable);
                continue;
            }

            // SQL /* */ comment
            if (span.StartsWith("/*"))
            {
                var i = span.IndexOf("*/");
                span = i > 0
                    ? span[(i + 2)..].TrimStart()
                    : throw new InvalidOperationException(RelationalStrings.FromSqlNonComposable);
                continue;
            }

            break;
        }

        CheckComposableSqlTrimmed(span);
    }

    /// <summary>
    ///     Checks whether a given SQL string is composable, i.e. can be embedded as a subquery within a
    ///     larger SQL query. The provided <paramref name="sql" /> is already trimmed for whitespace and comments.
    /// </summary>
    /// <param name="sql">An trimmed SQL string to be checked for composability.</param>
    /// <exception cref="InvalidOperationException">The given SQL isn't composable.</exception>
    protected virtual void CheckComposableSqlTrimmed(ReadOnlySpan<char> sql)
    {
        sql = sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
            ? sql["SELECT".Length..]
            : sql.StartsWith("WITH", StringComparison.OrdinalIgnoreCase)
                ? sql["WITH".Length..]
                : throw new InvalidOperationException(RelationalStrings.FromSqlNonComposable);

        if (sql.Length > 0
            && (char.IsWhiteSpace(sql[0]) || sql.StartsWith("--") || sql.StartsWith("/*")))
        {
            return;
        }

        throw new InvalidOperationException(RelationalStrings.FromSqlNonComposable);
    }

    /// <inheritdoc />
    protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
    {
        var requiresParentheses = RequiresParentheses(sqlBinaryExpression, sqlBinaryExpression.Left);

        if (requiresParentheses)
        {
            _relationalCommandBuilder.Append("(");
        }

        Visit(sqlBinaryExpression.Left);

        if (requiresParentheses)
        {
            _relationalCommandBuilder.Append(")");
        }

        _relationalCommandBuilder.Append(GetOperator(sqlBinaryExpression));

        requiresParentheses = RequiresParentheses(sqlBinaryExpression, sqlBinaryExpression.Right);

        if (requiresParentheses)
        {
            _relationalCommandBuilder.Append("(");
        }

        Visit(sqlBinaryExpression.Right);

        if (requiresParentheses)
        {
            _relationalCommandBuilder.Append(")");
        }

        return sqlBinaryExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
    {
        _relationalCommandBuilder
            .Append(sqlConstantExpression.TypeMapping!.GenerateSqlLiteral(sqlConstantExpression.Value));

        return sqlConstantExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
    {
        var invariantName = sqlParameterExpression.Name;
        var parameterName = sqlParameterExpression.Name;
        var typeMapping = sqlParameterExpression.TypeMapping!;

        // Try to see if a parameter already exists - if so, just integrate the same placeholder into the SQL instead of sending the same
        // data twice.
        // Note that if the type mapping differs, we do send the same data twice (e.g. the same string may be sent once as Unicode, once as
        // non-Unicode).
        // TODO: Note that we perform Equals comparison on the value converter. We should be able to do reference comparison, but for
        // that we need to ensure that there's only ever one type mapping instance (i.e. no type mappings are ever instantiated out of the
        // type mapping source). See #30677.
        var parameter = _relationalCommandBuilder.Parameters.FirstOrDefault(
            p =>
                p.InvariantName == parameterName
                && p is TypeMappedRelationalParameter { RelationalTypeMapping: var existingTypeMapping }
                && string.Equals(existingTypeMapping.StoreType, typeMapping.StoreType, StringComparison.OrdinalIgnoreCase)
                && (existingTypeMapping.Converter is null && typeMapping.Converter is null
                    || existingTypeMapping.Converter is not null && existingTypeMapping.Converter.Equals(typeMapping.Converter)));

        if (parameter is null)
        {
            parameterName = GetUniqueParameterName(parameterName);

            _relationalCommandBuilder.AddParameter(
                invariantName,
                _sqlGenerationHelper.GenerateParameterName(parameterName),
                sqlParameterExpression.TypeMapping!,
                sqlParameterExpression.IsNullable);
        }
        else
        {
            parameterName = ((TypeMappedRelationalParameter)parameter).Name;
        }

        _relationalCommandBuilder
            .Append(_sqlGenerationHelper.GenerateParameterNamePlaceholder(parameterName));

        return sqlParameterExpression;

        string GetUniqueParameterName(string currentName)
        {
            _repeatedParameterCounts ??= new Dictionary<string, int>();

            if (!_repeatedParameterCounts.TryGetValue(currentName, out var currentCount))
            {
                _repeatedParameterCounts[currentName] = 0;

                return currentName;
            }

            currentCount++;
            _repeatedParameterCounts[currentName] = currentCount;

            return currentName + "_" + currentCount;
        }
    }

    /// <inheritdoc />
    protected override Expression VisitOrdering(OrderingExpression orderingExpression)
    {
        if (orderingExpression.Expression is SqlConstantExpression or SqlParameterExpression)
        {
            _relationalCommandBuilder.Append("(SELECT 1)");
        }
        else
        {
            Visit(orderingExpression.Expression);
        }

        if (!orderingExpression.IsAscending)
        {
            _relationalCommandBuilder.Append(" DESC");
        }

        return orderingExpression;
    }

    /// <inheritdoc />
    protected sealed override Expression VisitLike(LikeExpression likeExpression)
    {
        GenerateLike(likeExpression, negated: false);

        return likeExpression;
    }

    /// <summary>
    ///     Generates SQL for the LIKE expression.
    /// </summary>
    /// <param name="likeExpression">The expression to visit.</param>
    /// <param name="negated">Whether the given <paramref name="likeExpression" /> is negated.</param>
    protected virtual void GenerateLike(LikeExpression likeExpression, bool negated)
    {
        Visit(likeExpression.Match);

        if (negated)
        {
            _relationalCommandBuilder.Append(" NOT");
        }

        _relationalCommandBuilder.Append(" LIKE ");

        Visit(likeExpression.Pattern);

        if (likeExpression.EscapeChar != null)
        {
            _relationalCommandBuilder.Append(" ESCAPE ");
            Visit(likeExpression.EscapeChar);
        }
    }

    /// <inheritdoc />
    protected override Expression VisitCollate(CollateExpression collateExpression)
    {
        Visit(collateExpression.Operand);

        _relationalCommandBuilder
            .Append(" COLLATE ")
            .Append(collateExpression.Collation);

        return collateExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitDistinct(DistinctExpression distinctExpression)
    {
        _relationalCommandBuilder.Append("DISTINCT (");
        Visit(distinctExpression.Operand);
        _relationalCommandBuilder.Append(")");

        return distinctExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitCase(CaseExpression caseExpression)
    {
        _relationalCommandBuilder.Append("CASE");

        if (caseExpression.Operand != null)
        {
            _relationalCommandBuilder.Append(" ");
            Visit(caseExpression.Operand);
        }

        using (_relationalCommandBuilder.Indent())
        {
            foreach (var whenClause in caseExpression.WhenClauses)
            {
                _relationalCommandBuilder
                    .AppendLine()
                    .Append("WHEN ");
                Visit(whenClause.Test);
                _relationalCommandBuilder.Append(" THEN ");
                Visit(whenClause.Result);
            }

            if (caseExpression.ElseResult != null)
            {
                _relationalCommandBuilder
                    .AppendLine()
                    .Append("ELSE ");
                Visit(caseExpression.ElseResult);
            }
        }

        _relationalCommandBuilder
            .AppendLine()
            .Append("END");

        return caseExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
    {
        switch (sqlUnaryExpression.OperatorType)
        {
            case ExpressionType.Convert:
            {
                _relationalCommandBuilder.Append("CAST(");
                var requiresParentheses = RequiresParentheses(sqlUnaryExpression, sqlUnaryExpression.Operand);
                if (requiresParentheses)
                {
                    _relationalCommandBuilder.Append("(");
                }

                Visit(sqlUnaryExpression.Operand);
                if (requiresParentheses)
                {
                    _relationalCommandBuilder.Append(")");
                }

                _relationalCommandBuilder.Append(" AS ");
                _relationalCommandBuilder.Append(sqlUnaryExpression.TypeMapping!.StoreType);
                _relationalCommandBuilder.Append(")");
                break;
            }

            case ExpressionType.Not
                when sqlUnaryExpression.Type == typeof(bool):
            {
                switch (sqlUnaryExpression.Operand)
                {
                    case InExpression inExpression:
                        GenerateIn(inExpression, negated: true);
                        break;

                    case ExistsExpression existsExpression:
                        GenerateExists(existsExpression, negated: true);
                        break;

                    case LikeExpression likeExpression:
                        GenerateLike(likeExpression, negated: true);
                        break;

                    default:
                        _relationalCommandBuilder.Append("NOT (");
                        Visit(sqlUnaryExpression.Operand);
                        _relationalCommandBuilder.Append(")");
                        break;
                }

                break;
            }

            case ExpressionType.Not:
            {
                _relationalCommandBuilder.Append("~");

                var requiresBrackets = RequiresParentheses(sqlUnaryExpression, sqlUnaryExpression.Operand);
                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append("(");
                }

                Visit(sqlUnaryExpression.Operand);
                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append(")");
                }

                break;
            }

            case ExpressionType.Equal:
            {
                var requiresBrackets = RequiresParentheses(sqlUnaryExpression, sqlUnaryExpression.Operand);
                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append("(");
                }

                Visit(sqlUnaryExpression.Operand);
                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append(")");
                }

                _relationalCommandBuilder.Append(" IS NULL");
                break;
            }

            case ExpressionType.NotEqual:
            {
                var requiresBrackets = RequiresParentheses(sqlUnaryExpression, sqlUnaryExpression.Operand);
                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append("(");
                }

                Visit(sqlUnaryExpression.Operand);
                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append(")");
                }

                _relationalCommandBuilder.Append(" IS NOT NULL");
                break;
            }

            case ExpressionType.Negate:
            {
                _relationalCommandBuilder.Append("-");
                var requiresBrackets = RequiresParentheses(sqlUnaryExpression, sqlUnaryExpression.Operand);
                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append("(");
                }

                Visit(sqlUnaryExpression.Operand);
                if (requiresBrackets)
                {
                    _relationalCommandBuilder.Append(")");
                }

                break;
            }
        }

        return sqlUnaryExpression;
    }

    /// <inheritdoc />
    protected sealed override Expression VisitExists(ExistsExpression existsExpression)
    {
        GenerateExists(existsExpression, negated: false);

        return existsExpression;
    }

    /// <summary>
    ///     Generates SQL for the EXISTS expression.
    /// </summary>
    /// <param name="existsExpression">The expression to visit.</param>
    /// <param name="negated">Whether the given <paramref name="existsExpression" /> is negated.</param>
    protected virtual void GenerateExists(ExistsExpression existsExpression, bool negated)
    {
        if (negated)
        {
            _relationalCommandBuilder.Append("NOT ");
        }

        _relationalCommandBuilder.AppendLine("EXISTS (");

        using (_relationalCommandBuilder.Indent())
        {
            Visit(existsExpression.Subquery);
        }

        _relationalCommandBuilder.Append(")");
    }

    /// <inheritdoc />
    protected sealed override Expression VisitIn(InExpression inExpression)
    {
        GenerateIn(inExpression, negated: false);

        return inExpression;
    }

    /// <summary>
    ///     Generates SQL for the IN expression.
    /// </summary>
    /// <param name="inExpression">The expression to visit.</param>
    /// <param name="negated">Whether the given <paramref name="inExpression" /> is negated.</param>
    protected virtual void GenerateIn(InExpression inExpression, bool negated)
    {
        Check.DebugAssert(
            inExpression.ValuesParameter is null,
            "InExpression.ValuesParameter must have been expanded to constants before SQL generation (i.e. in SqlNullabilityProcessor)");

        Visit(inExpression.Item);
        _relationalCommandBuilder.Append(negated ? " NOT IN (" : " IN (");

        if (inExpression.Values is not null)
        {
            GenerateList(inExpression.Values, e => Visit(e));
        }
        else
        {
            _relationalCommandBuilder.AppendLine();

            using (_relationalCommandBuilder.Indent())
            {
                Visit(inExpression.Subquery);
            }

            _relationalCommandBuilder.AppendLine();
        }

        _relationalCommandBuilder.Append(")");
    }

    /// <inheritdoc />
    protected override Expression VisitAtTimeZone(AtTimeZoneExpression atTimeZoneExpression)
    {
        var requiresBrackets = RequiresParentheses(atTimeZoneExpression, atTimeZoneExpression.Operand);

        if (requiresBrackets)
        {
            _relationalCommandBuilder.Append("(");
        }

        Visit(atTimeZoneExpression.Operand);

        if (requiresBrackets)
        {
            _relationalCommandBuilder.Append(")");
        }

        _relationalCommandBuilder.Append(" AT TIME ZONE ");

        requiresBrackets = RequiresParentheses(atTimeZoneExpression, atTimeZoneExpression.TimeZone);

        if (requiresBrackets)
        {
            _relationalCommandBuilder.Append("(");
        }

        Visit(atTimeZoneExpression.TimeZone);

        if (requiresBrackets)
        {
            _relationalCommandBuilder.Append(")");
        }

        return atTimeZoneExpression;
    }

    /// <summary>
    ///     Gets a SQL operator for a SQL binary operation.
    /// </summary>
    /// <param name="binaryExpression">A SQL binary operation.</param>
    /// <returns>A string representation of the binary operator.</returns>
    protected virtual string GetOperator(SqlBinaryExpression binaryExpression)
        => OperatorMap[binaryExpression.OperatorType];

    /// <summary>
    ///     Generates a TOP construct in the relational command
    /// </summary>
    /// <param name="selectExpression">A select expression to use.</param>
    protected virtual void GenerateTop(SelectExpression selectExpression)
    {
    }

    /// <summary>
    ///     Generates the projection in the relational command
    /// </summary>
    /// <param name="selectExpression">A select expression to use.</param>
    protected virtual void GenerateProjection(SelectExpression selectExpression)
    {
        if (selectExpression.Projection.Any())
        {
            GenerateList(selectExpression.Projection, e => Visit(e));
        }
        else
        {
            GenerateEmptyProjection(selectExpression);
        }
    }

    /// <summary>
    ///     Generates the tables in the relational command
    /// </summary>
    /// <param name="selectExpression">A select expression to use.</param>
    protected virtual void GenerateTables(SelectExpression selectExpression)
    {
        if (selectExpression.Tables.Any())
        {
            _relationalCommandBuilder.AppendLine().Append("FROM ");

            GenerateList(selectExpression.Tables, e => Visit(e), sql => sql.AppendLine());
        }
        else
        {
            GeneratePseudoFromClause();
        }
    }

    /// <summary>
    ///     Generates an ORDER BY clause in the relational command
    /// </summary>
    /// <param name="selectExpression">A select expression to use.</param>
    protected virtual void GenerateOrderings(SelectExpression selectExpression)
    {
        if (selectExpression.Orderings.Any())
        {
            var orderings = selectExpression.Orderings.ToList();

            if (selectExpression.Limit == null
                && selectExpression.Offset == null)
            {
                orderings.RemoveAll(oe => oe.Expression is SqlConstantExpression or SqlParameterExpression);
            }

            if (orderings.Count > 0)
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("ORDER BY ");

                GenerateList(orderings, e => Visit(e));
            }
        }
    }

    /// <summary>
    ///     Generates a LIMIT...OFFSET... construct in the relational command
    /// </summary>
    /// <param name="selectExpression">A select expression to use.</param>
    protected virtual void GenerateLimitOffset(SelectExpression selectExpression)
    {
        if (selectExpression.Offset != null)
        {
            _relationalCommandBuilder.AppendLine()
                .Append("OFFSET ");

            Visit(selectExpression.Offset);

            _relationalCommandBuilder.Append(" ROWS");

            if (selectExpression.Limit != null)
            {
                _relationalCommandBuilder.Append(" FETCH NEXT ");

                Visit(selectExpression.Limit);

                _relationalCommandBuilder.Append(" ROWS ONLY");
            }
        }
        else if (selectExpression.Limit != null)
        {
            _relationalCommandBuilder.AppendLine()
                .Append("FETCH FIRST ");

            Visit(selectExpression.Limit);

            _relationalCommandBuilder.Append(" ROWS ONLY");
        }
    }

    private void GenerateList<T>(
        IReadOnlyList<T> items,
        Action<T> generationAction,
        Action<IRelationalCommandBuilder>? joinAction = null)
    {
        joinAction ??= (isb => isb.Append(", "));

        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                joinAction(_relationalCommandBuilder);
            }

            generationAction(items[i]);
        }
    }

    /// <inheritdoc />
    protected override Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
    {
        _relationalCommandBuilder.Append("CROSS JOIN ");
        Visit(crossJoinExpression.Table);

        return crossJoinExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
    {
        _relationalCommandBuilder.Append("CROSS APPLY ");
        Visit(crossApplyExpression.Table);

        return crossApplyExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
    {
        _relationalCommandBuilder.Append("OUTER APPLY ");
        Visit(outerApplyExpression.Table);

        return outerApplyExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
    {
        _relationalCommandBuilder.Append("INNER JOIN ");
        Visit(innerJoinExpression.Table);
        _relationalCommandBuilder.Append(" ON ");
        Visit(innerJoinExpression.JoinPredicate);

        return innerJoinExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitLeftJoin(LeftJoinExpression leftJoinExpression)
    {
        _relationalCommandBuilder.Append("LEFT JOIN ");
        Visit(leftJoinExpression.Table);
        _relationalCommandBuilder.Append(" ON ");
        Visit(leftJoinExpression.JoinPredicate);

        return leftJoinExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitScalarSubquery(ScalarSubqueryExpression scalarSubqueryExpression)
    {
        _relationalCommandBuilder.AppendLine("(");
        using (_relationalCommandBuilder.Indent())
        {
            Visit(scalarSubqueryExpression.Subquery);
        }

        _relationalCommandBuilder.Append(")");

        return scalarSubqueryExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
    {
        _relationalCommandBuilder.Append("ROW_NUMBER() OVER(");
        if (rowNumberExpression.Partitions.Any())
        {
            _relationalCommandBuilder.Append("PARTITION BY ");
            GenerateList(rowNumberExpression.Partitions, e => Visit(e));
            _relationalCommandBuilder.Append(" ");
        }

        _relationalCommandBuilder.Append("ORDER BY ");
        GenerateList(rowNumberExpression.Orderings, e => Visit(e));
        _relationalCommandBuilder.Append(")");

        return rowNumberExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitRowValue(RowValueExpression rowValueExpression)
    {
        Sql.Append("(");

        var values = rowValueExpression.Values;
        var count = values.Count;
        for (var i = 0; i < count; i++)
        {
            if (i > 0)
            {
                Sql.Append(", ");
            }

            Visit(values[i]);
        }

        Sql.Append(")");

        return rowValueExpression;
    }

    /// <summary>
    ///     Generates a set operation in the relational command.
    /// </summary>
    /// <param name="setOperation">A set operation to print.</param>
    protected virtual void GenerateSetOperation(SetOperationBase setOperation)
    {
        GenerateSetOperationOperand(setOperation, setOperation.Source1);
        _relationalCommandBuilder
            .AppendLine()
            .Append(GetSetOperation(setOperation))
            .AppendLine(setOperation.IsDistinct ? string.Empty : " ALL");
        GenerateSetOperationOperand(setOperation, setOperation.Source2);

        static string GetSetOperation(SetOperationBase operation)
            => operation switch
            {
                ExceptExpression => "EXCEPT",
                IntersectExpression => "INTERSECT",
                UnionExpression => "UNION",
                _ => throw new InvalidOperationException(CoreStrings.UnknownEntity("SetOperationType"))
            };
    }

    /// <summary>
    ///     Generates an operand for a given set operation in the relational command.
    /// </summary>
    /// <param name="setOperation">A set operation to use.</param>
    /// <param name="operand">A set operation operand to print.</param>
    protected virtual void GenerateSetOperationOperand(SetOperationBase setOperation, SelectExpression operand)
    {
        // INTERSECT has higher precedence over UNION and EXCEPT, but otherwise evaluation is left-to-right.
        // To preserve meaning, add parentheses whenever a set operation is nested within a different set operation.
        if (IsNonComposedSetOperation(operand)
            && operand.Tables[0].GetType() != setOperation.GetType())
        {
            _relationalCommandBuilder.AppendLine("(");
            using (_relationalCommandBuilder.Indent())
            {
                Visit(operand);
            }

            _relationalCommandBuilder.AppendLine().Append(")");
        }
        else
        {
            Visit(operand);
        }
    }

    private void GenerateSetOperationHelper(SetOperationBase setOperation)
    {
        _relationalCommandBuilder.AppendLine("(");
        using (_relationalCommandBuilder.Indent())
        {
            GenerateSetOperation(setOperation);
        }

        _relationalCommandBuilder.AppendLine()
            .Append(")")
            .Append(AliasSeparator)
            .Append(_sqlGenerationHelper.DelimitIdentifier(setOperation.Alias));
    }

    /// <inheritdoc />
    protected override Expression VisitExcept(ExceptExpression exceptExpression)
    {
        GenerateSetOperationHelper(exceptExpression);

        return exceptExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitIntersect(IntersectExpression intersectExpression)
    {
        GenerateSetOperationHelper(intersectExpression);

        return intersectExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitUnion(UnionExpression unionExpression)
    {
        GenerateSetOperationHelper(unionExpression);

        return unionExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitUpdate(UpdateExpression updateExpression)
    {
        var selectExpression = updateExpression.SelectExpression;

        if (selectExpression is
            {
                Offset: null,
                Limit: null,
                Having: null,
                Orderings: [],
                GroupBy: [],
                Projection: [],
            }
            && (selectExpression.Tables.Count == 1
                || !ReferenceEquals(selectExpression.Tables[0], updateExpression.Table)
                || selectExpression.Tables[1] is InnerJoinExpression
                || selectExpression.Tables[1] is CrossJoinExpression))
        {
            _relationalCommandBuilder.Append("UPDATE ");
            Visit(updateExpression.Table);
            _relationalCommandBuilder.AppendLine();
            _relationalCommandBuilder.Append("SET ");
            _relationalCommandBuilder.Append(
                $"{_sqlGenerationHelper.DelimitIdentifier(updateExpression.ColumnValueSetters[0].Column.Name)} = ");
            Visit(updateExpression.ColumnValueSetters[0].Value);
            using (_relationalCommandBuilder.Indent())
            {
                foreach (var columnValueSetter in updateExpression.ColumnValueSetters.Skip(1))
                {
                    _relationalCommandBuilder.AppendLine(",");
                    _relationalCommandBuilder.Append($"{_sqlGenerationHelper.DelimitIdentifier(columnValueSetter.Column.Name)} = ");
                    Visit(columnValueSetter.Value);
                }
            }

            var predicate = selectExpression.Predicate;
            var firstTablePrinted = false;
            if (selectExpression.Tables.Count > 1)
            {
                _relationalCommandBuilder.AppendLine().Append("FROM ");
                for (var i = 0; i < selectExpression.Tables.Count; i++)
                {
                    var table = selectExpression.Tables[i];
                    var joinExpression = table as JoinExpressionBase;

                    if (ReferenceEquals(updateExpression.Table, joinExpression?.Table ?? table))
                    {
                        LiftPredicate(table);
                        continue;
                    }

                    if (firstTablePrinted)
                    {
                        _relationalCommandBuilder.AppendLine();
                    }
                    else
                    {
                        firstTablePrinted = true;
                        LiftPredicate(table);
                        table = joinExpression?.Table ?? table;
                    }

                    Visit(table);

                    void LiftPredicate(TableExpressionBase joinTable)
                    {
                        if (joinTable is PredicateJoinExpressionBase predicateJoinExpression)
                        {
                            predicate = predicate == null
                                ? predicateJoinExpression.JoinPredicate
                                : new SqlBinaryExpression(
                                    ExpressionType.AndAlso,
                                    predicateJoinExpression.JoinPredicate,
                                    predicate,
                                    typeof(bool),
                                    predicate.TypeMapping);
                        }
                    }
                }
            }

            if (predicate != null)
            {
                _relationalCommandBuilder.AppendLine().Append("WHERE ");
                Visit(predicate);
            }

            return updateExpression;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(nameof(RelationalQueryableExtensions.ExecuteUpdate)));
    }

    /// <inheritdoc />
    protected override Expression VisitValues(ValuesExpression valuesExpression)
    {
        _relationalCommandBuilder.Append("(");

        GenerateValues(valuesExpression);

        _relationalCommandBuilder
            .Append(")")
            .Append(AliasSeparator)
            .Append(_sqlGenerationHelper.DelimitIdentifier(valuesExpression.Alias));

        return valuesExpression;
    }

    /// <summary>
    ///     Generates a VALUES expression.
    /// </summary>
    protected virtual void GenerateValues(ValuesExpression valuesExpression)
    {
        if (valuesExpression.RowValues.Count == 0)
        {
            throw new InvalidOperationException(RelationalStrings.EmptyCollectionNotSupportedAsInlineQueryRoot);
        }

        var rowValues = valuesExpression.RowValues;

        // Some databases support providing the names of columns projected out of VALUES, e.g.
        // SQL Server/PG: (VALUES (1, 3), (2, 4)) AS x(a, b). Others unfortunately don't; so by default, we extract out the first row,
        // and generate a SELECT for it with the names, and a UNION ALL over the rest of the values.
        _relationalCommandBuilder.Append("SELECT ");

        Check.DebugAssert(rowValues.Count > 0, "rowValues.Count > 0");
        var firstRowValues = rowValues[0].Values;
        for (var i = 0; i < firstRowValues.Count; i++)
        {
            if (i > 0)
            {
                _relationalCommandBuilder.Append(", ");
            }

            Visit(firstRowValues[i]);

            _relationalCommandBuilder
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(valuesExpression.ColumnNames[i]));
        }

        if (rowValues.Count > 1)
        {
            _relationalCommandBuilder.Append(" UNION ALL VALUES ");

            for (var i = 1; i < rowValues.Count; i++)
            {
                if (i > 1)
                {
                    _relationalCommandBuilder.Append(", ");
                }

                Visit(valuesExpression.RowValues[i]);
            }
        }
    }

    /// <inheritdoc />
    protected override Expression VisitJsonScalar(JsonScalarExpression jsonScalarExpression)
        => throw new InvalidOperationException(
            RelationalStrings.JsonNodeMustBeHandledByProviderSpecificVisitor);

    /// <summary>
    ///     Returns a bool value indicating if the inner SQL expression required to be put inside parenthesis when generating SQL for outer
    ///     SQL expression.
    /// </summary>
    /// <param name="outerExpression">The outer expression which provides context in which SQL is being generated.</param>
    /// <param name="innerExpression">The inner expression which may need to be put inside parenthesis.</param>
    /// <returns>A bool value indicating that parenthesis is required or not. </returns>
    protected virtual bool RequiresParentheses(SqlExpression outerExpression, SqlExpression innerExpression)
    {
        int outerPrecedence, innerPrecedence;

        // Convert is rendered as a function (CAST()) and not as an operator, so we never need to add parentheses around the inner
        if (outerExpression is SqlUnaryExpression { OperatorType: ExpressionType.Convert })
        {
            return false;
        }

        switch (innerExpression)
        {
            case SqlUnaryExpression innerUnaryExpression:
            {
                // If the same unary operator is used in both outer and inner (e.g. NOT NOT), no parentheses are needed
                if (outerExpression is SqlUnaryExpression outerUnary
                    && innerUnaryExpression.OperatorType == outerUnary.OperatorType)
                {
                    // ... except for double negative (--), which is interpreted as a comment in SQL
                    return innerUnaryExpression.OperatorType == ExpressionType.Negate;
                }

                // If the provider defined precedence for the two expression, use that
                if (TryGetOperatorInfo(outerExpression, out outerPrecedence, out _)
                    && TryGetOperatorInfo(innerExpression, out innerPrecedence, out _))
                {
                    return outerPrecedence >= innerPrecedence;
                }

                // Otherwise, wrap IS (NOT) NULL operation, except if it's in a logical operator
                if (innerUnaryExpression.OperatorType is ExpressionType.Equal or ExpressionType.NotEqual
                    && outerExpression is not SqlBinaryExpression
                    {
                        OperatorType: ExpressionType.AndAlso or ExpressionType.OrElse or ExpressionType.Not
                    })
                {
                    return true;
                }

                return false;
            }

            case SqlBinaryExpression innerBinaryExpression:
            {
                // Precedence-wise AND is above OR but we still add parenthesis for ease of understanding
                if (innerBinaryExpression.OperatorType is ExpressionType.AndAlso or ExpressionType.And
                    && outerExpression is SqlBinaryExpression { OperatorType: ExpressionType.OrElse or ExpressionType.Or })
                {
                    return true;
                }

                // If the provider defined precedence for the two expression, use that
                if (TryGetOperatorInfo(outerExpression, out outerPrecedence, out var isOuterAssociative)
                    && TryGetOperatorInfo(innerExpression, out innerPrecedence, out _))
                {
                    return outerPrecedence.CompareTo(innerPrecedence) switch
                    {
                        > 0 => true,
                        < 0 => false,

                        // If both operators have the same precedence, add parentheses unless they're the same operator, and
                        // that operator is associative (e.g. a + b + c)
                        _ => outerExpression is not SqlBinaryExpression outerBinary
                            || outerBinary.OperatorType != innerBinaryExpression.OperatorType
                            || !isOuterAssociative
                            // Arithmetic operators on floating points aren't associative, because of rounding errors.
                            || outerExpression.Type == typeof(float)
                            || outerExpression.Type == typeof(double)
                            || innerExpression.Type == typeof(float)
                            || innerExpression.Type == typeof(double)
                    };
                }

                // Even if the provider doesn't define precedence, assume that AND has less precedence than any other binary operator
                // except for OR. This is universal, was our behavior before introducing provider precedence and removes the need for many
                // parentheses. Do the same for OR (though here we add parentheses around inner AND just for readability).
                if (outerExpression is SqlBinaryExpression outerBinary2)
                {
                    if (outerBinary2.OperatorType == ExpressionType.AndAlso)
                    {
                        return innerBinaryExpression.OperatorType == ExpressionType.OrElse;
                    }

                    if (outerBinary2.OperatorType == ExpressionType.OrElse)
                    {
                        // Precedence-wise AND is above OR but we still add parentheses for ease of understanding
                        return innerBinaryExpression.OperatorType == ExpressionType.AndAlso;
                    }
                }

                // Otherwise always parenthesize for safety
                return true;
            }

            case CollateExpression or LikeExpression or AtTimeZoneExpression or JsonScalarExpression:
                return !TryGetOperatorInfo(outerExpression, out outerPrecedence, out _)
                    || !TryGetOperatorInfo(innerExpression, out innerPrecedence, out _)
                    || outerPrecedence >= innerPrecedence;

            default:
                return false;
        }
    }

    /// <summary>
    ///     Returns a numeric value representing the precedence of the given <paramref name="expression" />, as well as its associativity.
    ///     These control whether parentheses are generated around the expression.
    /// </summary>
    /// <param name="expression">The expression for which to get the precedence and associativity.</param>
    /// <param name="precedence">
    ///     If the method returned <see langword="true" />, contains the precedence of the provided <paramref name="expression" />.
    ///     Otherwise, contains default values.
    /// </param>
    /// <param name="isAssociative">
    ///     If the method returned <see langword="true" />, contains the associativity of the provided <paramref name="expression" />.
    ///     Otherwise, contains default values.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the expression operator info is known and was returned in <paramref name="precedence" /> and
    ///     <paramref name="isAssociative" />. Otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     The default implementation always returns false, so that parentheses almost always get added. Providers can override this method
    ///     to remove parentheses where they aren't necessary.
    /// </remarks>
    protected virtual bool TryGetOperatorInfo(SqlExpression expression, out int precedence, out bool isAssociative)
    {
        (precedence, isAssociative) = (default, default);
        return false;
    }
}
