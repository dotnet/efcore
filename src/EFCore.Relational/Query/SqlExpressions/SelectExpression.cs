// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a SELECT in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     This class is not publicly constructable. If this is a problem for your application or provider, then please file
///     an issue at <see href="https://github.com/dotnet/efcore">github.com/dotnet/efcore</see>.
/// </remarks>
// Class is sealed because there are no public/protected constructors. Can be unsealed if this is changed.
public sealed partial class SelectExpression : TableExpressionBase
{
    private const string DiscriminatorColumnAlias = "Discriminator";
    private static readonly IdentifierComparer IdentifierComparerInstance = new();

    private static readonly Dictionary<ExpressionType, ExpressionType> MirroredOperationMap =
        new()
        {
            { ExpressionType.Equal, ExpressionType.Equal },
            { ExpressionType.NotEqual, ExpressionType.NotEqual },
            { ExpressionType.LessThan, ExpressionType.GreaterThan },
            { ExpressionType.LessThanOrEqual, ExpressionType.GreaterThanOrEqual },
            { ExpressionType.GreaterThan, ExpressionType.LessThan },
            { ExpressionType.GreaterThanOrEqual, ExpressionType.LessThanOrEqual }
        };

    private readonly List<ProjectionExpression> _projection = new();
    private readonly List<TableExpressionBase> _tables = new();
    private readonly List<TableReferenceExpression> _tableReferences = new();
    private readonly List<SqlExpression> _groupBy = new();
    private readonly List<OrderingExpression> _orderings = new();
    private HashSet<string> _usedAliases = new();

    private readonly List<(ColumnExpression Column, ValueComparer Comparer)> _identifier = new();
    private readonly List<(ColumnExpression Column, ValueComparer Comparer)> _childIdentifiers = new();

    private readonly List<int> _tptLeftJoinTables = new();
    private Dictionary<ProjectionMember, Expression> _projectionMapping = new();
    private List<Expression> _clientProjections = new();
    private readonly List<string?> _aliasForClientProjections = new();

    private SqlExpression? _groupingCorrelationPredicate;
    private Guid? _groupingParentSelectExpressionId;
    private CloningExpressionVisitor? _cloningExpressionVisitor;

    private SelectExpression(
        string? alias,
        List<ProjectionExpression> projections,
        List<TableExpressionBase> tables,
        List<TableReferenceExpression> tableReferences,
        List<SqlExpression> groupBy,
        List<OrderingExpression> orderings,
        IEnumerable<IAnnotation> annotations)
        : base(alias, annotations)
    {
        _projection = projections;
        _tables = tables;
        _tableReferences = tableReferences;
        _groupBy = groupBy;
        _orderings = orderings;
    }

    internal SelectExpression(SqlExpression? projection)
        : base(null)
    {
        if (projection != null)
        {
            _projectionMapping[new ProjectionMember()] = projection;
        }
    }

