// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using static System.Linq.Expressions.Expression;
using Constant = System.Reflection.Metadata.Constant;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlTreeQuoter : SqlExpressionVisitor, ISqlTreeQuoter
{
    private readonly List<ParameterExpression> _blockVariables = new();
    private readonly List<Expression> _blockExpressions = new();
    private readonly Dictionary<SelectExpression, ParameterExpression> _selectExpressionMap = new();

    private ParameterExpression _relationalModelParameter = null!;
    private ParameterExpression _relationalTypeMappingSourceParameter = null!;

    private string _rootSelectVariableName = null!;
    private HashSet<string> _variableNames = null!;

    private bool _seenRootExpression;

    #region Constructor and method infos

    private static ConstructorInfo? _annotationConstructor;
    private static ConstructorInfo? _atTimeZoneConstructor;
    private static ConstructorInfo? _caseConstructorWithOperand;
    private static ConstructorInfo? _caseConstructorWithoutOperand;
    private static ConstructorInfo? _caseWhenClauseConstructor;
    private static ConstructorInfo? _collateConstructor;
    private static ConstructorInfo? _concreteColumnConstructor;
    private static ConstructorInfo? _columnValueSetterConstructor;
    private static MethodInfo? _constantFactoryMethod;
    private static ConstructorInfo? _crossApplyConstructor;
    private static ConstructorInfo? _crossJoinConstructor;
    private static ConstructorInfo? _deleteConstructor;
    private static ConstructorInfo? _distinctConstructor;
    private static ConstructorInfo? _exceptConstructor;
    private static ConstructorInfo? _existsConstructor;
    // private static ConstructorInfo? _fromSqlConstructor;
    private static ConstructorInfo? _inConstructorWithSubquery;
    private static ConstructorInfo? _inConstructorWithValues;
    private static ConstructorInfo? _intersectConstructor;
    private static ConstructorInfo? _likeConstructor;
    private static ConstructorInfo? _innerJoinConstructor;
    private static ConstructorInfo? _leftJoinConstructor;
    private static ConstructorInfo? _orderingConstructor;
    private static ConstructorInfo? _outerApplyConstructor;
    private static MethodInfo? _parameterFactoryMethod;
    private static ConstructorInfo? _projectionConstructor;
    // private static ConstructorInfo? _tableValuedFunctionConstructor;
    private static MethodInfo? _relationalModelFindTableMethod;
    private static ConstructorInfo? _rowNumberConstructor;
    private static ConstructorInfo? _scalarSubqueryConstructor;
    private static ConstructorInfo? _selectConstructor;
    private static MethodInfo? _selectPopulateClausesMethod;
    private static ConstructorInfo? _sqlBinaryConstructor;
    private static ConstructorInfo? _sqlConstantConstructor;
    private static ConstructorInfo? _sqlFragmentConstructor;
    private static ConstructorInfo? _sqlFunctionConstructor;
    private static ConstructorInfo? _sqlParameterConstructor;
    private static ConstructorInfo? _sqlUnaryConstructor;
    private static ConstructorInfo? _tableConstructor;
    private static ConstructorInfo? _tableReferenceConstructor;
    private static ConstructorInfo? _unionConstructor;
    private static ConstructorInfo? _updateConstructor;
    // private static ConstructorInfo? _jsonScalarConstructor;

    private static MethodInfo? _hashSetAddMethod;

    private static readonly MethodInfo RelationalTypeMappingSourceFindMappingMethod
        = typeof(RelationalTypeMappingSource)
            .GetMethod(nameof(RelationalTypeMappingSource.FindMapping),
                new[]
                {
                    typeof(Type), typeof(string), typeof(bool), typeof(bool), typeof(int), typeof(bool), typeof(bool), typeof(int),
                    typeof(int)
                })!;

    #endregion

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public BlockExpression Quote(Expression expression, string rootSelectVariableName, HashSet<string> variableNames)
    {
        _rootSelectVariableName = rootSelectVariableName;
        _variableNames = variableNames;
        _seenRootExpression = false;

        _blockVariables.Clear();
        _blockExpressions.Clear();
        _selectExpressionMap.Clear();
        _relationalModelParameter = Parameter(typeof(RelationalModel), "relationalModel");
        _relationalTypeMappingSourceParameter = Parameter(typeof(RelationalTypeMappingSource), "relationalTypeMappingSource");

        Visit(expression);

        return Block(_blockVariables, _blockExpressions);
    }

    /// <inheritdoc />
    protected override Expression VisitAtTimeZone(AtTimeZoneExpression atTimeZoneExpression)
        => New(
            _atTimeZoneConstructor ??= typeof(AtTimeZoneExpression).GetConstructor(
                new[] { typeof(SqlExpression), typeof(SqlExpression), typeof(Type), typeof(RelationalTypeMapping) })!,
            Visit(atTimeZoneExpression.Operand),
            Visit(atTimeZoneExpression.TimeZone),
            Constant(atTimeZoneExpression.Type),
            RenderFindTypeMapping(atTimeZoneExpression.TypeMapping));

    /// <inheritdoc />
    protected override Expression VisitCase(CaseExpression caseExpression)
    {
        var whenClauses = NewArrayInit(
            typeof(CaseWhenClause),
            initializers: caseExpression.WhenClauses
                .Select(c => New(
                    _caseWhenClauseConstructor ??=
                        typeof(CaseWhenClause).GetConstructor(new[] { typeof(SqlExpression), typeof(SqlExpression) })!,
                    Visit(c.Test),
                    Visit(c.Result))));

        return caseExpression.Operand is null
            ? New(
                _caseConstructorWithoutOperand ??=
                    typeof(CaseExpression).GetConstructor(new[] { typeof(IReadOnlyList<CaseWhenClause>), typeof(SqlExpression) })!,
                whenClauses,
                VisitOrNull(caseExpression.ElseResult))
            : New(
                _caseConstructorWithOperand ??= typeof(CaseExpression).GetConstructor(
                    new[] { typeof(SqlExpression), typeof(IReadOnlyList<CaseWhenClause>), typeof(SqlExpression) })!,
                Visit(caseExpression.Operand),
                whenClauses,
                VisitOrNull(caseExpression.ElseResult));
    }

    /// <inheritdoc />
    protected override Expression VisitCollate(CollateExpression collateExpression)
        => New(
            _collateConstructor ??= typeof(CollateExpression).GetConstructor(new[] { typeof(SqlExpression), typeof(string) })!,
            Visit(collateExpression.Operand),
            Constant(collateExpression.Collation));

    /// <inheritdoc />
    protected override Expression VisitColumn(ColumnExpression columnExpression)
    {
        if (columnExpression is not ConcreteColumnExpression concreteColumnExpression)
        {
            throw new NotSupportedException("Unknown column type: " + columnExpression.GetType().Name);
        }

        var found = _selectExpressionMap.TryGetValue(concreteColumnExpression.SelectExpression, out var selectExpressionVariable);
        Debug.Assert(found, "Could not find SelectExpression in select expression map");

        return New(
            _concreteColumnConstructor ??= typeof(ConcreteColumnExpression).GetConstructor(
                new[] { typeof(string), typeof(TableReferenceExpression), typeof(Type), typeof(RelationalTypeMapping), typeof(bool) })!,
            Constant(columnExpression.Name),
            New(
                _tableReferenceConstructor ??=
                    typeof(TableReferenceExpression).GetConstructor(new[] { typeof(SelectExpression), typeof(string) })!,
                selectExpressionVariable!,
                Constant(columnExpression.TableAlias)),
            Constant(columnExpression.Type),
            RenderFindTypeMapping(columnExpression.TypeMapping),
            Constant(columnExpression.IsNullable));
    }

    /// <inheritdoc />
    protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
        => New(
            _crossApplyConstructor ??=
                typeof(CrossApplyExpression).GetConstructor(new[] { typeof(TableExpressionBase), typeof(IEnumerable<IAnnotation>) })!,
            Visit(crossApplyExpression.Table),
            RenderAnnotationArray(crossApplyExpression.GetAnnotations()));

    /// <inheritdoc />
    protected override Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
        => New(
            _crossJoinConstructor ??=
                typeof(CrossJoinExpression).GetConstructor(new[] { typeof(TableExpressionBase), typeof(IEnumerable<IAnnotation>) })!,
            Visit(crossJoinExpression.Table),
            RenderAnnotationArray(crossJoinExpression.GetAnnotations()));

    /// <inheritdoc />
    protected override Expression VisitDelete(DeleteExpression deleteExpression)
    {
        if (_seenRootExpression)
        {
            throw new NotSupportedException($"{nameof(DeleteExpression)} in non-root context");
        }

        _seenRootExpression = true;

        var deleteExpressionVariable = Parameter(typeof(DeleteExpression), _rootSelectVariableName);
        _blockVariables.Add(deleteExpressionVariable);
        _blockExpressions.Add(
            Assign(
                deleteExpressionVariable,
                New(
                    _deleteConstructor ??= typeof(DeleteExpression).GetConstructor(
                        new[] { typeof(TableExpression), typeof(SelectExpression), typeof(ISet<string>) })!,
                    Visit(deleteExpression.Table),
                    Visit(deleteExpression.SelectExpression),
                    CreateTagsExpression(deleteExpression.Tags))));

        return deleteExpressionVariable;
    }

    /// <inheritdoc />
    protected override Expression VisitDistinct(DistinctExpression distinctExpression)
        => New(
            _distinctConstructor ??= typeof(DistinctExpression).GetConstructor(new[] { typeof(SqlExpression) })!,
            Visit(distinctExpression.Operand));

    /// <inheritdoc />
    protected override Expression VisitExcept(ExceptExpression exceptExpression)
        => New(
            _exceptConstructor ??= typeof(ExceptExpression).GetConstructor(
                new[]
                {
                    typeof(string), typeof(SelectExpression), typeof(SelectExpression), typeof(bool), typeof(IEnumerable<IAnnotatable>)
                })!,
            Constant(exceptExpression.Alias, typeof(string)),
            Visit(exceptExpression.Source1),
            Visit(exceptExpression.Source2),
            Constant(exceptExpression.IsDistinct),
            RenderAnnotationArray(exceptExpression.GetAnnotations()));

    /// <inheritdoc />
    protected override Expression VisitExists(ExistsExpression existsExpression)
        => New(
            _existsConstructor ??=
                typeof(ExistsExpression).GetConstructor(new[] { typeof(SelectExpression), typeof(bool), typeof(RelationalTypeMapping) })!,
            Visit(existsExpression.Subquery),
            Constant(existsExpression),
            RenderFindTypeMapping(existsExpression.TypeMapping));

    /// <inheritdoc />
    protected override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
        => throw new NotImplementedException();
    // => New(
    //     _fromSqlConstructor ??= typeof(FromSqlExpression).GetConstructor(new[] { typeof(string), typeof(ITableBase), typeof(string), typeof(Expression), typeof(IEnumerable<IAnnotation>) })!,
    //     Constant(fromSqlExpression.Alias, typeof(string)),
    //     fromSqlExpression.Table, // TODO
    //     Constant(fromSqlExpression.Sql),
    //     Visit(fromSqlExpression.Arguments),
    //     Constant(fromSqlExpression.GetAnnotations().ToArray());

    /// <inheritdoc />
    protected override Expression VisitIn(InExpression inExpression)
        => inExpression.Subquery is null
            ? New(
                _inConstructorWithValues ??= typeof(InExpression).GetConstructor(
                    new[] { typeof(SqlExpression), typeof(SqlExpression), typeof(bool), typeof(RelationalTypeMapping) })!,
                Visit(inExpression.Item),
                Visit(inExpression.Values!),
                Constant(inExpression.IsNegated),
                RenderFindTypeMapping(inExpression.TypeMapping))
            : New(
                _inConstructorWithSubquery ??= typeof(InExpression).GetConstructor(
                    new[] { typeof(SqlExpression), typeof(SelectExpression), typeof(bool), typeof(RelationalTypeMapping) })!,
                Visit(inExpression.Item),
                Visit(inExpression.Subquery!),
                Constant(inExpression.Negate()),
                RenderFindTypeMapping(inExpression.TypeMapping));

    /// <inheritdoc />
    protected override Expression VisitIntersect(IntersectExpression intersectExpression)
        => New(
            _intersectConstructor ??= typeof(IntersectExpression).GetConstructor(
                new[]
                {
                    typeof(string), typeof(SelectExpression), typeof(SelectExpression), typeof(bool), typeof(IEnumerable<IAnnotation>)
                })!,
            Constant(intersectExpression.Alias, typeof(string)),
            Visit(intersectExpression.Source1),
            Visit(intersectExpression.Source2),
            Constant(intersectExpression.IsDistinct),
            RenderAnnotationArray(intersectExpression.GetAnnotations()));

    /// <inheritdoc />
    protected override Expression VisitLike(LikeExpression likeExpression)
        => New(
            _likeConstructor ??= typeof(LikeExpression).GetConstructor(
                new[] { typeof(SqlExpression), typeof(SqlExpression), typeof(SqlExpression), typeof(RelationalTypeMapping) })!,
            Visit(likeExpression.Match),
            Visit(likeExpression.Pattern),
            VisitOrNull(likeExpression.EscapeChar),
            RenderFindTypeMapping(likeExpression.TypeMapping));

    /// <inheritdoc />
    protected override Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
        => New(
            _innerJoinConstructor ??= typeof(InnerJoinExpression).GetConstructor(
                new[] { typeof(TableExpressionBase), typeof(SqlExpression), typeof(IEnumerable<IAnnotation>) })!,
            Visit(innerJoinExpression.Table),
            Visit(innerJoinExpression.JoinPredicate),
            RenderAnnotationArray(innerJoinExpression.GetAnnotations()));

    /// <inheritdoc />
    protected override Expression VisitLeftJoin(LeftJoinExpression leftJoinExpression)
        => New(
            _leftJoinConstructor ??= typeof(LeftJoinExpression).GetConstructor(
                new[] { typeof(TableExpressionBase), typeof(SqlExpression), typeof(IEnumerable<IAnnotation>) })!,
            Visit(leftJoinExpression.Table),
            Visit(leftJoinExpression.JoinPredicate),
            RenderAnnotationArray(leftJoinExpression.GetAnnotations()));

    /// <inheritdoc />
    protected override Expression VisitOrdering(OrderingExpression orderingExpression)
        => New(
            _orderingConstructor ??= typeof(OrderingExpression).GetConstructor(new[] { typeof(SqlExpression), typeof(bool) })!,
            Visit(orderingExpression.Expression),
            Constant(orderingExpression.IsAscending));

    /// <inheritdoc />
    protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
        => New(
            _outerApplyConstructor ??=
                typeof(OuterApplyExpression).GetConstructor(new[] { typeof(TableExpressionBase), typeof(IEnumerable<IAnnotation>) })!,
            Visit(outerApplyExpression.Table),
            RenderAnnotationArray(outerApplyExpression.GetAnnotations()));

    /// <inheritdoc />
    protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        => New(
            _projectionConstructor ??=
                typeof(ProjectionExpression).GetConstructor(new[] { typeof(SqlExpression), typeof(string) })!,
            Visit(projectionExpression.Expression),
            Constant(projectionExpression.Alias));

    /// <inheritdoc />
    protected override Expression VisitTableValuedFunction(TableValuedFunctionExpression tableValuedFunctionExpression)
        => throw new NotImplementedException();
    // => New(
    //     _tableValuedFunctionConstructor ??= typeof(TableValuedFunctionExpression).GetConstructor(
    //         new[]
    //         {
    //             typeof(string), typeof(IStoreFunction), typeof(IReadOnlyList<SqlExpression>), typeof(IEnumerable<IAnnotation>)
    //         })!,
    //     Constant(tableValuedFunctionExpression.Alias, typeof(string)),
    //     tableValuedFunctionExpression.StoreFunction,
    //     Constant(tableValuedFunctionExpression.Arguments),
    //     Constant(tableValuedFunctionExpression.GetAnnotations().ToArray());

    /// <inheritdoc />
    protected override Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
        => New(
            _rowNumberConstructor ??= typeof(RowNumberExpression).GetConstructor(
                new[] { typeof(IReadOnlyList<SqlExpression>), typeof(IReadOnlyList<OrderingExpression>), typeof(RelationalTypeMapping) })!,
            NewArrayInit(typeof(SqlExpression), initializers: rowNumberExpression.Orderings.Select(Visit)!),
            RenderFindTypeMapping(rowNumberExpression.TypeMapping));

    /// <inheritdoc />
    protected override Expression VisitScalarSubquery(ScalarSubqueryExpression scalarSubqueryExpression)
        => New(
            _scalarSubqueryConstructor ??= typeof(ScalarSubqueryExpression).GetConstructor(new[] { typeof(SelectExpression) })!,
            Visit(scalarSubqueryExpression.Subquery));

    /// <inheritdoc />
    protected sealed override Expression VisitSelect(SelectExpression selectExpression)
    {
        // Select is rendered out in two steps:
        // 1. Code that instantiates an empty SelectExpression - with only the alias and tables - assigning it to a variable.
        // 2. A PopulateClauses call that populates all the other clauses, which is where we recursively visit them.
        // This is necessary to allow subqueries to be referenced by their containing SelectExpressions, as well as by their contained
        // columns).

        // The very first SelectExpression we encounter is the root one, so we give it an externally given name (it needs to be
        // referenced later from the outside).
        // For other, nested select expressions, generate a uniquified name.
        string selectExpressionVariableName;
        if (_seenRootExpression)
        {
            var baseName = _rootSelectVariableName + "Subquery";
            var i = 1;
            do
            {
                selectExpressionVariableName = baseName + (i++);
            } while (_variableNames.Contains(selectExpressionVariableName));

            _variableNames.Add(selectExpressionVariableName);
        }
        else
        {
            selectExpressionVariableName = _rootSelectVariableName;
            _seenRootExpression = true;
        }

        var selectExpressionVariable = Parameter(typeof(SelectExpression), selectExpressionVariableName);
        _selectExpressionMap[selectExpression] = selectExpressionVariable;
        _blockVariables.Add(selectExpressionVariable);

        _blockExpressions.Add(
            Assign(
                selectExpressionVariable,
                New(
                    _selectConstructor ??= typeof(SelectExpression).GetConstructor(new[] { typeof(string) })!,
                    Constant(selectExpression.Alias, typeof(string)))));

        _blockExpressions.Add(
            Call(
                selectExpressionVariable,
                _selectPopulateClausesMethod ??= typeof(SelectExpression).GetMethod(
                    nameof(SelectExpression.PopulateClauses),
                    new[]
                    {
                        typeof(IReadOnlyList<TableExpressionBase>), // tables
                        typeof(SqlExpression), // predicate
                        typeof(SqlExpression), // limit
                        typeof(SqlExpression), // offset
                        typeof(IReadOnlyList<SqlExpression>), // groupby
                        typeof(SqlExpression), // having
                        typeof(IReadOnlyList<OrderingExpression>), // orderings
                        typeof(bool), // isDistinct
                        typeof(IReadOnlyList<ProjectionExpression>), // projections
                        typeof(ISet<string>), // tags
                        // TODO:
                        // typeof(SortedDictionary<string, IAnnotation>) // annotations
                    })!,
                NewArrayInit(
                    typeof(TableExpressionBase),
                    initializers: selectExpression.Tables.Select(Visit)!),
                VisitOrNull(selectExpression.Predicate),
                VisitOrNull(selectExpression.Limit),
                VisitOrNull(selectExpression.Offset),
                NewArrayInit(typeof(SqlExpression), initializers: selectExpression.GroupBy.Select(Visit)!),
                VisitOrNull(selectExpression.Having),
                NewArrayInit(typeof(OrderingExpression), initializers: selectExpression.Orderings.Select(Visit)!),
                Constant(selectExpression.IsDistinct),
                NewArrayInit(typeof(ProjectionExpression), initializers: selectExpression.Projection.Select(Visit)!),
                CreateTagsExpression(selectExpression.Tags)));

        return selectExpressionVariable;
    }

    /// <inheritdoc />
    protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
        => New(
            _sqlBinaryConstructor ??= typeof(SqlBinaryExpression).GetConstructor(
                new[]
                {
                    typeof(ExpressionType), typeof(SqlExpression), typeof(SqlExpression), typeof(Type), typeof(RelationalTypeMapping)
                })!,
            Constant(sqlBinaryExpression.OperatorType),
            Visit(sqlBinaryExpression.Left),
            Visit(sqlBinaryExpression.Right),
            Constant(sqlBinaryExpression.Type),
            RenderFindTypeMapping(sqlBinaryExpression.TypeMapping));

    /// <inheritdoc />
    protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
        => New(
            _sqlConstantConstructor ??= typeof(SqlConstantExpression).GetConstructor(
                new[] { typeof(ConstantExpression), typeof(RelationalTypeMapping) })!,
            Call(
                _constantFactoryMethod ??= typeof(Expression)
                    .GetMethod(nameof(Constant), new[] { typeof(object), typeof(Type) })!,
                sqlConstantExpression.Type.IsValueType
                    ? Convert(
                        Constant(sqlConstantExpression.Value, sqlConstantExpression.Type), typeof(object))
                    : Constant(sqlConstantExpression.Value, sqlConstantExpression.Type),
                Constant(sqlConstantExpression.Type)),
            RenderFindTypeMapping(sqlConstantExpression.TypeMapping));

    /// <inheritdoc />
    protected override Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression)
        => New(
            _sqlFragmentConstructor ??= typeof(SqlFragmentExpression).GetConstructor(new[] { typeof(string) })!,
            Constant(sqlFragmentExpression.Sql));

    /// <inheritdoc />
    protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        => New(
            _sqlFunctionConstructor ??= typeof(SqlFunctionExpression).GetConstructor(
                new[]
                {
                    typeof(SqlExpression), typeof(string), typeof(string), typeof(bool), typeof(IEnumerable<SqlExpression>),
                    typeof(bool), typeof(bool), typeof(IEnumerable<bool>), typeof(bool), typeof(Type), typeof(RelationalTypeMapping)
                })!,
            VisitOrNull(sqlFunctionExpression.Instance),
            Constant(sqlFunctionExpression.Schema, typeof(string)),
            Constant(sqlFunctionExpression.Name),
            Constant(sqlFunctionExpression.IsNiladic),
            sqlFunctionExpression.Arguments is null
                ? Constant(null, typeof(IEnumerable<SqlExpression>))
                : NewArrayInit(typeof(SqlExpression), initializers: sqlFunctionExpression.Arguments.Select(Visit)!),
            Constant(sqlFunctionExpression.IsNullable),
            Constant(sqlFunctionExpression.InstancePropagatesNullability, typeof(bool?)),
            sqlFunctionExpression.ArgumentsPropagateNullability is null
                ? Constant(null, typeof(IEnumerable<bool>))
                : NewArrayInit(
                    typeof(bool), initializers: sqlFunctionExpression.ArgumentsPropagateNullability.Select(n => Constant(n))),
            Constant(sqlFunctionExpression.IsBuiltIn),
            Constant(sqlFunctionExpression.Type),
            RenderFindTypeMapping(sqlFunctionExpression.TypeMapping));

    /// <inheritdoc />
    protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
        => New(
            _sqlParameterConstructor ??= typeof(SqlParameterExpression).GetConstructor(new[]
            {
                typeof(ParameterExpression), typeof(RelationalTypeMapping)
            })!,
            Call(
                _parameterFactoryMethod ??= typeof(Expression)
                    .GetMethod(nameof(Parameter), new[] { typeof(Type), typeof(string) })!,
                Constant(sqlParameterExpression.Type),
                Constant(sqlParameterExpression.Name, typeof(string))),
            RenderFindTypeMapping(sqlParameterExpression.TypeMapping));

    /// <inheritdoc />
    protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
        => New(
            _sqlUnaryConstructor ??= typeof(SqlUnaryExpression).GetConstructor(new[]
            {
                typeof(ExpressionType), typeof(SqlExpression), typeof(Type), typeof(RelationalTypeMapping)
            })!,
            Constant(sqlUnaryExpression.OperatorType),
            Visit(sqlUnaryExpression.Operand),
            Constant(sqlUnaryExpression.Type),
            RenderFindTypeMapping(sqlUnaryExpression.TypeMapping));

    /// <inheritdoc />
    protected override Expression VisitTable(TableExpression tableExpression)
        => New(
            _tableConstructor ??= typeof(TableExpression).GetConstructor(new[]
            {
                typeof(string), typeof(string), typeof(string), typeof(ITable)
            })!,
            Constant(tableExpression.Alias, typeof(string)),
            Constant(tableExpression.Name, typeof(string)),
            Constant(tableExpression.Schema, typeof(string)),
            QuoteTableBase(tableExpression.Table));

    /// <inheritdoc />
    protected override Expression VisitUnion(UnionExpression unionExpression)
        => New(
            _unionConstructor ??= typeof(UnionExpression).GetConstructor(
                new[]
                {
                    typeof(string), typeof(SelectExpression), typeof(SelectExpression), typeof(bool), typeof(IEnumerable<IAnnotation>)
                })!,
            Constant(unionExpression.Alias, typeof(string)),
            Visit(unionExpression.Source1),
            Visit(unionExpression.Source2),
            Constant(unionExpression.IsDistinct),
            RenderAnnotationArray(unionExpression.GetAnnotations()));

    /// <inheritdoc />
    protected override Expression VisitUpdate(UpdateExpression updateExpression)
    {
        if (_seenRootExpression)
        {
            throw new NotSupportedException($"{nameof(UpdateExpression)} in non-root context");
        }

        _seenRootExpression = true;

        var updateExpressionVariable = Parameter(typeof(UpdateExpression), _rootSelectVariableName);
        _blockVariables.Add(updateExpressionVariable);
        _blockExpressions.Add(
            Assign(
                updateExpressionVariable,
                New(
                    _updateConstructor ??= typeof(UpdateExpression).GetConstructor(
                        new[]
                        {
                            typeof(TableExpression), typeof(SelectExpression), typeof(IReadOnlyList<ColumnValueSetter>),
                            typeof(ISet<string>)
                        })!,
                    Visit(updateExpression.Table),
                    Visit(updateExpression.SelectExpression),
                    NewArrayInit(
                        typeof(ColumnValueSetter),
                        updateExpression.ColumnValueSetters
                            .Select(s => New(
                                _columnValueSetterConstructor ??=
                                    typeof(ColumnValueSetter).GetConstructor(new[] { typeof(ColumnExpression), typeof(SqlExpression) })!,
                                Visit(s.Column),
                                Visit(s.Value)))),
                    CreateTagsExpression(updateExpression.Tags))));

        return updateExpressionVariable;
    }

    /// <inheritdoc />
    protected override Expression VisitJsonScalar(JsonScalarExpression jsonScalarExpression)
        => throw new NotImplementedException();
    // => New(
    //     _jsonScalarConstructor ??= typeof(JsonScalarExpression).GetConstructor(new[] { typeof(ColumnExpression), typeof(IReadOnlyList<PathSegment>), typeof(Type), typeof(RelationalTypeMapping), typeof(bool) })!,
    //     Visit(jsonScalarExpression.JsonColumn),
    //     // TODO: Contains SqlExpression
    //     Constant(jsonScalarExpression.Type),
    //     RenderFindTypeMapping(jsonScalarExpression.TypeMapping),
    //     Constant(jsonScalarExpression.IsNullable));

    private Expression QuoteTableBase(ITableBase tableBase)
        => tableBase switch
        {
            Table table => Call(
                _relationalModelParameter,
                _relationalModelFindTableMethod ??=
                    typeof(RelationalModel).GetMethod(nameof(RelationalModel.FindTable), new[] { typeof(string), typeof(string) })!,
                Constant(table.Name, typeof(string)),
                Constant(table.Schema, typeof(string))),

            _ => throw new ArgumentOutOfRangeException($"Unsupported {nameof(ITableBase)} of type {tableBase.GetType().Name}")
        };

    private Expression RenderFindTypeMapping(RelationalTypeMapping? typeMapping)
        => typeMapping is null
            ? Constant(null, typeof(RelationalTypeMapping))
            : Call(
                _relationalTypeMappingSourceParameter,
                RelationalTypeMappingSourceFindMappingMethod,
                Constant(typeMapping.ClrType, typeof(Type)),
                Constant(typeMapping.StoreType, typeof(string)),
                Constant(false), // TODO: keyOrIndex not accessible
                Constant(typeMapping.IsUnicode, typeof(bool?)),
                Constant(typeMapping.Size, typeof(int?)),
                Constant(false, typeof(bool?)), // TODO: rowversion not accessible
                Constant(typeMapping.IsFixedLength, typeof(bool?)),
                Constant(typeMapping.Precision, typeof(int?)),
                Constant(typeMapping.Scale, typeof(int?)));

    private Expression RenderAnnotationArray(IEnumerable<IAnnotation> annotations)
        => NewArrayInit(typeof(Annotation), annotations.Select(a =>
            New(
                _annotationConstructor ??= typeof(Annotation).GetConstructor(new[] { typeof(string), typeof(object) })!,
                Constant(a.Name),
                Constant(a.Value))));

    private Expression VisitOrNull(Expression? expression)
        => expression is null ? Constant(null, typeof(SqlExpression)) : Visit(expression);

    private static Expression CreateTagsExpression(ISet<string> tags)
        => ListInit(
            New(typeof(HashSet<string>)),
            tags.Select(t =>
                ElementInit(
                    _hashSetAddMethod ??= typeof(HashSet<string>).GetMethod(nameof(HashSet<string>.Add))!,
                    Constant(t))));
}
