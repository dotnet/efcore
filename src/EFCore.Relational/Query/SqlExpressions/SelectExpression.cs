// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    /// <summary>
    ///     <para>
    ///         An expression that represents a SELECT in a SQL tree.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         This class is not publicly constructable. If this is a problem for your application or provider, then please file
    ///         an issue at https://github.com/dotnet/efcore.
    ///     </para>
    /// </summary>
    // Class is sealed because there are no public/protected constructors. Can be unsealed if this is changed.
    public sealed partial class SelectExpression : TableExpressionBase
    {
        private static readonly string _discriminatorColumnAlias = "Discriminator";
        private static readonly IdentifierComparer _identifierComparer = new();
        private static readonly Dictionary<ExpressionType, ExpressionType> _mirroredOperationMap =
            new()
            {
                { ExpressionType.Equal, ExpressionType.Equal },
                { ExpressionType.NotEqual, ExpressionType.NotEqual },
                { ExpressionType.LessThan, ExpressionType.GreaterThan },
                { ExpressionType.LessThanOrEqual, ExpressionType.GreaterThanOrEqual },
                { ExpressionType.GreaterThan, ExpressionType.LessThan },
                { ExpressionType.GreaterThanOrEqual, ExpressionType.LessThanOrEqual },
            };

        private readonly List<ProjectionExpression> _projection = new();
        private readonly List<TableExpressionBase> _tables = new();
        private readonly List<TableReferenceExpression> _tableReferences = new();
        private readonly List<SqlExpression> _groupBy = new();
        private readonly List<OrderingExpression> _orderings = new();
        private readonly HashSet<string> _usedAliases = new();

        private readonly List<(ColumnExpression Column, ValueComparer Comparer)> _identifier = new();
        private readonly List<(ColumnExpression Column, ValueComparer Comparer)> _childIdentifiers = new();
        private readonly List<SelectExpression?> _pendingCollections = new();

        private readonly List<int> _tptLeftJoinTables = new();
        private Dictionary<ProjectionMember, Expression> _projectionMapping = new();

        private SelectExpression(
            string? alias,
            List<ProjectionExpression> projections,
            List<TableExpressionBase> tables,
            List<TableReferenceExpression> tableReferences,
            List<SqlExpression> groupBy,
            List<OrderingExpression> orderings)
            : base(alias)
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
                        var innerColumns = keyProperties.Select(p => CreateColumnExpression(p, table, tableReferenceExpression, nullable: false));

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

                    var keyColumns = keyProperties.Select(p => CreateColumnExpression(p, table, tableReferenceExpression, nullable: true)).ToArray();

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
                _ => entityType.GetDefaultMappings().Single().Table,
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
        /// <param name="tags"> A list of tags to apply. </param>
        public void ApplyTags(ISet<string> tags)
        {
            Check.NotNull(tags, nameof(tags));

            Tags = tags;
        }

        /// <summary>
        ///     Applies DISTINCT operator to the projections of the <see cref="SelectExpression" />.
        /// </summary>
        public void ApplyDistinct()
        {
            if (_pendingCollections.Count > 0)
            {
                throw new InvalidOperationException(RelationalStrings.DistinctOnCollectionNotSupported);
            }

            if (Limit != null
                || Offset != null)
            {
                PushdownIntoSubquery();
            }

            IsDistinct = true;

            if (_projection.Count > 0)
            {
                // _childIdentifiers are empty at this point since we are still in translation phase
                if (!_identifier.All(e => _projection.Any(p => e.Column.Equals(p.Expression))))
                {
                    _identifier.Clear();
                    // If identifier is not in the list then we add whole current projection as identifier if all column expressions
                    if (_projection.All(p => p.Expression is ColumnExpression))
                    {
                        _identifier.AddRange(_projection.Select(p => ((ColumnExpression)p.Expression, p.Expression.TypeMapping!.KeyComparer)));
                    }
                }
            }
            else
            {
                if (_identifier.Count > 0)
                {
                    var entityProjectionIdentifiers = new List<ColumnExpression>();
                    var entityProjectionValueComparers = new List<ValueComparer>();
                    var otherExpressions = new List<SqlExpression>();
                    foreach (var projectionMapping in _projectionMapping)
                    {
                        if (projectionMapping.Value is EntityProjectionExpression entityProjection)
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
                        else if (projectionMapping.Value is SqlExpression sqlExpression)
                        {
                            otherExpressions.Add(sqlExpression);
                        }
                    }
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
        public void ApplyProjection()
        {
            if (Projection.Any())
            {
                return;
            }

            var result = new Dictionary<ProjectionMember, Expression>();
            var mapping = ApplyProjectionMapping(_projectionMapping);
            foreach (var keyValuePair in mapping)
            {
                result[keyValuePair.Key] = Constant(mapping[keyValuePair.Key]);
            }

            _projectionMapping = result;
        }

        /// <summary>
        ///     Clears all existing projections.
        /// </summary>
        public void ClearProjection()
        {
            _projection.Clear();
        }

        /// <summary>
        ///     Replaces current projection mapping with a new one to change what is being projected out from this <see cref="SelectExpression" />.
        /// </summary>
        /// <param name="projectionMapping"> A new projection mapping. </param>
        public void ReplaceProjectionMapping(IDictionary<ProjectionMember, Expression> projectionMapping)
        {
            Check.NotNull(projectionMapping, nameof(projectionMapping));

            _projectionMapping.Clear();
            foreach (var kvp in projectionMapping)
            {
                _projectionMapping[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        ///     Gets the projection mapped to the given <see cref="ProjectionMember" />.
        /// </summary>
        /// <param name="projectionMember"> A projection member to search in the mapping. </param>
        /// <returns> The mapped projection for given projection member. </returns>
        public Expression GetMappedProjection(ProjectionMember projectionMember)
        {
            Check.NotNull(projectionMember, nameof(projectionMember));

            return _projectionMapping[projectionMember];
        }

        /// <summary>
        ///     Adds given <see cref="EntityProjectionExpression" /> to the projection.
        /// </summary>
        /// <param name="entityProjection"> An entity projection to add. </param>
        /// <returns> A dictionary of <see cref="IProperty" /> to int indicating properties and their corresponding indexes in the projection list. </returns>
        public IReadOnlyDictionary<IProperty, int> AddToProjection(EntityProjectionExpression entityProjection)
        {
            Check.NotNull(entityProjection, nameof(entityProjection));

            var dictionary = new Dictionary<IProperty, int>();
            foreach (var property in GetAllPropertiesInHierarchy(entityProjection.EntityType))
            {
                dictionary[property] = AddToProjection(entityProjection.BindProperty(property));
            }

            if (entityProjection.DiscriminatorExpression != null)
            {
                AddToProjection(entityProjection.DiscriminatorExpression, _discriminatorColumnAlias);
            }

            return dictionary;
        }

        /// <summary>
        ///     Adds given <see cref="SqlExpression" /> to the projection.
        /// </summary>
        /// <param name="sqlExpression"> An expression to add. </param>
        /// <returns> An int value indicating the index at which the expression was added in the projection list. </returns>
        public int AddToProjection(SqlExpression sqlExpression)
        {
            Check.NotNull(sqlExpression, nameof(sqlExpression));

            return AddToProjection(sqlExpression, null);
        }

        private int AddToProjection(SqlExpression sqlExpression, string? alias)
        {
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

            sqlExpression = AssignUniqueAliases(sqlExpression);
            _projection.Add(new ProjectionExpression(sqlExpression, baseAlias ?? ""));

            return _projection.Count - 1;
        }

        /// <summary>
        ///     Adds a non-scalar single result to the projection of the <see cref="SelectExpression" />.
        /// </summary>
        /// <param name="shapedQueryExpression"> A shaped query expression for the subquery producing single non-scalar result. </param>
        /// <returns> A shaper expression to shape the result of this projection. </returns>
        public Expression AddSingleProjection(ShapedQueryExpression shapedQueryExpression)
        {
            Check.NotNull(shapedQueryExpression, nameof(shapedQueryExpression));

            var innerSelectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
            var shaperExpression = shapedQueryExpression.ShaperExpression;
            var innerExpression = RemoveConvert(shaperExpression);
            if (!(innerExpression is EntityShaperExpression))
            {
                var sentinelExpression = innerSelectExpression.Limit!;
                var sentinelNullableType = sentinelExpression.Type.MakeNullable();
                ProjectionBindingExpression dummyProjection;
                if (innerSelectExpression.Projection.Any())
                {
                    var index = innerSelectExpression.AddToProjection(sentinelExpression);
                    dummyProjection = new ProjectionBindingExpression(innerSelectExpression, index, sentinelNullableType);
                }
                else
                {
                    innerSelectExpression._projectionMapping[new ProjectionMember()] = sentinelExpression;
                    dummyProjection = new ProjectionBindingExpression(innerSelectExpression, new ProjectionMember(), sentinelNullableType);
                }

                var defaultResult = shapedQueryExpression.ResultCardinality == ResultCardinality.SingleOrDefault
                    ? (Expression)Default(shaperExpression.Type)
                    : Block(
                        Throw(
                            New(
                                typeof(InvalidOperationException).GetConstructors()
                                    .Single(ci =>
                                    {
                                        var parameters = ci.GetParameters();
                                        return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
                                    }),
                                Constant(CoreStrings.SequenceContainsNoElements))),
                        Default(shaperExpression.Type));

                shaperExpression = Condition(
                    Equal(dummyProjection, Default(sentinelNullableType)),
                    defaultResult,
                    shaperExpression);
            }

            var remapper = new ProjectionBindingExpressionRemappingExpressionVisitor(this);
            // Pending collections from inner are lifted to outer when adding join
            // So we need to update offsets in shaper
            var pendingCollectionOffset = _pendingCollections.Count;
            AddJoin(JoinType.OuterApply, ref innerSelectExpression);
            var projectionCount = innerSelectExpression.Projection.Count;

            if (projectionCount > 0)
            {
                var indexMap = new int[projectionCount];
                for (var i = 0; i < projectionCount; i++)
                {
                    var projectionToAdd = innerSelectExpression.Projection[i].Expression;
                    projectionToAdd = MakeNullable(projectionToAdd, nullable: true);
                    indexMap[i] = AddToProjection(projectionToAdd);
                }

                shaperExpression = remapper.RemapIndex(shaperExpression, indexMap, pendingCollectionOffset);
            }
            else
            {
                var mapping = ApplyProjectionMapping(innerSelectExpression._projectionMapping, makeNullable: true);
                shaperExpression = remapper.RemapProjectionMember(shaperExpression, mapping, pendingCollectionOffset);
            }

            return new EntityShaperNullableMarkingExpressionVisitor().Visit(shaperExpression);

            static Expression RemoveConvert(Expression expression)
                => expression is UnaryExpression unaryExpression
                    && unaryExpression.NodeType == ExpressionType.Convert
                        ? RemoveConvert(unaryExpression.Operand)
                        : expression;
        }

        /// <summary>
        ///     Adds a collection to the projection of the <see cref="SelectExpression" />.
        /// </summary>
        /// <param name="shapedQueryExpression"> A shaped query expression for the subquery producing collection result. </param>
        /// <param name="navigation"> A navigation associated with this collection, if any. </param>
        /// <param name="elementType"> The type of the element in the collection. </param>
        /// <returns> A <see cref="CollectionShaperExpression" /> which represents shaping of this collection. </returns>
        public CollectionShaperExpression AddCollectionProjection(
            ShapedQueryExpression shapedQueryExpression, INavigationBase? navigation, Type elementType)
        {
            Check.NotNull(shapedQueryExpression, nameof(shapedQueryExpression));

            var innerSelectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
            _pendingCollections.Add(innerSelectExpression);

            return new CollectionShaperExpression(
                new ProjectionBindingExpression(this, _pendingCollections.Count - 1, typeof(object)),
                shapedQueryExpression.ShaperExpression,
                navigation,
                elementType);
        }

        /// <summary>
        ///     Applies previously added collection projection.
        /// </summary>
        /// <param name="collectionIndex"> An int value specifing which collection from pending collection to apply. </param>
        /// <param name="collectionId"> An int value of unique collection id associated with this collection projection. </param>
        /// <param name="innerShaper"> A shaper expression to use for shaping the elements of this collection. </param>
        /// <param name="navigation"> A navigation associated with this collection, if any. </param>
        /// <param name="elementType"> The type of the element in the collection. </param>
        /// <param name="splitQuery"> A value indicating whether the collection query would be run with a different DbCommand. </param>
        /// <returns> An expression which represents shaping of this collection. </returns>
        public Expression? ApplyCollectionJoin(
            int collectionIndex,
            int collectionId,
            Expression innerShaper,
            INavigationBase? navigation,
            Type elementType,
            bool splitQuery = false)
        {
            Check.NotNull(innerShaper, nameof(innerShaper));
            Check.NotNull(elementType, nameof(elementType));

            var innerSelectExpression = _pendingCollections[collectionIndex]!;
            _pendingCollections[collectionIndex] = null;

            if (_identifier.Count == 0
                || innerSelectExpression._identifier.Count == 0)
            {
                throw new InvalidOperationException(RelationalStrings.InsufficientInformationToIdentifyOuterElementOfCollectionJoin);
            }

            if (splitQuery)
            {
                var containsReferenceToOuter = new SelectExpressionCorrelationFindingExpressionVisitor(this)
                    .ContainsOuterReference(innerSelectExpression);
                if (containsReferenceToOuter)
                {
                    return null;
                }

                var identifierFromParent = _identifier;
                if (innerSelectExpression.Tables.LastOrDefault(e => e is InnerJoinExpression) is InnerJoinExpression
                        collectionInnerJoinExpression
                    && collectionInnerJoinExpression.Table is TableExpressionBase collectionTableExpressionBase)
                {
                    // This computes true parent identifier count for correlation.
                    // The last inner joined table in innerSelectExpression brings collection data.
                    // Further tables load additional data on the collection (hence uses outer pattern)
                    // So identifier not coming from there (which would be at the start only) are for correlation with parent.
                    // Parent can have additional identifier if a owned reference was expanded.
                    var actualParentIdentifierCount = innerSelectExpression._identifier
                        .TakeWhile(e => !ReferenceEquals(e.Column.Table, collectionTableExpressionBase))
                        .Count();
                    identifierFromParent = _identifier.Take(actualParentIdentifierCount).ToList();
                }

                var parentIdentifier = GetIdentifierAccessor(this, identifierFromParent).Item1;
                // We apply projection here because the outer level visitor does not visit this.
                innerSelectExpression.ApplyProjection();

                for (var i = 0; i < identifierFromParent.Count; i++)
                {
                    AppendOrdering(new OrderingExpression(identifierFromParent[i].Column, ascending: true));
                }

                // Copy over ordering from previous collections
                var innerOrderingExpressions = new List<OrderingExpression>();
                for (var i = 0; i < innerSelectExpression.Tables.Count; i++)
                {
                    var table = innerSelectExpression.Tables[i];
                    if (table is InnerJoinExpression collectionJoinExpression
                        && collectionJoinExpression.Table is SelectExpression collectionSelectExpression
                        && collectionSelectExpression.Predicate != null
                        && collectionSelectExpression.Tables.Count == 1
                        && collectionSelectExpression.Tables[0] is SelectExpression rowNumberSubquery
                        && rowNumberSubquery.Projection.Select(pe => pe.Expression)
                            .OfType<RowNumberExpression>().SingleOrDefault() is RowNumberExpression rowNumberExpression)
                    {
                        var collectionSelectExpressionTableReference = innerSelectExpression._tableReferences[i];
                        var rowNumberSubqueryTableReference = collectionSelectExpression._tableReferences.Single();
                        foreach (var partition in rowNumberExpression.Partitions)
                        {
                            innerOrderingExpressions.Add(
                                new OrderingExpression(
                                    collectionSelectExpression.GenerateOuterColumn(
                                        collectionSelectExpressionTableReference,
                                        rowNumberSubquery.GenerateOuterColumn(rowNumberSubqueryTableReference, partition)),
                                    ascending: true));
                        }

                        foreach (var ordering in rowNumberExpression.Orderings)
                        {
                            innerOrderingExpressions.Add(
                                new OrderingExpression(
                                    collectionSelectExpression.GenerateOuterColumn(
                                        collectionSelectExpressionTableReference,
                                        rowNumberSubquery.GenerateOuterColumn(rowNumberSubqueryTableReference, ordering.Expression)),
                                    ordering.IsAscending));
                        }
                    }

                    if (table is CrossApplyExpression collectionApplyExpression
                        && collectionApplyExpression.Table is SelectExpression collectionSelectExpression2
                        && collectionSelectExpression2.Orderings.Count > 0)
                    {
                        var collectionSelectExpressionTableReference = innerSelectExpression._tableReferences[i];
                        foreach (var ordering in collectionSelectExpression2.Orderings)
                        {
                            if (innerSelectExpression._identifier.Any(e => e.Column.Equals(ordering.Expression)))
                            {
                                continue;
                            }

                            innerOrderingExpressions.Add(
                                new OrderingExpression(
                                    collectionSelectExpression2.GenerateOuterColumn(collectionSelectExpressionTableReference, ordering.Expression),
                                    ordering.IsAscending));
                        }
                    }
                }

                var (childIdentifier, childIdentifierValueComparers) = GetIdentifierAccessor(
                    innerSelectExpression, innerSelectExpression._identifier.Take(identifierFromParent.Count));

                var identifierIndex = 0;
                var orderingIndex = 0;
                for (var i = 0; i < Orderings.Count; i++)
                {
                    var outerOrdering = Orderings[i];
                    if (identifierIndex < identifierFromParent.Count
                        && outerOrdering.Expression.Equals(identifierFromParent[identifierIndex].Column))
                    {
                        innerSelectExpression.AppendOrdering(
                            new OrderingExpression(
                                innerSelectExpression._identifier[identifierIndex].Column, ascending: true));
                        identifierIndex++;
                    }
                    else
                    {
                        if (i < innerSelectExpression.Orderings.Count)
                        {
                            continue;
                        }

                        innerSelectExpression.AppendOrdering(innerOrderingExpressions[orderingIndex]);
                        orderingIndex++;
                    }
                }

                foreach (var orderingExpression in innerOrderingExpressions.Skip(orderingIndex))
                {
                    innerSelectExpression.AppendOrdering(orderingExpression);
                }

                return new RelationalSplitCollectionShaperExpression(
                    collectionId, parentIdentifier, childIdentifier, childIdentifierValueComparers,
                    innerSelectExpression, innerShaper, navigation, elementType);
            }
            else
            {
                var parentIdentifierList = _identifier.Except(_childIdentifiers, _identifierComparer).ToList();

                var (parentIdentifier, parentIdentifierValueComparers) = GetIdentifierAccessor(this, parentIdentifierList);
                var (outerIdentifier, outerIdentifierValueComparers) = GetIdentifierAccessor(this, _identifier);

                if (collectionIndex == 0)
                {
                    foreach (var identifier in _identifier)
                    {
                        AppendOrdering(new OrderingExpression(identifier.Column, ascending: true));
                    }
                }

                AddJoin(JoinType.OuterApply, ref innerSelectExpression);
                var innerOrderingExpressions = new List<OrderingExpression>();
                var joinedTable = innerSelectExpression.Tables.Single();
                if (joinedTable is SelectExpression collectionSelectExpression
                    && collectionSelectExpression.Predicate != null
                    && collectionSelectExpression.Tables.Count == 1
                    && collectionSelectExpression.Tables[0] is SelectExpression rowNumberSubquery
                    && rowNumberSubquery.Projection.Select(pe => pe.Expression)
                        .OfType<RowNumberExpression>().SingleOrDefault() is RowNumberExpression rowNumberExpression)
                {
                    var collectionSelectExpressionTableReference = innerSelectExpression._tableReferences.Single();
                    var rowNumberSubqueryTableReference = collectionSelectExpression._tableReferences.Single();
                    foreach (var partition in rowNumberExpression.Partitions)
                    {
                        innerOrderingExpressions.Add(
                            new OrderingExpression(
                                collectionSelectExpression.GenerateOuterColumn(
                                    collectionSelectExpressionTableReference,
                                    rowNumberSubquery.GenerateOuterColumn(rowNumberSubqueryTableReference, partition)),
                                ascending: true));
                    }

                    foreach (var ordering in rowNumberExpression.Orderings)
                    {
                        innerOrderingExpressions.Add(
                            new OrderingExpression(
                                collectionSelectExpression.GenerateOuterColumn(
                                    collectionSelectExpressionTableReference,
                                    rowNumberSubquery.GenerateOuterColumn(rowNumberSubqueryTableReference, ordering.Expression)),
                                ordering.IsAscending));
                    }
                }
                else if (joinedTable is SelectExpression collectionSelectExpression2
                    && collectionSelectExpression2.Orderings.Count > 0)
                {
                    var collectionSelectExpressionTableReference = innerSelectExpression._tableReferences.Single();
                    foreach (var ordering in collectionSelectExpression2.Orderings)
                    {
                        if (innerSelectExpression._identifier.Any(e => e.Column.Equals(ordering.Expression)))
                        {
                            continue;
                        }

                        innerOrderingExpressions.Add(
                            new OrderingExpression(
                                collectionSelectExpression2.GenerateOuterColumn(collectionSelectExpressionTableReference, ordering.Expression),
                                ordering.IsAscending));
                    }
                }
                else
                {
                    innerOrderingExpressions.AddRange(innerSelectExpression.Orderings);
                }

                foreach (var ordering in innerOrderingExpressions)
                {
                    AppendOrdering(ordering.Update(MakeNullable(ordering.Expression, nullable: true)));
                }

                var remapper = new ProjectionBindingExpressionRemappingExpressionVisitor(this);
                // Outer projection are already populated
                if (innerSelectExpression.Projection.Count > 0)
                {
                    // Add inner to projection and update indexes
                    var indexMap = new int[innerSelectExpression.Projection.Count];
                    for (var i = 0; i < innerSelectExpression.Projection.Count; i++)
                    {
                        var projectionToAdd = innerSelectExpression.Projection[i].Expression;
                        projectionToAdd = MakeNullable(projectionToAdd, nullable: true);
                        indexMap[i] = AddToProjection(projectionToAdd);
                    }

                    innerShaper = remapper.RemapIndex(innerShaper, indexMap);
                }
                else
                {
                    // Apply inner projection mapping and convert projection member binding to indexes
                    var mapping = ApplyProjectionMapping(innerSelectExpression._projectionMapping, makeNullable: true);
                    innerShaper = remapper.RemapProjectionMember(innerShaper, mapping);
                }

                innerShaper = new EntityShaperNullableMarkingExpressionVisitor().Visit(innerShaper);

                var (selfIdentifier, selfIdentifierValueComparers) = GetIdentifierAccessor(
                    this,
                    innerSelectExpression._identifier
                        .Except(innerSelectExpression._childIdentifiers, _identifierComparer)
                        .Select(e => (e.Column.MakeNullable(), e.Comparer)));

                foreach (var identifier in innerSelectExpression._identifier)
                {
                    var updatedColumn = identifier.Column.MakeNullable();
                    _childIdentifiers.Add((updatedColumn, identifier.Comparer));
                    AppendOrdering(new OrderingExpression(updatedColumn, ascending: true));
                }

                var result = new RelationalCollectionShaperExpression(
                    collectionId, parentIdentifier, outerIdentifier, selfIdentifier,
                    parentIdentifierValueComparers, outerIdentifierValueComparers, selfIdentifierValueComparers,
                    innerShaper, navigation, elementType);

                return result;
            }

            static (Expression, IReadOnlyList<ValueComparer>) GetIdentifierAccessor(
                SelectExpression selectExpression,
                IEnumerable<(ColumnExpression Column, ValueComparer Comparer)> identifyingProjection)
            {
                var updatedExpressions = new List<Expression>();
                var comparers = new List<ValueComparer>();
                foreach (var keyExpression in identifyingProjection)
                {
                    var index = selectExpression.AddToProjection(keyExpression.Column);
                    var projectionBindingExpression = new ProjectionBindingExpression(
                        selectExpression, index, keyExpression.Column.Type.MakeNullable());

                    updatedExpressions.Add(
                        projectionBindingExpression.Type.IsValueType
                            ? Convert(projectionBindingExpression, typeof(object))
                            : (Expression)projectionBindingExpression);
                    comparers.Add(keyExpression.Comparer);
                }

                return (NewArrayInit(typeof(object), updatedExpressions), comparers);
            }
        }

        /// <summary>
        ///     Applies filter predicate to the <see cref="SelectExpression" />.
        /// </summary>
        /// <param name="expression"> An expression to use for filtering. </param>
        public void ApplyPredicate(SqlExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (expression is SqlConstantExpression sqlConstant
                && sqlConstant.Value is bool boolValue
                && boolValue)
            {
                return;
            }

            if (Limit != null
                || Offset != null)
            {
                expression = PushdownIntoSubqueryInternal().Remap(expression);
            }

            expression = AssignUniqueAliases(expression);

            if (_groupBy.Count > 0)
            {
                Having = Having == null
                    ? expression
                    : new SqlBinaryExpression(
                        ExpressionType.AndAlso,
                        Having,
                        expression,
                        typeof(bool),
                        expression.TypeMapping);
            }
            else
            {
                Predicate = Predicate == null
                    ? expression
                    : new SqlBinaryExpression(
                        ExpressionType.AndAlso,
                        Predicate,
                        expression,
                        typeof(bool),
                        expression.TypeMapping);
            }
        }

        /// <summary>
        ///     Applies grouping from given key selector.
        /// </summary>
        /// <param name="keySelector"> An key selector expression for the GROUP BY. </param>
        public void ApplyGrouping(Expression keySelector)
        {
            Check.NotNull(keySelector, nameof(keySelector));

            ClearOrdering();

            AppendGroupBy(keySelector);

            if (!_identifier.All(e => _groupBy.Contains(e.Column)))
            {
                _identifier.Clear();
                if (_groupBy.All(e => e is ColumnExpression))
                {
                    _identifier.AddRange(_groupBy.Select(e => ((ColumnExpression)e, e.TypeMapping!.KeyComparer)));
                }
            }
        }

        private void AppendGroupBy(Expression keySelector)
        {
            Check.NotNull(keySelector, nameof(keySelector));

            switch (keySelector)
            {
                case SqlExpression sqlExpression:
                    if (!(sqlExpression is SqlConstantExpression
                        || sqlExpression is SqlParameterExpression))
                    {
                        _groupBy.Add(sqlExpression);
                    }

                    break;

                case NewExpression newExpression:
                    foreach (var argument in newExpression.Arguments)
                    {
                        AppendGroupBy(argument);
                    }

                    break;

                case MemberInitExpression memberInitExpression:
                    AppendGroupBy(memberInitExpression.NewExpression);
                    foreach (var argument in memberInitExpression.Bindings)
                    {
                        AppendGroupBy(((MemberAssignment)argument).Expression);
                    }

                    break;

                case UnaryExpression unaryExpression
                    when unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked:
                    AppendGroupBy(unaryExpression.Operand);
                    break;

                default:
                    throw new InvalidOperationException(RelationalStrings.InvalidKeySelectorForGroupBy(keySelector, keySelector.GetType()));
            }
        }

        /// <summary>
        ///     Applies ordering to the <see cref="SelectExpression" />. This overwrites any previous ordering specified.
        /// </summary>
        /// <param name="orderingExpression"> An ordering expression to use for ordering. </param>
        public void ApplyOrdering(OrderingExpression orderingExpression)
        {
            Check.NotNull(orderingExpression, nameof(orderingExpression));

            if (IsDistinct
                || Limit != null
                || Offset != null)
            {
                orderingExpression = orderingExpression.Update(PushdownIntoSubqueryInternal().Remap(orderingExpression.Expression));
            }

            _orderings.Clear();
            _orderings.Add(orderingExpression.Update(AssignUniqueAliases(orderingExpression.Expression)));
        }

        /// <summary>
        ///     Appends ordering to the existing orderings of the <see cref="SelectExpression" />.
        /// </summary>
        /// <param name="orderingExpression"> An ordering expression to use for ordering. </param>
        public void AppendOrdering(OrderingExpression orderingExpression)
        {
            Check.NotNull(orderingExpression, nameof(orderingExpression));

            if (_orderings.FirstOrDefault(o => o.Expression.Equals(orderingExpression.Expression)) == null)
            {
                _orderings.Add(orderingExpression.Update(AssignUniqueAliases(orderingExpression.Expression)));
            }
        }

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
        {
            _orderings.Clear();
        }

        /// <summary>
        ///     Applies limit to the <see cref="SelectExpression" /> to limit the number of rows returned in the result set.
        /// </summary>
        /// <param name="sqlExpression"> An expression representing limit row count. </param>
        public void ApplyLimit(SqlExpression sqlExpression)
        {
            Check.NotNull(sqlExpression, nameof(sqlExpression));

            if (Limit != null)
            {
                PushdownIntoSubquery();
            }

            Limit = sqlExpression;
        }

        /// <summary>
        ///     Applies offset to the <see cref="SelectExpression" /> to skip the number of rows in the result set.
        /// </summary>
        /// <param name="sqlExpression"> An expression representing offset row count. </param>
        public void ApplyOffset(SqlExpression sqlExpression)
        {
            Check.NotNull(sqlExpression, nameof(sqlExpression));

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
        /// <param name="source2"> A <see cref="SelectExpression" /> to perform the operation. </param>
        /// <param name="distinct"> A bool value indicating if resulting table source should remove duplicates. </param>
        public void ApplyExcept(SelectExpression source2, bool distinct)
        {
            Check.NotNull(source2, nameof(source2));

            ApplySetOperation(SetOperationType.Except, source2, distinct);
        }

        /// <summary>
        ///     Applies INTERSECT operation to the <see cref="SelectExpression" />.
        /// </summary>
        /// <param name="source2"> A <see cref="SelectExpression" /> to perform the operation. </param>
        /// <param name="distinct"> A bool value indicating if resulting table source should remove duplicates. </param>
        public void ApplyIntersect(SelectExpression source2, bool distinct)
        {
            Check.NotNull(source2, nameof(source2));

            ApplySetOperation(SetOperationType.Intersect, source2, distinct);
        }

        /// <summary>
        ///     Applies UNION operation to the <see cref="SelectExpression" />.
        /// </summary>
        /// <param name="source2"> A <see cref="SelectExpression" /> to perform the operation. </param>
        /// <param name="distinct"> A bool value indicating if resulting table source should remove duplicates. </param>
        public void ApplyUnion(SelectExpression source2, bool distinct)
        {
            Check.NotNull(source2, nameof(source2));

            ApplySetOperation(SetOperationType.Union, source2, distinct);
        }

        private void ApplySetOperation(SetOperationType setOperationType, SelectExpression select2, bool distinct)
        {
            // TODO: Introduce clone method? See issue#24460
            var select1 = new SelectExpression(
                null, new List<ProjectionExpression>(), _tables.ToList(), _tableReferences.ToList(), _groupBy.ToList(), _orderings.ToList())
            {
                IsDistinct = IsDistinct,
                Predicate = Predicate,
                Having = Having,
                Offset = Offset,
                Limit = Limit
            };
            Offset = null;
            Limit = null;
            IsDistinct = false;
            Predicate = null;
            Having = null;
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
            // select1 already has unique aliases. We unique-fy select2 and set operation alias.
            select2 = (SelectExpression)new AliasUniquefier(_usedAliases).Visit(select2);
            var setOperationAlias = GenerateUniqueAlias(_usedAliases, "t");

            var setExpression = setOperationType switch
            {
                SetOperationType.Except => (SetOperationBase)new ExceptExpression(setOperationAlias, select1, select2, distinct),
                SetOperationType.Intersect => new IntersectExpression(setOperationAlias, select1, select2, distinct),
                SetOperationType.Union => new UnionExpression(setOperationAlias, select1, select2, distinct),
                _ => throw new InvalidOperationException(CoreStrings.InvalidSwitch(nameof(setOperationType), setOperationType))
            };
            var tableReferenceExpression = new TableReferenceExpression(this, setExpression.Alias);
            _tables.Add(setExpression);
            _tableReferences.Add(tableReferenceExpression);

            if (_projection.Any()
                || select2._projection.Any()
                || _pendingCollections.Any()
                || select2._pendingCollections.Any())
            {
                throw new InvalidOperationException(RelationalStrings.SetOperationsNotAllowedAfterClientEvaluation);
            }

            if (select1._projectionMapping.Count != select2._projectionMapping.Count)
            {
                // For DTO each side can have different projection mapping if some columns are not present.
                // We need to project null for missing columns.
                throw new InvalidOperationException(RelationalStrings.ProjectionMappingCountMismatch);
            }

            var aliasUniquefier = new AliasUniquefier(_usedAliases);
            foreach (var joinedMapping in select1._projectionMapping.Join(
                select2._projectionMapping,
                kv => kv.Key,
                kv => kv.Key,
                (kv1, kv2) => (kv1.Key, Value1: kv1.Value, Value2: kv2.Value)))
            {
                if (joinedMapping.Value1 is EntityProjectionExpression entityProjection1
                    && joinedMapping.Value2 is EntityProjectionExpression entityProjection2)
                {
                    HandleEntityProjection(joinedMapping.Key, select1, entityProjection1, select2, entityProjection2);
                    continue;
                }

                // We have to unique-fy left side since those projections were never uniquefied
                // Right side is unique already when we did it when running select2 through it.
                var innerColumn1 = (SqlExpression)aliasUniquefier.Visit(joinedMapping.Value1);
                var innerColumn2 = (SqlExpression)joinedMapping.Value2;
                // For now, make sure that both sides output the same store type, otherwise the query may fail.
                // TODO: with #15586 we'll be able to also allow different store types which are implicitly convertible to one another.
                if (innerColumn1.TypeMapping!.StoreType != innerColumn2.TypeMapping!.StoreType)
                {
                    throw new InvalidOperationException(RelationalStrings.SetOperationsOnDifferentStoreTypes);
                }

                var alias = GenerateUniqueColumnAlias(
                    joinedMapping.Key.Last?.Name
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

                _projectionMapping[joinedMapping.Key] = outerProjection;

                if (outerIdentifiers.Length > 0)
                {
                    var index = select1._identifier.FindIndex(e => e.Column.Equals(joinedMapping.Value1));
                    if (index != -1)
                    {
                        if (select2._identifier[index].Column.Equals(joinedMapping.Value2))
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
                    var alias = GenerateUniqueColumnAlias(_discriminatorColumnAlias);
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
                    _ => true,
                };
        }

        /// <summary>
        ///     Applies <see cref="Queryable.DefaultIfEmpty{TSource}(IQueryable{TSource})" /> on the <see cref="SelectExpression" />.
        /// </summary>
        /// <param name="sqlExpressionFactory"> A factory to use for generating required sql expressions. </param>
        public void ApplyDefaultIfEmpty(ISqlExpressionFactory sqlExpressionFactory)
        {
            Check.NotNull(sqlExpressionFactory, nameof(sqlExpressionFactory));

            var nullSqlExpression = sqlExpressionFactory.ApplyDefaultTypeMapping(
                new SqlConstantExpression(Constant(null, typeof(string)), null));

            var dummySelectExpression = new SelectExpression(
                alias: "e",
                new List<ProjectionExpression> { new(nullSqlExpression, "empty") },
                new List<TableExpressionBase>(),
                new List<TableReferenceExpression>(),
                new List<SqlExpression>(),
                new List<OrderingExpression>());

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
            IEntityType entityType, ITableBase table, string? columnName, TableExpressionBase tableExpressionBase, bool nullable = true)
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
                var tableIndex = selectExpression._tables.FindIndex(e => ReferenceEquals(UnwrapJoinExpression(e), tableExpression));
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
                if (tableExpressionBase is TableExpression tableExpression)
                {
                    if (!string.Equals(tableExpression.Name, table.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        // Fetch the table for the type which is defining the navigation since dependent would be in that table
                        tableExpression = selectExpression.Tables
                            .Select(t => (t as JoinExpressionBase)?.Table ?? t)
                            .Cast<TableExpression>()
                            .First(t => t.Name == table.Name && t.Schema == table.Schema);
                    }

                    var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
                    var tableReferenceExpression = FindTableReference(selectExpression, tableExpression);
                    foreach (var property in entityType
                        .GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                        .SelectMany(t => t.GetDeclaredProperties()))
                    {
                        propertyExpressions[property] = new ConcreteColumnExpression(
                            property, table.FindColumn(property)!, tableReferenceExpression, nullable || !property.IsPrimaryKey());
                    }

                    return propertyExpressions;
                }

                if (tableExpressionBase is SelectExpression subquery)
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
                    var tableReferenceExpression = FindTableReference(selectExpression, subquery);
                    foreach (var item in subqueryPropertyExpressions)
                    {
                        newPropertyExpressions[item.Key] = new ConcreteColumnExpression(
                            subquery.Projection[subquery.AddToProjection(item.Value)], tableReferenceExpression);
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
            var pendingCollectionOffset = _pendingCollections.Count;
            AddJoin(joinType, ref innerSelectExpression, joinPredicate);

            var transparentIdentifierType = TransparentIdentifierFactory.Create(outerShaper.Type, innerShaper.Type);
            var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetRequiredDeclaredField("Outer");
            var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetRequiredDeclaredField("Inner");
            var outerClientEval = Projection.Count > 0;
            var innerClientEval = innerSelectExpression.Projection.Count > 0;
            var remapper = new ProjectionBindingExpressionRemappingExpressionVisitor(this);
            var innerNullable = joinType == JoinType.LeftJoin || joinType == JoinType.OuterApply;

            if (outerClientEval)
            {
                // Outer projection are already populated
                if (innerClientEval)
                {
                    // Add inner to projection and update indexes
                    var indexMap = new int[innerSelectExpression.Projection.Count];
                    for (var i = 0; i < innerSelectExpression.Projection.Count; i++)
                    {
                        var projectionToAdd = innerSelectExpression.Projection[i].Expression;
                        projectionToAdd = MakeNullable(projectionToAdd, innerNullable);
                        indexMap[i] = AddToProjection(projectionToAdd);
                    }

                    innerShaper = remapper.RemapIndex(innerShaper, indexMap, pendingCollectionOffset);
                }
                else
                {
                    // Apply inner projection mapping and convert projection member binding to indexes
                    var mapping = ApplyProjectionMapping(innerSelectExpression._projectionMapping, innerNullable);
                    innerShaper = remapper.RemapProjectionMember(innerShaper, mapping, pendingCollectionOffset);
                }
            }
            else
            {
                // Depending on inner, we may either need to populate outer projection or update projection members
                if (innerClientEval)
                {
                    // Since inner proojections are populated, we need to populate outer also
                    var mapping = ApplyProjectionMapping(_projectionMapping);
                    outerShaper = remapper.RemapProjectionMember(outerShaper, mapping);

                    var indexMap = new int[innerSelectExpression.Projection.Count];
                    for (var i = 0; i < innerSelectExpression.Projection.Count; i++)
                    {
                        var projectionToAdd = innerSelectExpression.Projection[i].Expression;
                        projectionToAdd = MakeNullable(projectionToAdd, innerNullable);
                        indexMap[i] = AddToProjection(projectionToAdd);
                    }

                    innerShaper = remapper.RemapIndex(innerShaper, indexMap, pendingCollectionOffset);
                }
                else
                {
                    var projectionMapping = new Dictionary<ProjectionMember, Expression>();
                    var mapping = new Dictionary<ProjectionMember, object>();

                    foreach (var projection in _projectionMapping)
                    {
                        var projectionMember = projection.Key;
                        var remappedProjectionMember = projection.Key.Prepend(outerMemberInfo);
                        mapping[projectionMember] = remappedProjectionMember;
                        projectionMapping[remappedProjectionMember] = projection.Value;
                    }

                    outerShaper = remapper.RemapProjectionMember(outerShaper, mapping);
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

                    innerShaper = remapper.RemapProjectionMember(innerShaper, mapping, pendingCollectionOffset);
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
            SqlExpression? joinPredicate = null)
        {
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
                                            ExpressionType.AndAlso, offsetPredicate, limitPredicate, typeof(bool), joinPredicate.TypeMapping)
                                        : offsetPredicate
                                    : limitPredicate;
                                innerSelectExpression.ApplyPredicate(predicate!);
                            }


                            AddJoin(joinType == JoinType.CrossApply ? JoinType.InnerJoin : JoinType.LeftJoin,
                                ref innerSelectExpression, joinPredicate);

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

            if (innerSelectExpression.Orderings.Count > 0
                || innerSelectExpression.Limit != null
                || innerSelectExpression.Offset != null
                || innerSelectExpression.IsDistinct
                || innerSelectExpression.Predicate != null
                || innerSelectExpression.Tables.Count > 1
                || innerSelectExpression.GroupBy.Count > 0)
            {
                joinPredicate = innerSelectExpression.PushdownIntoSubqueryInternal().Remap(joinPredicate);
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
            // Copy over pending collection if in join else that info would be lost.
            // The calling method is supposed to take care of remapping the shaper so that copied over collection indexes match.
            _pendingCollections.AddRange(innerSelectExpression._pendingCollections);
            innerSelectExpression._pendingCollections.Clear();

            var joinTable = joinType switch
            {
                JoinType.InnerJoin => new InnerJoinExpression(innerTable, joinPredicate!),
                JoinType.LeftJoin => new LeftJoinExpression(innerTable, joinPredicate!),
                JoinType.CrossJoin => new CrossJoinExpression(innerTable),
                JoinType.CrossApply => new CrossApplyExpression(innerTable),
                JoinType.OuterApply => (TableExpressionBase)new OuterApplyExpression(innerTable),
                _ => throw new InvalidOperationException(CoreStrings.InvalidSwitch(nameof(joinType), joinType))
            };

            AddTable(joinTable, innerSelectExpression._tableReferences.Single());

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
                if (inner.Limit == null
                    && inner.Offset == null
                    && inner.Predicate != null)
                {
                    var columnExpressions = new List<ColumnExpression>();
                    var joinPredicate = TryExtractJoinKey(
                        outer,
                        inner,
                        inner.Predicate,
                        columnExpressions,
                        allowNonEquality,
                        out var predicate);

                    if (joinPredicate != null)
                    {
                        joinPredicate = RemoveRedundantNullChecks(joinPredicate, columnExpressions);
                    }
                    // TODO: verify the case for GroupBy. See issue#24474
                    // We extract join predicate from Predicate part but GroupBy would have last Having. Changing predicate can change groupings

                    // we can't convert apply to join in case of distinct and groupby, if the projection doesn't already contain the join keys
                    // since we can't add the missing keys to the projection - only convert to join if all the keys are already there
                    if (joinPredicate != null
                        && (inner.IsDistinct
                        || inner.GroupBy.Count > 0))
                    {
                        var innerKeyColumns = new List<ColumnExpression>();
                        InnerKeyColumns(inner.Tables, joinPredicate, innerKeyColumns);

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

                    inner.Predicate = predicate;

                    return joinPredicate;
                }

                return null;

                static SqlExpression? TryExtractJoinKey(
                    SelectExpression outer,
                    SelectExpression inner,
                    SqlExpression predicate,
                    List<ColumnExpression> columnExpressions,
                    bool allowNonEquality,
                    out SqlExpression? updatedPredicate)
                {
                    if (predicate is SqlBinaryExpression sqlBinaryExpression)
                    {
                        var joinPredicate = ValidateKeyComparison(outer, inner, sqlBinaryExpression, columnExpressions, allowNonEquality);
                        if (joinPredicate != null)
                        {
                            updatedPredicate = null;

                            return joinPredicate;
                        }

                        if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                        {
                            var leftJoinKey = TryExtractJoinKey(
                                outer, inner, sqlBinaryExpression.Left, columnExpressions, allowNonEquality, out var leftPredicate);
                            var rightJoinKey = TryExtractJoinKey(
                                outer, inner, sqlBinaryExpression.Right, columnExpressions, allowNonEquality, out var rightPredicate);

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
                    List<ColumnExpression> columnExpressions,
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
                        if (sqlBinaryExpression.Left is ColumnExpression leftColumn
                            && sqlBinaryExpression.Right is ColumnExpression rightColumn)
                        {
                            if (outer.ContainsTableReference(leftColumn)
                                && inner.ContainsTableReference(rightColumn))
                            {
                                columnExpressions.Add(leftColumn);

                                return sqlBinaryExpression;
                            }

                            if (outer.ContainsTableReference(rightColumn)
                                && inner.ContainsTableReference(leftColumn))
                            {
                                columnExpressions.Add(rightColumn);

                                return new SqlBinaryExpression(
                                    _mirroredOperationMap[sqlBinaryExpression.OperatorType],
                                    sqlBinaryExpression.Right,
                                    sqlBinaryExpression.Left,
                                    sqlBinaryExpression.Type,
                                    sqlBinaryExpression.TypeMapping);
                            }
                        }
                    }

                    // null checks are considered part of join key
                    if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
                    {
                        if (sqlBinaryExpression.Left is ColumnExpression leftNullCheckColumn
                            && outer.ContainsTableReference(leftNullCheckColumn)
                            && sqlBinaryExpression.Right is SqlConstantExpression rightConstant
                            && rightConstant.Value == null)
                        {
                            return sqlBinaryExpression;
                        }

                        if (sqlBinaryExpression.Right is ColumnExpression rightNullCheckColumn
                            && outer.ContainsTableReference(rightNullCheckColumn)
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

                static void InnerKeyColumns(IEnumerable<TableExpressionBase> tables, SqlExpression joinPredicate, List<ColumnExpression> resultColumns)
                {
                    if (joinPredicate is SqlBinaryExpression sqlBinaryExpression)
                    {
                        InnerKeyColumns(tables, sqlBinaryExpression.Left, resultColumns);
                        InnerKeyColumns(tables, sqlBinaryExpression.Right, resultColumns);
                    }
                    else if (joinPredicate is ColumnExpression columnExpression
                        && tables.Contains(columnExpression.Table))
                    {
                        resultColumns.Add(columnExpression);
                    }
                }

                static List<ColumnExpression> ExtractColumnsFromProjectionMapping(IDictionary<ProjectionMember, Expression> projectionMapping)
                {
                    var result = new List<ColumnExpression>();
                    foreach (var mappingElement in projectionMapping)
                    {
                        if (mappingElement.Value is EntityProjectionExpression entityProjection)
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
                        else if (mappingElement.Value is ColumnExpression column)
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

                static SqlExpression? RemoveRedundantNullChecks(SqlExpression predicate, List<ColumnExpression> columnExpressions)
                {
                    if (predicate is SqlBinaryExpression sqlBinaryExpression)
                    {
                        if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual
                            && sqlBinaryExpression.Left is ColumnExpression leftColumn
                            && columnExpressions.Contains(leftColumn)
                            && sqlBinaryExpression.Right is SqlConstantExpression sqlConstantExpression
                            && sqlConstantExpression.Value == null)
                        {
                            return null;
                        }

                        if (sqlBinaryExpression.OperatorType == ExpressionType.AndAlso)
                        {
                            var leftPredicate = RemoveRedundantNullChecks(sqlBinaryExpression.Left, columnExpressions);
                            var rightPredicate = RemoveRedundantNullChecks(sqlBinaryExpression.Right, columnExpressions);

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
        /// <param name="innerSelectExpression"> A <see cref="SelectExpression" /> to join with. </param>
        /// <param name="joinPredicate"> A predicate to use for the join. </param>
        public void AddInnerJoin(SelectExpression innerSelectExpression, SqlExpression joinPredicate)
        {
            Check.NotNull(innerSelectExpression, nameof(innerSelectExpression));
            Check.NotNull(joinPredicate, nameof(joinPredicate));

            AddJoin(JoinType.InnerJoin, ref innerSelectExpression, joinPredicate);
        }

        /// <summary>
        ///     Adds the given <see cref="SelectExpression" /> to table sources using LEFT JOIN.
        /// </summary>
        /// <param name="innerSelectExpression"> A <see cref="SelectExpression" /> to join with. </param>
        /// <param name="joinPredicate"> A predicate to use for the join. </param>
        public void AddLeftJoin(SelectExpression innerSelectExpression, SqlExpression joinPredicate)
        {
            Check.NotNull(innerSelectExpression, nameof(innerSelectExpression));
            Check.NotNull(joinPredicate, nameof(joinPredicate));

            AddJoin(JoinType.LeftJoin, ref innerSelectExpression, joinPredicate);
        }

        /// <summary>
        ///     Adds the given <see cref="SelectExpression" /> to table sources using CROSS JOIN.
        /// </summary>
        /// <param name="innerSelectExpression"> A <see cref="SelectExpression" /> to join with. </param>
        public void AddCrossJoin(SelectExpression innerSelectExpression)
        {
            Check.NotNull(innerSelectExpression, nameof(innerSelectExpression));

            AddJoin(JoinType.CrossJoin, ref innerSelectExpression);
        }

        /// <summary>
        ///     Adds the given <see cref="SelectExpression" /> to table sources using CROSS APPLY.
        /// </summary>
        /// <param name="innerSelectExpression"> A <see cref="SelectExpression" /> to join with. </param>
        public void AddCrossApply(SelectExpression innerSelectExpression)
        {
            Check.NotNull(innerSelectExpression, nameof(innerSelectExpression));

            AddJoin(JoinType.CrossApply, ref innerSelectExpression);
        }

        /// <summary>
        ///     Adds the given <see cref="SelectExpression" /> to table sources using OUTER APPLY.
        /// </summary>
        /// <param name="innerSelectExpression"> A <see cref="SelectExpression" /> to join with. </param>
        public void AddOuterApply(SelectExpression innerSelectExpression)
        {
            Check.NotNull(innerSelectExpression, nameof(innerSelectExpression));

            AddJoin(JoinType.OuterApply, ref innerSelectExpression);
        }

        /// <summary>
        ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using INNER JOIN and combine shapers.
        /// </summary>
        /// <param name="innerSource"> A <see cref="ShapedQueryExpression" /> to join with. </param>
        /// <param name="joinPredicate"> A predicate to use for the join. </param>
        /// <param name="outerShaper"> An expression for outer shaper. </param>
        /// <returns> An expression which shapes the result of this join. </returns>
        public Expression AddInnerJoin(
            ShapedQueryExpression innerSource,
            SqlExpression joinPredicate,
            Expression outerShaper)
        {
            Check.NotNull(innerSource, nameof(innerSource));
            Check.NotNull(joinPredicate, nameof(joinPredicate));
            Check.NotNull(outerShaper, nameof(outerShaper));

            return AddJoin(
                JoinType.InnerJoin, (SelectExpression)innerSource.QueryExpression, outerShaper, innerSource.ShaperExpression,
                joinPredicate);
        }

        /// <summary>
        ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using LEFT JOIN and combine shapers.
        /// </summary>
        /// <param name="innerSource"> A <see cref="ShapedQueryExpression" /> to join with. </param>
        /// <param name="joinPredicate"> A predicate to use for the join. </param>
        /// <param name="outerShaper"> An expression for outer shaper. </param>
        /// <returns> An expression which shapes the result of this join. </returns>
        public Expression AddLeftJoin(
            ShapedQueryExpression innerSource,
            SqlExpression joinPredicate,
            Expression outerShaper)
        {
            Check.NotNull(innerSource, nameof(innerSource));
            Check.NotNull(joinPredicate, nameof(joinPredicate));
            Check.NotNull(outerShaper, nameof(outerShaper));

            return AddJoin(
                JoinType.LeftJoin, (SelectExpression)innerSource.QueryExpression, outerShaper, innerSource.ShaperExpression, joinPredicate);
        }

        /// <summary>
        ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using CROSS JOIN and combine shapers.
        /// </summary>
        /// <param name="innerSource"> A <see cref="ShapedQueryExpression" /> to join with. </param>
        /// <param name="outerShaper"> An expression for outer shaper. </param>
        /// <returns> An expression which shapes the result of this join. </returns>
        public Expression AddCrossJoin(
            ShapedQueryExpression innerSource,
            Expression outerShaper)
        {
            Check.NotNull(innerSource, nameof(innerSource));
            Check.NotNull(outerShaper, nameof(outerShaper));

            return AddJoin(JoinType.CrossJoin, (SelectExpression)innerSource.QueryExpression, outerShaper, innerSource.ShaperExpression);
        }

        /// <summary>
        ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using CROSS APPLY and combine shapers.
        /// </summary>
        /// <param name="innerSource"> A <see cref="ShapedQueryExpression" /> to join with. </param>
        /// <param name="outerShaper"> An expression for outer shaper. </param>
        /// <returns> An expression which shapes the result of this join. </returns>
        public Expression AddCrossApply(
            ShapedQueryExpression innerSource,
            Expression outerShaper)
        {
            Check.NotNull(innerSource, nameof(innerSource));
            Check.NotNull(outerShaper, nameof(outerShaper));

            return AddJoin(JoinType.CrossApply, (SelectExpression)innerSource.QueryExpression, outerShaper, innerSource.ShaperExpression);
        }

        /// <summary>
        ///     Adds the query expression of the given <see cref="ShapedQueryExpression" /> to table sources using OUTER APPLY and combine shapers.
        /// </summary>
        /// <param name="innerSource"> A <see cref="ShapedQueryExpression" /> to join with. </param>
        /// <param name="outerShaper"> An expression for outer shaper. </param>
        /// <returns> An expression which shapes the result of this join. </returns>
        public Expression AddOuterApply(
            ShapedQueryExpression innerSource,
            Expression outerShaper)
        {
            Check.NotNull(innerSource, nameof(innerSource));
            Check.NotNull(outerShaper, nameof(outerShaper));

            return AddJoin(JoinType.OuterApply, (SelectExpression)innerSource.QueryExpression, outerShaper, innerSource.ShaperExpression);
        }

        #region ObsoleteMethods

        [Obsolete]
        private void AddJoin(
            JoinType joinType,
            SelectExpression innerSelectExpression,
            Type? transparentIdentifierType,
            SqlExpression? joinPredicate)
        {
            AddJoin(joinType, ref innerSelectExpression, joinPredicate);

            if (transparentIdentifierType != null)
            {
                var outerMemberInfo = transparentIdentifierType.GetTypeInfo().GetRequiredDeclaredField("Outer");
                var projectionMapping = new Dictionary<ProjectionMember, Expression>();
                foreach (var projection in _projectionMapping)
                {
                    projectionMapping[projection.Key.Prepend(outerMemberInfo)] = projection.Value;
                }

                var innerMemberInfo = transparentIdentifierType.GetTypeInfo().GetRequiredDeclaredField("Inner");
                var innerNullable = joinType == JoinType.LeftJoin || joinType == JoinType.OuterApply;
                foreach (var projection in innerSelectExpression._projectionMapping)
                {
                    var projectionToAdd = projection.Value;
                    if (innerNullable)
                    {
                        if (projectionToAdd is EntityProjectionExpression entityProjection)
                        {
                            projectionToAdd = entityProjection.MakeNullable();
                        }
                        else if (projectionToAdd is ColumnExpression column)
                        {
                            projectionToAdd = column.MakeNullable();
                        }
                    }

                    projectionMapping[projection.Key.Prepend(innerMemberInfo)] = projectionToAdd;
                }

                _projectionMapping = projectionMapping;
            }
        }

        /// <summary>
        ///     Adds the given <see cref="SelectExpression" /> to table sources using INNER JOIN.
        /// </summary>
        /// <param name="innerSelectExpression"> A <see cref="SelectExpression" /> to join with. </param>
        /// <param name="joinPredicate"> A predicate to use for the join. </param>
        /// <param name="transparentIdentifierType"> The type of the result generated after performing the join. </param>
        [Obsolete("Use the other overloads.")]
        public void AddInnerJoin(
            SelectExpression innerSelectExpression,
            SqlExpression joinPredicate,
            Type? transparentIdentifierType)
        {
            Check.NotNull(innerSelectExpression, nameof(innerSelectExpression));
            Check.NotNull(joinPredicate, nameof(joinPredicate));

            AddJoin(JoinType.InnerJoin, innerSelectExpression, transparentIdentifierType, joinPredicate);
        }

        /// <summary>
        ///     Adds the given <see cref="SelectExpression" /> to table sources using LEFT JOIN.
        /// </summary>
        /// <param name="innerSelectExpression"> A <see cref="SelectExpression" /> to join with. </param>
        /// <param name="joinPredicate"> A predicate to use for the join. </param>
        /// <param name="transparentIdentifierType"> The type of the result generated after performing the join. </param>
        [Obsolete("Use the other overloads.")]
        public void AddLeftJoin(
            SelectExpression innerSelectExpression,
            SqlExpression joinPredicate,
            Type? transparentIdentifierType)
        {
            Check.NotNull(innerSelectExpression, nameof(innerSelectExpression));
            Check.NotNull(joinPredicate, nameof(joinPredicate));

            AddJoin(JoinType.LeftJoin, innerSelectExpression, transparentIdentifierType, joinPredicate);
        }

        /// <summary>
        ///     Adds the given <see cref="SelectExpression" /> to table sources using CROSS JOIN.
        /// </summary>
        /// <param name="innerSelectExpression"> A <see cref="SelectExpression" /> to join with. </param>
        /// <param name="transparentIdentifierType"> The type of the result generated after performing the join. </param>
        [Obsolete("Use the other overloads.")]
        public void AddCrossJoin(SelectExpression innerSelectExpression, Type? transparentIdentifierType)
        {
            Check.NotNull(innerSelectExpression, nameof(innerSelectExpression));

            AddJoin(JoinType.CrossJoin, innerSelectExpression, transparentIdentifierType, null);
        }

        /// <summary>
        ///     Adds the given <see cref="SelectExpression" /> to table sources using CROSS APPLY.
        /// </summary>
        /// <param name="innerSelectExpression"> A <see cref="SelectExpression" /> to join with. </param>
        /// <param name="transparentIdentifierType"> The type of the result generated after performing the join. </param>
        [Obsolete("Use the other overloads.")]
        public void AddCrossApply(SelectExpression innerSelectExpression, Type? transparentIdentifierType)
        {
            Check.NotNull(innerSelectExpression, nameof(innerSelectExpression));

            AddJoin(JoinType.CrossApply, innerSelectExpression, transparentIdentifierType, null);
        }

        /// <summary>
        ///     Adds the given <see cref="SelectExpression" /> to table sources using OUTER APPLY.
        /// </summary>
        /// <param name="innerSelectExpression"> A <see cref="SelectExpression" /> to join with. </param>
        /// <param name="transparentIdentifierType"> The type of the result generated after performing the join. </param>
        [Obsolete("Use the other overloads.")]
        public void AddOuterApply(SelectExpression innerSelectExpression, Type? transparentIdentifierType)
        {
            Check.NotNull(innerSelectExpression, nameof(innerSelectExpression));

            AddJoin(JoinType.OuterApply, innerSelectExpression, transparentIdentifierType, null);
        }

        #endregion

        /// <summary>
        ///     Pushes down the <see cref="SelectExpression" /> into a subquery.
        /// </summary>
        public void PushdownIntoSubquery()
        {
            PushdownIntoSubqueryInternal();
        }

        private SqlRemappingVisitor PushdownIntoSubqueryInternal()
        {
            var subqueryAlias = GenerateUniqueAlias(_usedAliases, "t");
            var subquery = new SelectExpression(
                subqueryAlias, new List<ProjectionExpression>(), _tables.ToList(), _tableReferences.ToList(), _groupBy.ToList(), _orderings.ToList())
            {
                IsDistinct = IsDistinct,
                Predicate = Predicate,
                Having = Having,
                Offset = Offset,
                Limit = Limit
            };
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

            var projectionMap = new Dictionary<SqlExpression, ColumnExpression>(ReferenceEqualityComparer.Instance);

            // Projection would be present for client eval.
            if (_projection.Any())
            {
                var projections = _projection.ToList();
                _projection.Clear();
                foreach (var projection in projections)
                {
                    var outerColumn = subquery.GenerateOuterColumn(subqueryTableReferenceExpression, projection.Expression, projection.Alias);
                    AddToProjection(outerColumn);
                    projectionMap[projection.Expression] = outerColumn;
                }
            }

            foreach (var mapping in _projectionMapping.ToList())
            {
                // If projectionMapping's value is ConstantExpression then projection has already been applied
                // And captured in _projections above so we don't need to process this.
                if (mapping.Value is ConstantExpression)
                {
                    break;
                }

                if (mapping.Value is EntityProjectionExpression entityProjection)
                {
                    _projectionMapping[mapping.Key] = LiftEntityProjectionFromSubquery(entityProjection);
                }
                else
                {
                    var innerColumn = (SqlExpression)mapping.Value;
                    var outerColumn = subquery.GenerateOuterColumn(subqueryTableReferenceExpression, innerColumn, mapping.Key.Last?.Name);
                    projectionMap[innerColumn] = outerColumn;
                    _projectionMapping[mapping.Key] = outerColumn;
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
            foreach (var identifier in identifiers)
            {
                // Invariant, identifier should not contain term which cannot be projected out.
                if (!projectionMap.TryGetValue(identifier.Column, out var outerColumn))
                {
                    outerColumn = subquery.GenerateOuterColumn(subqueryTableReferenceExpression, identifier.Column);
                }
                _identifier.Add((outerColumn, identifier.Comparer));
            }

            var childIdentifiers = _childIdentifiers.ToList();
            _childIdentifiers.Clear();
            foreach (var identifier in childIdentifiers)
            {
                // Invariant, identifier should not contain term which cannot be projected out.
                if (!projectionMap.TryGetValue(identifier.Column, out var outerColumn))
                {
                    outerColumn = subquery.GenerateOuterColumn(subqueryTableReferenceExpression, identifier.Column);
                }
                _childIdentifiers.Add((outerColumn, identifier.Comparer));
            }

            foreach (var ordering in subquery._orderings)
            {
                var orderingExpression = ordering.Expression;
                if (projectionMap.TryGetValue(orderingExpression, out var outerColumn))
                {
                    _orderings.Add(ordering.Update(outerColumn));
                }
                else if (!IsDistinct
                    && GroupBy.Count == 0 || GroupBy.Contains(orderingExpression))
                {
                    _orderings.Add(ordering.Update(subquery.GenerateOuterColumn(subqueryTableReferenceExpression, orderingExpression)));
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

            var pendingCollections = _pendingCollections.ToList();
            _pendingCollections.Clear();
            foreach (var collection in pendingCollections)
            {
                // We need to update tableReferences first in case the collection has correlated element to this select expression
                _pendingCollections.Add(sqlRemappingVisitor.Remap(
                    (SelectExpression)tableReferenceUpdatingExpressionVisitor.Visit(collection)!));
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
                        subqueryTableReferenceExpression, entityProjection.DiscriminatorExpression, _discriminatorColumnAlias);
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
        ///     Checks whether this <see cref="SelectExpression" /> representes a <see cref="FromSqlExpression" /> which is not composed upon.
        /// </summary>
        /// <returns> A bool value indicating a non-composed <see cref="FromSqlExpression" />. </returns>
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

        private Dictionary<ProjectionMember, object> ApplyProjectionMapping(
            Dictionary<ProjectionMember, Expression> projectionMapping,
            bool makeNullable = false)
        {
            var mapping = new Dictionary<ProjectionMember, object>();
            var entityProjectionCache = new Dictionary<EntityProjectionExpression, IReadOnlyDictionary<IProperty, int>>(ReferenceEqualityComparer.Instance);
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
                        value = AddToProjection(entityProjection);
                        entityProjectionCache[entityProjectionToCache] = value;
                    }

                    mapping[projectionMember] = value;
                }
                else
                {
                    projectionToAdd = MakeNullable(projectionToAdd, makeNullable);
                    mapping[projectionMember] = AddToProjection((SqlExpression)projectionToAdd, projectionMember.Last?.Name);
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
            => UnwrapJoinExpression(tableExpressionBase).Alias!;

        private static TableExpressionBase UnwrapJoinExpression(TableExpressionBase tableExpressionBase)
            => (tableExpressionBase as JoinExpressionBase)?.Table ?? tableExpressionBase;

        private static IEnumerable<IProperty> GetAllPropertiesInHierarchy(IEntityType entityType)
            => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetDeclaredProperties());

        private static ConcreteColumnExpression CreateColumnExpression(
            IProperty property, ITableBase table, TableReferenceExpression tableExpression, bool nullable)
            => new(property, table.FindColumn(property)!, tableExpression, nullable);

        private ConcreteColumnExpression GenerateOuterColumn(
            TableReferenceExpression tableReferenceExpression, SqlExpression projection, string? alias = null)
        {
            var index = AddToProjection(projection, alias);

            return new ConcreteColumnExpression(_projection[index], tableReferenceExpression);
        }

        private bool ContainsTableReference(ColumnExpression column)
        // This method is used when evaluating join correlations.
        // At that point aliases are not unique-fied across so we need to match tables
        // TODO: Avoid need of using UnwrapJoinExpression on both. See issue#24473
            => Tables.Any(e => ReferenceEquals(UnwrapJoinExpression(e), UnwrapJoinExpression(column.Table)));


        private void AddTable(TableExpressionBase tableExpressionBase, TableReferenceExpression tableReferenceExpression)
        {
            Check.DebugAssert(_tables.Count == _tableReferences.Count, "All the tables should have their associated TableReferences.");
            Check.DebugAssert(
                string.Equals(GetAliasFromTableExpressionBase(tableExpressionBase), tableReferenceExpression.Alias),
                "Alias of table and table reference should be the same.");

            var uniqueAlias = GenerateUniqueAlias(_usedAliases, tableReferenceExpression.Alias);
            UnwrapJoinExpression(tableExpressionBase).Alias = uniqueAlias;
            tableReferenceExpression.Alias = uniqueAlias;

            tableExpressionBase = (TableExpressionBase)new AliasUniquefier(_usedAliases).Visit(tableExpressionBase);
            _tables.Add(tableExpressionBase);
            _tableReferences.Add(tableReferenceExpression);
        }

        private SqlExpression AssignUniqueAliases(SqlExpression expression)
            => (SqlExpression)new AliasUniquefier(_usedAliases).Visit(expression);

        private static string GenerateUniqueAlias(HashSet<string> usedAliases, string currentAlias)
        {
            var counter = 0;
            var baseAlias = currentAlias[0..1];

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
            Check.NotNull(visitor, nameof(visitor));

            // If there are pending collections, then do in-place mutation.
            // Post translation we want not in place mutation so that cached SelectExpression inside relational command doesn't get mutated.
            if (_pendingCollections.Any(e => e != null))
            {
                if (Projection.Any())
                {
                    var projections = _projection.ToList();
                    _projection.Clear();
                    _projection.AddRange(projections.Select(e => (ProjectionExpression)visitor.Visit(e)));
                }
                else
                {
                    var projectionMapping = new Dictionary<ProjectionMember, Expression>();
                    foreach (var mapping in _projectionMapping)
                    {
                        var newProjection = visitor.Visit(mapping.Value);

                        projectionMapping[mapping.Key] = newProjection;
                    }

                    _projectionMapping = projectionMapping;
                }

                // We cannot erase _tables before visiting all because joinPredicate may reference them which breaks referential integrity
                var visitedTables = new List<TableExpressionBase>();
                visitedTables.AddRange(_tables.Select(e => (TableExpressionBase)visitor.Visit(e)));
                Check.DebugAssert(
                    visitedTables.Select(e => GetAliasFromTableExpressionBase(e)).SequenceEqual(_tableReferences.Select(e => e.Alias)),
                    "Aliases of Table/TableReferences must match after visit.");
                _tables.Clear();
                _tables.AddRange(visitedTables);

                Predicate = (SqlExpression?)visitor.Visit(Predicate);

                var groupBy = _groupBy.ToList();
                _groupBy.Clear();
                _groupBy.AddRange(
                    groupBy.Select(e => (SqlExpression)visitor.Visit(e))
                        .Where(e => !(e is SqlConstantExpression || e is SqlParameterExpression)));

                Having = (SqlExpression?)visitor.Visit(Having);

                var orderings = _orderings.ToList();
                _orderings.Clear();
                _orderings.AddRange(orderings.Select(e => e.Update((SqlExpression)visitor.Visit(e.Expression))));

                Offset = (SqlExpression?)visitor.Visit(Offset);
                Limit = (SqlExpression?)visitor.Visit(Limit);

                if (visitor is SqlRemappingVisitor)
                {
                    // We have to traverse pending collections for remapping so that columns from outer are updated.
                    var pendingCollections = _pendingCollections.ToList();
                    _pendingCollections.Clear();
                    _pendingCollections.AddRange(pendingCollections.Select(e => (SelectExpression)visitor.Visit(e)!));
                }

                return this;
            }

            var changed = false;

            var newProjections = _projection;
            var newProjectionMapping = _projectionMapping;
            if (_projection.Any())
            {
                for (var i = 0; i < _projection.Count; i++)
                {
                    var item = _projection[i];
                    var projection = (ProjectionExpression)visitor.Visit(item);
                    if (projection != item
                        && newProjections == _projection)
                    {
                        newProjections = new List<ProjectionExpression>(_projection.Count);
                        for (var j = 0; j < i; j++)
                        {
                            newProjections.Add(_projection[j]);
                        }

                        changed = true;
                    }

                    if (newProjections != _projection)
                    {
                        newProjections.Add(projection);
                    }
                }
            }
            else
            {
                foreach (var mapping in _projectionMapping)
                {
                    var newProjection = visitor.Visit(mapping.Value);
                    if (newProjection != mapping.Value
                        && newProjectionMapping == _projectionMapping)
                    {
                        newProjectionMapping = new Dictionary<ProjectionMember, Expression>(_projectionMapping);
                        changed = true;
                    }

                    if (newProjectionMapping != _projectionMapping)
                    {
                        newProjectionMapping[mapping.Key] = newProjection;
                    }
                }
            }

            var newTables = _tables;
            for (var i = 0; i < _tables.Count; i++)
            {
                var table = _tables[i];
                var newTable = (TableExpressionBase)visitor.Visit(table);
                Check.DebugAssert(GetAliasFromTableExpressionBase(newTable) == _tableReferences[i].Alias,
                    "Alias of updated table must match the old table.");
                if (newTable != table
                    && newTables == _tables)
                {
                    newTables = new List<TableExpressionBase>(_tables.Count);
                    for (var j = 0; j < i; j++)
                    {
                        newTables.Add(_tables[j]);
                    }

                    changed = true;
                }

                if (newTables != _tables)
                {
                    newTables.Add(newTable);
                }
            }

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

            var newOrderings = _orderings;
            for (var i = 0; i < _orderings.Count; i++)
            {
                var ordering = _orderings[i];
                var newOrdering = (OrderingExpression)visitor.Visit(ordering);
                if (newOrdering != ordering
                    && newOrderings == _orderings)
                {
                    newOrderings = new List<OrderingExpression>(_orderings.Count);
                    for (var j = 0; j < i; j++)
                    {
                        newOrderings.Add(_orderings[j]);
                    }

                    changed = true;
                }

                if (newOrderings != _orderings)
                {
                    newOrderings.Add(newOrdering);
                }
            }

            var offset = (SqlExpression?)visitor.Visit(Offset);
            changed |= offset != Offset;

            var limit = (SqlExpression?)visitor.Visit(Limit);
            changed |= limit != Limit;

            if (changed)
            {
                var newTableReferences = _tableReferences.ToList();
                var newSelectExpression = new SelectExpression(Alias, newProjections, newTables, newTableReferences, newGroupBy, newOrderings)
                {
                    _projectionMapping = newProjectionMapping,
                    Predicate = predicate,
                    Having = havingExpression,
                    Offset = offset,
                    Limit = limit,
                    IsDistinct = IsDistinct,
                    Tags = Tags
                };

                newSelectExpression._identifier.AddRange(_identifier);
                newSelectExpression._identifier.AddRange(_childIdentifiers);

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

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="projections"> The <see cref="Projection" /> property of the result. </param>
        /// <param name="tables"> The <see cref="Tables" /> property of the result. </param>
        /// <param name="predicate"> The <see cref="Predicate" /> property of the result. </param>
        /// <param name="groupBy"> The <see cref="GroupBy" /> property of the result. </param>
        /// <param name="having"> The <see cref="Having" /> property of the result. </param>
        /// <param name="orderings"> The <see cref="Orderings" /> property of the result. </param>
        /// <param name="limit"> The <see cref="Limit" /> property of the result. </param>
        /// <param name="offset"> The <see cref="Offset" /> property of the result. </param>
        /// <param name="distinct"> The <see cref="IsDistinct" /> property of the result. </param>
        /// <param name="alias"> The <see cref="P:Alias" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        // This does not take internal states since when using this method SelectExpression should be finalized
        [Obsolete("Use the overload which does not require distinct & alias parameter.")]
        public SelectExpression Update(
            IReadOnlyList<ProjectionExpression> projections,
            IReadOnlyList<TableExpressionBase> tables,
            SqlExpression? predicate,
            IReadOnlyList<SqlExpression> groupBy,
            SqlExpression? having,
            IReadOnlyList<OrderingExpression> orderings,
            SqlExpression? limit,
            SqlExpression? offset,
            bool distinct,
            string? alias)
        {
            Check.NotNull(projections, nameof(projections));
            Check.NotNull(tables, nameof(tables));
            Check.NotNull(groupBy, nameof(groupBy));
            Check.NotNull(orderings, nameof(orderings));

            var projectionMapping = new Dictionary<ProjectionMember, Expression>();
            foreach (var kvp in _projectionMapping)
            {
                projectionMapping[kvp.Key] = kvp.Value;
            }

            var newTableReferences = _tableReferences.ToList();
            var newSelectExpression = new SelectExpression(
                alias, projections.ToList(), tables.ToList(), newTableReferences, groupBy.ToList(), orderings.ToList())
            {
                _projectionMapping = projectionMapping,
                Predicate = predicate,
                Having = having,
                Offset = offset,
                Limit = limit,
                IsDistinct = distinct,
                Tags = Tags
            };

            // We don't copy identifiers because when we are doing reconstruction pending collections are already applied.
            // We don't visit pending collection with TableReferenceUpdatingExpressionVisitor for same reason.

            // Remap tableReferences in new select expression
            foreach (var tableReference in newTableReferences)
            {
                tableReference.UpdateTableReference(this, newSelectExpression);
            }

            var tableReferenceUpdatingExpressionVisitor = new TableReferenceUpdatingExpressionVisitor(this, newSelectExpression);
            tableReferenceUpdatingExpressionVisitor.Visit(newSelectExpression);

            return newSelectExpression;
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="projections"> The <see cref="Projection" /> property of the result. </param>
        /// <param name="tables"> The <see cref="Tables" /> property of the result. </param>
        /// <param name="predicate"> The <see cref="Predicate" /> property of the result. </param>
        /// <param name="groupBy"> The <see cref="GroupBy" /> property of the result. </param>
        /// <param name="having"> The <see cref="Having" /> property of the result. </param>
        /// <param name="orderings"> The <see cref="Orderings" /> property of the result. </param>
        /// <param name="limit"> The <see cref="Limit" /> property of the result. </param>
        /// <param name="offset"> The <see cref="Offset" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
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
            Check.NotNull(projections, nameof(projections));
            Check.NotNull(tables, nameof(tables));
            Check.NotNull(groupBy, nameof(groupBy));
            Check.NotNull(orderings, nameof(orderings));

#pragma warning disable CS0618 // Type or member is obsolete
            return Update(projections, tables, predicate, groupBy, having, orderings, limit, offset, IsDistinct, Alias);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <inheritdoc />
        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine("Projection Mapping:");
            using (expressionPrinter.Indent())
            {
                foreach (var projectionMappingEntry in _projectionMapping)
                {
                    expressionPrinter.AppendLine();
                    expressionPrinter.Append(projectionMappingEntry.Key + " -> ");
                    expressionPrinter.Visit(projectionMappingEntry.Value);
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
        {
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
            return ReferenceEquals(this, selectExpression);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());

            foreach (var projection in _projection)
            {
                hash.Add(projection);
            }

            foreach (var projectionMapping in _projectionMapping)
            {
                hash.Add(projectionMapping.Key);
                hash.Add(projectionMapping.Value);
            }

            foreach (var tag in Tags)
            {
                hash.Add(tag);
            }

            foreach (var table in _tables)
            {
                hash.Add(table);
            }

            hash.Add(Predicate);

            foreach (var groupingKey in _groupBy)
            {
                hash.Add(groupingKey);
            }

            hash.Add(Having);

            foreach (var ordering in _orderings)
            {
                hash.Add(ordering);
            }

            hash.Add(Offset);
            hash.Add(Limit);
            hash.Add(IsDistinct);

            return hash.ToHashCode();
        }
    }
}