    internal SelectExpression(IEntityType entityType, ISqlExpressionFactory sqlExpressionFactory)
        : base(null)
    {
        if ((entityType.BaseType == null && !entityType.GetDirectlyDerivedTypes().Any())
            || entityType.FindDiscriminatorProperty() != null)
        {
            ITableBase table;
            TableExpressionBase tableExpression;
            if (entityType.GetFunctionMappings().SingleOrDefault(e => e.IsDefaultFunctionMapping) is IFunctionMapping functionMapping)
            {
                var storeFunction = functionMapping.Table;

                table = storeFunction;
                tableExpression = new TableValuedFunctionExpression((IStoreFunction)storeFunction, Array.Empty<SqlExpression>());
            }
            else
            {
                table = entityType.GetViewOrTableMappings().Single().Table;
                tableExpression = new TableExpression(table);
            }

            var tableReferenceExpression = new TableReferenceExpression(this, tableExpression.Alias!);
            AddTable(tableExpression, tableReferenceExpression);

            var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
            foreach (var property in GetAllPropertiesInHierarchy(entityType))
            {
                propertyExpressions[property] = CreateColumnExpression(property, table, tableReferenceExpression, nullable: false);
            }

            var entityProjection = new EntityProjectionExpression(entityType, propertyExpressions);
            _projectionMapping[new ProjectionMember()] = entityProjection;

            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey != null)
            {
                foreach (var property in primaryKey.Properties)
                {
                    _identifier.Add((propertyExpressions[property], property.GetKeyValueComparer()));
                }
            }
        }
        else
        {
            // TPT
            var keyProperties = entityType.FindPrimaryKey()!.Properties;
            List<ColumnExpression> joinColumns = default!;
            var tables = new List<ITableBase>();
            var columns = new Dictionary<IProperty, ColumnExpression>();
            foreach (var baseType in entityType.GetAllBaseTypesInclusive())
            {
                var table = baseType.GetViewOrTableMappings().Single(m => !tables.Contains(m.Table)).Table;
                tables.Add(table);
                var tableExpression = new TableExpression(table);
                var tableReferenceExpression = new TableReferenceExpression(this, tableExpression.Alias);

                foreach (var property in baseType.GetDeclaredProperties())
                {
                    columns[property] = CreateColumnExpression(property, table, tableReferenceExpression, nullable: false);
                }

                if (_tables.Count == 0)
                {
                    AddTable(tableExpression, tableReferenceExpression);
                    joinColumns = new List<ColumnExpression>();
                    foreach (var property in keyProperties)
                    {
                        var columnExpression = columns[property];
                        joinColumns.Add(columnExpression);
                        _identifier.Add((columnExpression, property.GetKeyValueComparer()));
                    }
                }
                else
                {
                    var innerColumns = keyProperties.Select(
                        p => CreateColumnExpression(p, table, tableReferenceExpression, nullable: false));

                    var joinPredicate = joinColumns.Zip(innerColumns, (l, r) => sqlExpressionFactory.Equal(l, r))
                        .Aggregate((l, r) => sqlExpressionFactory.AndAlso(l, r));

                    var joinExpression = new InnerJoinExpression(tableExpression, joinPredicate);
                    AddTable(joinExpression, tableReferenceExpression);
                }
            }

            var caseWhenClauses = new List<CaseWhenClause>();
            foreach (var derivedType in entityType.GetDerivedTypes())
            {
                var table = derivedType.GetViewOrTableMappings().Single(m => !tables.Contains(m.Table)).Table;
                tables.Add(table);
                var tableExpression = new TableExpression(table);
                var tableReferenceExpression = new TableReferenceExpression(this, tableExpression.Alias);
                foreach (var property in derivedType.GetDeclaredProperties())
                {
                    columns[property] = CreateColumnExpression(property, table, tableReferenceExpression, nullable: true);
                }

                var keyColumns = keyProperties.Select(p => CreateColumnExpression(p, table, tableReferenceExpression, nullable: true))
                    .ToArray();

                if (!derivedType.IsAbstract())
                {
                    caseWhenClauses.Add(
                        new CaseWhenClause(
                            sqlExpressionFactory.IsNotNull(keyColumns[0]),
                            sqlExpressionFactory.Constant(derivedType.ShortName())));
                }

                var joinPredicate = joinColumns.Zip(keyColumns, (l, r) => sqlExpressionFactory.Equal(l, r))
                    .Aggregate((l, r) => sqlExpressionFactory.AndAlso(l, r));

                var joinExpression = new LeftJoinExpression(tableExpression, joinPredicate);
                _tptLeftJoinTables.Add(_tables.Count);
                AddTable(joinExpression, tableReferenceExpression);
            }

            caseWhenClauses.Reverse();
            var discriminatorExpression = caseWhenClauses.Count == 0
                ? null
                : sqlExpressionFactory.ApplyDefaultTypeMapping(
                    sqlExpressionFactory.Case(caseWhenClauses, elseResult: null));
            var entityProjection = new EntityProjectionExpression(entityType, columns, discriminatorExpression);
            _projectionMapping[new ProjectionMember()] = entityProjection;
        }
    }

    internal SelectExpression(IEntityType entityType, TableExpressionBase tableExpressionBase)
        : base(null)
    {
        if ((entityType.BaseType != null || entityType.GetDirectlyDerivedTypes().Any())
            && entityType.FindDiscriminatorProperty() == null)
        {
            throw new InvalidOperationException(RelationalStrings.SelectExpressionNonTPHWithCustomTable(entityType.DisplayName()));
        }

        var table = tableExpressionBase switch
        {
            TableExpression tableExpression => tableExpression.Table,
            TableValuedFunctionExpression tableValuedFunctionExpression => tableValuedFunctionExpression.StoreFunction,
            _ => entityType.GetDefaultMappings().Single().Table
        };

        var tableReferenceExpression = new TableReferenceExpression(this, tableExpressionBase.Alias!);
        AddTable(tableExpressionBase, tableReferenceExpression);

        var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
        foreach (var property in GetAllPropertiesInHierarchy(entityType))
        {
            propertyExpressions[property] = CreateColumnExpression(property, table, tableReferenceExpression, nullable: false);
        }

        var entityProjection = new EntityProjectionExpression(entityType, propertyExpressions);
        _projectionMapping[new ProjectionMember()] = entityProjection;

        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey != null)
        {
            foreach (var property in primaryKey.Properties)
            {
                _identifier.Add((propertyExpressions[property], property.GetKeyValueComparer()));
            }
        }
    }

    /// <summary>
    ///     The list of tags applied to this <see cref="SelectExpression" />.
    /// </summary>
    public ISet<string> Tags { get; private set; } = new HashSet<string>();

    /// <summary>
    ///     A bool value indicating if DISTINCT is applied to projection of this <see cref="SelectExpression" />.
    /// </summary>
    public bool IsDistinct { get; private set; }

    /// <summary>
    ///     The list of expressions being projected out from the result set.
    /// </summary>
    public IReadOnlyList<ProjectionExpression> Projection
        => _projection;

    /// <summary>
    ///     The list of tables sources used to generate the result set.
    /// </summary>
    public IReadOnlyList<TableExpressionBase> Tables
        => _tables;

    /// <summary>
    ///     The WHERE predicate for the SELECT.
    /// </summary>
    public SqlExpression? Predicate { get; private set; }

    /// <summary>
    ///     The SQL GROUP BY clause for the SELECT.
    /// </summary>
    public IReadOnlyList<SqlExpression> GroupBy
        => _groupBy;

    /// <summary>
    ///     The HAVING predicate for the SELECT when <see cref="GroupBy" /> clause exists.
    /// </summary>
    public SqlExpression? Having { get; private set; }

    /// <summary>
    ///     The list of orderings used to sort the result set.
    /// </summary>
    public IReadOnlyList<OrderingExpression> Orderings
        => _orderings;

    /// <summary>
    ///     The limit applied to the number of rows in the result set.
    /// </summary>
    public SqlExpression? Limit { get; private set; }

    /// <summary>
    ///     The offset to skip rows from the result set.
    /// </summary>
    public SqlExpression? Offset { get; private set; }

    /// <summary>
    ///     Applies a given set of tags.
    /// </summary>
    /// <param name="tags">A list of tags to apply.</param>
    public void ApplyTags(ISet<string> tags)
        => Tags = tags;

    /// <summary>
    ///     Applies DISTINCT operator to the projections of the <see cref="SelectExpression" />.
    /// </summary>
    public void ApplyDistinct()
    {
        if (_clientProjections.Count > 0
            && _clientProjections.Any(e => e is ShapedQueryExpression sqe && sqe.ResultCardinality == ResultCardinality.Enumerable))
        {
            throw new InvalidOperationException(RelationalStrings.DistinctOnCollectionNotSupported);
        }

        if (Limit != null
            || Offset != null)
        {
            PushdownIntoSubquery();
        }

        IsDistinct = true;

        if (_identifier.Count > 0)
        {
            var entityProjectionIdentifiers = new List<ColumnExpression>();
            var entityProjectionValueComparers = new List<ValueComparer>();
            var otherExpressions = new List<SqlExpression>();
            var nonProcessableExpressionFound = false;

            var projections = _clientProjections.Count > 0 ? _clientProjections : _projectionMapping.Values.ToList();
            foreach (var projection in projections)
            {
                if (projection is EntityProjectionExpression entityProjection)
                {
                    var primaryKey = entityProjection.EntityType.FindPrimaryKey();
                    // If there are any existing identifier then all entity projection must have a key
                    // else keyless entity would have wiped identifier when generating join.
                    Check.DebugAssert(primaryKey != null, "primary key is null.");
                    foreach (var property in primaryKey.Properties)
                    {
                        entityProjectionIdentifiers.Add(entityProjection.BindProperty(property));
                        entityProjectionValueComparers.Add(property.GetKeyValueComparer());
                    }
                }
                else if (projection is SqlExpression sqlExpression)
                {
                    otherExpressions.Add(sqlExpression);
                }
                else
                {
                    nonProcessableExpressionFound = true;
                    break;
                }
            }

            if (nonProcessableExpressionFound)
            {
                _identifier.Clear();
            }
            else
            {
                var allOtherExpressions = entityProjectionIdentifiers.Concat(otherExpressions).ToList();
                if (!_identifier.All(e => allOtherExpressions.Contains(e.Column)))
                {
                    _identifier.Clear();
                    if (otherExpressions.Count == 0)
                    {
                        // If there are no other expressions then we can use all entityProjectionIdentifiers
                        _identifier.AddRange(entityProjectionIdentifiers.Zip(entityProjectionValueComparers));
                    }
                    else if (otherExpressions.All(e => e is ColumnExpression))
                    {
                        _identifier.AddRange(entityProjectionIdentifiers.Zip(entityProjectionValueComparers));
                        _identifier.AddRange(otherExpressions.Select(e => ((ColumnExpression)e, e.TypeMapping!.KeyComparer)));
                    }
                }
            }
        }

        ClearOrdering();
    }

    /// <summary>
    ///     Adds expressions from projection mapping to projection if not done already.
    /// </summary>
    /// <param name="shaperExpression">Current shaper expression which will shape results of this select expression.</param>
    /// <param name="resultCardinality">The result cardinality of this query expression.</param>
    /// <param name="querySplittingBehavior">The query splitting behavior to use when applying projection for nested collections.</param>
    /// <returns>Returns modified shaper expression to shape results of this select expression.</returns>
    public Expression ApplyProjection(
        Expression shaperExpression,
        ResultCardinality resultCardinality,
        QuerySplittingBehavior querySplittingBehavior)
    {
        if (Projection.Any())
        {
            throw new InvalidOperationException("Applying projection on already finalized select expression");
        }

        if (_clientProjections.Count > 0)
        {
            EntityShaperNullableMarkingExpressionVisitor? entityShaperNullableMarkingExpressionVisitor = null;
            CloningExpressionVisitor? cloningExpressionVisitor = null;
            var pushdownOccurred = false;
            var containsCollection = false;
            var containsSingleResult = false;
            foreach (var projection in _clientProjections)
            {
                if (projection is ShapedQueryExpression sqe)
                {
                    if (sqe.ResultCardinality == ResultCardinality.Enumerable)
                    {
                        containsCollection = true;
                    }

                    if (sqe.ResultCardinality == ResultCardinality.Single
                        || sqe.ResultCardinality == ResultCardinality.SingleOrDefault)
                    {
                        containsSingleResult = true;
                    }
                }
            }

            if (containsSingleResult
                || (querySplittingBehavior == QuerySplittingBehavior.SingleQuery && containsCollection))
            {
                // Pushdown outer since we will be adding join to this
                if (Limit != null
                    || Offset != null
                    || IsDistinct
                    || GroupBy.Count > 0)
                {
                    PushdownIntoSubqueryInternal();
                    pushdownOccurred = true;
                }

                entityShaperNullableMarkingExpressionVisitor = new EntityShaperNullableMarkingExpressionVisitor();
            }

            if (containsSingleResult || containsCollection)
            {
                cloningExpressionVisitor = new CloningExpressionVisitor();
            }

            SelectExpression? baseSelectExpression = null;
            if (querySplittingBehavior == QuerySplittingBehavior.SplitQuery && containsCollection)
            {
                baseSelectExpression = (SelectExpression)cloningExpressionVisitor!.Visit(this);
                if (resultCardinality == ResultCardinality.Single
                    || resultCardinality == ResultCardinality.SingleOrDefault)
                {
                    // Update limit since split queries don't need limit 2
                    if (pushdownOccurred)
                    {
                        UpdateLimit((SelectExpression)baseSelectExpression.Tables[0]);
                    }
                    else
                    {
                        UpdateLimit(baseSelectExpression);
                    }

                    static void UpdateLimit(SelectExpression selectExpression)
                    {
                        if (selectExpression.Limit is SqlConstantExpression limitConstantExpression
                            && limitConstantExpression.Value is int limitValue
                            && limitValue == 2)
                        {
                            selectExpression.Limit = new SqlConstantExpression(Constant(1), limitConstantExpression.TypeMapping);
                        }
                    }
                }
            }

            var earlierClientProjectionCount = _clientProjections.Count;
            var newClientProjections = new List<Expression>();
            var clientProjectionIndexMap = new List<object>();
            var remappingRequired = false;
            for (var i = 0; i < _clientProjections.Count; i++)
            {
                if (i == earlierClientProjectionCount)
                {
                    // Since we lift nested client projections for single results up, we may need to re-clone the baseSelectExpression
                    // again so it does contain the single result subquery too. We erase projections for it since it would be non-empty.
                    earlierClientProjectionCount = _clientProjections.Count;
                    baseSelectExpression = (SelectExpression)cloningExpressionVisitor!.Visit(this);
                    baseSelectExpression._projection.Clear();
                }

                var value = _clientProjections[i];
                switch (value)
                {
                    case EntityProjectionExpression entityProjection:
                    {
                        var result = AddEntityProjection(entityProjection);
                        newClientProjections.Add(result);
                        clientProjectionIndexMap.Add(newClientProjections.Count - 1);

                        break;
                    }

                    case SqlExpression sqlExpression:
                    {
                        var result = Constant(AddToProjection(sqlExpression, _aliasForClientProjections[i]));
                        newClientProjections.Add(result);
                        clientProjectionIndexMap.Add(newClientProjections.Count - 1);

                        break;
                    }

                    case ShapedQueryExpression shapedQueryExpression
                        when shapedQueryExpression.ResultCardinality == ResultCardinality.Single
                        || shapedQueryExpression.ResultCardinality == ResultCardinality.SingleOrDefault:
                    {
                        var innerSelectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
                        innerSelectExpression._groupingCorrelationPredicate = null;
                        var innerShaperExpression = shapedQueryExpression.ShaperExpression;
                        if (innerSelectExpression._clientProjections.Count == 0)
                        {
                            var mapping = innerSelectExpression.ConvertProjectionMappingToClientProjections(
                                innerSelectExpression._projectionMapping);
                            innerShaperExpression =
                                new ProjectionMemberToIndexConvertingExpressionVisitor(innerSelectExpression, mapping)
                                    .Visit(innerShaperExpression);
                        }

                        var innerExpression = RemoveConvert(innerShaperExpression);
                        if (!(innerExpression is EntityShaperExpression
                                || innerExpression is IncludeExpression))
                        {
                            var sentinelExpression = innerSelectExpression.Limit!;
                            var sentinelNullableType = sentinelExpression.Type.MakeNullable();
                            innerSelectExpression._clientProjections.Add(sentinelExpression);
                            innerSelectExpression._aliasForClientProjections.Add(null);
                            var dummyProjection = new ProjectionBindingExpression(
                                innerSelectExpression, innerSelectExpression._clientProjections.Count - 1, sentinelNullableType);

                            var defaultResult = shapedQueryExpression.ResultCardinality == ResultCardinality.SingleOrDefault
                                ? (Expression)Default(innerShaperExpression.Type)
                                : Block(
                                    Throw(
                                        New(
                                            typeof(InvalidOperationException).GetConstructors()
                                                .Single(
                                                    ci =>
                                                    {
                                                        var parameters = ci.GetParameters();
                                                        return parameters.Length == 1
                                                            && parameters[0].ParameterType == typeof(string);
                                                    }),
                                            Constant(CoreStrings.SequenceContainsNoElements))),
                                    Default(innerShaperExpression.Type));

                            innerShaperExpression = Condition(
                                Equal(dummyProjection, Default(sentinelNullableType)),
                                defaultResult,
                                innerShaperExpression);
                        }

                        AddJoin(JoinType.OuterApply, ref innerSelectExpression, out _);
                        var offset = _clientProjections.Count;
                        var count = innerSelectExpression._clientProjections.Count;
                        _clientProjections.AddRange(
                            innerSelectExpression._clientProjections.Select(e => MakeNullable(e, nullable: true)));
                        _aliasForClientProjections.AddRange(innerSelectExpression._aliasForClientProjections);
                        innerShaperExpression = new ProjectionIndexRemappingExpressionVisitor(
                                innerSelectExpression,
                                this,
                                Enumerable.Range(offset, count).ToArray())
                            .Visit(innerShaperExpression);
                        innerShaperExpression = entityShaperNullableMarkingExpressionVisitor!.Visit(innerShaperExpression);
                        clientProjectionIndexMap.Add(innerShaperExpression);
                        remappingRequired = true;
                        break;

                        static Expression RemoveConvert(Expression expression)
                            => expression is UnaryExpression unaryExpression
                                && unaryExpression.NodeType == ExpressionType.Convert
                                    ? RemoveConvert(unaryExpression.Operand)
                                    : expression;
                    }

                    case ShapedQueryExpression shapedQueryExpression
                        when shapedQueryExpression.ResultCardinality == ResultCardinality.Enumerable:
                    {
                        var innerSelectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
                        innerSelectExpression._groupingCorrelationPredicate = null;
                        if (_identifier.Count == 0
                            || innerSelectExpression._identifier.Count == 0)
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin);
                        }

                        var innerShaperExpression = shapedQueryExpression.ShaperExpression;
                        if (innerSelectExpression._clientProjections.Count == 0)
                        {
                            var mapping = innerSelectExpression.ConvertProjectionMappingToClientProjections(
                                innerSelectExpression._projectionMapping);
                            innerShaperExpression =
                                new ProjectionMemberToIndexConvertingExpressionVisitor(innerSelectExpression, mapping)
                                    .Visit(innerShaperExpression);
                        }

                        if (querySplittingBehavior == QuerySplittingBehavior.SplitQuery)
                        {
                            var outerSelectExpression = (SelectExpression)cloningExpressionVisitor!.Visit(baseSelectExpression!);
                            innerSelectExpression =
                                (SelectExpression)new ColumnExpressionReplacingExpressionVisitor(this, outerSelectExpression._tableReferences)
                                    .Visit(innerSelectExpression);

                            if (outerSelectExpression.Limit != null
                                || outerSelectExpression.Offset != null
                                || outerSelectExpression.IsDistinct
                                || outerSelectExpression._groupBy.Count > 0)
                            {
                                // We do pushdown after making sure that inner contains references to outer only
                                // so that when we do pushdown, we can update inner and maintain graph
                                var sqlRemappingVisitor = outerSelectExpression.PushdownIntoSubqueryInternal();
                                innerSelectExpression = sqlRemappingVisitor.Remap(innerSelectExpression);
                            }

                            var actualParentIdentifier = _identifier.Take(outerSelectExpression._identifier.Count).ToList();
                            var containsOrdering = innerSelectExpression.Orderings.Count > 0;
                            List<OrderingExpression>? orderingsToBeErased = null;
                            if (containsOrdering
                                && innerSelectExpression.Limit == null
                                && innerSelectExpression.Offset == null)
                            {
                                orderingsToBeErased = innerSelectExpression.Orderings.ToList();
                            }
#if DEBUG
                            Check.DebugAssert(
                                !(new SelectExpressionCorrelationFindingExpressionVisitor(this)
                                    .ContainsOuterReference(innerSelectExpression)), "Split query contains outer reference");
#endif
                            var parentIdentifier = GetIdentifierAccessor(this, newClientProjections, actualParentIdentifier).Item1;

                            outerSelectExpression.AddJoin(
                                JoinType.CrossApply, ref innerSelectExpression, out var pushdownOccurredWhenJoining);
                            outerSelectExpression._clientProjections.AddRange(innerSelectExpression._clientProjections);
                            outerSelectExpression._aliasForClientProjections.AddRange(innerSelectExpression._aliasForClientProjections);
                            innerSelectExpression = outerSelectExpression;

                            for (var j = 0; j < actualParentIdentifier.Count; j++)
                            {
                                AppendOrdering(new OrderingExpression(actualParentIdentifier[j].Column, ascending: true));
                                innerSelectExpression.AppendOrdering(
                                    new OrderingExpression(innerSelectExpression._identifier[j].Column, ascending: true));
                            }

                            // Copy over any nested ordering if there were any
                            if (containsOrdering)
                            {
                                var collectionJoinedInnerTable = ((JoinExpressionBase)innerSelectExpression._tables[^1]).Table;
                                var collectionJoinedTableReference = innerSelectExpression._tableReferences[^1];
                                var innerOrderingExpressions = new List<OrderingExpression>();
                                if (orderingsToBeErased != null)
                                {
                                    // Ordering was present but erased so we add again
                                    if (pushdownOccurredWhenJoining)
                                    {
                                        // We lift from inner subquery if pushdown occurred with ordering erased
                                        var subquery = (SelectExpression)collectionJoinedInnerTable;
                                        foreach (var ordering in orderingsToBeErased)
                                        {
                                            innerOrderingExpressions.Add(
                                                new OrderingExpression(
                                                    subquery.GenerateOuterColumn(collectionJoinedTableReference, ordering.Expression),
                                                    ordering.IsAscending));
                                        }
                                    }
                                    else
                                    {
                                        // We copy from inner if pushdown did not happen but ordering was left behind when
                                        // generating join
                                        innerOrderingExpressions.AddRange(orderingsToBeErased);
                                    }
                                }
                                else
                                {
                                    // If orderings were not erased then they must be present in inner
                                    GetOrderingsFromInnerTable(
                                        collectionJoinedInnerTable,
                                        collectionJoinedTableReference,
                                        innerOrderingExpressions);
                                }

                                foreach (var ordering in innerOrderingExpressions)
                                {
                                    innerSelectExpression.AppendOrdering(ordering);
                                }
                            }

                            innerShaperExpression = innerSelectExpression.ApplyProjection(
                                innerShaperExpression, shapedQueryExpression.ResultCardinality, querySplittingBehavior);

                            var (childIdentifier, childIdentifierValueComparers) = GetIdentifierAccessor(
                                innerSelectExpression,
                                innerSelectExpression._clientProjections,
                                innerSelectExpression._identifier.Take(_identifier.Count));

                            var result = new SplitCollectionInfo(
                                parentIdentifier, childIdentifier, childIdentifierValueComparers,
                                innerSelectExpression, innerShaperExpression);
                            clientProjectionIndexMap.Add(result);
                        }
                        else
                        {
                            var parentIdentifierList = _identifier.Except(_childIdentifiers, IdentifierComparerInstance).ToList();
                            var (parentIdentifier, parentIdentifierValueComparers) = GetIdentifierAccessor(
                                this, newClientProjections, parentIdentifierList);
                            var (outerIdentifier, outerIdentifierValueComparers) = GetIdentifierAccessor(
                                this, newClientProjections, _identifier);

                            foreach (var identifier in _identifier)
                            {
                                AppendOrdering(new OrderingExpression(identifier.Column, ascending: true));
                            }

                            innerShaperExpression = innerSelectExpression.ApplyProjection(
                                innerShaperExpression, shapedQueryExpression.ResultCardinality, querySplittingBehavior);

                            var containsOrdering = innerSelectExpression.Orderings.Count > 0;
                            List<OrderingExpression>? orderingsToBeErased = null;
                            if (containsOrdering
                                && innerSelectExpression.Limit == null
                                && innerSelectExpression.Offset == null)
                            {
                                orderingsToBeErased = innerSelectExpression.Orderings.ToList();
                            }

                            AddJoin(JoinType.OuterApply, ref innerSelectExpression, out var pushdownOccurredWhenJoining);

                            // Copy over any nested ordering if there were any
                            if (containsOrdering)
                            {
                                var collectionJoinedInnerTable = innerSelectExpression._tables[0];
                                var collectionJoinedTableReference = innerSelectExpression._tableReferences[0];
                                var innerOrderingExpressions = new List<OrderingExpression>();
                                if (orderingsToBeErased != null)
                                {
                                    // Ordering was present but erased so we add again
                                    if (pushdownOccurredWhenJoining)
                                    {
                                        // We lift from inner subquery if pushdown occurred with ordering erased
                                        var subquery = (SelectExpression)collectionJoinedInnerTable;
                                        foreach (var ordering in orderingsToBeErased)
                                        {
                                            innerOrderingExpressions.Add(
                                                new OrderingExpression(
                                                    subquery.GenerateOuterColumn(collectionJoinedTableReference, ordering.Expression),
                                                    ordering.IsAscending));
                                        }
                                    }
                                    else
                                    {
                                        // We copy from inner if pushdown did not happen but ordering was left behind when
                                        // generating join
                                        innerOrderingExpressions.AddRange(orderingsToBeErased);
                                    }
                                }
                                else
                                {
                                    // If orderings were not erased then they must be present in inner
                                    GetOrderingsFromInnerTable(
                                        collectionJoinedInnerTable,
                                        collectionJoinedTableReference,
                                        innerOrderingExpressions);
                                }

                                foreach (var ordering in innerOrderingExpressions)
                                {
                                    AppendOrdering(ordering.Update(MakeNullable(ordering.Expression, nullable: true)));
                                }
                            }

                            innerShaperExpression = CopyProjectionToOuter(innerSelectExpression, innerShaperExpression);
                            var (selfIdentifier, selfIdentifierValueComparers) = GetIdentifierAccessor(
                                this,
                                newClientProjections,
                                innerSelectExpression._identifier
                                    .Except(innerSelectExpression._childIdentifiers, IdentifierComparerInstance)
                                    .Select(e => (e.Column.MakeNullable(), e.Comparer)));

                            OrderingExpression? pendingOrdering = null;
                            foreach (var (identifierColumn, identifierComparer) in innerSelectExpression._identifier)
                            {
                                var updatedColumn = identifierColumn.MakeNullable();
                                _childIdentifiers.Add((updatedColumn, identifierComparer));

                                // We omit the last ordering as an optimization
                                var orderingExpression = new OrderingExpression(updatedColumn, ascending: true);

                                if (!_orderings.Any(o => o.Expression.Equals(updatedColumn)))
                                {
                                    if (pendingOrdering is not null)
                                    {
                                        if (orderingExpression.Equals(pendingOrdering))
                                        {
                                            continue;
                                        }

                                        AppendOrderingInternal(pendingOrdering);
                                    }

                                    pendingOrdering = orderingExpression;
                                }
                            }

                            var result = new SingleCollectionInfo(
                                parentIdentifier, outerIdentifier, selfIdentifier,
                                parentIdentifierValueComparers, outerIdentifierValueComparers, selfIdentifierValueComparers,
                                innerShaperExpression);
                            clientProjectionIndexMap.Add(result);
                        }

                        remappingRequired = true;

                        static (Expression, IReadOnlyList<ValueComparer>) GetIdentifierAccessor(
                            SelectExpression selectExpression,
                            List<Expression> clientProjectionList,
                            IEnumerable<(ColumnExpression Column, ValueComparer Comparer)> identifyingProjection)
                        {
                            var updatedExpressions = new List<Expression>();
                            var comparers = new List<ValueComparer>();
                            foreach (var (column, comparer) in identifyingProjection)
                            {
                                var index = selectExpression.AddToProjection(column, null);
                                var clientProjectionToAdd = Constant(index);
                                var existingIndex = clientProjectionList.FindIndex(
                                    e => ExpressionEqualityComparer.Instance.Equals(e, clientProjectionToAdd));
                                if (existingIndex == -1)
                                {
                                    clientProjectionList.Add(Constant(index));
                                    existingIndex = clientProjectionList.Count - 1;
                                }

                                var projectionBindingExpression = new ProjectionBindingExpression(
                                    selectExpression, existingIndex, column.Type.MakeNullable());

                                updatedExpressions.Add(
                                    projectionBindingExpression.Type.IsValueType
                                        ? Convert(projectionBindingExpression, typeof(object))
                                        : projectionBindingExpression);
                                comparers.Add(comparer);
                            }

                            return (NewArrayInit(typeof(object), updatedExpressions), comparers);
                        }

                        break;
                    }

                    default:
                        throw new InvalidOperationException(value.GetType().ToString());
                }
            }

            if (remappingRequired)
            {
                shaperExpression = new ClientProjectionRemappingExpressionVisitor(clientProjectionIndexMap).Visit(shaperExpression);
            }

            _clientProjections = newClientProjections;
            _aliasForClientProjections.Clear();

            return shaperExpression;

            void GetOrderingsFromInnerTable(
                TableExpressionBase tableExpressionBase,
                TableReferenceExpression tableReferenceExpression,
                List<OrderingExpression> orderings)
            {
                // If operation was converted to predicate join (inner/left join),
                // then ordering will be in rownumber expression
                if (tableExpressionBase is SelectExpression joinedSubquery
                    && joinedSubquery.Predicate != null
                    && joinedSubquery.Tables.Count == 1
                    && joinedSubquery.Tables[0] is SelectExpression rowNumberSubquery
                    && rowNumberSubquery.Projection.Select(pe => pe.Expression)
                        .OfType<RowNumberExpression>().SingleOrDefault() is RowNumberExpression rowNumberExpression)
                {
                    var rowNumberSubqueryTableReference = joinedSubquery._tableReferences.Single();
                    foreach (var partition in rowNumberExpression.Partitions)
                    {
                        orderings.Add(
                            new OrderingExpression(
                                joinedSubquery.GenerateOuterColumn(
                                    tableReferenceExpression,
                                    rowNumberSubquery.GenerateOuterColumn(rowNumberSubqueryTableReference, partition)),
                                ascending: true));
                    }

                    foreach (var ordering in rowNumberExpression.Orderings)
                    {
                        orderings.Add(
                            new OrderingExpression(
                                joinedSubquery.GenerateOuterColumn(
                                    tableReferenceExpression,
                                    rowNumberSubquery.GenerateOuterColumn(rowNumberSubqueryTableReference, ordering.Expression)),
                                ordering.IsAscending));
                    }
                }
                // If operation remained apply then ordering will be in the subquery
                else if (tableExpressionBase is SelectExpression collectionSelectExpression
                         && collectionSelectExpression.Orderings.Count > 0)
                {
                    foreach (var ordering in collectionSelectExpression.Orderings)
                    {
                        orderings.Add(
                            new OrderingExpression(
                                collectionSelectExpression.GenerateOuterColumn(tableReferenceExpression, ordering.Expression),
                                ordering.IsAscending));
                    }
                }
            }

            Expression CopyProjectionToOuter(SelectExpression innerSelectExpression, Expression innerShaperExpression)
            {
                var projectionIndexMap = new int[innerSelectExpression._projection.Count];
                for (var j = 0; j < projectionIndexMap.Length; j++)
                {
                    var projection = MakeNullable(innerSelectExpression._projection[j].Expression, nullable: true);
                    var index = AddToProjection(projection);
                    projectionIndexMap[j] = index;
                }

                var indexMap = new int[innerSelectExpression._clientProjections.Count];
                for (var j = 0; j < indexMap.Length; j++)
                {
                    var constantValue = ((ConstantExpression)innerSelectExpression._clientProjections[j]).Value!;
                    ConstantExpression remappedConstant;
                    if (constantValue is Dictionary<IProperty, int> entityDictionary)
                    {
                        var newDictionary = new Dictionary<IProperty, int>(entityDictionary.Count);
                        foreach (var (property, value) in entityDictionary)
                        {
                            newDictionary[property] = projectionIndexMap[value];
                        }

                        remappedConstant = Constant(newDictionary);
                    }
                    else
                    {
                        remappedConstant = Constant(projectionIndexMap[(int)constantValue]);
                    }

                    newClientProjections.Add(remappedConstant);
                    indexMap[j] = newClientProjections.Count - 1;
                }

                innerSelectExpression._clientProjections.Clear();
                innerSelectExpression._aliasForClientProjections.Clear();
                innerShaperExpression =
                    new ProjectionIndexRemappingExpressionVisitor(innerSelectExpression, this, indexMap).Visit(innerShaperExpression);
                innerShaperExpression = entityShaperNullableMarkingExpressionVisitor!.Visit(innerShaperExpression);

                return innerShaperExpression;
            }
        }

        {
            var result = new Dictionary<ProjectionMember, Expression>(_projectionMapping.Count);
            foreach (var (projectionMember, expression) in _projectionMapping)
            {
                result[projectionMember] = expression is EntityProjectionExpression entityProjection
                    ? AddEntityProjection(entityProjection)
                    : Constant(AddToProjection((SqlExpression)expression, projectionMember.Last?.Name));
            }

            _projectionMapping.Clear();
            _projectionMapping = result;

            return shaperExpression;
        }

        ConstantExpression AddEntityProjection(EntityProjectionExpression entityProjectionExpression)
        {
            var dictionary = new Dictionary<IProperty, int>();
            foreach (var property in GetAllPropertiesInHierarchy(entityProjectionExpression.EntityType))
            {
                dictionary[property] = AddToProjection(entityProjectionExpression.BindProperty(property), null);
            }

            if (entityProjectionExpression.DiscriminatorExpression != null)
            {
                AddToProjection(entityProjectionExpression.DiscriminatorExpression, DiscriminatorColumnAlias);
            }

            return Constant(dictionary);
        }
    }

    /// <summary>
    ///     Replaces current projection mapping with a new one to change what is being projected out from this <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="projectionMapping">A new projection mapping.</param>
    public void ReplaceProjection(IReadOnlyDictionary<ProjectionMember, Expression> projectionMapping)
    {
        _projectionMapping.Clear();
        foreach (var (projectionMember, expression) in projectionMapping)
        {
            Check.DebugAssert(
                expression is SqlExpression
                || expression is EntityProjectionExpression,
                "Invalid operation in the projection.");
            _projectionMapping[projectionMember] = expression;
        }
    }

    /// <summary>
    ///     Replaces current projection mapping with a new one to change what is being projected out from this <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="clientProjections">A new projection mapping.</param>
    public void ReplaceProjection(IReadOnlyList<Expression> clientProjections)
    {
        _projectionMapping.Clear();
        _clientProjections.Clear();
        _aliasForClientProjections.Clear();
        foreach (var expression in clientProjections)
        {
            Check.DebugAssert(
                expression is SqlExpression
                || expression is EntityProjectionExpression
                || expression is ShapedQueryExpression,
                "Invalid operation in the projection.");
            _clientProjections.Add(expression);
            _aliasForClientProjections.Add(null);
        }
    }

    /// <summary>
    ///     Gets the projection mapped to the given <see cref="ProjectionBindingExpression" />.
    /// </summary>
    /// <param name="projectionBindingExpression">A projection binding to search.</param>
    /// <returns>The mapped projection for given projection binding.</returns>
    public Expression GetProjection(ProjectionBindingExpression projectionBindingExpression)
        => projectionBindingExpression.ProjectionMember is ProjectionMember projectionMember
            ? _projectionMapping[projectionMember]
            : _clientProjections[projectionBindingExpression.Index!.Value];

    /// <summary>
    ///     Adds given <see cref="SqlExpression" /> to the projection.
    /// </summary>
    /// <param name="sqlExpression">An expression to add.</param>
    /// <returns>An int value indicating the index at which the expression was added in the projection list.</returns>
    public int AddToProjection(SqlExpression sqlExpression)
        => AddToProjection(sqlExpression, null);

    private int AddToProjection(SqlExpression sqlExpression, string? alias, bool assignUniqueTableAlias = true)
    {
        sqlExpression = TryLiftGroupByAggregate(sqlExpression);

        var existingIndex = _projection.FindIndex(pe => pe.Expression.Equals(sqlExpression));
        if (existingIndex != -1)
        {
            return existingIndex;
        }

        var baseAlias = !string.IsNullOrEmpty(alias)
            ? alias
            : (sqlExpression as ColumnExpression)?.Name;
        if (Alias != null)
        {
            baseAlias ??= "c";
            var counter = 0;

            var currentAlias = baseAlias;
            while (_projection.Any(pe => string.Equals(pe.Alias, currentAlias, StringComparison.OrdinalIgnoreCase)))
            {
                currentAlias = $"{baseAlias}{counter++}";
            }

            baseAlias = currentAlias;
        }

        if (assignUniqueTableAlias)
        {
            sqlExpression = AssignUniqueAliases(sqlExpression);
        }

        _projection.Add(new ProjectionExpression(sqlExpression, baseAlias ?? ""));

        return _projection.Count - 1;
    }

    /// <summary>
    ///     Applies filter predicate to the <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="sqlExpression">An expression to use for filtering.</param>
    public void ApplyPredicate(SqlExpression sqlExpression)
    {
        if (sqlExpression is SqlConstantExpression sqlConstant
            && sqlConstant.Value is bool boolValue
            && boolValue)
        {
            return;
        }

        if (Limit != null
            || Offset != null)
        {
            sqlExpression = PushdownIntoSubqueryInternal().Remap(sqlExpression);
        }

        sqlExpression = TryLiftGroupByAggregate(sqlExpression);
        sqlExpression = AssignUniqueAliases(sqlExpression);

        if (_groupBy.Count > 0)
        {
            Having = Having == null
                ? sqlExpression
                : new SqlBinaryExpression(
                    ExpressionType.AndAlso,
                    Having,
                    sqlExpression,
                    typeof(bool),
                    sqlExpression.TypeMapping);
        }
        else
        {
            Predicate = Predicate == null
                ? sqlExpression
                : new SqlBinaryExpression(
                    ExpressionType.AndAlso,
                    Predicate,
                    sqlExpression,
                    typeof(bool),
                    sqlExpression.TypeMapping);
        }
    }

    internal void UpdatePredicate(SqlExpression predicate)
    {
        Predicate = predicate;
    }

    /// <summary>
    ///     Applies grouping from given key selector.
    /// </summary>
    /// <param name="keySelector">An key selector expression for the GROUP BY.</param>
    public void ApplyGrouping(Expression keySelector)
    {
        ClearOrdering();

        var groupByTerms = new List<SqlExpression>();
        var groupByAliases = new List<string?>();
        PopulateGroupByTerms(keySelector, groupByTerms, groupByAliases, "Key");

        if (groupByTerms.Any(e => e is SqlConstantExpression || e is SqlParameterExpression || e is ScalarSubqueryExpression))
        {
            var sqlRemappingVisitor = PushdownIntoSubqueryInternal();
            var newGroupByTerms = new List<SqlExpression>(groupByTerms.Count);
            var subquery = (SelectExpression)_tables[0];
            var subqueryTableReference = _tableReferences[0];
            for (var i = 0; i < groupByTerms.Count; i++)
            {
                var item = groupByTerms[i];
                var newItem = subquery._projection.Any(e => e.Expression.Equals(item))
                    ? sqlRemappingVisitor.Remap(item)
                    : subquery.GenerateOuterColumn(subqueryTableReference, item, groupByAliases[i] ?? "Key");
                newGroupByTerms.Add(newItem);
            }

            new ReplacingExpressionVisitor(groupByTerms, newGroupByTerms).Visit(keySelector);
            groupByTerms = newGroupByTerms;
        }

        _groupBy.AddRange(groupByTerms);

        if (!_identifier.All(e => _groupBy.Contains(e.Column)))
        {
            _identifier.Clear();
            if (_groupBy.All(e => e is ColumnExpression))
            {
                _identifier.AddRange(_groupBy.Select(e => ((ColumnExpression)e, e.TypeMapping!.KeyComparer)));
            }
        }
    }

    /// <summary>
    ///     Applies grouping from given key selector and generate <see cref="GroupByShaperExpression" /> to shape results.
    /// </summary>
    /// <param name="keySelector">An key selector expression for the GROUP BY.</param>
    /// <param name="shaperExpression">The shaper expression for current query.</param>
    /// <param name="sqlExpressionFactory">The sql expression factory to use.</param>
    /// <returns>A <see cref="GroupByShaperExpression" /> which represents the result of the grouping operation.</returns>
    public GroupByShaperExpression ApplyGrouping(
        Expression keySelector,
        Expression shaperExpression,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        ClearOrdering();

        var keySelectorToAdd = keySelector;
        var emptyKey = keySelector is NewExpression newExpression
            && newExpression.Arguments.Count == 0;
        if (emptyKey)
        {
            keySelectorToAdd = sqlExpressionFactory.ApplyDefaultTypeMapping(sqlExpressionFactory.Constant(1));
        }

        var groupByTerms = new List<SqlExpression>();
        var groupByAliases = new List<string?>();
        PopulateGroupByTerms(keySelectorToAdd, groupByTerms, groupByAliases, "Key");

        if (groupByTerms.Any(e => e is SqlConstantExpression || e is SqlParameterExpression || e is ScalarSubqueryExpression))
        {
            // emptyKey will always hit this path.
            var sqlRemappingVisitor = PushdownIntoSubqueryInternal();
            var newGroupByTerms = new List<SqlExpression>(groupByTerms.Count);
            var subquery = (SelectExpression)_tables[0];
            var subqueryTableReference = _tableReferences[0];
            for (var i = 0; i < groupByTerms.Count; i++)
            {
                var item = groupByTerms[i];
                var newItem = subquery._projection.Any(e => e.Expression.Equals(item))
                    ? sqlRemappingVisitor.Remap(item)
                    : subquery.GenerateOuterColumn(subqueryTableReference, item, groupByAliases[i] ?? "Key");
                newGroupByTerms.Add(newItem);
            }

            if (!emptyKey)
            {
                // If non-empty key then we need to regenerate the key selector
                keySelector = new ReplacingExpressionVisitor(groupByTerms, newGroupByTerms).Visit(keySelector);
            }

            groupByTerms = newGroupByTerms;
        }

        _groupBy.AddRange(groupByTerms);

        // We generate the cloned expression before changing identifier for this SelectExpression
        // because we are going to erase grouping for cloned expression.
        _groupingParentSelectExpressionId = Guid.NewGuid();
        var clonedSelectExpression = Clone();
        var correlationPredicate = groupByTerms.Zip(clonedSelectExpression._groupBy)
            .Select(e => sqlExpressionFactory.Equal(e.First, e.Second))
            .Aggregate((l, r) => sqlExpressionFactory.AndAlso(l, r));
        clonedSelectExpression._groupBy.Clear();
        clonedSelectExpression.ApplyPredicate(correlationPredicate);
        clonedSelectExpression._groupingCorrelationPredicate = clonedSelectExpression.Predicate;

        if (!_identifier.All(e => _groupBy.Contains(e.Column)))
        {
            _identifier.Clear();
            if (_groupBy.All(e => e is ColumnExpression))
            {
                _identifier.AddRange(_groupBy.Select(e => ((ColumnExpression)e, e.TypeMapping!.KeyComparer)));
            }
        }

        return new GroupByShaperExpression(
            keySelector,
            new ShapedQueryExpression(
                clonedSelectExpression,
                new QueryExpressionReplacingExpressionVisitor(this, clonedSelectExpression).Visit(shaperExpression)));
    }

    private static void PopulateGroupByTerms(
        Expression keySelector,
        List<SqlExpression> groupByTerms,
        List<string?> groupByAliases,
        string? name)
    {
        switch (keySelector)
        {
            case SqlExpression sqlExpression:
                groupByTerms.Add(sqlExpression);
                groupByAliases.Add(name);
                break;

            case NewExpression newExpression:
                for (var i = 0; i < newExpression.Arguments.Count; i++)
                {
                    PopulateGroupByTerms(newExpression.Arguments[i], groupByTerms, groupByAliases, newExpression.Members?[i].Name);
                }

                break;

            case MemberInitExpression memberInitExpression:
                PopulateGroupByTerms(memberInitExpression.NewExpression, groupByTerms, groupByAliases, null);
                foreach (var argument in memberInitExpression.Bindings)
                {
                    var memberAssignment = (MemberAssignment)argument;
                    PopulateGroupByTerms(memberAssignment.Expression, groupByTerms, groupByAliases, memberAssignment.Member.Name);
                }

                break;

            case UnaryExpression unaryExpression
                when unaryExpression.NodeType == ExpressionType.Convert
                || unaryExpression.NodeType == ExpressionType.ConvertChecked:
                PopulateGroupByTerms(unaryExpression.Operand, groupByTerms, groupByAliases, name);
                break;

            default:
                throw new InvalidOperationException(RelationalStrings.InvalidKeySelectorForGroupBy(keySelector, keySelector.GetType()));
        }
    }

    /// <summary>
    ///     Applies ordering to the <see cref="SelectExpression" />. This overwrites any previous ordering specified.
    /// </summary>
    /// <param name="orderingExpression">An ordering expression to use for ordering.</param>
    public void ApplyOrdering(OrderingExpression orderingExpression)
    {
        if (IsDistinct
            || Limit != null
            || Offset != null)
        {
            orderingExpression = orderingExpression.Update(PushdownIntoSubqueryInternal().Remap(orderingExpression.Expression));
        }

        _orderings.Clear();
        AppendOrdering(orderingExpression);
    }

    /// <summary>
    ///     Appends ordering to the existing orderings of the <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="orderingExpression">An ordering expression to use for ordering.</param>
    public void AppendOrdering(OrderingExpression orderingExpression)
    {
        if (_groupBy.Count > 0)
        {
            orderingExpression = orderingExpression.Update(
                (SqlExpression)new GroupByAggregateLiftingExpressionVisitor(this).Visit(orderingExpression.Expression));
        }

        if (!_orderings.Any(o => o.Expression.Equals(orderingExpression.Expression)))
        {
            AppendOrderingInternal(orderingExpression);
        }
    }

    private void AppendOrderingInternal(OrderingExpression orderingExpression)
        => _orderings.Add(orderingExpression.Update(AssignUniqueAliases(orderingExpression.Expression)));

    /// <summary>
    ///     Reverses the existing orderings on the <see cref="SelectExpression" />.
    /// </summary>
    public void ReverseOrderings()
    {
        if (Limit != null
            || Offset != null)
        {
            PushdownIntoSubquery();
        }

        var existingOrdering = _orderings.ToArray();

        _orderings.Clear();

        for (var i = 0; i < existingOrdering.Length; i++)
        {
            _orderings.Add(
                new OrderingExpression(
                    existingOrdering[i].Expression,
                    !existingOrdering[i].IsAscending));
        }
    }

    /// <summary>
    ///     Clears existing orderings.
    /// </summary>
    public void ClearOrdering()
        => _orderings.Clear();

    /// <summary>
    ///     Applies limit to the <see cref="SelectExpression" /> to limit the number of rows returned in the result set.
    /// </summary>
    /// <param name="sqlExpression">An expression representing limit row count.</param>
    public void ApplyLimit(SqlExpression sqlExpression)
    {
        if (Limit != null)
        {
            PushdownIntoSubquery();
        }

        Limit = sqlExpression;
    }

    /// <summary>
    ///     Applies offset to the <see cref="SelectExpression" /> to skip the number of rows in the result set.
    /// </summary>
    /// <param name="sqlExpression">An expression representing offset row count.</param>
    public void ApplyOffset(SqlExpression sqlExpression)
    {
        if (Limit != null
            || Offset != null
            || (IsDistinct && Orderings.Count == 0))
        {
            PushdownIntoSubquery();
        }

        Offset = sqlExpression;
    }

    private enum SetOperationType
    {
        Except,
        Intersect,
        Union
    }

    /// <summary>
    ///     Applies EXCEPT operation to the <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="source2">A <see cref="SelectExpression" /> to perform the operation.</param>
    /// <param name="distinct">A bool value indicating if resulting table source should remove duplicates.</param>
    public void ApplyExcept(SelectExpression source2, bool distinct)
        => ApplySetOperation(SetOperationType.Except, source2, distinct);

    /// <summary>
    ///     Applies INTERSECT operation to the <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="source2">A <see cref="SelectExpression" /> to perform the operation.</param>
    /// <param name="distinct">A bool value indicating if resulting table source should remove duplicates.</param>
    public void ApplyIntersect(SelectExpression source2, bool distinct)
        => ApplySetOperation(SetOperationType.Intersect, source2, distinct);

    /// <summary>
    ///     Applies UNION operation to the <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="source2">A <see cref="SelectExpression" /> to perform the operation.</param>
    /// <param name="distinct">A bool value indicating if resulting table source should remove duplicates.</param>
    public void ApplyUnion(SelectExpression source2, bool distinct)
        => ApplySetOperation(SetOperationType.Union, source2, distinct);

    private void ApplySetOperation(SetOperationType setOperationType, SelectExpression select2, bool distinct)
    {
        // TODO: Introduce clone method? See issue#24460
        var select1 = new SelectExpression(
            null, new List<ProjectionExpression>(), _tables.ToList(), _tableReferences.ToList(), _groupBy.ToList(), _orderings.ToList(), GetAnnotations())
        {
            IsDistinct = IsDistinct,
            Predicate = Predicate,
            Having = Having,
            Offset = Offset,
            Limit = Limit,
            _groupingParentSelectExpressionId = _groupingParentSelectExpressionId,
            _groupingCorrelationPredicate = _groupingCorrelationPredicate
        };
        Offset = null;
        Limit = null;
        IsDistinct = false;
        Predicate = null;
        Having = null;
        _groupingCorrelationPredicate = null;
        _groupingParentSelectExpressionId = null;
        _groupBy.Clear();
        _orderings.Clear();
        _tables.Clear();
        _tableReferences.Clear();
        select1._projectionMapping = new Dictionary<ProjectionMember, Expression>(_projectionMapping);
        _projectionMapping.Clear();

        // Remap tableReferences in select1
        foreach (var tableReference in select1._tableReferences)
        {
            tableReference.UpdateTableReference(this, select1);
        }

        var tableReferenceUpdatingExpressionVisitor = new TableReferenceUpdatingExpressionVisitor(this, select1);
        tableReferenceUpdatingExpressionVisitor.Visit(select1);

        select1._identifier.AddRange(_identifier);
        _identifier.Clear();
        var outerIdentifiers = select1._identifier.Count == select2._identifier.Count
            ? new ColumnExpression?[select1._identifier.Count]
            : Array.Empty<ColumnExpression?>();
        var entityProjectionIdentifiers = new List<ColumnExpression>();
        var entityProjectionValueComparers = new List<ValueComparer>();
        var otherExpressions = new List<SqlExpression>();

        if (select1.Orderings.Count != 0
            || select1.Limit != null
            || select1.Offset != null)
        {
            // If we are pushing down here, we need to make sure to assign unique alias to subquery also.
            var subqueryAlias = GenerateUniqueAlias(_usedAliases, "t");
            select1.PushdownIntoSubquery();
            select1._tables[0].Alias = subqueryAlias;
            select1._tableReferences[0].Alias = subqueryAlias;
            select1.ClearOrdering();
        }

        if (select2.Orderings.Count != 0
            || select2.Limit != null
            || select2.Offset != null)
        {
            select2.PushdownIntoSubquery();
            select2.ClearOrdering();
        }

        if (_clientProjections.Count > 0
            || select2._clientProjections.Count > 0)
        {
            throw new InvalidOperationException(RelationalStrings.SetOperationsNotAllowedAfterClientEvaluation);
        }

        if (select1._projectionMapping.Count != select2._projectionMapping.Count)
        {
            // For DTO each side can have different projection mapping if some columns are not present.
            // We need to project null for missing columns.
            throw new InvalidOperationException(RelationalStrings.ProjectionMappingCountMismatch);
        }

        var setOperationAlias = GenerateUniqueAlias(_usedAliases, "t");
        var tableReferenceExpression = new TableReferenceExpression(this, setOperationAlias);

        var aliasUniquifier = new AliasUniquifier(_usedAliases);
        foreach (var (projectionMember, expression1, expression2) in select1._projectionMapping.Join(
                     select2._projectionMapping,
                     kv => kv.Key,
                     kv => kv.Key,
                     (kv1, kv2) => (kv1.Key, Value1: kv1.Value, Value2: kv2.Value)))
        {
            if (expression1 is EntityProjectionExpression entityProjection1
                && expression2 is EntityProjectionExpression entityProjection2)
            {
                HandleEntityProjection(projectionMember, select1, entityProjection1, select2, entityProjection2);
                continue;
            }

            var innerColumn1 = (SqlExpression)expression1;
            var innerColumn2 = (SqlExpression)expression2;
            // For now, make sure that both sides output the same store type, otherwise the query may fail.
            // TODO: with #15586 we'll be able to also allow different store types which are implicitly convertible to one another.
            if (innerColumn1.TypeMapping!.StoreType != innerColumn2.TypeMapping!.StoreType)
            {
                throw new InvalidOperationException(RelationalStrings.SetOperationsOnDifferentStoreTypes);
            }

            innerColumn1 = select1.TryLiftGroupByAggregate(innerColumn1);
            innerColumn2 = select2.TryLiftGroupByAggregate(innerColumn2);

            // We have to unique-fy left side since those projections were never uniquified
            // Right side is unique already when we did it when running select2 through it.
            innerColumn1 = (SqlExpression)aliasUniquifier.Visit(innerColumn1);

            var alias = GenerateUniqueColumnAlias(
                projectionMember.Last?.Name
                ?? (innerColumn1 as ColumnExpression)?.Name
                ?? "c");

            var innerProjection1 = new ProjectionExpression(innerColumn1, alias);
            var innerProjection2 = new ProjectionExpression(innerColumn2, alias);
            select1._projection.Add(innerProjection1);
            select2._projection.Add(innerProjection2);
            var outerProjection = new ConcreteColumnExpression(innerProjection1, tableReferenceExpression);

            if (IsNullableProjection(innerProjection1)
                || IsNullableProjection(innerProjection2))
            {
                outerProjection = outerProjection.MakeNullable();
            }

            _projectionMapping[projectionMember] = outerProjection;

            if (outerIdentifiers.Length > 0)
            {
                var index = select1._identifier.FindIndex(e => e.Column.Equals(expression1));
                if (index != -1)
                {
                    if (select2._identifier[index].Column.Equals(expression2))
                    {
                        outerIdentifiers[index] = outerProjection;
                    }
                    else
                    {
                        // If select1 matched but select2 did not then we erase all identifiers
                        // TODO: We could make this little more robust by allow the indexes to be different. See issue#24475
                        // i.e. Identifier ordering being different.
                        outerIdentifiers = Array.Empty<ColumnExpression>();
                    }
                }

                otherExpressions.Add(outerProjection);
            }
        }

        // We generate actual set operation after applying projection to lift group by aggregate
        // select1 already has unique aliases. We unique-fy select2 and set operation alias.
        select2 = (SelectExpression)aliasUniquifier.Visit(select2);
        var setExpression = setOperationType switch
        {
            SetOperationType.Except => (SetOperationBase)new ExceptExpression(setOperationAlias, select1, select2, distinct),
            SetOperationType.Intersect => new IntersectExpression(setOperationAlias, select1, select2, distinct),
            SetOperationType.Union => new UnionExpression(setOperationAlias, select1, select2, distinct),
            _ => throw new InvalidOperationException(CoreStrings.InvalidSwitch(nameof(setOperationType), setOperationType))
        };
        _tables.Add(setExpression);
        _tableReferences.Add(tableReferenceExpression);

        // We should apply _identifiers only when it is distinct and actual select expression had identifiers.
        if (distinct
            && outerIdentifiers.Length > 0)
        {
            // If we find matching identifier in outer level then we just use them.
            if (outerIdentifiers.All(e => e != null))
            {
                _identifier.AddRange(outerIdentifiers.Zip(select1._identifier, (c, i) => (c!, i.Comparer)));
            }
            else
            {
                _identifier.Clear();
                if (otherExpressions.Count == 0)
                {
                    // If there are no other expressions then we can use all entityProjectionIdentifiers
                    _identifier.AddRange(entityProjectionIdentifiers.Zip(entityProjectionValueComparers));
                }
                else if (otherExpressions.All(e => e is ColumnExpression))
                {
                    _identifier.AddRange(entityProjectionIdentifiers.Zip(entityProjectionValueComparers));
                    _identifier.AddRange(otherExpressions.Select(e => ((ColumnExpression)e, e.TypeMapping!.KeyComparer)));
                }
            }
        }

        void HandleEntityProjection(
            ProjectionMember projectionMember,
            SelectExpression select1,
            EntityProjectionExpression projection1,
            SelectExpression select2,
            EntityProjectionExpression projection2)
        {
            if (projection1.EntityType != projection2.EntityType)
            {
                throw new InvalidOperationException(RelationalStrings.SetOperationsOnDifferentStoreTypes);
            }

            var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
            foreach (var property in GetAllPropertiesInHierarchy(projection1.EntityType))
            {
                var column1 = projection1.BindProperty(property);
                var column2 = projection2.BindProperty(property);
                var alias = GenerateUniqueColumnAlias(column1.Name);
                var innerProjection = new ProjectionExpression(column1, alias);
                select1._projection.Add(innerProjection);
                select2._projection.Add(new ProjectionExpression(column2, alias));
                var outerExpression = new ConcreteColumnExpression(innerProjection, tableReferenceExpression);
                if (column1.IsNullable
                    || column2.IsNullable)
                {
                    outerExpression = outerExpression.MakeNullable();
                }

                propertyExpressions[property] = outerExpression;

                if (outerIdentifiers.Length > 0)
                {
                    var index = select1._identifier.FindIndex(e => e.Column.Equals(column1));
                    if (index != -1)
                    {
                        if (select2._identifier[index].Column.Equals(column2))
                        {
                            outerIdentifiers[index] = outerExpression;
                        }
                        else
                        {
                            // If select1 matched but select2 did not then we erase all identifiers
                            // TODO: We could make this little more robust by allow the indexes to be different. See issue#24475
                            // i.e. Identifier ordering being different.
                            outerIdentifiers = Array.Empty<ColumnExpression>();
                        }
                    }
                }
            }

            var discriminatorExpression = projection1.DiscriminatorExpression;
            if (projection1.DiscriminatorExpression != null
                && projection2.DiscriminatorExpression != null)
            {
                var alias = GenerateUniqueColumnAlias(DiscriminatorColumnAlias);
                var innerProjection = new ProjectionExpression(projection1.DiscriminatorExpression, alias);
                select1._projection.Add(innerProjection);
                select2._projection.Add(new ProjectionExpression(projection2.DiscriminatorExpression, alias));
                discriminatorExpression = new ConcreteColumnExpression(innerProjection, tableReferenceExpression);
            }

            var entityProjection = new EntityProjectionExpression(projection1.EntityType, propertyExpressions, discriminatorExpression);

            if (outerIdentifiers.Length > 0)
            {
                var primaryKey = entityProjection.EntityType.FindPrimaryKey();
                // If there are any existing identifier then all entity projection must have a key
                // else keyless entity would have wiped identifier when generating join.
                Check.DebugAssert(primaryKey != null, "primary key is null.");
                foreach (var property in primaryKey.Properties)
                {
                    entityProjectionIdentifiers.Add(entityProjection.BindProperty(property));
                    entityProjectionValueComparers.Add(property.GetKeyValueComparer());
                }
            }

            _projectionMapping[projectionMember] = entityProjection;
        }

        string GenerateUniqueColumnAlias(string baseAlias)
        {
            var currentAlias = baseAlias;
            var counter = 0;
            while (select1._projection.Any(pe => string.Equals(pe.Alias, currentAlias, StringComparison.OrdinalIgnoreCase)))
            {
                currentAlias = $"{baseAlias}{counter++}";
            }

            return currentAlias;
        }

        static bool IsNullableProjection(ProjectionExpression projectionExpression)
            => projectionExpression.Expression switch
            {
                ColumnExpression columnExpression => columnExpression.IsNullable,
                SqlConstantExpression sqlConstantExpression => sqlConstantExpression.Value == null,
                _ => true
            };
    }

    /// <summary>
    ///     Applies <see cref="Queryable.DefaultIfEmpty{TSource}(IQueryable{TSource})" /> on the <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="sqlExpressionFactory">A factory to use for generating required sql expressions.</param>
    public void ApplyDefaultIfEmpty(ISqlExpressionFactory sqlExpressionFactory)
    {
        var nullSqlExpression = sqlExpressionFactory.ApplyDefaultTypeMapping(
            new SqlConstantExpression(Constant(null, typeof(string)), null));

        var dummySelectExpression = new SelectExpression(
            alias: "e",
            new List<ProjectionExpression> { new(nullSqlExpression, "empty") },
            new List<TableExpressionBase>(),
            new List<TableReferenceExpression>(),
            new List<SqlExpression>(),
            new List<OrderingExpression>(),
            Enumerable.Empty<IAnnotation>());

        if (Orderings.Any()
            || Limit != null
            || Offset != null
            || IsDistinct
            || Predicate != null
            || Tables.Count > 1
            || GroupBy.Count > 0)
        {
            PushdownIntoSubquery();
        }

        var joinPredicate = sqlExpressionFactory.Equal(sqlExpressionFactory.Constant(1), sqlExpressionFactory.Constant(1));
        var joinTable = new LeftJoinExpression(Tables.Single(), joinPredicate);
        var joinTableReferenceExpression = _tableReferences.Single();
        _tables.Clear();
        _tableReferences.Clear();
        AddTable(dummySelectExpression, new TableReferenceExpression(this, dummySelectExpression.Alias!));
        // Do NOT use AddTable here since we are adding the same table which was current as join table we don't need to traverse it.
        _tables.Add(joinTable);
        _tableReferences.Add(joinTableReferenceExpression);

        var projectionMapping = new Dictionary<ProjectionMember, Expression>();
        foreach (var projection in _projectionMapping)
        {
            var projectionToAdd = projection.Value;
            if (projectionToAdd is EntityProjectionExpression entityProjection)
            {
                projectionToAdd = entityProjection.MakeNullable();
            }
            else if (projectionToAdd is ColumnExpression column)
            {
                projectionToAdd = column.MakeNullable();
            }

            projectionMapping[projection.Key] = projectionToAdd;
        }

        // ChildIdentifiers shouldn't be required to be updated since during translation they should be empty.
        for (var i = 0; i < _identifier.Count; i++)
        {
            if (_identifier[i].Column is ColumnExpression column)
            {
                _identifier[i] = (column.MakeNullable(), _identifier[i].Comparer);
            }
        }

        _projectionMapping = projectionMapping;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public EntityProjectionExpression? GenerateWeakEntityProjectionExpression(
        IEntityType entityType,
        ITableBase table,
        string? columnName,
        TableExpressionBase tableExpressionBase,
        bool nullable = true)
    {
        if (columnName == null)
        {
            // This is when projections are coming from a joined table.
            var propertyExpressions = GetPropertyExpressionsFromJoinedTable(
                entityType, table, FindTableReference(this, tableExpressionBase));

            return new EntityProjectionExpression(entityType, propertyExpressions);
        }
        else
        {
            var propertyExpressions = GetPropertyExpressionFromSameTable(
                entityType, table, this, tableExpressionBase, columnName, nullable);

            return propertyExpressions == null
                ? null
                : new EntityProjectionExpression(entityType, propertyExpressions);
        }

        static TableReferenceExpression FindTableReference(SelectExpression selectExpression, TableExpressionBase tableExpression)
        {
            var tableIndex = selectExpression._tables.FindIndex(e => ReferenceEquals(e, tableExpression));
            return selectExpression._tableReferences[tableIndex];
        }

        static IReadOnlyDictionary<IProperty, ColumnExpression>? GetPropertyExpressionFromSameTable(
            IEntityType entityType,
            ITableBase table,
            SelectExpression selectExpression,
            TableExpressionBase tableExpressionBase,
            string columnName,
            bool nullable)
        {
            var unwrappedTable = UnwrapJoinExpression(tableExpressionBase);
            if (unwrappedTable is TableExpression tableExpression)
            {
                if (!string.Equals(tableExpression.Name, table.Name, StringComparison.OrdinalIgnoreCase))
                {
                    // Fetch the table for the type which is defining the navigation since dependent would be in that table
                    tableExpressionBase = selectExpression.Tables
                        .First(
                            e =>
                            {
                                var t = (TableExpression)UnwrapJoinExpression(e);
                                return t.Name == table.Name && t.Schema == table.Schema;
                            });
                }

                var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
                var tableReferenceExpression = FindTableReference(selectExpression, tableExpressionBase);
                foreach (var property in entityType
                             .GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                             .SelectMany(t => t.GetDeclaredProperties()))
                {
                    propertyExpressions[property] = new ConcreteColumnExpression(
                        property, table.FindColumn(property)!, tableReferenceExpression, nullable || !property.IsPrimaryKey());
                }

                return propertyExpressions;
            }

            if (unwrappedTable is SelectExpression subquery)
            {
                var subqueryIdentifyingColumn = (ColumnExpression)subquery.Projection
                    .Single(e => string.Equals(e.Alias, columnName, StringComparison.OrdinalIgnoreCase))
                    .Expression;

                var subqueryPropertyExpressions = GetPropertyExpressionFromSameTable(
                    entityType, table, subquery, subqueryIdentifyingColumn.Table, subqueryIdentifyingColumn.Name, nullable);
                if (subqueryPropertyExpressions == null)
                {
                    return null;
                }

                var newPropertyExpressions = new Dictionary<IProperty, ColumnExpression>();
                var tableReferenceExpression = FindTableReference(selectExpression, tableExpressionBase);
                foreach (var (property, columnExpression) in subqueryPropertyExpressions)
                {
                    newPropertyExpressions[property] = subquery.GenerateOuterColumn(tableReferenceExpression, columnExpression);
                }

                return newPropertyExpressions;
            }

            return null;
        }

        static IReadOnlyDictionary<IProperty, ColumnExpression> GetPropertyExpressionsFromJoinedTable(
            IEntityType entityType,
            ITableBase table,
            TableReferenceExpression tableReferenceExpression)
        {
            var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
            foreach (var property in entityType
                         .GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                         .SelectMany(t => t.GetDeclaredProperties()))
            {
                propertyExpressions[property] = new ConcreteColumnExpression(
                    property, table.FindColumn(property)!, tableReferenceExpression, nullable: true);
            }

            return propertyExpressions;
        }
    }

    private enum JoinType
    {
        InnerJoin,
        LeftJoin,
        CrossJoin,
        CrossApply,
        OuterApply
    }

    private Expression AddJoin(
        JoinType joinType,
        SelectExpression innerSelectExpression,
        Expression outerShaper,
        Expression innerShaper,
        SqlExpression? joinPredicate = null)
    {
        AddJoin(joinType, ref innerSelectExpression, out _, joinPredicate);

        var transparentIdentifierType = TransparentIdentifierFactory.Create(outerShaper.Type, innerShaper.Type);
        var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer")!;
        var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner")!;
        var outerClientEval = _clientProjections.Count > 0;
        var innerClientEval = innerSelectExpression._clientProjections.Count > 0;
        var innerNullable = joinType == JoinType.LeftJoin || joinType == JoinType.OuterApply;

        if (outerClientEval)
        {
            // Outer projection are already populated
            if (innerClientEval)
            {
                // Add inner to projection and update indexes
                var indexMap = new int[innerSelectExpression._clientProjections.Count];
                for (var i = 0; i < innerSelectExpression._clientProjections.Count; i++)
                {
                    var projectionToAdd = innerSelectExpression._clientProjections[i];
                    projectionToAdd = MakeNullable(projectionToAdd, innerNullable);
                    _clientProjections.Add(projectionToAdd);
                    _aliasForClientProjections.Add(innerSelectExpression._aliasForClientProjections[i]);
                    indexMap[i] = _clientProjections.Count - 1;
                }

                innerSelectExpression._clientProjections.Clear();
                innerSelectExpression._aliasForClientProjections.Clear();

                innerShaper = new ProjectionIndexRemappingExpressionVisitor(innerSelectExpression, this, indexMap).Visit(innerShaper);
            }
            else
            {
                // Apply inner projection mapping and convert projection member binding to indexes
                var mapping = ConvertProjectionMappingToClientProjections(innerSelectExpression._projectionMapping, innerNullable);
                innerShaper = new ProjectionMemberToIndexConvertingExpressionVisitor(this, mapping).Visit(innerShaper);
            }
        }
        else
        {
            // Depending on inner, we may either need to populate outer projection or update projection members
            if (innerClientEval)
            {
                // Since inner projections are populated, we need to populate outer also
                var mapping = ConvertProjectionMappingToClientProjections(_projectionMapping);
                outerShaper = new ProjectionMemberToIndexConvertingExpressionVisitor(this, mapping).Visit(outerShaper);

                var indexMap = new int[innerSelectExpression._clientProjections.Count];
                for (var i = 0; i < innerSelectExpression._clientProjections.Count; i++)
                {
                    var projectionToAdd = innerSelectExpression._clientProjections[i];
                    projectionToAdd = MakeNullable(projectionToAdd, innerNullable);
                    _clientProjections.Add(projectionToAdd);
                    _aliasForClientProjections.Add(innerSelectExpression._aliasForClientProjections[i]);
                    indexMap[i] = _clientProjections.Count - 1;
                }

                innerSelectExpression._clientProjections.Clear();
                innerSelectExpression._aliasForClientProjections.Clear();

                innerShaper = new ProjectionIndexRemappingExpressionVisitor(innerSelectExpression, this, indexMap).Visit(innerShaper);
            }
            else
            {
                var projectionMapping = new Dictionary<ProjectionMember, Expression>();
                var mapping = new Dictionary<ProjectionMember, ProjectionMember>();

                foreach (var (projectionMember, expression) in _projectionMapping)
                {
                    var remappedProjectionMember = projectionMember.Prepend(outerMemberInfo);
                    mapping[projectionMember] = remappedProjectionMember;
                    projectionMapping[remappedProjectionMember] = expression;
                }

                outerShaper = new ProjectionMemberRemappingExpressionVisitor(this, mapping).Visit(outerShaper);
                mapping.Clear();

                foreach (var projection in innerSelectExpression._projectionMapping)
                {
                    var projectionMember = projection.Key;
                    var remappedProjectionMember = projection.Key.Prepend(innerMemberInfo);
                    mapping[projectionMember] = remappedProjectionMember;
                    var projectionToAdd = projection.Value;
                    projectionToAdd = MakeNullable(projectionToAdd, innerNullable);
                    projectionMapping[remappedProjectionMember] = projectionToAdd;
                }

                innerShaper = new ProjectionMemberRemappingExpressionVisitor(this, mapping).Visit(innerShaper);
                _projectionMapping = projectionMapping;
                innerSelectExpression._projectionMapping.Clear();
            }
        }

        if (innerNullable)
        {
            innerShaper = new EntityShaperNullableMarkingExpressionVisitor().Visit(innerShaper);
        }

        return New(
            transparentIdentifierType.GetTypeInfo().DeclaredConstructors.Single(),
            new[] { outerShaper, innerShaper }, outerMemberInfo, innerMemberInfo);
    }

    private void AddJoin(
        JoinType joinType,
        ref SelectExpression innerSelectExpression,
        out bool innerPushdownOccurred,
        SqlExpression? joinPredicate = null)
    {
        innerPushdownOccurred = false;
        // Try to convert Apply to normal join
        if (joinType == JoinType.CrossApply
            || joinType == JoinType.OuterApply)
        {
            var limit = innerSelectExpression.Limit;
            var offset = innerSelectExpression.Offset;
            if (!innerSelectExpression.IsDistinct
                || (limit == null && offset == null))
            {
                innerSelectExpression.Limit = null;
                innerSelectExpression.Offset = null;

                joinPredicate = TryExtractJoinKey(this, innerSelectExpression, allowNonEquality: limit == null && offset == null);
                if (joinPredicate != null)
                {
                    var containsOuterReference = new SelectExpressionCorrelationFindingExpressionVisitor(this)
                        .ContainsOuterReference(innerSelectExpression);
                    if (!containsOuterReference)
                    {
                        if (limit != null || offset != null)
                        {
                            var partitions = new List<SqlExpression>();
                            GetPartitions(innerSelectExpression, joinPredicate, partitions);
                            var orderings = innerSelectExpression.Orderings.Count > 0
                                ? innerSelectExpression.Orderings
                                : innerSelectExpression._identifier.Count > 0
                                    ? innerSelectExpression._identifier.Select(e => new OrderingExpression(e.Column, true))
                                    : new[] { new OrderingExpression(new SqlFragmentExpression("(SELECT 1)"), true) };

                            var rowNumberExpression = new RowNumberExpression(
                                partitions, orderings.ToList(), (limit ?? offset)!.TypeMapping);
                            innerSelectExpression.ClearOrdering();

                            joinPredicate = innerSelectExpression.PushdownIntoSubqueryInternal().Remap(joinPredicate);

                            var subqueryTableReference = innerSelectExpression._tableReferences.Single();
                            var outerColumn = ((SelectExpression)innerSelectExpression.Tables[0]).GenerateOuterColumn(
                                subqueryTableReference, rowNumberExpression, "row");
                            SqlExpression? offsetPredicate = null;
                            SqlExpression? limitPredicate = null;
                            if (offset != null)
                            {
                                offsetPredicate = new SqlBinaryExpression(
                                    ExpressionType.LessThan, offset, outerColumn, typeof(bool), joinPredicate.TypeMapping);
                            }

                            if (limit != null)
                            {
                                if (offset != null)
                                {
                                    limit = offset is SqlConstantExpression offsetConstant
                                        && limit is SqlConstantExpression limitConstant
                                            ? new SqlConstantExpression(
                                                Constant((int)offsetConstant.Value! + (int)limitConstant.Value!),
                                                limit.TypeMapping)
                                            : new SqlBinaryExpression(ExpressionType.Add, offset, limit, limit.Type, limit.TypeMapping);
                                }

                                limitPredicate = new SqlBinaryExpression(
                                    ExpressionType.LessThanOrEqual, outerColumn, limit, typeof(bool), joinPredicate.TypeMapping);
                            }

                            var predicate = offsetPredicate != null
                                ? limitPredicate != null
                                    ? new SqlBinaryExpression(
                                        ExpressionType.AndAlso, offsetPredicate, limitPredicate, typeof(bool),
                                        joinPredicate.TypeMapping)
                                    : offsetPredicate
                                : limitPredicate;
                            innerSelectExpression.ApplyPredicate(predicate!);
                        }

                        AddJoin(
                            joinType == JoinType.CrossApply ? JoinType.InnerJoin : JoinType.LeftJoin,
                            ref innerSelectExpression,
                            out innerPushdownOccurred,
                            joinPredicate);

                        return;
                    }

                    innerSelectExpression.ApplyPredicate(joinPredicate);
                    joinPredicate = null;
                }

                // Order matters Apply Offset before Limit
                if (offset != null)
                {
                    innerSelectExpression.ApplyOffset(offset);
                }

                if (limit != null)
                {
                    innerSelectExpression.ApplyLimit(limit);
                }
            }
        }

        if (Limit != null
            || Offset != null
            || IsDistinct
            || GroupBy.Count > 0)
        {
            var sqlRemappingVisitor = PushdownIntoSubqueryInternal();
            innerSelectExpression = sqlRemappingVisitor.Remap(innerSelectExpression);
            joinPredicate = sqlRemappingVisitor.Remap(joinPredicate);
        }

        if (innerSelectExpression.Limit != null
            || innerSelectExpression.Offset != null
            || innerSelectExpression.IsDistinct
            || innerSelectExpression.Predicate != null
            || innerSelectExpression.Tables.Count > 1
            || innerSelectExpression.GroupBy.Count > 0)
        {
            joinPredicate = innerSelectExpression.PushdownIntoSubqueryInternal().Remap(joinPredicate);
            innerPushdownOccurred = true;
        }

        if (_identifier.Count > 0
            && innerSelectExpression._identifier.Count > 0)
        {
            if (joinType == JoinType.LeftJoin
                || joinType == JoinType.OuterApply)
            {
                _identifier.AddRange(innerSelectExpression._identifier.Select(e => (e.Column.MakeNullable(), e.Comparer)));
            }
            else
            {
                _identifier.AddRange(innerSelectExpression._identifier);
            }
        }
        else
        {
            // if the subquery that is joined to can't be uniquely identified
            // then the entire join should also not be marked as non-identifiable
            _identifier.Clear();
            innerSelectExpression._identifier.Clear();
        }

        var innerTable = innerSelectExpression.Tables.Single();
        var joinTable = joinType switch
        {
            JoinType.InnerJoin => new InnerJoinExpression(innerTable, joinPredicate!),
            JoinType.LeftJoin => new LeftJoinExpression(innerTable, joinPredicate!),
            JoinType.CrossJoin => new CrossJoinExpression(innerTable),
            JoinType.CrossApply => new CrossApplyExpression(innerTable),
            JoinType.OuterApply => (TableExpressionBase)new OuterApplyExpression(innerTable),
            _ => throw new InvalidOperationException(CoreStrings.InvalidSwitch(nameof(joinType), joinType))
        };

        var tableReferenceExpression = innerSelectExpression._tableReferences[0];
        tableReferenceExpression.UpdateTableReference(innerSelectExpression, this);
        AddTable(joinTable, tableReferenceExpression);

        static void GetPartitions(SelectExpression selectExpression, SqlExpression sqlExpression, List<SqlExpression> partitions)
        {
            if (sqlExpression is SqlBinaryExpression sqlBinaryExpression)
            {
                if (sqlBinaryExpression.OperatorType == ExpressionType.Equal)
                {
                    if (sqlBinaryExpression.Left is ColumnExpression columnExpression
                        && selectExpression.ContainsTableReference(columnExpression))
                    {
                        partitions.Add(sqlBinaryExpression.Left);
                    }
                    else
                    {
                        partitions.Add(sqlBinaryExpression.Right);
                    }
                }
                else if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                {
                    GetPartitions(selectExpression, sqlBinaryExpression.Left, partitions);
                    GetPartitions(selectExpression, sqlBinaryExpression.Right, partitions);
                }
            }
        }

        static SqlExpression? TryExtractJoinKey(SelectExpression outer, SelectExpression inner, bool allowNonEquality)
        {
            if (inner.Limit != null
                || inner.Offset != null)
            {
                return null;
            }

            var predicate = inner.GroupBy.Count > 0 ? inner.Having : inner.Predicate;
            if (predicate == null)
            {
                return null;
            }

            var outerColumnExpressions = new List<SqlExpression>();
            var joinPredicate = TryExtractJoinKey(
                outer,
                inner,
                predicate,
                outerColumnExpressions,
                allowNonEquality,
                out var updatedPredicate);

            if (joinPredicate != null)
            {
                joinPredicate = RemoveRedundantNullChecks(joinPredicate, outerColumnExpressions);
            }

            // we can't convert apply to join in case of distinct and groupby, if the projection doesn't already contain the join keys
            // since we can't add the missing keys to the projection - only convert to join if all the keys are already there
            if (joinPredicate != null
                && (inner.IsDistinct
                    || inner.GroupBy.Count > 0))
            {
                var innerKeyColumns = new List<ColumnExpression>();
                PopulateInnerKeyColumns(inner.Tables, joinPredicate, innerKeyColumns);

                // if projection has already been applied we can use it directly
                // otherwise we extract future projection columns from projection mapping
                // and based on that we determine whether we can convert from APPLY to JOIN
                var projectionColumns = inner.Projection.Count > 0
                    ? inner.Projection.Select(p => p.Expression)
                    : ExtractColumnsFromProjectionMapping(inner._projectionMapping);

                foreach (var innerColumn in innerKeyColumns)
                {
                    if (!projectionColumns.Contains(innerColumn))
                    {
                        return null;
                    }
                }
            }

            if (inner.GroupBy.Count > 0)
            {
                inner.Having = updatedPredicate;
            }
            else
            {
                inner.Predicate = updatedPredicate;
            }

            return joinPredicate;

            static SqlExpression? TryExtractJoinKey(
                SelectExpression outer,
                SelectExpression inner,
                SqlExpression predicate,
                List<SqlExpression> outerColumnExpressions,
                bool allowNonEquality,
                out SqlExpression? updatedPredicate)
            {
                if (predicate is SqlBinaryExpression sqlBinaryExpression)
                {
                    var joinPredicate = ValidateKeyComparison(
                        outer, inner, sqlBinaryExpression, outerColumnExpressions, allowNonEquality);
                    if (joinPredicate != null)
                    {
                        updatedPredicate = null;

                        return joinPredicate;
                    }

                    if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                    {
                        var leftJoinKey = TryExtractJoinKey(
                            outer, inner, sqlBinaryExpression.Left, outerColumnExpressions, allowNonEquality, out var leftPredicate);
                        var rightJoinKey = TryExtractJoinKey(
                            outer, inner, sqlBinaryExpression.Right, outerColumnExpressions, allowNonEquality, out var rightPredicate);

                        updatedPredicate = CombineNonNullExpressions(leftPredicate, rightPredicate);

                        return CombineNonNullExpressions(leftJoinKey, rightJoinKey);
                    }
                }

                updatedPredicate = predicate;

                return null;
            }

            static SqlBinaryExpression? ValidateKeyComparison(
                SelectExpression outer,
                SelectExpression inner,
                SqlBinaryExpression sqlBinaryExpression,
                List<SqlExpression> outerColumnExpressions,
                bool allowNonEquality)
            {
                if (sqlBinaryExpression.OperatorType == ExpressionType.Equal
                    || (allowNonEquality
                        && (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual
                            || sqlBinaryExpression.OperatorType == ExpressionType.GreaterThan
                            || sqlBinaryExpression.OperatorType == ExpressionType.GreaterThanOrEqual
                            || sqlBinaryExpression.OperatorType == ExpressionType.LessThan
                            || sqlBinaryExpression.OperatorType == ExpressionType.LessThanOrEqual)))
                {
                    if (IsContainedColumn(outer, sqlBinaryExpression.Left)
                        && IsContainedColumn(inner, sqlBinaryExpression.Right))
                    {
                        outerColumnExpressions.Add(sqlBinaryExpression.Left);

                        return sqlBinaryExpression;
                    }

                    if (IsContainedColumn(outer, sqlBinaryExpression.Right)
                        && IsContainedColumn(inner, sqlBinaryExpression.Left))
                    {
                        outerColumnExpressions.Add(sqlBinaryExpression.Right);

                        return new SqlBinaryExpression(
                            MirroredOperationMap[sqlBinaryExpression.OperatorType],
                            sqlBinaryExpression.Right,
                            sqlBinaryExpression.Left,
                            sqlBinaryExpression.Type,
                            sqlBinaryExpression.TypeMapping);
                    }
                }

                // null checks are considered part of join key
                if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
                {
                    if (IsContainedColumn(outer, sqlBinaryExpression.Left)
                        && sqlBinaryExpression.Right is SqlConstantExpression rightConstant
                        && rightConstant.Value == null)
                    {
                        return sqlBinaryExpression;
                    }

                    if (IsContainedColumn(outer, sqlBinaryExpression.Right)
                        && sqlBinaryExpression.Left is SqlConstantExpression leftConstant
                        && leftConstant.Value == null)
                    {
                        return sqlBinaryExpression.Update(
                            sqlBinaryExpression.Right,
                            sqlBinaryExpression.Left);
                    }
                }

                return null;
            }

            static bool IsContainedColumn(SelectExpression selectExpression, SqlExpression sqlExpression)
            {
                switch (sqlExpression)
                {
                    case ColumnExpression columnExpression:
                        return selectExpression.ContainsTableReference(columnExpression);

                    case SqlConstantExpression sqlConstantExpression
                        when sqlConstantExpression.Value == null:
                        return true;

                    case SqlBinaryExpression sqlBinaryExpression
                        when sqlBinaryExpression.OperatorType == ExpressionType.AndAlso
                        || sqlBinaryExpression.OperatorType == ExpressionType.OrElse
                        || sqlBinaryExpression.OperatorType == ExpressionType.NotEqual:
                        return IsContainedColumn(selectExpression, sqlBinaryExpression.Left)
                            && IsContainedColumn(selectExpression, sqlBinaryExpression.Right);

                    case CaseExpression caseExpression
                        when caseExpression.ElseResult == null
                        && caseExpression.Operand == null
                        && caseExpression.WhenClauses.Count == 1:
                        return IsContainedColumn(selectExpression, caseExpression.WhenClauses[0].Test)
                            && IsContainedColumn(selectExpression, caseExpression.WhenClauses[0].Result);

                    default:
                        return false;
                }
            }

            static void PopulateInnerKeyColumns(
                IEnumerable<TableExpressionBase> tables,
                SqlExpression joinPredicate,
                List<ColumnExpression> resultColumns)
            {
                if (joinPredicate is SqlBinaryExpression sqlBinaryExpression)
                {
                    PopulateInnerKeyColumns(tables, sqlBinaryExpression.Left, resultColumns);
                    PopulateInnerKeyColumns(tables, sqlBinaryExpression.Right, resultColumns);
                }
                else if (joinPredicate is ColumnExpression columnExpression
                         && tables.Contains(columnExpression.Table))
                {
                    resultColumns.Add(columnExpression);
                }
            }

            static List<ColumnExpression> ExtractColumnsFromProjectionMapping(
                IDictionary<ProjectionMember, Expression> projectionMapping)
            {
                var result = new List<ColumnExpression>();
                foreach (var (projectionMember, expression) in projectionMapping)
                {
                    if (expression is EntityProjectionExpression entityProjection)
                    {
                        foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
                        {
                            result.Add(entityProjection.BindProperty(property));
                        }

                        if (entityProjection.DiscriminatorExpression != null
                            && entityProjection.DiscriminatorExpression is ColumnExpression discriminatorColumn)
                        {
                            result.Add(discriminatorColumn);
                        }
                    }
                    else if (expression is ColumnExpression column)
                    {
                        result.Add(column);
                    }
                }

                return result;
            }

            static SqlExpression? CombineNonNullExpressions(SqlExpression? left, SqlExpression? right)
                => left != null
                    ? right != null
                        ? new SqlBinaryExpression(ExpressionType.AndAlso, left, right, left.Type, left.TypeMapping)
                        : left
                    : right;

            static SqlExpression? RemoveRedundantNullChecks(SqlExpression predicate, List<SqlExpression> outerColumnExpressions)
            {
                if (predicate is SqlBinaryExpression sqlBinaryExpression)
                {
                    if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual
                        && outerColumnExpressions.Contains(sqlBinaryExpression.Left)
                        && sqlBinaryExpression.Right is SqlConstantExpression sqlConstantExpression
                        && sqlConstantExpression.Value == null)
                    {
                        return null;
                    }

                    if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                    {
                        var leftPredicate = RemoveRedundantNullChecks(sqlBinaryExpression.Left, outerColumnExpressions);
                        var rightPredicate = RemoveRedundantNullChecks(sqlBinaryExpression.Right, outerColumnExpressions);

                        return CombineNonNullExpressions(leftPredicate, rightPredicate);
                    }
                }

                return predicate;
            }
        }
    }

    /// <summary>
    ///     Adds the given <see cref="SelectExpression" /> to table sources using INNER JOIN.
    /// </summary>
    /// <param name="innerSelectExpression">A <see cref="SelectExpression" /> to join with.</param>
    /// <param name="joinPredicate">A predicate to use for the join.</param>
    public void AddInnerJoin(SelectExpression innerSelectExpression, SqlExpression joinPredicate)
        => AddJoin(JoinType.InnerJoin, ref innerSelectExpression, out _, joinPredicate);

    /// <summary>
    ///     Adds the given <see cref="SelectExpression" /> to table sources using LEFT JOIN.
    /// </summary>
    /// <param name="innerSelectExpression">A <see cref="SelectExpression" /> to join with.</param>
    /// <param name="joinPredicate">A predicate to use for the join.</param>
    public void AddLeftJoin(SelectExpression innerSelectExpression, SqlExpression joinPredicate)
        => AddJoin(JoinType.LeftJoin, ref innerSelectExpression, out _, joinPredicate);

    /// <summary>
    ///     Adds the given <see cref="SelectExpression" /> to table sources using CROSS JOIN.
    /// </summary>
    /// <param name="innerSelectExpression">A <see cref="SelectExpression" /> to join with.</param>
    public void AddCrossJoin(SelectExpression innerSelectExpression)
        => AddJoin(JoinType.CrossJoin, ref innerSelectExpression, out _);

    /// <summary>
    ///     Adds the given <see cref="SelectExpression" /> to table sources using CROSS APPLY.
    /// </summary>
    /// <param name="innerSelectExpression">A <see cref="SelectExpression" /> to join with.</param>
    public void AddCrossApply(SelectExpression innerSelectExpression)
        => AddJoin(JoinType.CrossApply, ref innerSelectExpression, out _);

    /// <summary>
    ///     Adds the given <see cref="SelectExpression" /> to table sources using OUTER APPLY.
    /// </summary>
    /// <param name="innerSelectExpression">A <see cref="SelectExpression" /> to join with.</param>
    public void AddOuterApply(SelectExpression innerSelectExpression)
        => AddJoin(JoinType.OuterApply, ref innerSelectExpression, out _);

    /// <summary>
    ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using INNER JOIN and combine shapers.
    /// </summary>
    /// <param name="innerSource">A <see cref="ShapedQueryExpression" /> to join with.</param>
    /// <param name="joinPredicate">A predicate to use for the join.</param>
    /// <param name="outerShaper">An expression for outer shaper.</param>
    /// <returns>An expression which shapes the result of this join.</returns>
    public Expression AddInnerJoin(
        ShapedQueryExpression innerSource,
        SqlExpression joinPredicate,
        Expression outerShaper)
        => AddJoin(
            JoinType.InnerJoin, (SelectExpression)innerSource.QueryExpression, outerShaper, innerSource.ShaperExpression,
            joinPredicate);

    /// <summary>
    ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using LEFT JOIN and combine shapers.
    /// </summary>
    /// <param name="innerSource">A <see cref="ShapedQueryExpression" /> to join with.</param>
    /// <param name="joinPredicate">A predicate to use for the join.</param>
    /// <param name="outerShaper">An expression for outer shaper.</param>
    /// <returns>An expression which shapes the result of this join.</returns>
    public Expression AddLeftJoin(
        ShapedQueryExpression innerSource,
        SqlExpression joinPredicate,
        Expression outerShaper)
        => AddJoin(
            JoinType.LeftJoin, (SelectExpression)innerSource.QueryExpression, outerShaper, innerSource.ShaperExpression, joinPredicate);

    /// <summary>
    ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using CROSS JOIN and combine shapers.
    /// </summary>
    /// <param name="innerSource">A <see cref="ShapedQueryExpression" /> to join with.</param>
    /// <param name="outerShaper">An expression for outer shaper.</param>
    /// <returns>An expression which shapes the result of this join.</returns>
    public Expression AddCrossJoin(
        ShapedQueryExpression innerSource,
        Expression outerShaper)
        => AddJoin(JoinType.CrossJoin, (SelectExpression)innerSource.QueryExpression, outerShaper, innerSource.ShaperExpression);

    /// <summary>
    ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using CROSS APPLY and combine shapers.
    /// </summary>
    /// <param name="innerSource">A <see cref="ShapedQueryExpression" /> to join with.</param>
    /// <param name="outerShaper">An expression for outer shaper.</param>
    /// <returns>An expression which shapes the result of this join.</returns>
    public Expression AddCrossApply(
        ShapedQueryExpression innerSource,
        Expression outerShaper)
        => AddJoin(JoinType.CrossApply, (SelectExpression)innerSource.QueryExpression, outerShaper, innerSource.ShaperExpression);

    /// <summary>
    ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using OUTER APPLY and combine shapers.
    /// </summary>
    /// <param name="innerSource">A <see cref="ShapedQueryExpression" /> to join with.</param>
    /// <param name="outerShaper">An expression for outer shaper.</param>
    /// <returns>An expression which shapes the result of this join.</returns>
    public Expression AddOuterApply(
        ShapedQueryExpression innerSource,
        Expression outerShaper)
        => AddJoin(JoinType.OuterApply, (SelectExpression)innerSource.QueryExpression, outerShaper, innerSource.ShaperExpression);

    /// <summary>
    ///     Pushes down the <see cref="SelectExpression" /> into a subquery.
    /// </summary>
    public void PushdownIntoSubquery()
        => PushdownIntoSubqueryInternal();

    private SqlRemappingVisitor PushdownIntoSubqueryInternal()
    {
        var subqueryAlias = GenerateUniqueAlias(_usedAliases, "t");
        var subquery = new SelectExpression(
            subqueryAlias, new List<ProjectionExpression>(), _tables.ToList(), _tableReferences.ToList(), _groupBy.ToList(),
            _orderings.ToList(), GetAnnotations())
        {
            IsDistinct = IsDistinct,
            Predicate = Predicate,
            Having = Having,
            Offset = Offset,
            Limit = Limit,
            _groupingParentSelectExpressionId = _groupingParentSelectExpressionId
        };
        subquery._usedAliases = _usedAliases;
        _tables.Clear();
        _tableReferences.Clear();
        _groupBy.Clear();
        _orderings.Clear();
        IsDistinct = false;
        Predicate = null;
        Having = null;
        Offset = null;
        Limit = null;
        subquery._tptLeftJoinTables.AddRange(_tptLeftJoinTables);
        _tptLeftJoinTables.Clear();

        var subqueryTableReferenceExpression = new TableReferenceExpression(this, subquery.Alias!);
        // Do NOT use AddTable here. The subquery already have unique aliases we don't need to traverse it again to make it unique.
        _tables.Add(subquery);
        _tableReferences.Add(subqueryTableReferenceExpression);

        // Remap tableReferences in inner so that all components follow referential integrity.
        foreach (var tableReference in subquery._tableReferences)
        {
            tableReference.UpdateTableReference(this, subquery);
        }

        var projectionMap = new Dictionary<SqlExpression, ColumnExpression>(ReferenceEqualityComparer.Instance);

        if (_projection.Count > 0)
        {
            var projections = _projection.ToList();
            _projection.Clear();
            foreach (var projection in projections)
            {
                // Since these projections are already added, they have unique table alias already.
                // The only new alias added was for "t" which we already made unique at the start of the method.
                var outerColumn = subquery.GenerateOuterColumn(
                    subqueryTableReferenceExpression, projection.Expression, projection.Alias, assignUniqueTableAlias: false);
                AddToProjection(outerColumn, null);
                projectionMap[projection.Expression] = outerColumn;
            }
        }

        var nestedQueryInProjection = false;
        // Projection would be present for client eval case
        if (_clientProjections.Count > 0)
        {
            for (var i = 0; i < _clientProjections.Count; i++)
            {
                var item = _clientProjections[i];
                // If item's value is ConstantExpression then projection has already been applied
                if (item is ConstantExpression)
                {
                    break;
                }

                if (item is EntityProjectionExpression entityProjection)
                {
                    _clientProjections[i] = LiftEntityProjectionFromSubquery(entityProjection);
                }
                else if (item is SqlExpression sqlExpression)
                {
                    var alias = _aliasForClientProjections[i];
                    var outerColumn = subquery.GenerateOuterColumn(subqueryTableReferenceExpression, sqlExpression, alias);
                    projectionMap[sqlExpression] = outerColumn;
                    _clientProjections[i] = outerColumn;
                    _aliasForClientProjections[i] = null;
                }
                else
                {
                    nestedQueryInProjection = true;
                }
            }
        }
        else
        {
            foreach (var (projectionMember, expression) in _projectionMapping.ToList())
            {
                // If projectionMapping's value is ConstantExpression then projection has already been applied
                if (expression is ConstantExpression)
                {
                    break;
                }

                if (expression is EntityProjectionExpression entityProjection)
                {
                    _projectionMapping[projectionMember] = LiftEntityProjectionFromSubquery(entityProjection);
                }
                else
                {
                    var innerColumn = (SqlExpression)expression;
                    var outerColumn = subquery.GenerateOuterColumn(
                        subqueryTableReferenceExpression, innerColumn, projectionMember.Last?.Name);
                    projectionMap[innerColumn] = outerColumn;
                    _projectionMapping[projectionMember] = outerColumn;
                }
            }
        }

        if (subquery._groupBy.Count > 0)
        {
            foreach (var key in subquery._groupBy)
            {
                projectionMap[key] = subquery.GenerateOuterColumn(subqueryTableReferenceExpression, key);
            }
        }

        var identifiers = _identifier.ToList();
        _identifier.Clear();
        foreach (var (column, comparer) in identifiers)
        {
            // Invariant, identifier should not contain term which cannot be projected out.
            if (!projectionMap.TryGetValue(column, out var outerColumn))
            {
                outerColumn = subquery.GenerateOuterColumn(subqueryTableReferenceExpression, column);
            }

            _identifier.Add((outerColumn, Comparer: comparer));
        }

        var childIdentifiers = _childIdentifiers.ToList();
        _childIdentifiers.Clear();
        foreach (var (column, comparer) in childIdentifiers)
        {
            // Invariant, identifier should not contain term which cannot be projected out.
            if (!projectionMap.TryGetValue(column, out var outerColumn))
            {
                outerColumn = subquery.GenerateOuterColumn(subqueryTableReferenceExpression, column);
            }

            _childIdentifiers.Add((outerColumn, Comparer: comparer));
        }

        foreach (var ordering in subquery._orderings)
        {
            var orderingExpression = ordering.Expression;
            if (projectionMap.TryGetValue(orderingExpression, out var outerColumn))
            {
                _orderings.Add(ordering.Update(outerColumn));
            }
            else if (!IsDistinct
                     && GroupBy.Count == 0
                     || GroupBy.Contains(orderingExpression))
            {
                _orderings.Add(
                    ordering.Update(
                        subquery.GenerateOuterColumn(
                            subqueryTableReferenceExpression, orderingExpression, assignUniqueTableAlias: false)));
            }
            else
            {
                _orderings.Clear();
                break;
            }
        }

        if (subquery.Offset == null
            && subquery.Limit == null)
        {
            subquery.ClearOrdering();
        }

        // Remap tableReferences in inner
        foreach (var tableReference in subquery._tableReferences)
        {
            tableReference.UpdateTableReference(this, subquery);
        }

        var tableReferenceUpdatingExpressionVisitor = new TableReferenceUpdatingExpressionVisitor(this, subquery);
        var sqlRemappingVisitor = new SqlRemappingVisitor(projectionMap, subquery, subqueryTableReferenceExpression);
        tableReferenceUpdatingExpressionVisitor.Visit(subquery);

        if (nestedQueryInProjection)
        {
            for (var i = 0; i < _clientProjections.Count; i++)
            {
                if (_clientProjections[i] is ShapedQueryExpression shapedQueryExpression)
                {
                    _clientProjections[i] = shapedQueryExpression.Update(
                        sqlRemappingVisitor.Visit(shapedQueryExpression.QueryExpression), shapedQueryExpression.ShaperExpression);
                }
            }
        }

        return sqlRemappingVisitor;

        EntityProjectionExpression LiftEntityProjectionFromSubquery(EntityProjectionExpression entityProjection)
        {
            var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
            foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
            {
                var innerColumn = entityProjection.BindProperty(property);
                var outerColumn = subquery.GenerateOuterColumn(subqueryTableReferenceExpression, innerColumn);
                projectionMap[innerColumn] = outerColumn;
                propertyExpressions[property] = outerColumn;
            }

            ColumnExpression? discriminatorExpression = null;
            if (entityProjection.DiscriminatorExpression != null)
            {
                discriminatorExpression = subquery.GenerateOuterColumn(
                    subqueryTableReferenceExpression, entityProjection.DiscriminatorExpression, DiscriminatorColumnAlias);
                projectionMap[entityProjection.DiscriminatorExpression] = discriminatorExpression;
            }

            var newEntityProjection = new EntityProjectionExpression(
                entityProjection.EntityType, propertyExpressions, discriminatorExpression);

            // Also lift nested entity projections
            foreach (var navigation in entityProjection.EntityType
                         .GetAllBaseTypes().Concat(entityProjection.EntityType.GetDerivedTypesInclusive())
                         .SelectMany(t => t.GetDeclaredNavigations()))
            {
                var boundEntityShaperExpression = entityProjection.BindNavigation(navigation);
                if (boundEntityShaperExpression != null)
                {
                    var innerEntityProjection = (EntityProjectionExpression)boundEntityShaperExpression.ValueBufferExpression;
                    var newInnerEntityProjection = LiftEntityProjectionFromSubquery(innerEntityProjection);
                    boundEntityShaperExpression = boundEntityShaperExpression.Update(newInnerEntityProjection);
                    newEntityProjection.AddNavigationBinding(navigation, boundEntityShaperExpression);
                }
            }

            return newEntityProjection;
        }
    }

    /// <summary>
    ///     Checks whether this <see cref="SelectExpression" /> represents a <see cref="FromSqlExpression" /> which is not composed upon.
    /// </summary>
    /// <returns>A bool value indicating a non-composed <see cref="FromSqlExpression" />.</returns>
    public bool IsNonComposedFromSql()
        => Limit == null
            && Offset == null
            && !IsDistinct
            && Predicate == null
            && GroupBy.Count == 0
            && Having == null
            && Orderings.Count == 0
            && Tables.Count == 1
            && Tables[0] is FromSqlExpression fromSql
            && Projection.All(
                pe => pe.Expression is ColumnExpression column
                    && string.Equals(fromSql.Alias, column.TableAlias, StringComparison.OrdinalIgnoreCase))
            && _projectionMapping.TryGetValue(new ProjectionMember(), out var mapping)
            && mapping.Type == typeof(Dictionary<IProperty, int>);

    /// <summary>
    ///     Prepares the <see cref="SelectExpression" /> to apply aggregate operation over it.
    /// </summary>
    public void PrepareForAggregate()
    {
        if (IsDistinct
            || Limit != null
            || Offset != null)
        {
            PushdownIntoSubquery();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public SelectExpression Clone()
    {
        _cloningExpressionVisitor ??= new CloningExpressionVisitor();

        return (SelectExpression)_cloningExpressionVisitor.Visit(this);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public SelectExpression Prune()
        => Prune(referencedColumns: null);

    private SelectExpression Prune(IReadOnlyCollection<string>? referencedColumns = null)
    {
        if (referencedColumns != null
            && !IsDistinct)
        {
            for (var i = _projection.Count - 1; i >= 0; i--)
            {
                if (!referencedColumns.Contains(_projection[i].Alias))
                {
                    _projection.RemoveAt(i);
                }
            }
        }

        _identifier.Clear();
        _childIdentifiers.Clear();
        var columnExpressionFindingExpressionVisitor = new ColumnExpressionFindingExpressionVisitor();
        var columnsMap = columnExpressionFindingExpressionVisitor.FindColumns(this);
        var removedTableCount = 0;
        for (var i = 0; i < _tables.Count; i++)
        {
            var table = _tables[i];
            var tableAlias = GetAliasFromTableExpressionBase(table);
            if (columnsMap[tableAlias] == null
                && (table is LeftJoinExpression
                    || table is OuterApplyExpression)
                && _tptLeftJoinTables?.Contains(i + removedTableCount) == true)
            {
                _tables.RemoveAt(i);
                _tableReferences.RemoveAt(i);
                removedTableCount++;
                i--;

                continue;
            }

            if (UnwrapJoinExpression(table) is SelectExpression innerSelectExpression)
            {
                innerSelectExpression.Prune(columnsMap[tableAlias]);
            }
        }

        return this;
    }

    private Dictionary<ProjectionMember, int> ConvertProjectionMappingToClientProjections(
        Dictionary<ProjectionMember, Expression> projectionMapping,
        bool makeNullable = false)
    {
        var mapping = new Dictionary<ProjectionMember, int>();
        var entityProjectionCache = new Dictionary<EntityProjectionExpression, int>(ReferenceEqualityComparer.Instance);
        foreach (var projection in projectionMapping)
        {
            var projectionMember = projection.Key;
            var projectionToAdd = projection.Value;

            if (projectionToAdd is EntityProjectionExpression entityProjection)
            {
                if (!entityProjectionCache.TryGetValue(entityProjection, out var value))
                {
                    var entityProjectionToCache = entityProjection;
                    if (makeNullable)
                    {
                        entityProjection = entityProjection.MakeNullable();
                    }

                    _clientProjections.Add(entityProjection);
                    _aliasForClientProjections.Add(null);
                    value = _clientProjections.Count - 1;
                    entityProjectionCache[entityProjectionToCache] = value;
                }

                mapping[projectionMember] = value;
            }
            else
            {
                projectionToAdd = MakeNullable(projectionToAdd, makeNullable);
                var existingIndex = _clientProjections.FindIndex(e => e.Equals(projectionToAdd));
                if (existingIndex == -1)
                {
                    _clientProjections.Add(projectionToAdd);
                    _aliasForClientProjections.Add(projectionMember.Last?.Name);
                    existingIndex = _clientProjections.Count - 1;
                }

                mapping[projectionMember] = existingIndex;
            }
        }

        projectionMapping.Clear();

        return mapping;
    }

    private static SqlExpression MakeNullable(SqlExpression expression, bool nullable)
        => nullable && expression is ColumnExpression column ? column.MakeNullable() : expression;

    private static Expression MakeNullable(Expression expression, bool nullable)
    {
        if (nullable)
        {
            if (expression is EntityProjectionExpression entityProjection)
            {
                return entityProjection.MakeNullable();
            }

            if (expression is ColumnExpression column)
            {
                return column.MakeNullable();
            }
        }

        return expression;
    }

    private static string GetAliasFromTableExpressionBase(TableExpressionBase tableExpressionBase)
        // We unwrap here since alias are not assigned to wrapper expressions
        => UnwrapJoinExpression(tableExpressionBase).Alias!;

    private static TableExpressionBase UnwrapJoinExpression(TableExpressionBase tableExpressionBase)
        => (tableExpressionBase as JoinExpressionBase)?.Table ?? tableExpressionBase;

    private static IEnumerable<IProperty> GetAllPropertiesInHierarchy(IEntityType entityType)
        => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
            .SelectMany(t => t.GetDeclaredProperties());

    private static ConcreteColumnExpression CreateColumnExpression(
        IProperty property,
        ITableBase table,
        TableReferenceExpression tableExpression,
        bool nullable)
        => new(property, table.FindColumn(property)!, tableExpression, nullable);

    private ConcreteColumnExpression GenerateOuterColumn(
        TableReferenceExpression tableReferenceExpression,
        SqlExpression projection,
        string? alias = null,
        bool assignUniqueTableAlias = true)
    {
        // TODO: Add check if we can add projection in subquery to generate out column
        // Subquery having Distinct or GroupBy can block it.
        var index = AddToProjection(projection, alias, assignUniqueTableAlias);

        return new ConcreteColumnExpression(_projection[index], tableReferenceExpression);
    }

    private bool ContainsTableReference(ColumnExpression column)
        // This method is used when evaluating join correlations.
        // At that point aliases are not uniquified across so we need to match tables
        => Tables.Any(e => ReferenceEquals(e, column.Table));

    private SqlExpression TryLiftGroupByAggregate(SqlExpression sqlExpression)
        => _groupBy.Count > 0
            ? (SqlExpression)new GroupByAggregateLiftingExpressionVisitor(this).Visit(sqlExpression)
            : sqlExpression;

    private void AddTable(TableExpressionBase tableExpressionBase, TableReferenceExpression tableReferenceExpression)
    {
        Check.DebugAssert(_tables.Count == _tableReferences.Count, "All the tables should have their associated TableReferences.");
        Check.DebugAssert(
            string.Equals(
                GetAliasFromTableExpressionBase(tableExpressionBase), tableReferenceExpression.Alias, StringComparison.Ordinal),
            "Alias of table and table reference should be the same.");

        var uniqueAlias = GenerateUniqueAlias(_usedAliases, tableReferenceExpression.Alias);
        // We unwrap here since alias are not assigned to wrapper expressions
        UnwrapJoinExpression(tableExpressionBase).Alias = uniqueAlias;
        tableReferenceExpression.Alias = uniqueAlias;

        tableExpressionBase = (TableExpressionBase)new AliasUniquifier(_usedAliases).Visit(tableExpressionBase);
        _tables.Add(tableExpressionBase);
        _tableReferences.Add(tableReferenceExpression);
    }

    private SqlExpression AssignUniqueAliases(SqlExpression expression)
        => (SqlExpression)new AliasUniquifier(_usedAliases).Visit(expression);

    private static string GenerateUniqueAlias(HashSet<string> usedAliases, string currentAlias)
    {
        var counter = 0;
        var baseAlias = currentAlias[..1];

        while (usedAliases.Contains(currentAlias))
        {
            currentAlias = baseAlias + counter++;
        }

        usedAliases.Add(currentAlias);
        return currentAlias;
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        if (_projection.Count == 0)
        {
            // If projection is not populated then we need to treat this as mutable object since it is not final yet.
            if (_clientProjections.Count > 0)
            {
                VisitList(_clientProjections, inPlace: true, out _);
            }
            else
            {
                var projectionMapping = new Dictionary<ProjectionMember, Expression>();
                foreach (var (projectionMember, expression) in _projectionMapping)
                {
                    var newProjection = visitor.Visit(expression);

                    projectionMapping[projectionMember] = newProjection;
                }

                _projectionMapping = projectionMapping;
            }

            var newTables = VisitList(_tables, inPlace: true, out var tablesChanged);
            Check.DebugAssert(
                !tablesChanged
                || newTables.Select(e => GetAliasFromTableExpressionBase(e)).SequenceEqual(_tableReferences.Select(e => e.Alias)),
                "Alias of updated tables must match the old tables.");
            Predicate = (SqlExpression?)visitor.Visit(Predicate);

            var newGroupBy = _groupBy;
            for (var i = 0; i < _groupBy.Count; i++)
            {
                var groupingKey = _groupBy[i];
                var newGroupingKey = (SqlExpression)visitor.Visit(groupingKey);
                if (newGroupingKey != groupingKey
                    || newGroupingKey is SqlConstantExpression
                    || newGroupingKey is SqlParameterExpression)
                {
                    if (newGroupBy == _groupBy)
                    {
                        newGroupBy = new List<SqlExpression>(_groupBy.Count);
                        for (var j = 0; j < i; j++)
                        {
                            newGroupBy.Add(_groupBy[j]);
                        }
                    }
                }

                if (newGroupBy != _groupBy
                    && !(newGroupingKey is SqlConstantExpression
                        || newGroupingKey is SqlParameterExpression))
                {
                    newGroupBy.Add(newGroupingKey);
                }
            }

            if (newGroupBy != _groupBy)
            {
                _groupBy.Clear();
                _groupBy.AddRange(newGroupBy);
            }

            Having = (SqlExpression?)visitor.Visit(Having);

            VisitList(_orderings, inPlace: true, out _);

            Offset = (SqlExpression?)visitor.Visit(Offset);
            Limit = (SqlExpression?)visitor.Visit(Limit);
            _groupingCorrelationPredicate = (SqlExpression?)visitor.Visit(_groupingCorrelationPredicate);

            var identifier = VisitList(_identifier.Select(e => e.Column).ToList(), inPlace: true, out _)
                .Zip(_identifier, (a, b) => (a, b.Comparer))
                .ToList();
            _identifier.Clear();
            _identifier.AddRange(identifier);

            var childIdentifier = VisitList(_childIdentifiers.Select(e => e.Column).ToList(), inPlace: true, out _)
                .Zip(_childIdentifiers, (a, b) => (a, b.Comparer))
                .ToList();
            _childIdentifiers.Clear();
            _childIdentifiers.AddRange(childIdentifier);

            return this;
        }
        else
        {
            // If projection is populated then
            // Either this SelectExpression is not bound to a shaped query expression
            // Or it is post-translation phase where it will update the shaped query expression
            // So we will treat it as immutable
            var newProjections = VisitList(_projection, inPlace: false, out var changed);

            // We don't need to visit _clientProjection/_projectionMapping here
            // because once projection is populated both of them contains expressions for client binding rather than a server query.

            var newTables = VisitList(_tables, inPlace: false, out var tablesChanged);
            changed |= tablesChanged;

            Check.DebugAssert(
                !tablesChanged
                || newTables.Select(e => GetAliasFromTableExpressionBase(e)).SequenceEqual(_tableReferences.Select(e => e.Alias)),
                "Alias of updated tables must match the old tables.");

            var predicate = (SqlExpression?)visitor.Visit(Predicate);
            changed |= predicate != Predicate;

            var newGroupBy = _groupBy;
            for (var i = 0; i < _groupBy.Count; i++)
            {
                var groupingKey = _groupBy[i];
                var newGroupingKey = (SqlExpression)visitor.Visit(groupingKey);
                if (newGroupingKey != groupingKey
                    || newGroupingKey is SqlConstantExpression
                    || newGroupingKey is SqlParameterExpression)
                {
                    if (newGroupBy == _groupBy)
                    {
                        newGroupBy = new List<SqlExpression>(_groupBy.Count);
                        for (var j = 0; j < i; j++)
                        {
                            newGroupBy.Add(_groupBy[j]);
                        }
                    }

                    changed = true;
                }

                if (newGroupBy != _groupBy
                    && !(newGroupingKey is SqlConstantExpression
                        || newGroupingKey is SqlParameterExpression))
                {
                    newGroupBy.Add(newGroupingKey);
                }
            }

            var havingExpression = (SqlExpression?)visitor.Visit(Having);
            changed |= havingExpression != Having;

            var newOrderings = VisitList(_orderings, inPlace: false, out var orderingChanged);
            changed |= orderingChanged;

            var offset = (SqlExpression?)visitor.Visit(Offset);
            changed |= offset != Offset;

            var limit = (SqlExpression?)visitor.Visit(Limit);
            changed |= limit != Limit;

            var groupingCorrelationPredicate = (SqlExpression?)visitor.Visit(_groupingCorrelationPredicate);
            changed |= groupingCorrelationPredicate != _groupingCorrelationPredicate;

            var identifier = VisitList(_identifier.Select(e => e.Column).ToList(), inPlace: false, out var identifierChanged);
            changed |= identifierChanged;

            var childIdentifier = VisitList(
                _childIdentifiers.Select(e => e.Column).ToList(), inPlace: false, out var childIdentifierChanged);
            changed |= childIdentifierChanged;

            if (changed)
            {
                var newTableReferences = _tableReferences.ToList();
                var newSelectExpression = new SelectExpression(
                    Alias, newProjections, newTables, newTableReferences, newGroupBy, newOrderings, GetAnnotations())
                {
                    _clientProjections = _clientProjections,
                    _projectionMapping = _projectionMapping,
                    Predicate = predicate,
                    Having = havingExpression,
                    Offset = offset,
                    Limit = limit,
                    IsDistinct = IsDistinct,
                    Tags = Tags,
                    _usedAliases = _usedAliases,
                    _groupingCorrelationPredicate = groupingCorrelationPredicate,
                    _groupingParentSelectExpressionId = _groupingParentSelectExpressionId
                };

                newSelectExpression._tptLeftJoinTables.AddRange(_tptLeftJoinTables);

                newSelectExpression._identifier.AddRange(identifier.Zip(_identifier).Select(e => (e.First, e.Second.Comparer)));
                newSelectExpression._childIdentifiers.AddRange(
                    childIdentifier.Zip(_childIdentifiers).Select(e => (e.First, e.Second.Comparer)));

                // Remap tableReferences in new select expression
                foreach (var tableReference in newTableReferences)
                {
                    tableReference.UpdateTableReference(this, newSelectExpression);
                }

                var tableReferenceUpdatingExpressionVisitor = new TableReferenceUpdatingExpressionVisitor(this, newSelectExpression);
                tableReferenceUpdatingExpressionVisitor.Visit(newSelectExpression);

                return newSelectExpression;
            }

            return this;
        }

        List<T> VisitList<T>(List<T> list, bool inPlace, out bool changed)
            where T : Expression
        {
            changed = false;
            var newList = list;
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                var newItem = item is ShapedQueryExpression shapedQueryExpression
                    ? shapedQueryExpression.Update(
                        visitor.Visit(shapedQueryExpression.QueryExpression), shapedQueryExpression.ShaperExpression)
                    : visitor.Visit(item);
                if (newItem != item
                    && newList == list)
                {
                    newList = new List<T>(list.Count);
                    for (var j = 0; j < i; j++)
                    {
                        newList.Add(list[j]);
                    }

                    changed = true;
                }

                if (newList != list)
                {
                    newList.Add((T)newItem);
                }
            }

            if (inPlace
                && changed)
            {
                list.Clear();
                list.AddRange(newList);

                return list;
            }

            return newList;
        }
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="projections">The <see cref="Projection" /> property of the result.</param>
    /// <param name="tables">The <see cref="Tables" /> property of the result.</param>
    /// <param name="predicate">The <see cref="Predicate" /> property of the result.</param>
    /// <param name="groupBy">The <see cref="GroupBy" /> property of the result.</param>
    /// <param name="having">The <see cref="Having" /> property of the result.</param>
    /// <param name="orderings">The <see cref="Orderings" /> property of the result.</param>
    /// <param name="limit">The <see cref="Limit" /> property of the result.</param>
    /// <param name="offset">The <see cref="Offset" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    // This does not take internal states since when using this method SelectExpression should be finalized
    public SelectExpression Update(
        IReadOnlyList<ProjectionExpression> projections,
        IReadOnlyList<TableExpressionBase> tables,
        SqlExpression? predicate,
        IReadOnlyList<SqlExpression> groupBy,
        SqlExpression? having,
        IReadOnlyList<OrderingExpression> orderings,
        SqlExpression? limit,
        SqlExpression? offset)
    {
        var projectionMapping = new Dictionary<ProjectionMember, Expression>();
        foreach (var (projectionMember, expression) in _projectionMapping)
        {
            projectionMapping[projectionMember] = expression;
        }

        var newTableReferences = _tableReferences.ToList();
        var newSelectExpression = new SelectExpression(
            Alias, projections.ToList(), tables.ToList(), newTableReferences, groupBy.ToList(), orderings.ToList(), GetAnnotations())
        {
            _projectionMapping = projectionMapping,
            _clientProjections = _clientProjections.ToList(),
            Predicate = predicate,
            Having = having,
            Offset = offset,
            Limit = limit,
            IsDistinct = IsDistinct,
            Tags = Tags
        };

        // We don't copy identifiers because when we are doing reconstruction so projection is already applied.
        // Update method should not be used pre-projection application. There are other methods to change SelectExpression.

        // Remap tableReferences in new select expression
        foreach (var tableReference in newTableReferences)
        {
            tableReference.UpdateTableReference(this, newSelectExpression);
        }

        var tableReferenceUpdatingExpressionVisitor = new TableReferenceUpdatingExpressionVisitor(this, newSelectExpression);
        tableReferenceUpdatingExpressionVisitor.Visit(newSelectExpression);

        return newSelectExpression;
    }

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        if (_clientProjections.Count > 0)
        {
            expressionPrinter.AppendLine("Client Projections:");
            using (expressionPrinter.Indent())
            {
                for (var i = 0; i < _clientProjections.Count; i++)
                {
                    expressionPrinter.AppendLine();
                    expressionPrinter.Append(i.ToString()).Append(" -> ");
                    expressionPrinter.Visit(_clientProjections[i]);
                }
            }
        }
        else if (_projectionMapping.Count > 0)
        {
            expressionPrinter.AppendLine("Projection Mapping:");
            using (expressionPrinter.Indent())
            {
                foreach (var (projectionMember, expression) in _projectionMapping)
                {
                    expressionPrinter.AppendLine();
                    expressionPrinter.Append(projectionMember.ToString()).Append(" -> ");
                    expressionPrinter.Visit(expression);
                }
            }
        }

        expressionPrinter.AppendLine();

        foreach (var tag in Tags)
        {
            expressionPrinter.Append($"-- {tag}");
        }

        IDisposable? indent = null;

        if (Alias != null)
        {
            expressionPrinter.AppendLine("(");
            indent = expressionPrinter.Indent();
        }

        expressionPrinter.Append("SELECT ");

        if (IsDistinct)
        {
            expressionPrinter.Append("DISTINCT ");
        }

        if (Limit != null
            && Offset == null)
        {
            expressionPrinter.Append("TOP(");
            expressionPrinter.Visit(Limit);
            expressionPrinter.Append(") ");
        }

        if (Projection.Any())
        {
            expressionPrinter.VisitCollection(Projection);
        }
        else
        {
            expressionPrinter.Append("1");
        }

        if (Tables.Any())
        {
            expressionPrinter.AppendLine().Append("FROM ");

            expressionPrinter.VisitCollection(Tables, p => p.AppendLine());
        }

        if (Predicate != null)
        {
            expressionPrinter.AppendLine().Append("WHERE ");
            expressionPrinter.Visit(Predicate);
        }

        if (GroupBy.Any())
        {
            expressionPrinter.AppendLine().Append("GROUP BY ");
            expressionPrinter.VisitCollection(GroupBy);
        }

        if (Having != null)
        {
            expressionPrinter.AppendLine().Append("HAVING ");
            expressionPrinter.Visit(Having);
        }

        if (Orderings.Any())
        {
            expressionPrinter.AppendLine().Append("ORDER BY ");
            expressionPrinter.VisitCollection(Orderings);
        }
        else if (Offset != null)
        {
            expressionPrinter.AppendLine().Append("ORDER BY (SELECT 1)");
        }

        if (Offset != null)
        {
            expressionPrinter.AppendLine().Append("OFFSET ");
            expressionPrinter.Visit(Offset);
            expressionPrinter.Append(" ROWS");

            if (Limit != null)
            {
                expressionPrinter.Append(" FETCH NEXT ");
                expressionPrinter.Visit(Limit);
                expressionPrinter.Append(" ROWS ONLY");
            }
        }

        PrintAnnotations(expressionPrinter);

        if (Alias != null)
        {
            indent?.Dispose();
            expressionPrinter.AppendLine().Append(") AS " + Alias);
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SelectExpression selectExpression
                && Equals(selectExpression));

    private bool Equals(SelectExpression selectExpression)
        /*
         * This is intentionally reference equals.
         * SelectExpression can appear at 2 levels,
         * 1. Top most level which is always same reference when translation phase, post-translation it can change in
         * ShapedQueryExpression where it would cause reconstruction. Reconstruction is cheaper than computing whole Equals
         * 2. Nested level component inside top level SelectExpression where it could change the reference and reconstruct SQL tree.
         * Since we assign unique aliases to components, 2 different SelectExpression would never match. And only positive case could
         * happen when it is reference equal.
         * If inner changed with in-place mutation then reference would be same, if inner changed with no mutation then it will cause
         * reconstruction causing different reference.
         */
        => ReferenceEquals(this, selectExpression);

    /// <inheritdoc />
    public override int GetHashCode()
        // Since equality above is reference equality, hash code can also be based on reference.
        => RuntimeHelpers.GetHashCode(this);
}
