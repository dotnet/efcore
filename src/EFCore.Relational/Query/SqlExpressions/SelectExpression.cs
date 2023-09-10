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
[DebuggerDisplay("{PrintShortSql(), nq}")]
public sealed partial class SelectExpression : TableExpressionBase
{
    private const string DiscriminatorColumnAlias = "Discriminator";
    private static readonly IdentifierComparer IdentifierComparerInstance = new();

    private readonly List<ProjectionExpression> _projection = new();
    private readonly List<TableExpressionBase> _tables = new();
    private readonly List<TableReferenceExpression> _tableReferences = new();
    private readonly List<SqlExpression> _groupBy = new();
    private readonly List<OrderingExpression> _orderings = new();

    private readonly List<(ColumnExpression Column, ValueComparer Comparer)> _identifier = new();
    private readonly List<(ColumnExpression Column, ValueComparer Comparer)> _childIdentifiers = new();
    private readonly List<int> _removableJoinTables = new();

    private readonly Dictionary<TpcTablesExpression, (ColumnExpression, List<string>)> _tpcDiscriminatorValues
        = new(ReferenceEqualityComparer.Instance);

    private bool _mutable = true;
    private HashSet<string> _usedAliases = new();
    private Dictionary<ProjectionMember, Expression> _projectionMapping = new();
    private List<Expression> _clientProjections = new();
    private readonly List<string?> _aliasForClientProjections = new();
    private CloningExpressionVisitor? _cloningExpressionVisitor;

    private SortedDictionary<string, IAnnotation>? _annotations;

    // We need to remember identifiers before GroupBy in case it is final GroupBy and element selector has a collection
    // This state doesn't need to propagate
    // It should be only at top-level otherwise GroupBy won't be final operator.
    // Cloning skips it altogether (we don't clone top level with GroupBy)
    // Pushdown should null it out as if GroupBy was present was pushed down.
    private List<(ColumnExpression Column, ValueComparer Comparer)>? _preGroupByIdentifier;

#if DEBUG
    private List<string>? _removedAliases;
#endif

    private SelectExpression(
        string? alias,
        List<ProjectionExpression> projections,
        List<TableExpressionBase> tables,
        List<TableReferenceExpression> tableReferences,
        List<SqlExpression> groupBy,
        List<OrderingExpression> orderings,
        IEnumerable<IAnnotation> annotations)
        : base(alias)
    {
        _projection = projections;
        _tables = tables;
        _tableReferences = tableReferences;
        _groupBy = groupBy;
        _orderings = orderings;

        if (annotations != null)
        {
            _annotations = new SortedDictionary<string, IAnnotation>();
            foreach (var annotation in annotations)
            {
                _annotations[annotation.Name] = annotation;
            }
        }
    }

    private SelectExpression(string? alias)
        : base(alias)
    {
    }

    internal SelectExpression(SqlExpression? projection)
        : base(null)
    {
        if (projection != null)
        {
            _projectionMapping[new ProjectionMember()] = projection;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public SelectExpression(
        TableExpressionBase tableExpression,
        string columnName,
        Type columnType,
        RelationalTypeMapping? columnTypeMapping,
        bool? isColumnNullable = null,
        string? identifierColumnName = null,
        Type? identifierColumnType = null,
        RelationalTypeMapping? identifierColumnTypeMapping = null)
        : base(null)
    {
        var tableReferenceExpression = new TableReferenceExpression(this, tableExpression.Alias!);
        AddTable(tableExpression, tableReferenceExpression);

        var columnExpression = new ConcreteColumnExpression(
            columnName,
            tableReferenceExpression,
            columnType.UnwrapNullableType(),
            columnTypeMapping,
            isColumnNullable ?? columnType.IsNullableType());

        _projectionMapping[new ProjectionMember()] = columnExpression;

        if (identifierColumnName != null && identifierColumnType != null && identifierColumnTypeMapping != null)
        {
            var identifierColumn = new ConcreteColumnExpression(
                identifierColumnName,
                tableReferenceExpression,
                identifierColumnType.UnwrapNullableType(),
                identifierColumnTypeMapping,
                identifierColumnType.IsNullableType());

            _identifier.Add((identifierColumn, identifierColumnTypeMapping!.Comparer));
        }
        else
        {
            Debug.Assert(
                identifierColumnName == null && identifierColumnType == null && identifierColumnTypeMapping == null,
                "Either provide all identity information (column name, type and type mapping), or don't provide any.");
        }
    }

    internal SelectExpression(IEntityType entityType, ISqlExpressionFactory sqlExpressionFactory)
        : base(null)
    {
        switch (entityType.GetMappingStrategy())
        {
            case RelationalAnnotationNames.TptMappingStrategy:
            {
                var keyProperties = entityType.FindPrimaryKey()!.Properties;
                List<ColumnExpression> joinColumns = default!;
                var tableMap = new Dictionary<ITableBase, TableReferenceExpression>();
                var columns = new Dictionary<IProperty, ColumnExpression>();
                foreach (var baseType in entityType.GetAllBaseTypesInclusive())
                {
                    var table = GetTableBaseFiltered(baseType, tableMap);
                    var tableExpression = new TableExpression(table);
                    var tableReferenceExpression = new TableReferenceExpression(this, tableExpression.Alias);
                    tableMap.Add(table, tableReferenceExpression);

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

                        var joinPredicate = joinColumns
                            .Zip(innerColumns, sqlExpressionFactory.Equal)
                            .Aggregate(sqlExpressionFactory.AndAlso);

                        var joinExpression = new InnerJoinExpression(tableExpression, joinPredicate);
                        AddTable(joinExpression, tableReferenceExpression);
                    }
                }

                var caseWhenClauses = new List<CaseWhenClause>();
                foreach (var derivedType in entityType.GetDerivedTypes())
                {
                    var table = GetTableBaseFiltered(derivedType, tableMap);
                    var tableExpression = new TableExpression(table);
                    var tableReferenceExpression = new TableReferenceExpression(this, tableExpression.Alias);
                    tableMap.Add(table, tableReferenceExpression);

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

                    var joinPredicate = joinColumns
                        .Zip(keyColumns, sqlExpressionFactory.Equal)
                        .Aggregate(sqlExpressionFactory.AndAlso);

                    var joinExpression = new LeftJoinExpression(tableExpression, joinPredicate);
                    _removableJoinTables.Add(_tables.Count);
                    AddTable(joinExpression, tableReferenceExpression);
                }

                caseWhenClauses.Reverse();
                var discriminatorExpression = caseWhenClauses.Count == 0
                    ? null
                    : sqlExpressionFactory.ApplyDefaultTypeMapping(
                        sqlExpressionFactory.Case(caseWhenClauses, elseResult: null));
                _projectionMapping[new ProjectionMember()] =
                    new StructuralTypeProjectionExpression(entityType, columns, tableMap, nullable: false, discriminatorExpression);
            }

            break;

            case RelationalAnnotationNames.TpcMappingStrategy:
            {
                // Drop additional table if ofType/is operator used Issue#27957
                var entityTypes = entityType.GetDerivedTypesInclusive().Where(e => !e.IsAbstract()).ToArray();
                if (entityTypes.Length == 1)
                {
                    // For single entity case, we don't need discriminator.
                    var table = entityTypes[0].GetViewOrTableMappings().Single().Table;
                    var tableExpression = new TableExpression(table);
                    var tableReferenceExpression = new TableReferenceExpression(this, tableExpression.Alias);
                    var tableMap = new Dictionary<ITableBase, TableReferenceExpression> { [table] = tableReferenceExpression };
                    AddTable(tableExpression, tableReferenceExpression);

                    var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
                    foreach (var property in GetAllPropertiesInHierarchy(entityType))
                    {
                        propertyExpressions[property] = CreateColumnExpression(property, table, tableReferenceExpression, nullable: false);
                    }

                    _projectionMapping[new ProjectionMember()] = new StructuralTypeProjectionExpression(entityType, propertyExpressions, tableMap);

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
                    var tables = entityTypes.Select(e => e.GetViewOrTableMappings().Single().Table).ToArray();
                    var properties = GetAllFlattenedPropertiesInHierarchy(entityType).ToArray();
                    var propertyNamesMap = new Dictionary<IProperty, string>();
                    for (var i = 0; i < entityTypes.Length; i++)
                    {
                        foreach (var property in entityTypes[i].GetFlattenedProperties())
                        {
                            if (!propertyNamesMap.ContainsKey(property))
                            {
                                propertyNamesMap[property] = tables[i].FindColumn(property)!.Name;
                            }
                        }
                    }

                    // Uniquify the column names
                    var propertyNames = new string[properties.Length];
                    for (var i = 0; i < properties.Length; i++)
                    {
                        var candidateName = propertyNamesMap[properties[i]];
                        var uniqueAliasIndex = 0;
                        var currentName = candidateName;
                        while (propertyNames.Take(i).Any(e => string.Equals(e, currentName, StringComparison.OrdinalIgnoreCase)))
                        {
                            currentName = candidateName + uniqueAliasIndex++;
                        }

                        propertyNames[i] = currentName;
                    }

                    // Uniquify the discriminator column name
                    var discriminatorColumnName = DiscriminatorColumnAlias;
                    if (propertyNames.Any(e => string.Equals(discriminatorColumnName, e, StringComparison.OrdinalIgnoreCase)))
                    {
                        var uniqueAliasIndex = 0;
                        var currentName = discriminatorColumnName;
                        while (propertyNames.Any(e => string.Equals(e, currentName, StringComparison.OrdinalIgnoreCase)))
                        {
                            currentName = discriminatorColumnName + uniqueAliasIndex++;
                        }

                        discriminatorColumnName = currentName;
                    }

                    var subSelectExpressions = new List<SelectExpression>();
                    var discriminatorValues = new List<string>();
                    for (var i = 0; i < entityTypes.Length; i++)
                    {
                        var concreteEntityType = entityTypes[i];
                        var table = tables[i];
                        var selectExpression = new SelectExpression(alias: null);
                        // We intentionally do not assign unique aliases here in case some select expression gets pruned later
                        var tableExpression = new TableExpression(table);
                        var tableReferenceExpression = new TableReferenceExpression(selectExpression, tableExpression.Alias);
                        selectExpression._tables.Add(tableExpression);
                        selectExpression._tableReferences.Add(tableReferenceExpression);

                        for (var j = 0; j < properties.Length; j++)
                        {
                            var property = properties[j];
                            var declaringEntityType = property.DeclaringType.ContainingEntityType;
                            var projection = declaringEntityType.IsAssignableFrom(concreteEntityType)
                                ? CreateColumnExpression(property, table, tableReferenceExpression, declaringEntityType != entityType)
                                : (SqlExpression)sqlExpressionFactory.Constant(
                                    null, property.ClrType.MakeNullable(), property.GetRelationalTypeMapping());
                            selectExpression._projection.Add(new ProjectionExpression(projection, propertyNames[j]));
                        }

                        selectExpression._projection.Add(
                            new ProjectionExpression(
                                sqlExpressionFactory.ApplyDefaultTypeMapping(sqlExpressionFactory.Constant(concreteEntityType.ShortName())),
                                discriminatorColumnName));
                        discriminatorValues.Add(concreteEntityType.ShortName());
                        subSelectExpressions.Add(selectExpression);
                        selectExpression._mutable = false;
                    }

                    // We only assign unique alias to Tpc
                    var tableAlias = GenerateUniqueAlias(_usedAliases, "t");
                    var tpcTables = new TpcTablesExpression(tableAlias, entityType, subSelectExpressions);
                    var tpcTableReference = new TableReferenceExpression(this, tableAlias);
                    _tables.Add(tpcTables);
                    _tableReferences.Add(tpcTableReference);
                    var firstSelectExpression = subSelectExpressions[0];
                    var columns = new Dictionary<IProperty, ColumnExpression>();
                    for (var i = 0; i < properties.Length; i++)
                    {
                        columns[properties[i]] = new ConcreteColumnExpression(firstSelectExpression._projection[i], tpcTableReference);
                    }

                    foreach (var property in entityType.FindPrimaryKey()!.Properties)
                    {
                        var columnExpression = columns[property];
                        _identifier.Add((columnExpression, property.GetKeyValueComparer()));
                    }

                    var discriminatorColumn = new ConcreteColumnExpression(firstSelectExpression._projection[^1], tpcTableReference);
                    _tpcDiscriminatorValues[tpcTables] = (discriminatorColumn, discriminatorValues);
                    var tableMap = tables.ToDictionary(t => t, _ => tpcTableReference);

                    _projectionMapping[new ProjectionMember()] =
                        new StructuralTypeProjectionExpression(entityType, columns, tableMap, nullable: false, discriminatorColumn);
                }
            }

            break;

            default:
            {
                // Also covers TPH
                if (entityType.GetFunctionMappings().SingleOrDefault(e => e.IsDefaultFunctionMapping) is IFunctionMapping functionMapping)
                {
                    var storeFunction = functionMapping.Table;

                    GenerateNonHierarchyNonSplittingEntityType(
                        storeFunction, new TableValuedFunctionExpression((IStoreFunction)storeFunction, Array.Empty<SqlExpression>()));
                }
                else
                {
                    var mappings = entityType.GetViewOrTableMappings().ToList();
                    if (mappings.Count == 1)
                    {
                        var table = mappings[0].Table;

                        GenerateNonHierarchyNonSplittingEntityType(table, new TableExpression(table));
                    }
                    else
                    {
                        // entity splitting
                        var keyProperties = entityType.FindPrimaryKey()!.Properties;
                        List<ColumnExpression> joinColumns = default!;
                        var columns = new Dictionary<IProperty, ColumnExpression>();
                        var tableReferenceExpressionMap = new Dictionary<ITableBase, TableReferenceExpression>();
                        foreach (var mapping in mappings)
                        {
                            var table = mapping.Table;
                            var tableExpression = new TableExpression(table);
                            var tableReferenceExpression = new TableReferenceExpression(this, tableExpression.Alias);
                            tableReferenceExpressionMap[table] = tableReferenceExpression;

                            if (_tables.Count == 0)
                            {
                                AddTable(tableExpression, tableReferenceExpression);
                                joinColumns = new List<ColumnExpression>();
                                foreach (var property in keyProperties)
                                {
                                    var columnExpression = CreateColumnExpression(
                                        property, table, tableReferenceExpression, nullable: false);
                                    columns[property] = columnExpression;
                                    joinColumns.Add(columnExpression);
                                    _identifier.Add((columnExpression, property.GetKeyValueComparer()));
                                }
                            }
                            else
                            {
                                var innerColumns = keyProperties.Select(
                                    p => CreateColumnExpression(p, table, tableReferenceExpression, nullable: false));

                                var joinPredicate = joinColumns
                                    .Zip(innerColumns, sqlExpressionFactory.Equal)
                                    .Aggregate(sqlExpressionFactory.AndAlso);

                                var joinExpression = new InnerJoinExpression(tableExpression, joinPredicate);
                                _removableJoinTables.Add(_tables.Count);
                                AddTable(joinExpression, tableReferenceExpression);
                            }
                        }

                        foreach (var property in entityType.GetProperties())
                        {
                            if (property.IsPrimaryKey())
                            {
                                continue;
                            }

                            var columnBase = mappings.Select(e => e.Table.FindColumn(property)).First(e => e != null)!;
                            columns[property] = CreateColumnExpression(
                                property, columnBase, tableReferenceExpressionMap[columnBase.Table], nullable: false);
                        }

                        _projectionMapping[new ProjectionMember()] =
                            new StructuralTypeProjectionExpression(entityType, columns, tableReferenceExpressionMap);
                    }
                }
            }

            break;
        }

        void GenerateNonHierarchyNonSplittingEntityType(ITableBase table, TableExpressionBase tableExpression)
        {
            var tableReferenceExpression = new TableReferenceExpression(this, tableExpression.Alias!);
            AddTable(tableExpression, tableReferenceExpression);

            var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
            foreach (var property in GetAllPropertiesInHierarchy(entityType))
            {
                propertyExpressions[property] = CreateColumnExpression(property, table, tableReferenceExpression, nullable: false);
            }

            var projection = new StructuralTypeProjectionExpression(
                entityType,
                propertyExpressions,
                new Dictionary<ITableBase, TableReferenceExpression> { [table] = tableReferenceExpression });

            AddJsonNavigationBindings(entityType, projection, propertyExpressions, tableReferenceExpression);
            _projectionMapping[new ProjectionMember()] = projection;

            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey != null)
            {
                foreach (var property in primaryKey.Properties)
                {
                    _identifier.Add((propertyExpressions[property], property.GetKeyValueComparer()));
                }
            }
        }

        static ITableBase GetTableBaseFiltered(IEntityType entityType, Dictionary<ITableBase, TableReferenceExpression> existingTables)
            => entityType.GetViewOrTableMappings().Single(m => !existingTables.ContainsKey(m.Table)).Table;
    }

    internal SelectExpression(IEntityType entityType, TableExpressionBase tableExpressionBase)
        : base(null)
    {
        if ((entityType.BaseType != null || entityType.GetDirectlyDerivedTypes().Any())
            && entityType.FindDiscriminatorProperty() == null)
        {
            throw new InvalidOperationException(RelationalStrings.SelectExpressionNonTphWithCustomTable(entityType.DisplayName()));
        }

        var table = (tableExpressionBase as ITableBasedExpression)?.Table;
        Check.DebugAssert(table is not null, "SelectExpression with unexpected missing table");

        var tableReferenceExpression = new TableReferenceExpression(this, tableExpressionBase.Alias!);
        var tableMap = new Dictionary<ITableBase, TableReferenceExpression> { [table] = tableReferenceExpression };
        AddTable(tableExpressionBase, tableReferenceExpression);

        var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();

        foreach (var property in GetAllPropertiesInHierarchy(entityType))
        {
            propertyExpressions[property] = CreateColumnExpression(property, table, tableReferenceExpression, nullable: false);
        }

        var projection = new StructuralTypeProjectionExpression(entityType, propertyExpressions, tableMap);
        AddJsonNavigationBindings(entityType, projection, propertyExpressions, tableReferenceExpression);
        _projectionMapping[new ProjectionMember()] = projection;

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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public SelectExpression(
        JsonQueryExpression jsonQueryExpression,
        TableExpressionBase tableExpressionBase,
        string identifierColumnName,
        Type identifierColumnType,
        RelationalTypeMapping identifierColumnTypeMapping)
        : base(null)
    {
        if (!jsonQueryExpression.IsCollection)
        {
            throw new ArgumentException(RelationalStrings.SelectCanOnlyBeBuiltOnCollectionJsonQuery, nameof(jsonQueryExpression));
        }

        var entityType = jsonQueryExpression.EntityType;

        Check.DebugAssert(
            entityType.BaseType is null && !entityType.GetDirectlyDerivedTypes().Any(),
            "Inheritance encountered inside a JSON document");

        var tableReferenceExpression = new TableReferenceExpression(this, tableExpressionBase.Alias!);
        AddTable(tableExpressionBase, tableReferenceExpression);

        // Create a dictionary mapping all properties to their ColumnExpressions, for the SelectExpression's projection.
        var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();

        foreach (var property in GetAllPropertiesInHierarchy(entityType))
        {
            // also adding column(s) representing key of the parent (non-JSON) entity, on top of all the projections from OPENJSON/json_each/etc.
            if (jsonQueryExpression.KeyPropertyMap.TryGetValue(property, out var ownerKeyColumn))
            {
                propertyExpressions[property] = ownerKeyColumn;
                continue;
            }

            // Skip also properties with no JSON name (i.e. shadow keys containing the index in the collection, which don't actually exist
            // in the JSON document and can't be bound to)
            if (property.GetJsonPropertyName() is string jsonPropertyName)
            {
                propertyExpressions[property] = CreateColumnExpression(
                    tableExpressionBase, jsonPropertyName, property.ClrType, property.GetRelationalTypeMapping(),
                    /*jsonQueryExpression.IsNullable || */property.IsNullable);
            }
        }

        var table = entityType.GetViewOrTableMappings().SingleOrDefault()?.Table ?? entityType.GetDefaultMappings().Single().Table;

        // TODO: We'll need to make sure this is correct when we add support for JSON complex types.
        var tableMap = new Dictionary<ITableBase, TableReferenceExpression> { [table] = tableReferenceExpression };

        var projection = new StructuralTypeProjectionExpression(
            entityType,
            propertyExpressions,
            tableMap);

        var containerColumnName = entityType.GetContainerColumnName()!;
        var containerColumn = table.FindColumn(containerColumnName)!;
        var containerColumnTypeMapping = containerColumn.StoreTypeMapping;
        foreach (var ownedJsonNavigation in GetAllNavigationsInHierarchy(entityType)
                     .Where(
                         n => n.ForeignKey.IsOwnership
                             && n.TargetEntityType.IsMappedToJson()
                             && n.ForeignKey.PrincipalToDependent == n))
        {
            var targetEntityType = ownedJsonNavigation.TargetEntityType;
            var jsonNavigationName = ownedJsonNavigation.TargetEntityType.GetJsonPropertyName();
            Check.DebugAssert(jsonNavigationName is not null, "Invalid navigation found on JSON-mapped entity");
            var isNullable = containerColumn.IsNullable
                || !ownedJsonNavigation.ForeignKey.IsRequiredDependent
                || ownedJsonNavigation.IsCollection;

            // The TableExpressionBase represents a relational expansion of the JSON collection. We now need a ColumnExpression to represent
            // the specific JSON property (projected as a relational column) which holds the JSON subtree for the target entity.
            var column = new ConcreteColumnExpression(
                jsonNavigationName,
                tableReferenceExpression,
                containerColumnTypeMapping.ClrType,
                containerColumnTypeMapping,
                isNullable);

            // need to remap key property map to use target entity key properties
            var newKeyPropertyMap = new Dictionary<IProperty, ColumnExpression>();
            var targetPrimaryKeyProperties = targetEntityType.FindPrimaryKey()!.Properties.Take(jsonQueryExpression.KeyPropertyMap.Count);
            var sourcePrimaryKeyProperties = jsonQueryExpression.EntityType.FindPrimaryKey()!.Properties.Take(jsonQueryExpression.KeyPropertyMap.Count);
            foreach (var (target, source) in targetPrimaryKeyProperties.Zip(sourcePrimaryKeyProperties, (t, s) => (t, s)))
            {
                newKeyPropertyMap[target] = jsonQueryExpression.KeyPropertyMap[source];
            }

            var entityShaperExpression = new RelationalStructuralTypeShaperExpression(
                targetEntityType,
                new JsonQueryExpression(
                    targetEntityType,
                    column,
                    newKeyPropertyMap,
                    ownedJsonNavigation.ClrType,
                    ownedJsonNavigation.IsCollection),
                isNullable);

            projection.AddNavigationBinding(ownedJsonNavigation, entityShaperExpression);
        }

        _projectionMapping[new ProjectionMember()] = projection;

        var identifierColumn = new ConcreteColumnExpression(
            identifierColumnName,
            tableReferenceExpression,
            identifierColumnType.UnwrapNullableType(),
            identifierColumnTypeMapping,
            identifierColumnType.IsNullableType());

        _identifier.Add((identifierColumn, identifierColumnTypeMapping!.Comparer));
    }

    private void AddJsonNavigationBindings(
        IEntityType entityType,
        StructuralTypeProjectionExpression projection,
        Dictionary<IProperty, ColumnExpression> propertyExpressions,
        TableReferenceExpression tableReferenceExpression)
    {
        foreach (var ownedJsonNavigation in GetAllNavigationsInHierarchy(entityType)
                     .Where(
                         n => n.ForeignKey.IsOwnership
                             && n.TargetEntityType.IsMappedToJson()
                             && n.ForeignKey.PrincipalToDependent == n))
        {
            var targetEntityType = ownedJsonNavigation.TargetEntityType;
            var containerColumnName = targetEntityType.GetContainerColumnName()!;
            var containerColumn = (entityType.GetViewOrTableMappings().SingleOrDefault()?.Table
                    ?? entityType.GetDefaultMappings().Single().Table)
                .FindColumn(containerColumnName)!;
            var containerColumnTypeMapping = containerColumn.StoreTypeMapping;
            var isNullable = containerColumn.IsNullable
                || !ownedJsonNavigation.ForeignKey.IsRequiredDependent
                || ownedJsonNavigation.IsCollection;

            var column = new ConcreteColumnExpression(
                containerColumnName,
                tableReferenceExpression,
                containerColumnTypeMapping.ClrType,
                containerColumnTypeMapping,
                isNullable);

            // for json collections we need to skip ordinal key (which is always the last one)
            // simple copy from parent is safe here, because we only do it at top level
            // so there is no danger of multiple keys being synthesized (like we have in multi-level nav chains)
            var keyPropertiesMap = new Dictionary<IProperty, ColumnExpression>();
            var keyProperties = targetEntityType.FindPrimaryKey()!.Properties;
            var keyPropertiesCount = ownedJsonNavigation.IsCollection
                ? keyProperties.Count - 1
                : keyProperties.Count;

            for (var i = 0; i < keyPropertiesCount; i++)
            {
                var correspondingParentKeyProperty = ownedJsonNavigation.ForeignKey.PrincipalKey.Properties[i];
                keyPropertiesMap[keyProperties[i]] = propertyExpressions[correspondingParentKeyProperty];
            }

            var entityShaperExpression = new RelationalStructuralTypeShaperExpression(
                targetEntityType,
                new JsonQueryExpression(
                    targetEntityType,
                    column,
                    keyPropertiesMap,
                    ownedJsonNavigation.ClrType,
                    ownedJsonNavigation.IsCollection),
                isNullable);

            projection.AddNavigationBinding(ownedJsonNavigation, entityShaperExpression);
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
            && _clientProjections.Any(e => e is ShapedQueryExpression { ResultCardinality: ResultCardinality.Enumerable }))
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
            var typeProjectionIdentifiers = new List<ColumnExpression>();
            var typeProjectionValueComparers = new List<ValueComparer>();
            var otherExpressions = new List<SqlExpression>();
            var nonProcessableExpressionFound = false;

            var projections = _clientProjections.Count > 0 ? _clientProjections : _projectionMapping.Values.ToList();
            foreach (var projection in projections)
            {
                switch (projection)
                {
                    case StructuralTypeProjectionExpression { StructuralType: IEntityType entityType } entityProjection
                        when entityType.IsMappedToJson():
                    {
                        // For JSON entities, the identifier is the key that was generated when we convert from json to query root
                        // (OPENJSON, json_each, etc), but we can't use it for distinct, as it would warp the results.
                        // Instead, we will treat every non-key property as identifier.

                        foreach (var property in entityType.GetDeclaredProperties().Where(p => !p.IsPrimaryKey()))
                        {
                            typeProjectionIdentifiers.Add(entityProjection.BindProperty(property));
                            typeProjectionValueComparers.Add(property.GetKeyValueComparer());
                        }

                        break;
                    }

                    case StructuralTypeProjectionExpression { StructuralType: IEntityType entityType } entityProjection
                        when !entityType.IsMappedToJson():
                    {
                        var primaryKey = entityType.FindPrimaryKey();
                        // We know that there are existing identifiers (see condition above); we know we must have a key since a keyless
                        // entity type would have wiped the identifiers when generating the join.
                        Check.DebugAssert(primaryKey != null, "primary key is null.");

                        foreach (var property in primaryKey.Properties)
                        {
                            typeProjectionIdentifiers.Add(entityProjection.BindProperty(property));
                            typeProjectionValueComparers.Add(property.GetKeyValueComparer());
                        }

                        break;
                    }

                    case StructuralTypeProjectionExpression { StructuralType: IComplexType } complexTypeProjection:
                        // When distinct is applied to complex types, all properties - including ones in nested complex types - become
                        // the identifier.
                        ProcessComplexType(complexTypeProjection);

                        void ProcessComplexType(StructuralTypeProjectionExpression complexTypeProjection)
                        {
                            var complexType = (IComplexType)complexTypeProjection.StructuralType;

                            foreach (var property in complexType.GetProperties())
                            {
                                typeProjectionIdentifiers.Add(complexTypeProjection.BindProperty(property));
                                typeProjectionValueComparers.Add(property.GetKeyValueComparer());
                            }

                            foreach (var complexProperty in complexType.GetComplexProperties())
                            {
                                ProcessComplexType(
                                    (StructuralTypeProjectionExpression)complexTypeProjection.BindComplexProperty(complexProperty)
                                        .ValueBufferExpression);
                            }
                        }

                        break;

                    case JsonQueryExpression jsonQueryExpression:
                        if (jsonQueryExpression.IsCollection)
                        {
                            throw new InvalidOperationException(RelationalStrings.DistinctOnCollectionNotSupported);
                        }

                        var primaryKeyProperties = jsonQueryExpression.EntityType.FindPrimaryKey()!.Properties;
                        var primaryKeyPropertiesCount = jsonQueryExpression.IsCollection
                            ? primaryKeyProperties.Count - 1
                            : primaryKeyProperties.Count;

                        for (var i = 0; i < primaryKeyPropertiesCount; i++)
                        {
                            var keyProperty = primaryKeyProperties[i];
                            typeProjectionIdentifiers.Add((ColumnExpression)jsonQueryExpression.BindProperty(keyProperty));
                            typeProjectionValueComparers.Add(keyProperty.GetKeyValueComparer());
                        }

                        break;

                    case SqlExpression sqlExpression:
                        otherExpressions.Add(sqlExpression);
                        break;

                    default:
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
                var allOtherExpressions = typeProjectionIdentifiers.Concat(otherExpressions).ToList();
                if (!_identifier.All(e => allOtherExpressions.Contains(e.Column)))
                {
                    _identifier.Clear();
                    if (otherExpressions.Count == 0)
                    {
                        // If there are no other expressions then we can use all entityProjectionIdentifiers
                        _identifier.AddRange(typeProjectionIdentifiers.Zip(typeProjectionValueComparers));
                    }
                    else if (otherExpressions.All(e => e is ColumnExpression))
                    {
                        _identifier.AddRange(typeProjectionIdentifiers.Zip(typeProjectionValueComparers));
                        _identifier.AddRange(otherExpressions.Select(e => ((ColumnExpression)e, e.TypeMapping!.KeyComparer)));
                    }
                }
            }
        }

        ClearOrdering();
    }

    /// <summary>
    ///     Adds expressions from projection mapping to projection ignoring the shaper expression. This method should only be used
    ///     when populating projection in subquery.
    /// </summary>
    public void ApplyProjection()
    {
        if (!_mutable)
        {
            throw new InvalidOperationException("Applying projection on already finalized select expression");
        }

        _mutable = false;
        if (_clientProjections.Count > 0)
        {
            for (var i = 0; i < _clientProjections.Count; i++)
            {
                switch (_clientProjections[i])
                {
                    case StructuralTypeProjectionExpression projection:
                        AddStructuralTypeProjection(projection);
                        break;

                    case SqlExpression sqlExpression:
                        AddToProjection(sqlExpression, _aliasForClientProjections[i]);
                        break;

                    default:
                        throw new InvalidOperationException(
                            "Invalid type of projection to add when not associated with shaper expression.");
                }
            }

            _clientProjections.Clear();
        }
        else
        {
            foreach (var (_, expression) in _projectionMapping)
            {
                if (expression is StructuralTypeProjectionExpression projection)
                {
                    AddStructuralTypeProjection(projection);
                }
                else
                {
                    AddToProjection((SqlExpression)expression);
                }
            }

            _projectionMapping.Clear();
        }

        void AddStructuralTypeProjection(StructuralTypeProjectionExpression projection)
        {
            if (_projection.Count == 0
                && projection is { StructuralType: IComplexType complexType, IsNullable: true })
            {
                throw new InvalidOperationException(RelationalStrings.CannotProjectNullableComplexType(complexType.DisplayName()));
            }

            ProcessTypeProjection(projection);

            void ProcessTypeProjection(StructuralTypeProjectionExpression projection)
            {
                foreach (var property in GetAllPropertiesInHierarchy(projection.StructuralType))
                {
                    AddToProjection(projection.BindProperty(property), alias: null);
                }

                foreach (var complexProperty in GetAllComplexPropertiesInHierarchy(projection.StructuralType))
                {
                    ProcessTypeProjection(
                        (StructuralTypeProjectionExpression)projection.BindComplexProperty(complexProperty).ValueBufferExpression);
                }
            }

            if (projection.DiscriminatorExpression != null)
            {
                AddToProjection(projection.DiscriminatorExpression, DiscriminatorColumnAlias);
            }
        }
    }

    /// <summary>
    ///     Adds expressions from projection mapping to projection and generate updated shaper expression for materialization.
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
        if (!_mutable)
        {
            throw new InvalidOperationException("Applying projection on already finalized select expression");
        }

        _mutable = false;
        if (shaperExpression is RelationalGroupByShaperExpression relationalGroupByShaperExpression)
        {
            // This is final GroupBy operation
            Check.DebugAssert(_groupBy.Count > 0, "The selectExpression doesn't have grouping terms.");

            if (_clientProjections.Count == 0)
            {
                // Force client projection because we would be injecting keys and client-side key comparison
                var mapping = ConvertProjectionMappingToClientProjections(_projectionMapping);
                var innerShaperExpression = new ProjectionMemberToIndexConvertingExpressionVisitor(this, mapping).Visit(
                    relationalGroupByShaperExpression.ElementSelector);
                shaperExpression = new RelationalGroupByShaperExpression(
                    relationalGroupByShaperExpression.KeySelector,
                    innerShaperExpression,
                    relationalGroupByShaperExpression.GroupingEnumerable);
            }

            // Convert GroupBy to OrderBy
            foreach (var groupingTerm in _groupBy)
            {
                AppendOrdering(new OrderingExpression(groupingTerm, ascending: true));
            }
            _groupBy.Clear();
            // We do processing of adding key terms to projection when applying projection so we can move offsets for other
            // projections correctly
        }

        if (_clientProjections.Count > 0)
        {
            EntityShaperNullableMarkingExpressionVisitor? entityShaperNullableMarkingExpressionVisitor = null;
            CloningExpressionVisitor? cloningExpressionVisitor = null;
            var pushdownOccurred = false;
            var containsCollection = false;
            var containsSingleResult = false;
            var jsonClientProjectionsCount = 0;

            foreach (var projection in _clientProjections)
            {
                if (projection is ShapedQueryExpression sqe)
                {
                    if (sqe.ResultCardinality == ResultCardinality.Enumerable)
                    {
                        containsCollection = true;
                    }

                    if (sqe.ResultCardinality is ResultCardinality.Single or ResultCardinality.SingleOrDefault)
                    {
                        containsSingleResult = true;
                    }
                }

                if (projection is JsonQueryExpression)
                {
                    jsonClientProjectionsCount++;
                }
            }

            if (containsSingleResult
                || (querySplittingBehavior == QuerySplittingBehavior.SingleQuery && containsCollection))
            {
                // Pushdown outer since we will be adding join to this
                // For grouping query pushdown will not occur since we don't allow this terms to compose (yet!).
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

            if (querySplittingBehavior == QuerySplittingBehavior.SplitQuery
                && (containsSingleResult || containsCollection))
            {
                // SingleResult can lift collection from inner
                cloningExpressionVisitor = new CloningExpressionVisitor();
            }

            var earlierClientProjectionCount = _clientProjections.Count;
            var newClientProjections = new List<Expression>();
            var clientProjectionIndexMap = new List<object>();
            var remappingRequired = false;

            if (shaperExpression is RelationalGroupByShaperExpression groupByShaper)
            {
                // We need to add key to projection and generate key selector in terms of projectionBindings
                var projectionBindingMap = new Dictionary<SqlExpression, Expression>();
                var keySelector = AddGroupByKeySelectorToProjection(
                    this, newClientProjections, projectionBindingMap, groupByShaper.KeySelector);
                var (keyIdentifier, keyIdentifierValueComparers) = GetIdentifierAccessor(
                    this, newClientProjections, projectionBindingMap, _identifier);
                _identifier.Clear();
                _identifier.AddRange(_preGroupByIdentifier!);
                _preGroupByIdentifier!.Clear();

                Expression AddGroupByKeySelectorToProjection(
                    SelectExpression selectExpression,
                    List<Expression> clientProjectionList,
                    Dictionary<SqlExpression, Expression> projectionBindingMap,
                    Expression keySelector)
                {
                    switch (keySelector)
                    {
                        case SqlExpression sqlExpression:
                        {
                            var index = selectExpression.AddToProjection(sqlExpression);
                            var clientProjectionToAdd = Constant(index);
                            var existingIndex = clientProjectionList.FindIndex(
                                e => ExpressionEqualityComparer.Instance.Equals(e, clientProjectionToAdd));
                            if (existingIndex == -1)
                            {
                                clientProjectionList.Add(clientProjectionToAdd);
                                existingIndex = clientProjectionList.Count - 1;
                            }

                            var projectionBindingExpression = sqlExpression.Type.IsNullableType()
                                ? (Expression)new ProjectionBindingExpression(selectExpression, existingIndex, sqlExpression.Type)
                                : Convert(new ProjectionBindingExpression(
                                    selectExpression, existingIndex, sqlExpression.Type.MakeNullable()),
                                    sqlExpression.Type);
                            projectionBindingMap[sqlExpression] = projectionBindingExpression;
                            return projectionBindingExpression;
                        }

                        case NewExpression newExpression:
                            var newArguments = new Expression[newExpression.Arguments.Count];
                            for (var i = 0; i < newExpression.Arguments.Count; i++)
                            {
                                var newArgument = AddGroupByKeySelectorToProjection(
                                    selectExpression, clientProjectionList, projectionBindingMap, newExpression.Arguments[i]);
                                newArguments[i] = newExpression.Arguments[i].Type != newArgument.Type
                                    ? Convert(newArgument, newExpression.Arguments[i].Type)
                                    : newArgument;
                            }

                            return newExpression.Update(newArguments);

                        case MemberInitExpression memberInitExpression:
                            var updatedNewExpression = AddGroupByKeySelectorToProjection(
                                selectExpression, clientProjectionList, projectionBindingMap, memberInitExpression.NewExpression);
                            var newBindings = new MemberBinding[memberInitExpression.Bindings.Count];
                            for (var i = 0; i < newBindings.Length; i++)
                            {
                                var memberAssignment = (MemberAssignment)memberInitExpression.Bindings[i];
                                var newAssignmentExpression = AddGroupByKeySelectorToProjection(
                                    selectExpression, clientProjectionList, projectionBindingMap, memberAssignment.Expression);
                                newBindings[i] = memberAssignment.Update(
                                    memberAssignment.Expression.Type != newAssignmentExpression.Type
                                    ? Convert(newAssignmentExpression, memberAssignment.Expression.Type)
                                    : newAssignmentExpression);
                            }

                            return memberInitExpression.Update((NewExpression)updatedNewExpression, newBindings);

                        case UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression:
                            return unaryExpression.Update(
                                AddGroupByKeySelectorToProjection(
                                    selectExpression, clientProjectionList, projectionBindingMap, unaryExpression.Operand));

                        case StructuralTypeShaperExpression
                        {
                            ValueBufferExpression: StructuralTypeProjectionExpression projection
                        } shaper:
                        {
                            var clientProjectionToAdd = AddStructuralTypeProjection(projection);
                            var existingIndex = clientProjectionList.FindIndex(
                                e => ExpressionEqualityComparer.Instance.Equals(e, clientProjectionToAdd));
                            if (existingIndex == -1)
                            {
                                clientProjectionList.Add(clientProjectionToAdd);
                                existingIndex = clientProjectionList.Count - 1;
                            }

                            return shaper.Update(
                                new ProjectionBindingExpression(selectExpression, existingIndex, typeof(ValueBuffer)));
                        }

                        default:
                            throw new InvalidOperationException(
                                RelationalStrings.InvalidKeySelectorForGroupBy(keySelector, keySelector.GetType()));
                    }
                }

                static (Expression, IReadOnlyList<ValueComparer>) GetIdentifierAccessor(
                    SelectExpression selectExpression,
                    List<Expression> clientProjectionList,
                    Dictionary<SqlExpression, Expression> projectionBindingMap,
                    IEnumerable<(ColumnExpression Column, ValueComparer Comparer)> identifyingProjection)
                {
                    var updatedExpressions = new List<Expression>();
                    var comparers = new List<ValueComparer>();
                    foreach (var (column, comparer) in identifyingProjection)
                    {
                        if (!projectionBindingMap.TryGetValue(column, out var mappedExpression))
                        {
                            var index = selectExpression.AddToProjection(column);
                            var clientProjectionToAdd = Constant(index);
                            var existingIndex = clientProjectionList.FindIndex(
                                e => ExpressionEqualityComparer.Instance.Equals(e, clientProjectionToAdd));
                            if (existingIndex == -1)
                            {
                                clientProjectionList.Add(clientProjectionToAdd);
                                existingIndex = clientProjectionList.Count - 1;
                            }

                            mappedExpression = new ProjectionBindingExpression(selectExpression, existingIndex, column.Type.MakeNullable());
                        }

                        updatedExpressions.Add(
                            mappedExpression.Type.IsValueType
                                ? Convert(mappedExpression, typeof(object))
                                : mappedExpression);
                        comparers.Add(comparer);
                    }

                    return (NewArrayInit(typeof(object), updatedExpressions), comparers);
                }
                remappingRequired = true;
                shaperExpression = new RelationalGroupByResultExpression(
                    keyIdentifier, keyIdentifierValueComparers, keySelector, groupByShaper.ElementSelector);
            }

            SelectExpression? baseSelectExpression = null;
            if (querySplittingBehavior == QuerySplittingBehavior.SplitQuery && containsCollection)
            {
                // Needs to happen after converting final GroupBy so we clone correct form.
                baseSelectExpression = (SelectExpression)cloningExpressionVisitor!.Visit(this);
                // We mark this as mutable because the split query will combine into this and take it over.
                baseSelectExpression._mutable = true;
                if (resultCardinality is ResultCardinality.Single or ResultCardinality.SingleOrDefault)
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
                        if (selectExpression.Limit is SqlConstantExpression { Value: 2 } limitConstantExpression)
                        {
                            selectExpression.Limit = new SqlConstantExpression(Constant(1), limitConstantExpression.TypeMapping);
                        }
                    }
                }
            }

            for (var i = 0; i < _clientProjections.Count; i++)
            {
                if (i == earlierClientProjectionCount)
                {
                    // Since we lift nested client projections for single results up, we may need to re-clone the baseSelectExpression
                    // again so it does contain the single result subquery too. We erase projections for it since it would be non-empty.
                    earlierClientProjectionCount = _clientProjections.Count;
                    if (cloningExpressionVisitor != null)
                    {
                        baseSelectExpression = (SelectExpression)cloningExpressionVisitor.Visit(this);
                        baseSelectExpression._mutable = true;
                        baseSelectExpression._projection.Clear();
                    }
                }

                var value = _clientProjections[i];
                switch (value)
                {
                    case StructuralTypeProjectionExpression projection:
                    {
                        var result = AddStructuralTypeProjection(projection);
                        newClientProjections.Add(result);
                        clientProjectionIndexMap.Add(newClientProjections.Count - 1);

                        break;
                    }

                    case JsonQueryExpression jsonQueryExpression:
                    {
                        var jsonProjectionResult = AddJsonProjection(jsonQueryExpression);
                        newClientProjections.Add(jsonProjectionResult);
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

                    case ShapedQueryExpression
                    {
                        ResultCardinality: ResultCardinality.Single or ResultCardinality.SingleOrDefault
                    } shapedQueryExpression:
                    {
                        var innerSelectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
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
                        if (innerExpression is not (StructuralTypeShaperExpression or IncludeExpression))
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
                            => expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
                                ? RemoveConvert(unaryExpression.Operand)
                                : expression;
                    }

                    case ShapedQueryExpression { ResultCardinality: ResultCardinality.Enumerable } shapedQueryExpression:
                    {
                        var innerSelectExpression = (SelectExpression)shapedQueryExpression.QueryExpression;
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
                                (SelectExpression)new ColumnExpressionReplacingExpressionVisitor(
                                        this, outerSelectExpression._tableReferences)
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
                // If operation was converted to predicate join (inner/left join), then ordering will be in a RowNumberExpression
                if (tableExpressionBase is SelectExpression
                    {
                        Tables: [SelectExpression rowNumberSubquery],
                        Predicate: not null
                    } joinedSubquery
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
                else if (tableExpressionBase is SelectExpression { Orderings.Count: > 0 } collectionSelectExpression)
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
                    else if (constantValue is JsonProjectionInfo jsonProjectionInfo)
                    {
                        var newKeyAccessInfo = new List<(IProperty?, int?, int?)>();
                        foreach (var (keyProperty, constantKeyValue, keyProjectionIndex) in jsonProjectionInfo.KeyAccessInfo)
                        {
                            newKeyAccessInfo.Add((keyProperty, constantKeyValue, keyProjectionIndex != null ? projectionIndexMap[keyProjectionIndex.Value] : null));
                        }

                        remappedConstant = Constant(
                            new JsonProjectionInfo(
                                projectionIndexMap[jsonProjectionInfo.JsonColumnIndex],
                                newKeyAccessInfo));
                    }
                    else if (constantValue is QueryableJsonProjectionInfo queryableJsonProjectionInfo)
                    {
                        var newPropertyIndexMap = new Dictionary<IProperty, int>(queryableJsonProjectionInfo.PropertyIndexMap.Count);
                        foreach (var (property, value) in queryableJsonProjectionInfo.PropertyIndexMap)
                        {
                            newPropertyIndexMap[property] = projectionIndexMap[value];
                        }

                        var newChildrenProjectionInfo = new List<(JsonProjectionInfo, INavigation)>();
                        foreach (var childProjectionInfo in queryableJsonProjectionInfo.ChildrenProjectionInfo)
                        {
                            var newKeyAccessInfo = new List<(IProperty?, int?, int?)>();
                            foreach (var (keyProperty, constantKeyValue, keyProjectionIndex) in childProjectionInfo.JsonProjectionInfo.KeyAccessInfo)
                            {
                                newKeyAccessInfo.Add((keyProperty, constantKeyValue, keyProjectionIndex != null ? projectionIndexMap[keyProjectionIndex.Value] : null));
                            }

                            newChildrenProjectionInfo.Add(
                                (new JsonProjectionInfo(
                                    projectionIndexMap[childProjectionInfo.JsonProjectionInfo.JsonColumnIndex],
                                    newKeyAccessInfo),
                                childProjectionInfo.Navigation));
                        }

                        remappedConstant = Constant(
                            new QueryableJsonProjectionInfo(newPropertyIndexMap, newChildrenProjectionInfo));
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
        else
        {
            var result = new Dictionary<ProjectionMember, Expression>(_projectionMapping.Count);

            foreach (var (projectionMember, expression) in _projectionMapping)
            {
                result[projectionMember] = expression switch
                {
                    StructuralTypeProjectionExpression projection => AddStructuralTypeProjection(projection),
                    JsonQueryExpression jsonQueryExpression => AddJsonProjection(jsonQueryExpression),
                    _ => Constant(AddToProjection((SqlExpression)expression, projectionMember.Last?.Name))
                };
            }

            _projectionMapping.Clear();
            _projectionMapping = result;

            return shaperExpression;
        }

        ConstantExpression AddStructuralTypeProjection(StructuralTypeProjectionExpression projection)
        {
            if (projection is { StructuralType: IComplexType complexType, IsNullable: true })
            {
                throw new InvalidOperationException(RelationalStrings.CannotProjectNullableComplexType(complexType.DisplayName()));
            }

            // JSON entity that had some query operations applied on it - it has been converted to a query root via OPENJSON/json_each
            // so it requires different materialization path than regular entity
            // e.g. we need to also add all the child navigations, JSON entity builds all the includes as part of it's own materializer
            // rather than relying on IncludeExpressions in the shaper query
            // also, we don't want to add projection map for synthesized keys, whereas regular entity needs to project every single property it has
            if (projection is { StructuralType: IEntityType entityType }
                && entityType.IsMappedToJson())
            {
                var propertyIndexMap = new Dictionary<IProperty, int>();
                var ownerEntity = entityType;

                do
                {
                    var ownership = ownerEntity.FindOwnership();
                    if (ownership != null)
                    {
                        ownerEntity = ownership.PrincipalEntityType;
                    }
                }
                while (ownerEntity.IsMappedToJson());

                var keyPropertyCount = ownerEntity.FindPrimaryKey()!.Properties.Count;
                foreach (var property in entityType.FindPrimaryKey()!.Properties.Take(keyPropertyCount)
                    .Concat(entityType.GetDeclaredProperties().Where(p => p.GetJsonPropertyName() is not null)))
                {
                    propertyIndexMap[property] = AddToProjection(projection.BindProperty(property), null);
                }

                var childrenProjectionInfo = new List<(JsonProjectionInfo, INavigation)>();
                foreach (var ownedNavigation in entityType.GetNavigations().Where(
                    n => n.TargetEntityType.IsMappedToJson() && n.ForeignKey.IsOwnership && n == n.ForeignKey.PrincipalToDependent))
                {
                    var jsonQueryExpression = (JsonQueryExpression)projection.BindNavigation(ownedNavigation)!.ValueBufferExpression;
                    var jsonProjectionInfo = (JsonProjectionInfo)AddJsonProjection(jsonQueryExpression).Value!;
                    childrenProjectionInfo.Add((jsonProjectionInfo, ownedNavigation));
                }

                return Constant(new QueryableJsonProjectionInfo(propertyIndexMap, childrenProjectionInfo));
            }
            else
            {
                var projections = new Dictionary<IProperty, int>();

                ProcessType(projection);

                void ProcessType(StructuralTypeProjectionExpression typeProjection)
                {
                    foreach (var property in GetAllPropertiesInHierarchy(typeProjection.StructuralType))
                    {
                        if (typeProjection is { StructuralType: IEntityType entityType }
                            && entityType.IsMappedToJson()
                            && property.IsOrdinalKeyProperty())
                        {
                            continue;
                        }

                        projections[property] = AddToProjection(typeProjection.BindProperty(property), alias: null);
                    }

                    foreach (var complexProperty in GetAllComplexPropertiesInHierarchy(typeProjection.StructuralType))
                    {
                        ProcessType((StructuralTypeProjectionExpression)typeProjection.BindComplexProperty(complexProperty).ValueBufferExpression);
                    }
                }

                if (projection.DiscriminatorExpression is not null)
                {
                    AddToProjection(projection.DiscriminatorExpression, DiscriminatorColumnAlias);
                }

                return Constant(projections);
            }
        }

        ConstantExpression AddJsonProjection(JsonQueryExpression jsonQueryExpression)
        {
            var jsonScalarExpression = new JsonScalarExpression(
                jsonQueryExpression.JsonColumn,
                jsonQueryExpression.Path,
                jsonQueryExpression.JsonColumn.Type,
                jsonQueryExpression.JsonColumn.TypeMapping!,
                jsonQueryExpression.IsNullable);

            var sqlExpression = AssignUniqueAliases(jsonScalarExpression);
            _projection.Add(new ProjectionExpression(sqlExpression, ""));
            var jsonColumnIndex = _projection.Count - 1;
            var keyAccessInfo = new List<(IProperty?, int?, int?)>();
            var keyProperties = GetMappedKeyProperties(jsonQueryExpression.EntityType.FindPrimaryKey()!);
            foreach (var keyProperty in keyProperties)
            {
                var keyColumn = jsonQueryExpression.BindProperty(keyProperty);
                keyAccessInfo.Add((keyProperty, null, AddToProjection(keyColumn)));
            }

            foreach (var elementAccessSegment in jsonScalarExpression.Path.Where(x => x.ArrayIndex != null))
            {
                if (elementAccessSegment.ArrayIndex is SqlConstantExpression { Value: int intValue })
                {
                    keyAccessInfo.Add((null, intValue, null));
                }
                else
                {
                    keyAccessInfo.Add((null, null, AddToProjection(elementAccessSegment.ArrayIndex!)));
                }
            }

            return Constant(
                new JsonProjectionInfo(
                    jsonColumnIndex,
                    keyAccessInfo));
        }

        static IReadOnlyList<IProperty> GetMappedKeyProperties(IKey key)
        {
            if (!key.DeclaringEntityType.IsMappedToJson())
            {
                return key.Properties;
            }

            // TODO: fix this once we enable json entity being owned by another owned non-json entity (issue #28441)

            // for json collections we need to filter out the ordinal key as it's not mapped to any column
            // there could be multiple of these in deeply nested structures,
            // so we traverse to the outermost owner to see how many mapped keys there are
            var currentEntity = key.DeclaringEntityType;
            while (currentEntity.IsMappedToJson())
            {
                currentEntity = currentEntity.FindOwnership()!.PrincipalEntityType;
            }

            var count = currentEntity.FindPrimaryKey()!.Properties.Count;

            return key.Properties.Take(count).ToList();
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
                expression is SqlExpression or StructuralTypeProjectionExpression or JsonQueryExpression,
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
                expression is SqlExpression or StructuralTypeProjectionExpression or ShapedQueryExpression or JsonQueryExpression,
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
        if (sqlExpression is SqlConstantExpression { Value: true })
        {
            return;
        }

        if (Limit != null
            || Offset != null)
        {
            sqlExpression = PushdownIntoSubqueryInternal().Remap(sqlExpression);
        }

        if (sqlExpression is SqlBinaryExpression { OperatorType: ExpressionType.Equal } or InExpression { Subquery: null }
            && _groupBy.Count == 0)
        {
            // If the intersection is empty then we don't remove predicate so that the filter empty out all results.
            if (sqlExpression is SqlBinaryExpression sqlBinaryExpression)
            {
                if (sqlBinaryExpression.Left is ColumnExpression { Table: TpcTablesExpression leftTpc } leftColumn
                    && _tpcDiscriminatorValues.TryGetValue(leftTpc, out var leftTuple)
                    && leftTuple.Item1.Equals(leftColumn)
                    && sqlBinaryExpression.Right is SqlConstantExpression { Value: string s1 })
                {
                    var newList = leftTuple.Item2.Intersect(new List<string> { s1 }).ToList();
                    if (newList.Count > 0)
                    {
                        _tpcDiscriminatorValues[leftTpc] = (leftColumn, newList);
                        return;
                    }
                }
                else if (sqlBinaryExpression.Right is ColumnExpression { Table: TpcTablesExpression rightTpc } rightColumn
                         && _tpcDiscriminatorValues.TryGetValue(rightTpc, out var rightTuple)
                         && rightTuple.Item1.Equals(rightColumn)
                         && sqlBinaryExpression.Left is SqlConstantExpression { Value: string s2 })
                {
                    var newList = rightTuple.Item2.Intersect(new List<string> { s2 }).ToList();
                    if (newList.Count > 0)
                    {
                        _tpcDiscriminatorValues[rightTpc] = (rightColumn, newList);
                        return;
                    }
                }
            }
            // Identify application of a predicate which narrows the discriminator (e.g. OfType) for TPC, apply it to
            // _tpcDiscriminatorValues (which will be handled later) instead of as a WHERE predicate.
            else if (sqlExpression is InExpression
                     {
                         Item: ColumnExpression { Table: TpcTablesExpression itemTpc } itemColumn,
                         Values: IReadOnlyList<SqlExpression> valueExpressions
                     }
                     && _tpcDiscriminatorValues.TryGetValue(itemTpc, out var itemTuple)
                     && itemTuple.Item1.Equals(itemColumn))
            {
                var constantValues = new string[valueExpressions.Count];
                for (var i = 0; i < constantValues.Length; i++)
                {
                    if (valueExpressions[i] is SqlConstantExpression { Value: string value })
                    {
                        constantValues[i] = value;
                    }
                    else
                    {
                        goto ApplyPredicate;
                    }
                }

                var newList = itemTuple.Item2.Intersect(constantValues).ToList();
                if (newList.Count > 0)
                {
                    _tpcDiscriminatorValues[itemTpc] = (itemColumn, newList);
                    return;
                }
            }
        }

        ApplyPredicate:
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

        if (groupByTerms.Any(e => e is not ColumnExpression))
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
    ///     Applies grouping from given key selector and generate <see cref="RelationalGroupByShaperExpression" /> to shape results.
    /// </summary>
    /// <param name="keySelector">An key selector expression for the GROUP BY.</param>
    /// <param name="shaperExpression">The shaper expression for current query.</param>
    /// <param name="sqlExpressionFactory">The sql expression factory to use.</param>
    /// <returns>A <see cref="RelationalGroupByShaperExpression" /> which represents the result of the grouping operation.</returns>
    public RelationalGroupByShaperExpression ApplyGrouping(
        Expression keySelector,
        Expression shaperExpression,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        ClearOrdering();

        var keySelectorToAdd = keySelector;
        var emptyKey = keySelector is NewExpression { Arguments.Count: 0 };
        if (emptyKey)
        {
            keySelectorToAdd = sqlExpressionFactory.ApplyDefaultTypeMapping(sqlExpressionFactory.Constant(1));
        }

        var groupByTerms = new List<SqlExpression>();
        var groupByAliases = new List<string?>();
        PopulateGroupByTerms(keySelectorToAdd, groupByTerms, groupByAliases, "Key");

        if (groupByTerms.Any(e => e is not ColumnExpression))
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

        var clonedSelectExpression = Clone();
        var correlationPredicate = groupByTerms.Zip(clonedSelectExpression._groupBy)
            .Select(e => sqlExpressionFactory.Equal(e.First, e.Second))
            .Aggregate(sqlExpressionFactory.AndAlso);
        clonedSelectExpression._groupBy.Clear();
        clonedSelectExpression.ApplyPredicate(correlationPredicate);

        if (!_identifier.All(e => _groupBy.Contains(e.Column)))
        {
            _preGroupByIdentifier = _identifier.ToList();
            _identifier.Clear();
            if (_groupBy.All(e => e is ColumnExpression))
            {
                _identifier.AddRange(_groupBy.Select(e => ((ColumnExpression)e, e.TypeMapping!.KeyComparer)));
            }
        }

        return new RelationalGroupByShaperExpression(
            keySelector,
            shaperExpression,
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

            case UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression:
                PopulateGroupByTerms(unaryExpression.Operand, groupByTerms, groupByAliases, name);
                break;

            case StructuralTypeShaperExpression { ValueBufferExpression: StructuralTypeProjectionExpression projection }:
                foreach (var property in GetAllPropertiesInHierarchy(projection.StructuralType))
                {
                    PopulateGroupByTerms(projection.BindProperty(property), groupByTerms, groupByAliases, name: null);
                }

                if (projection.DiscriminatorExpression != null)
                {
                    PopulateGroupByTerms(
                        projection.DiscriminatorExpression, groupByTerms, groupByAliases, name: DiscriminatorColumnAlias);
                }

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
            null, new List<ProjectionExpression>(), _tables.ToList(), _tableReferences.ToList(), _groupBy.ToList(), _orderings.ToList(),
            GetAnnotations())
        {
            IsDistinct = IsDistinct,
            Predicate = Predicate,
            Having = Having,
            Offset = Offset,
            Limit = Limit,
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
        select1._identifier.AddRange(_identifier);
        _identifier.Clear();
        select1._removableJoinTables.AddRange(_removableJoinTables);
        _removableJoinTables.Clear();
        foreach (var kvp in _tpcDiscriminatorValues)
        {
            select1._tpcDiscriminatorValues[kvp.Key] = kvp.Value;
        }

        _tpcDiscriminatorValues.Clear();

        // Remap tableReferences in select1
        foreach (var tableReference in select1._tableReferences)
        {
            tableReference.UpdateTableReference(this, select1);
        }

        var tableReferenceUpdatingExpressionVisitor = new TableReferenceUpdatingExpressionVisitor(this, select1);
        tableReferenceUpdatingExpressionVisitor.Visit(select1);

        var outerIdentifiers = select1._identifier.Count == select2._identifier.Count
            ? new ColumnExpression?[select1._identifier.Count]
            : Array.Empty<ColumnExpression?>();
        var entityProjectionIdentifiers = new List<ColumnExpression>();
        var entityProjectionValueComparers = new List<ValueComparer>();
        var otherExpressions = new List<(SqlExpression Expression, ValueComparer Comparer)>();

        // Push down into a subquery if limit/offset are defined. If not, any orderings can be discarded as set operations don't preserve
        // them.
        // Note that in some databases it may be possible to preserve the internal ordering of the set operands for Concat, but we don't
        // currently support that.
        if (select1.Limit != null || select1.Offset != null)
        {
            // If we are pushing down here, we need to make sure to assign unique alias to subquery also.
            var subqueryAlias = GenerateUniqueAlias(_usedAliases, "t");
            select1.PushdownIntoSubqueryInternal(liftOrderings: false);
            select1._tables[0].Alias = subqueryAlias;
            select1._tableReferences[0].Alias = subqueryAlias;
        }
        else
        {
            select1.ClearOrdering();
        }

        // Do the same for the other side of the set operation
        if (select2.Limit != null || select2.Offset != null)
        {
            select2.PushdownIntoSubqueryInternal(liftOrderings: false);
        }
        else
        {
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
            if (expression1 is StructuralTypeProjectionExpression projection1
                && expression2 is StructuralTypeProjectionExpression projection2)
            {
                HandleStructuralTypeProjection(projectionMember, select1, projection1, select2, projection2);
                continue;
            }

            var innerColumn1 = (SqlExpression)expression1;
            var innerColumn2 = (SqlExpression)expression2;

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
                // If we happen to project identifier columns, make them candidates for lifting up to be the outer identifiers for the
                // set operation result. Note that we check below that *all* identifier columns are projected out, since a partial
                // identifier (e.g. one column in a composite key) is insufficient.
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

                // we need comparer (that we get from type mapping) for identifiers
                // it may happen that one side of the set operation comes from collection parameter
                // and therefore doesn't have type mapping (yet - we infer those after the translation is complete)
                // but for set operation at least one side should have type mapping, otherwise whole thing would have been parameterized out
                // this can only happen in compiled query, since we always parameterize parameters there - if this happens we throw
                var outerTypeMapping = innerProjection1.Expression.TypeMapping ?? innerProjection2.Expression.TypeMapping;
                if (outerTypeMapping == null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.SetOperationsRequireAtLeastOneSideWithValidTypeMapping(setOperationType));
                }

                otherExpressions.Add((outerProjection, outerTypeMapping.KeyComparer));
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

        select1._projectionMapping.Clear();
        select2._projectionMapping.Clear();

        // Mark both inner subqueries as immutable
        select1._mutable = false;
        select2._mutable = false;

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
                else if (otherExpressions.All(e => e.Expression is ColumnExpression))
                {
                    _identifier.AddRange(entityProjectionIdentifiers.Zip(entityProjectionValueComparers));
                    _identifier.AddRange(otherExpressions.Select(e => ((ColumnExpression)e.Expression, e.Comparer)));
                }
            }
        }

        void HandleStructuralTypeProjection(
            ProjectionMember projectionMember,
            SelectExpression select1,
            StructuralTypeProjectionExpression projection1,
            SelectExpression select2,
            StructuralTypeProjectionExpression projection2)
        {
            if (projection1.StructuralType != projection2.StructuralType)
            {
                throw new InvalidOperationException(
                    RelationalStrings.SetOperationOverDifferentStructuralTypes(
                        projection1.StructuralType.DisplayName(), projection2.StructuralType.DisplayName()));
            }

            var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();

            ProcessStructuralType(projection1, projection2);

            void ProcessStructuralType(
                StructuralTypeProjectionExpression nestedProjection1,
                StructuralTypeProjectionExpression nestedProjection2)
            {
                var type = nestedProjection1.StructuralType;

                foreach (var property in GetAllPropertiesInHierarchy(type))
                {
                    var column1 = nestedProjection1.BindProperty(property);
                    var column2 = nestedProjection2.BindProperty(property);
                    var alias = GenerateUniqueColumnAlias(column1.Name);
                    var innerProjection = new ProjectionExpression(column1, alias);
                    select1._projection.Add(innerProjection);
                    select2._projection.Add(new ProjectionExpression(column2, alias));
                    var outerColumn = new ConcreteColumnExpression(innerProjection, tableReferenceExpression);
                    if (column1.IsNullable
                        || column2.IsNullable)
                    {
                        outerColumn = outerColumn.MakeNullable();
                    }

                    propertyExpressions[property] = outerColumn;

                    // Lift up any identifier columns to the set operation result (the outer).
                    // This is typically the entity primary key columns, but can also be all of a complex type's properties if Distinct
                    // was previously called.
                    if (outerIdentifiers.Length > 0)
                    {
                        var index = select1._identifier.FindIndex(e => e.Column.Equals(column1));
                        if (index != -1)
                        {
                            if (select2._identifier[index].Column.Equals(column2))
                            {
                                outerIdentifiers[index] = outerColumn;
                            }
                            else
                            {
                                // If select1 matched but select2 did not then we erase all identifiers
                                // TODO: We could make this little more robust by allow the indexes to be different. See issue#24475
                                // i.e. Identifier ordering being different.
                                outerIdentifiers = Array.Empty<ColumnExpression>();
                            }
                        }
                        // If the top-level projection - not the current nested one - is a complex type and not an entity type, then add
                        // all its columns to the "otherExpressions" list (i.e. columns not part of a an entity primary key). This is
                        // the same as with a non-structural type projection.
                        else if (projection1.StructuralType is IComplexType)
                        {
                            var outerTypeMapping = column1.TypeMapping ?? column1.TypeMapping;
                            if (outerTypeMapping == null)
                            {
                                throw new InvalidOperationException(
                                    RelationalStrings.SetOperationsRequireAtLeastOneSideWithValidTypeMapping(setOperationType));
                            }

                            otherExpressions.Add((outerColumn, outerTypeMapping.KeyComparer));
                        }
                    }
                }

                foreach (var complexProperty in GetAllComplexPropertiesInHierarchy(nestedProjection1.StructuralType))
                {
                    ProcessStructuralType(
                        (StructuralTypeProjectionExpression)nestedProjection1.BindComplexProperty(complexProperty).ValueBufferExpression,
                        (StructuralTypeProjectionExpression)nestedProjection2.BindComplexProperty(complexProperty).ValueBufferExpression);
                }
            }

            Check.DebugAssert(
                projection1.TableMap.Count == projection2.TableMap.Count,
                "Set operation over entity projections with different table map counts");
            Check.DebugAssert(
                projection1.TableMap.Keys.All(t => projection2.TableMap.ContainsKey(t)),
                "Set operation over entity projections with table map discrepancy");

            var tableMap = projection1.TableMap.ToDictionary(kvp => kvp.Key, kvp => tableReferenceExpression);

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

            var outerProjection = new StructuralTypeProjectionExpression(
                projection1.StructuralType, propertyExpressions, tableMap, nullable: false, discriminatorExpression);

            if (outerIdentifiers.Length > 0 && outerProjection is { StructuralType: IEntityType entityType })
            {
                var primaryKey = entityType.FindPrimaryKey();

                // We know that there are existing identifiers (see condition above); we know we must have a key since a keyless
                // entity type would have wiped the identifiers when generating the join.
                Check.DebugAssert(primaryKey != null, "primary key is null.");
                foreach (var property in primaryKey.Properties)
                {
                    entityProjectionIdentifiers.Add(outerProjection.BindProperty(property));
                    entityProjectionValueComparers.Add(property.GetKeyValueComparer());
                }
            }

            _projectionMapping[projectionMember] = outerProjection;
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

        var dummySelectExpression = new SelectExpression(alias: "e");
        dummySelectExpression._projection.Add(new ProjectionExpression(nullSqlExpression, "empty"));
        dummySelectExpression._mutable = false;

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
            if (projectionToAdd is StructuralTypeProjectionExpression typeProjection)
            {
                projectionToAdd = typeProjection.MakeNullable();
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
    public StructuralTypeShaperExpression GenerateOwnedReferenceEntityProjectionExpression(
        StructuralTypeProjectionExpression principalEntityProjection,
        INavigation navigation,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        // We first find the select expression where principal tableExpressionBase is located
        // That is where we find shared tableExpressionBase to pull columns from or add joins
        var identifyingColumn = principalEntityProjection.BindProperty(
            navigation.DeclaringEntityType.FindPrimaryKey()!.Properties.First());

        var expressions = GetPropertyExpressions(sqlExpressionFactory, navigation, this, identifyingColumn);

        var entityShaper = new RelationalStructuralTypeShaperExpression(
            navigation.TargetEntityType,
            new StructuralTypeProjectionExpression(navigation.TargetEntityType, expressions, principalEntityProjection.TableMap),
            identifyingColumn.IsNullable || navigation.DeclaringEntityType.BaseType != null || !navigation.ForeignKey.IsRequiredDependent);
        principalEntityProjection.AddNavigationBinding(navigation, entityShaper);

        return entityShaper;

        // Owned types don't support inheritance See https://github.com/dotnet/efcore/issues/9630
        // So there is no handling for dependent having hierarchy
        // TODO: The following code should also handle Function and SqlQuery mappings when supported on owned type
        static IReadOnlyDictionary<IProperty, ColumnExpression> GetPropertyExpressions(
            ISqlExpressionFactory sqlExpressionFactory,
            INavigation navigation,
            SelectExpression selectExpression,
            ColumnExpression identifyingColumn)
        {
            var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
            var tableExpressionBase = UnwrapJoinExpression(identifyingColumn.Table);
            if (tableExpressionBase is SelectExpression subquery)
            {
                // If identifying column is from a subquery then the owner table is inside subquery
                // so we need to traverse in
                var subqueryIdentifyingColumn = (ColumnExpression)subquery.Projection
                    .Single(e => string.Equals(e.Alias, identifyingColumn.Name, StringComparison.OrdinalIgnoreCase))
                    .Expression;

                var subqueryPropertyExpressions = GetPropertyExpressions(
                    sqlExpressionFactory, navigation, subquery, subqueryIdentifyingColumn);
                var changeNullability = identifyingColumn.IsNullable && !subqueryIdentifyingColumn.IsNullable;
                var tableIndex = selectExpression._tables.FindIndex(e => ReferenceEquals(e, identifyingColumn.Table));
                var subqueryTableReferenceExpression = selectExpression._tableReferences[tableIndex];
                foreach (var (property, columnExpression) in subqueryPropertyExpressions)
                {
                    var outerColumn = subquery.GenerateOuterColumn(subqueryTableReferenceExpression, columnExpression);
                    if (changeNullability)
                    {
                        outerColumn = outerColumn.MakeNullable();
                    }
                    propertyExpressions[property] = outerColumn;
                }

                return propertyExpressions;
            }

            // This is the select expression where owner table exists
            // where we would look for same table or generate joins
            var sourceTableForAnnotations = FindRootTableExpressionForColumn(identifyingColumn.Table, identifyingColumn.Name);
            var ownerType = navigation.DeclaringEntityType;
            var entityType = navigation.TargetEntityType;
            var principalMappings = ownerType.GetViewOrTableMappings().Select(e => e.Table);
            var derivedType = ownerType.BaseType != null;
            var derivedTpt = derivedType && ownerType.GetMappingStrategy() == RelationalAnnotationNames.TptMappingStrategy;
            var parentNullable = identifyingColumn.IsNullable;
            var pkColumnsNullable = parentNullable
                || (derivedType && ownerType.GetMappingStrategy() != RelationalAnnotationNames.TphMappingStrategy);
            var newColumnsNullable = pkColumnsNullable || !navigation.ForeignKey.IsRequiredDependent;
            if (derivedTpt)
            {
                principalMappings = principalMappings.Except(ownerType.BaseType!.GetViewOrTableMappings().Select(e => e.Table));
            }

            var principalTables = principalMappings.ToList();
            var dependentTables = entityType.GetViewOrTableMappings().Select(e => e.Table).ToList();
            var baseTableIndex = selectExpression._tables.FindIndex(teb => ReferenceEquals(teb, identifyingColumn.Table));
            var dependentMainTable = dependentTables[0];
            var tableReferenceExpressionMap = new Dictionary<ITableBase, TableReferenceExpression>();
            var keyProperties = entityType.FindPrimaryKey()!.Properties;
            TableReferenceExpression mainTableReferenceExpression;
            TableReferenceExpression tableReferenceExpression;
            if (tableExpressionBase is TableExpression)
            {
                // This has potential to pull data from existing table
                // PrincipalTables count will be 1 except for entity splitting
                var matchingTableIndex = principalTables.FindIndex(e => e == dependentMainTable);
                // If dependent main table is not sharing then there is no table sharing at all in fragment
                if (matchingTableIndex != -1)
                {
                    // Dependent is table sharing with principal in some form, we don't need to generate join to owner
                    // TableExpression from identifying column will point to base type for TPT
                    // This may not be table which originates Owned type
                    if (derivedTpt)
                    {
                        baseTableIndex = selectExpression._tables.FindIndex(
                            teb => ((TableExpression)UnwrapJoinExpression(teb)).Table == principalTables[0]);
                    }
                    var tableIndex = baseTableIndex + matchingTableIndex;
                    mainTableReferenceExpression = selectExpression._tableReferences[tableIndex];
                    tableReferenceExpressionMap[dependentMainTable] = mainTableReferenceExpression;
                    if (dependentTables.Count > 1)
                    {
                        var joinColumns = new List<ColumnExpression>();
                        foreach (var property in keyProperties)
                        {
                            var columnExpression = new ConcreteColumnExpression(
                                property, dependentMainTable.FindColumn(property)!, mainTableReferenceExpression,
                                pkColumnsNullable);
                            propertyExpressions[property] = columnExpression;
                            joinColumns.Add(columnExpression);
                        }

                        for (var i = 1; i < dependentTables.Count; i++)
                        {
                            var table = dependentTables[i];
                            matchingTableIndex = principalTables.FindIndex(e => e == table);
                            if (matchingTableIndex != -1)
                            {
                                // We don't need to generate join for this
                                tableReferenceExpressionMap[table] = selectExpression._tableReferences[baseTableIndex + matchingTableIndex];
                            }
                            else
                            {
                                TableExpressionBase tableExpression = new TableExpression(table);
                                foreach (var annotation in sourceTableForAnnotations.GetAnnotations())
                                {
                                    tableExpression = tableExpression.AddAnnotation(annotation.Name, annotation.Value);
                                }
                                tableReferenceExpression = new TableReferenceExpression(selectExpression, tableExpression.Alias!);
                                tableReferenceExpressionMap[table] = tableReferenceExpression;

                                var innerColumns = keyProperties.Select(
                                    p => CreateColumnExpression(p, table, tableReferenceExpression, nullable: false));
                                var joinPredicate = joinColumns
                                    .Zip(innerColumns, sqlExpressionFactory.Equal)
                                    .Aggregate(sqlExpressionFactory.AndAlso);

                                var joinExpression = new LeftJoinExpression(tableExpression, joinPredicate);
                                selectExpression._removableJoinTables.Add(selectExpression._tables.Count);
                                selectExpression.AddTable(joinExpression, tableReferenceExpression);
                            }
                        }
                    }

                    foreach (var property in entityType.GetProperties())
                    {
                        if (property.IsPrimaryKey()
                            && dependentTables.Count > 1)
                        {
                            continue;
                        }

                        var columnBase = dependentTables.Count == 1
                            ? dependentMainTable.FindColumn(property)!
                            : dependentTables.Select(e => e.FindColumn(property)).First(e => e != null)!;
                        propertyExpressions[property] = CreateColumnExpression(
                            property, columnBase, tableReferenceExpressionMap[columnBase.Table],
                            nullable: property.IsPrimaryKey() ? pkColumnsNullable : newColumnsNullable);
                    }

                    return propertyExpressions;
                }
            }

            // Either we encountered a custom table source or dependent is not sharing table
            // In either case we need to generate join to owner
            var ownerJoinColumns = new List<ColumnExpression>();
            var ownerTableReferenceExpression = selectExpression._tableReferences[baseTableIndex];
            foreach (var property in navigation.ForeignKey.PrincipalKey.Properties)
            {
                var columnBase = principalTables.Select(e => e.FindColumn(property)).First(e => e != null)!;
                var columnExpression = new ConcreteColumnExpression(
                    property, columnBase, ownerTableReferenceExpression, pkColumnsNullable);
                ownerJoinColumns.Add(columnExpression);
            }
            TableExpressionBase ownedTable = new TableExpression(dependentMainTable);
            foreach (var annotation in sourceTableForAnnotations.GetAnnotations())
            {
                ownedTable = ownedTable.AddAnnotation(annotation.Name, annotation.Value);
            }
            mainTableReferenceExpression = new TableReferenceExpression(selectExpression, ownedTable.Alias!);
            var outerJoinPredicate = ownerJoinColumns
                .Zip(navigation.ForeignKey.Properties
                    .Select(p => CreateColumnExpression(p, dependentMainTable, mainTableReferenceExpression, nullable: false)))
                .Select(i => sqlExpressionFactory.Equal(i.First, i.Second))
                .Aggregate(sqlExpressionFactory.AndAlso);
            var joinedTable = new LeftJoinExpression(ownedTable, outerJoinPredicate);
            tableReferenceExpressionMap[dependentMainTable] = mainTableReferenceExpression;
            selectExpression.AddTable(joinedTable, mainTableReferenceExpression);
            if (dependentTables.Count > 1)
            {
                var joinColumns = new List<ColumnExpression>();
                foreach (var property in keyProperties)
                {
                    var columnExpression = new ConcreteColumnExpression(
                        property, dependentMainTable.FindColumn(property)!, mainTableReferenceExpression, newColumnsNullable);
                    propertyExpressions[property] = columnExpression;
                    joinColumns.Add(columnExpression);
                }

                for (var i = 1; i < dependentTables.Count; i++)
                {
                    var table = dependentTables[i];
                    TableExpressionBase tableExpression = new TableExpression(table);
                    foreach (var annotation in sourceTableForAnnotations.GetAnnotations())
                    {
                        tableExpression = tableExpression.AddAnnotation(annotation.Name, annotation.Value);
                    }
                    tableReferenceExpression = new TableReferenceExpression(selectExpression, tableExpression.Alias!);
                    tableReferenceExpressionMap[table] = tableReferenceExpression;

                    var innerColumns = keyProperties.Select(
                        p => CreateColumnExpression(p, table, tableReferenceExpression, nullable: false));
                    var joinPredicate = joinColumns
                        .Zip(innerColumns, sqlExpressionFactory.Equal)
                        .Aggregate(sqlExpressionFactory.AndAlso);

                    var joinExpression = new LeftJoinExpression(tableExpression, joinPredicate);
                    selectExpression._removableJoinTables.Add(selectExpression._tables.Count);
                    selectExpression.AddTable(joinExpression, tableReferenceExpression);
                }
            }

            foreach (var property in entityType.GetProperties())
            {
                if (property.IsPrimaryKey()
                    && dependentTables.Count > 1)
                {
                    continue;
                }

                var columnBase = dependentTables.Count == 1
                    ? dependentMainTable.FindColumn(property)!
                    : dependentTables.Select(e => e.FindColumn(property)).First(e => e != null)!;
                propertyExpressions[property] = CreateColumnExpression(
                    property, columnBase, tableReferenceExpressionMap[columnBase.Table],
                    nullable: newColumnsNullable);
            }

            foreach (var property in keyProperties)
            {
                selectExpression._identifier.Add((propertyExpressions[property], property.GetKeyValueComparer()));
            }

            return propertyExpressions;
        }

        static TableExpressionBase FindRootTableExpressionForColumn(TableExpressionBase table, string columnName)
        {
            if (table is JoinExpressionBase joinExpressionBase)
            {
                table = joinExpressionBase.Table;
            }
            else if (table is SetOperationBase setOperationBase)
            {
                table = setOperationBase.Source1;
            }

            if (table is SelectExpression selectExpression)
            {
                var matchingProjection = (ColumnExpression)selectExpression.Projection.Single(p => p.Alias == columnName).Expression;

                return FindRootTableExpressionForColumn(matchingProjection.Table, matchingProjection.Name);
            }

            return table;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static StructuralTypeShaperExpression GenerateComplexPropertyShaperExpression(
        StructuralTypeProjectionExpression containerProjection,
        IComplexProperty complexProperty)
    {
        var propertyExpressionMap = new Dictionary<IProperty, ColumnExpression>();

        // We do not support complex type splitting, so we will only ever have a single table/view mapping to it.
        var complexTypeTable = complexProperty.ComplexType.GetViewOrTableMappings().Single().Table;
        var tableReferenceExpression = containerProjection.TableMap[complexTypeTable];

        var isComplexTypeNullable = containerProjection.IsNullable || complexProperty.IsNullable;

        // If the complex property is declared on a type that's derived relative to the type being projected, the projected column is
        // nullable.
        if (!isComplexTypeNullable
            && containerProjection.StructuralType is IEntityType entityType
            && !entityType.GetAllBaseTypesInclusiveAscending().Contains(complexProperty.DeclaringType))
        {
            isComplexTypeNullable = true;
        }

        foreach (var property in complexProperty.ComplexType.GetProperties())
        {
            // TODO: Reimplement EntityProjectionExpression via TableMap, and then use that here
            var column = complexTypeTable.FindColumn(property)!;
            propertyExpressionMap[property] = CreateColumnExpression(
                property, column, tableReferenceExpression, isComplexTypeNullable || column.IsNullable);
        }

        // The table map of the target complex type should only ever contains a single table (no table splitting).
        // If the source is itself a complex type (nested complex type), its table map is already suitable and we can just pass it on.
        var newTableMap = containerProjection.TableMap.Count == 1
            ? containerProjection.TableMap
            : new Dictionary<ITableBase, TableReferenceExpression> { [complexTypeTable] = tableReferenceExpression };

        Check.DebugAssert(newTableMap.Single().Key == complexTypeTable, "Bad new table map");

        var entityShaper = new RelationalStructuralTypeShaperExpression(
            complexProperty.ComplexType,
            new StructuralTypeProjectionExpression(complexProperty.ComplexType, propertyExpressionMap, newTableMap, isComplexTypeNullable),
            isComplexTypeNullable);

        return entityShaper;
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
        var innerNullable = joinType is JoinType.LeftJoin or JoinType.OuterApply;

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
        if (joinType is JoinType.CrossApply or JoinType.OuterApply)
        {
            var limit = innerSelectExpression.Limit;
            var offset = innerSelectExpression.Offset;
            if (!innerSelectExpression.IsDistinct
                || (limit == null && offset == null))
            {
                innerSelectExpression.Limit = null;
                innerSelectExpression.Offset = null;

                var originalInnerSelectPredicate = innerSelectExpression.GroupBy.Count > 0
                    ? innerSelectExpression.Having
                    : innerSelectExpression.Predicate;

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

                    if (originalInnerSelectPredicate != null)
                    {
                        if (innerSelectExpression.GroupBy.Count > 0)
                        {
                            innerSelectExpression.Having = originalInnerSelectPredicate;
                        }
                        else
                        {
                            innerSelectExpression.Predicate = originalInnerSelectPredicate;
                        }
                    }

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

        foreach (var kvp in innerSelectExpression._tpcDiscriminatorValues)
        {
            _tpcDiscriminatorValues[kvp.Key] = kvp.Value;
        }

        if (_identifier.Count > 0
            && innerSelectExpression._identifier.Count > 0)
        {
            if (joinType is JoinType.LeftJoin or JoinType.OuterApply)
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

            // we can't convert apply to join in case of distinct and group by, if the projection doesn't already contain the join keys
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
                        && sqlBinaryExpression.OperatorType is ExpressionType.NotEqual
                            or ExpressionType.GreaterThan
                            or ExpressionType.GreaterThanOrEqual
                            or ExpressionType.LessThan
                            or ExpressionType.LessThanOrEqual))
                {
                    if (IsContainedSql(outer, sqlBinaryExpression.Left)
                        && IsContainedSql(inner, sqlBinaryExpression.Right))
                    {
                        outerColumnExpressions.Add(sqlBinaryExpression.Left);

                        return sqlBinaryExpression;
                    }

                    if (IsContainedSql(outer, sqlBinaryExpression.Right)
                        && IsContainedSql(inner, sqlBinaryExpression.Left))
                    {
                        outerColumnExpressions.Add(sqlBinaryExpression.Right);

                        var mirroredOperation = sqlBinaryExpression.OperatorType switch
                        {
                            ExpressionType.Equal => ExpressionType.Equal,
                            ExpressionType.NotEqual => ExpressionType.NotEqual,
                            ExpressionType.LessThan => ExpressionType.GreaterThan,
                            ExpressionType.LessThanOrEqual => ExpressionType.GreaterThanOrEqual,
                            ExpressionType.GreaterThan => ExpressionType.LessThan,
                            ExpressionType.GreaterThanOrEqual => ExpressionType.LessThanOrEqual,

                            _ => throw new UnreachableException()
                        };

                        return new SqlBinaryExpression(
                            mirroredOperation,
                            sqlBinaryExpression.Right,
                            sqlBinaryExpression.Left,
                            sqlBinaryExpression.Type,
                            sqlBinaryExpression.TypeMapping);
                    }
                }

                // null checks are considered part of join key
                if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
                {
                    if (IsContainedSql(outer, sqlBinaryExpression.Left)
                        && sqlBinaryExpression.Right is SqlConstantExpression { Value: null })
                    {
                        return sqlBinaryExpression;
                    }

                    if (IsContainedSql(outer, sqlBinaryExpression.Right)
                        && sqlBinaryExpression.Left is SqlConstantExpression { Value: null })
                    {
                        return sqlBinaryExpression.Update(
                            sqlBinaryExpression.Right,
                            sqlBinaryExpression.Left);
                    }
                }

                return null;
            }

            static bool IsContainedSql(SelectExpression selectExpression, SqlExpression sqlExpression)
                => sqlExpression switch
                {
                    ColumnExpression columnExpression => selectExpression.ContainsTableReference(columnExpression),

                    // We check condition in a separate function to avoid matching structure of condition outside of case block
                    CaseExpression
                        {
                            Operand: null,
                            WhenClauses: [{ Result: ColumnExpression resultColumn } whenClause],
                            ElseResult: null
                        }
                        => IsContainedCondition(selectExpression, whenClause.Test) && selectExpression.ContainsTableReference(resultColumn),

                    _ => false
                };

            static bool IsContainedCondition(SelectExpression selectExpression, SqlExpression condition)
            {
                if (condition is not SqlBinaryExpression
                    {
                        OperatorType: ExpressionType.AndAlso or ExpressionType.OrElse or ExpressionType.NotEqual
                    } sqlBinaryExpression)
                {
                    return false;
                }

                if (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual)
                {
                    // We don't check left/right inverted because we generate this.
                    return sqlBinaryExpression is { Left: ColumnExpression column, Right: SqlConstantExpression { Value: null } }
                        && selectExpression.ContainsTableReference(column);
                }

                return IsContainedCondition(selectExpression, sqlBinaryExpression.Left)
                    && IsContainedCondition(selectExpression, sqlBinaryExpression.Right);
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
                foreach (var (_, expression) in projectionMapping)
                {
                    if (expression is StructuralTypeProjectionExpression projection)
                    {
                        foreach (var property in GetAllPropertiesInHierarchy(projection.StructuralType))
                        {
                            result.Add(projection.BindProperty(property));
                        }

                        if (projection.DiscriminatorExpression is ColumnExpression discriminatorColumn)
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
                        && sqlBinaryExpression.Right is SqlConstantExpression { Value: null })
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

    /// <summary>
    ///     Pushes down the <see cref="SelectExpression" /> into a subquery.
    /// </summary>
    /// <param name="liftOrderings">Whether orderings on the query should be lifted out of the subquery.</param>
    private SqlRemappingVisitor PushdownIntoSubqueryInternal(bool liftOrderings = true)
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
            _usedAliases = _usedAliases,
            _mutable = false
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
        _preGroupByIdentifier = null;
        subquery._removableJoinTables.AddRange(_removableJoinTables);
        _removableJoinTables.Clear();
        foreach (var kvp in _tpcDiscriminatorValues)
        {
            subquery._tpcDiscriminatorValues[kvp.Key] = kvp.Value;
        }

        _tpcDiscriminatorValues.Clear();

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

                if (item is StructuralTypeProjectionExpression projection)
                {
                    _clientProjections[i] = LiftEntityProjectionFromSubquery(projection, subqueryTableReferenceExpression);
                }
                else if (item is JsonQueryExpression jsonQueryExpression)
                {
                    _clientProjections[i] = LiftJsonQueryFromSubquery(jsonQueryExpression);
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

                if (expression is StructuralTypeProjectionExpression projection)
                {
                    _projectionMapping[projectionMember] = LiftEntityProjectionFromSubquery(projection, subqueryTableReferenceExpression);
                }
                else if (expression is JsonQueryExpression jsonQueryExpression)
                {
                    _projectionMapping[projectionMember] = LiftJsonQueryFromSubquery(jsonQueryExpression);
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

        if (subquery._groupBy.Count > 0
            && !subquery.IsDistinct)
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
            if (liftOrderings && projectionMap.TryGetValue(orderingExpression, out var outerColumn))
            {
                _orderings.Add(ordering.Update(outerColumn));
            }
            else if (liftOrderings
                     && (!IsDistinct
                         && GroupBy.Count == 0
                         || GroupBy.Contains(orderingExpression)))
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
                    _clientProjections[i] = shapedQueryExpression.UpdateQueryExpression(
                        sqlRemappingVisitor.Remap((SelectExpression)shapedQueryExpression.QueryExpression));
                }
            }
        }

        return sqlRemappingVisitor;

        StructuralTypeProjectionExpression LiftEntityProjectionFromSubquery(
            StructuralTypeProjectionExpression projection,
            TableReferenceExpression subqueryTableReference)
        {
            var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();

            HandleTypeProjection(projection);

            void HandleTypeProjection(StructuralTypeProjectionExpression typeProjection)
            {
                foreach (var property in GetAllPropertiesInHierarchy(typeProjection.StructuralType))
                {
                    // json entity projection (i.e. JSON entity that was transformed into query root) may have synthesized keys
                    // but they don't correspond to any columns - we need to skip those
                    if (typeProjection is { StructuralType: IEntityType entityType }
                        && entityType.IsMappedToJson()
                        && property.IsOrdinalKeyProperty())
                    {
                        continue;
                    }

                    var innerColumn = typeProjection.BindProperty(property);
                    var outerColumn = subquery.GenerateOuterColumn(subqueryTableReferenceExpression, innerColumn);
                    projectionMap[innerColumn] = outerColumn;
                    propertyExpressions[property] = outerColumn;
                }

                foreach (var complexProperty in GetAllComplexPropertiesInHierarchy(typeProjection.StructuralType))
                {
                    HandleTypeProjection((StructuralTypeProjectionExpression)typeProjection.BindComplexProperty(complexProperty).ValueBufferExpression);
                }
            }

            ColumnExpression? discriminatorExpression = null;
            if (projection.DiscriminatorExpression != null)
            {
                discriminatorExpression = subquery.GenerateOuterColumn(
                    subqueryTableReferenceExpression, projection.DiscriminatorExpression, DiscriminatorColumnAlias);
                projectionMap[projection.DiscriminatorExpression] = discriminatorExpression;
            }

            var tableMap = projection.TableMap.ToDictionary(kvp => kvp.Key, _ => subqueryTableReference);

            var newEntityProjection = new StructuralTypeProjectionExpression(
                projection.StructuralType, propertyExpressions, tableMap, nullable: false, discriminatorExpression);

            if (projection.StructuralType is IEntityType entityType2)
            {
                // Also lift nested entity projections
                foreach (var navigation in entityType2
                             .GetAllBaseTypes().Concat(entityType2.GetDerivedTypesInclusive())
                             .SelectMany(t => t.GetDeclaredNavigations()))
                {
                    var boundEntityShaperExpression = projection.BindNavigation(navigation);
                    if (boundEntityShaperExpression != null)
                    {
                        var newValueBufferExpression =
                            boundEntityShaperExpression.ValueBufferExpression is StructuralTypeProjectionExpression innerEntityProjection
                                ? (Expression)LiftEntityProjectionFromSubquery(innerEntityProjection, subqueryTableReferenceExpression)
                                : LiftJsonQueryFromSubquery((JsonQueryExpression)boundEntityShaperExpression.ValueBufferExpression);

                        boundEntityShaperExpression = boundEntityShaperExpression.Update(newValueBufferExpression);
                        newEntityProjection.AddNavigationBinding(navigation, boundEntityShaperExpression);
                    }
                }
            }

            return newEntityProjection;
        }

        JsonQueryExpression LiftJsonQueryFromSubquery(JsonQueryExpression jsonQueryExpression)
        {
            var jsonScalarExpression = new JsonScalarExpression(
                jsonQueryExpression.JsonColumn,
                jsonQueryExpression.Path,
                jsonQueryExpression.JsonColumn.TypeMapping!.ClrType,
                jsonQueryExpression.JsonColumn.TypeMapping,
                jsonQueryExpression.IsNullable);

            var newJsonColumn = subquery.GenerateOuterColumn(subqueryTableReferenceExpression, jsonScalarExpression);

            var newKeyPropertyMap = new Dictionary<IProperty, ColumnExpression>();
            var keyProperties = jsonQueryExpression.KeyPropertyMap.Keys.ToList();
            for (var i = 0; i < keyProperties.Count; i++)
            {
                var keyProperty = keyProperties[i];
                var innerColumn = jsonQueryExpression.BindProperty(keyProperty);
                var outerColumn = subquery.GenerateOuterColumn(subqueryTableReferenceExpression, innerColumn);
                projectionMap[innerColumn] = outerColumn;
                newKeyPropertyMap[keyProperty] = outerColumn;
            }

            // clear up the json path - we start from empty path after pushdown
            return new JsonQueryExpression(
                jsonQueryExpression.EntityType,
                newJsonColumn,
                newKeyPropertyMap,
                jsonQueryExpression.Type,
                jsonQueryExpression.IsCollection);
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
            && Tables is [FromSqlExpression fromSql]
            && Projection.All(
                pe => pe.Expression is ColumnExpression column
                    && string.Equals(fromSql.Alias, column.TableAlias, StringComparison.OrdinalIgnoreCase))
            && _projectionMapping.TryGetValue(new ProjectionMember(), out var mapping)
            && mapping.Type == (fromSql.Table == null ? typeof(int) : typeof(Dictionary<IProperty, int>));

    /// <summary>
    ///     Prepares the <see cref="SelectExpression" /> to apply aggregate operation over it.
    /// </summary>
    public void PrepareForAggregate(bool liftOrderings = true)
    {
        if (IsDistinct
            || Limit != null
            || Offset != null
            || _groupBy.Count > 0)
        {
            PushdownIntoSubqueryInternal(liftOrderings);
        }
    }

    /// <summary>
    ///     Creates a <see cref="ColumnExpression" /> that references a table on this <see cref="SelectExpression" />.
    /// </summary>
    /// <param name="tableExpression">The table expression referenced by the column.</param>
    /// <param name="columnName">The column name.</param>
    /// <param name="type">The column CLR type.</param>
    /// <param name="typeMapping">The column's type mapping.</param>
    /// <param name="columnNullable">Whether the column is nullable.</param>
    public ColumnExpression CreateColumnExpression(
        TableExpressionBase tableExpression,
        string columnName,
        Type type,
        RelationalTypeMapping? typeMapping,
        bool? columnNullable = null)
    {
        var tableIndex = _tables.FindIndex(teb => ReferenceEquals(teb, tableExpression));
        var tableReferenceExpression = _tableReferences[tableIndex];

        return new ConcreteColumnExpression(
            columnName,
            tableReferenceExpression,
            type.UnwrapNullableType(),
            typeMapping,
            columnNullable ?? type.IsNullableType());
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
    {
        var selectExpression = (SelectExpression)new TpcTableExpressionRemovingExpressionVisitor(_usedAliases).Visit(this);
#if DEBUG
        selectExpression._removedAliases = new List<string>();
        selectExpression = selectExpression.Prune(referencedColumns: null, selectExpression._removedAliases);
#else
        selectExpression = selectExpression.Prune(referencedColumns: null);
#endif
        return selectExpression;
    }

#if DEBUG
    private SelectExpression Prune(IReadOnlyCollection<string>? referencedColumns, List<string> removedAliases)
#else
    private SelectExpression Prune(IReadOnlyCollection<string>? referencedColumns)
#endif
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
        // Start at 1 because we don't drop main table.
        // Dropping main table is more complex because other tables need to unwrap joins to be main
        for (var i = 0; i < _tables.Count; i++)
        {
            var table = _tables[i];
            var tableAlias = GetAliasFromTableExpressionBase(table);
            if (columnsMap[tableAlias] == null
                // InnerJoin is only valid for removable join table which are from entity splitting
                && table is LeftJoinExpression or OuterApplyExpression or InnerJoinExpression
                && _removableJoinTables?.Contains(i + removedTableCount) == true)
            {
                _tables.RemoveAt(i);
                _tableReferences.RemoveAt(i);
                removedTableCount++;
                i--;
#if DEBUG
                removedAliases.Add(tableAlias);
#endif
                continue;
            }

            if (UnwrapJoinExpression(table) is SelectExpression innerSelectExpression)
            {
#if DEBUG
                innerSelectExpression.Prune(columnsMap[tableAlias], removedAliases);
#else
                innerSelectExpression.Prune(columnsMap[tableAlias]);
#endif
            }
            else if (table is SetOperationBase { IsDistinct: false } setOperation)
            {
                if (setOperation.Source1.IsDistinct
                    || setOperation.Source2.IsDistinct)
                {
                    continue;
                }

#if DEBUG
                setOperation.Source1.Prune(columnsMap[tableAlias], removedAliases);
                setOperation.Source2.Prune(columnsMap[tableAlias], removedAliases);
#else
                setOperation.Source1.Prune(columnsMap[tableAlias]);
                setOperation.Source2.Prune(columnsMap[tableAlias]);
#endif
            }
        }

        return this;
    }

    private Dictionary<ProjectionMember, int> ConvertProjectionMappingToClientProjections(
        Dictionary<ProjectionMember, Expression> projectionMapping,
        bool makeNullable = false)
    {
        var mapping = new Dictionary<ProjectionMember, int>();
        var typeProjectionCache = new Dictionary<StructuralTypeProjectionExpression, int>(ReferenceEqualityComparer.Instance);
        foreach (var projection in projectionMapping)
        {
            var projectionMember = projection.Key;
            var projectionToAdd = projection.Value;

            if (projectionToAdd is StructuralTypeProjectionExpression typeProjection)
            {
                if (!typeProjectionCache.TryGetValue(typeProjection, out var value))
                {
                    var entityProjectionToCache = typeProjection;
                    if (makeNullable)
                    {
                        typeProjection = typeProjection.MakeNullable();
                    }

                    _clientProjections.Add(typeProjection);
                    _aliasForClientProjections.Add(null);
                    value = _clientProjections.Count - 1;
                    typeProjectionCache[entityProjectionToCache] = value;
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
        => nullable
            ? expression switch
            {
                StructuralTypeProjectionExpression projection => projection.MakeNullable(),
                ColumnExpression column => column.MakeNullable(),
                JsonQueryExpression jsonQueryExpression => jsonQueryExpression.MakeNullable(),
                _ => expression
            }
            : expression;

    private static string GetAliasFromTableExpressionBase(TableExpressionBase tableExpressionBase)
        // We unwrap here since alias are not assigned to wrapper expressions
        => UnwrapJoinExpression(tableExpressionBase).Alias!;

    private static TableExpressionBase UnwrapJoinExpression(TableExpressionBase tableExpressionBase)
        => (tableExpressionBase as JoinExpressionBase)?.Table ?? tableExpressionBase;

    private static IEnumerable<IProperty> GetAllPropertiesInHierarchy(ITypeBase structuralType)
        => structuralType switch
        {
            IEntityType entityType => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetDeclaredProperties()),
            IComplexType complexType => complexType.GetDeclaredProperties(),
            _ => throw new UnreachableException()
        };

    private static IEnumerable<IProperty> GetAllFlattenedPropertiesInHierarchy(ITypeBase structuralType)
        => structuralType switch
        {
            IEntityType entityType => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetFlattenedDeclaredProperties()),
            IComplexType complexType => complexType.GetFlattenedDeclaredProperties(),
            _ => throw new UnreachableException()
        };

    private static IEnumerable<IComplexProperty> GetAllComplexPropertiesInHierarchy(ITypeBase structuralType)
        => structuralType switch
        {
            IEntityType entityType => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetDeclaredComplexProperties()),
            IComplexType complexType => complexType.GetDeclaredComplexProperties(),
            _ => throw new UnreachableException()
        };

    private static IEnumerable<INavigation> GetAllNavigationsInHierarchy(IEntityType entityType)
        => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
            .SelectMany(t => t.GetDeclaredNavigations());

    private static ConcreteColumnExpression CreateColumnExpression(
        IProperty property,
        ITableBase table,
        TableReferenceExpression tableExpression,
        bool nullable)
        => CreateColumnExpression(property, table.FindColumn(property)!, tableExpression, nullable);

    private static ConcreteColumnExpression CreateColumnExpression(
        IProperty property,
        IColumnBase columnBase,
        TableReferenceExpression tableExpression,
        bool nullable)
        => new(property, columnBase, tableExpression, nullable);

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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public SqlExpression AssignUniqueAliases(SqlExpression expression)
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
        if (_mutable)
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
                || newTables.Select(GetAliasFromTableExpressionBase).SequenceEqual(_tableReferences.Select(e => e.Alias)),
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
                    && newGroupingKey is not (SqlConstantExpression or SqlParameterExpression))
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
            foreach (var kvp in _tpcDiscriminatorValues)
            {
                _tpcDiscriminatorValues[kvp.Key] = ((ColumnExpression)visitor.Visit(kvp.Value.Item1), kvp.Value.Item2);
            }

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
                || newTables.Select(GetAliasFromTableExpressionBase).SequenceEqual(_tableReferences.Select(e => e.Alias)),
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
                    && newGroupingKey is not (SqlConstantExpression or SqlParameterExpression))
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

            var identifier = VisitList(_identifier.Select(e => e.Column).ToList(), inPlace: false, out var identifierChanged);
            changed |= identifierChanged;

            var childIdentifier = VisitList(
                _childIdentifiers.Select(e => e.Column).ToList(), inPlace: false, out var childIdentifierChanged);
            changed |= childIdentifierChanged;
            var newTpcDiscriminatorValues = new Dictionary<TpcTablesExpression, (ColumnExpression, List<string>)>();
            foreach (var kvp in _tpcDiscriminatorValues)
            {
                var newDiscriminatorColumnForTpc = (ColumnExpression)visitor.Visit(kvp.Value.Item1);
                changed |= newDiscriminatorColumnForTpc != kvp.Value.Item1;
                newTpcDiscriminatorValues[kvp.Key] = (newDiscriminatorColumnForTpc, kvp.Value.Item2);
            }

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
                    _mutable = false
                };
                newSelectExpression._removableJoinTables.AddRange(_removableJoinTables);
                foreach (var kvp in newTpcDiscriminatorValues)
                {
                    newSelectExpression._tpcDiscriminatorValues[kvp.Key] = kvp.Value;
                }

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
                    ? shapedQueryExpression.UpdateQueryExpression(visitor.Visit(shapedQueryExpression.QueryExpression))
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
        Check.DebugAssert(!_mutable, "SelectExpression shouldn't be mutable when calling this method.");

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
            Tags = Tags,
            _mutable = false
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
    protected override TableExpressionBase CreateWithAnnotations(IEnumerable<IAnnotation> annotations)
        => throw new NotImplementedException("inconceivable");

    /// <inheritdoc />
    public override TableExpressionBase AddAnnotation(string name, object? value)
    {
        var oldAnnotation = FindAnnotation(name);
        if (oldAnnotation != null)
        {
            return Equals(oldAnnotation.Value, value)
                ? this
                : throw new InvalidOperationException(CoreStrings.DuplicateAnnotation(name, this.Print()));
        }

        _annotations ??= new SortedDictionary<string, IAnnotation>();
        _annotations[name] = new Annotation(name, value);

        return this;
    }

    /// <inheritdoc />
    public override IAnnotation? FindAnnotation(string name)
        => _annotations == null
            ? null
            : _annotations.TryGetValue(name, out var annotation)
                ? annotation
                : null;

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> GetAnnotations()
        => _annotations?.Values ?? Enumerable.Empty<IAnnotation>();

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        PrintProjections(expressionPrinter);
        expressionPrinter.AppendLine();
        PrintSql(expressionPrinter);
    }

    private void PrintProjections(ExpressionPrinter expressionPrinter)
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
    }

    private void PrintSql(ExpressionPrinter expressionPrinter, bool withTags = true)
    {
        if (withTags)
        {
            foreach (var tag in Tags)
            {
                expressionPrinter.Append($"-- {tag}");
            }
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

    private string PrintShortSql()
    {
        var expressionPrinter = new ExpressionPrinter();
        PrintSql(expressionPrinter, withTags: false);
        return expressionPrinter.ToString();
    }

    /// <summary>
    ///     <para>
    ///         Expand this property in the debugger for a human-readable representation of this <see cref="SelectExpression" />.
    ///     </para>
    ///     <para>
    ///         Warning: Do not rely on the format of the debug strings.
    ///         They are designed for debugging only and may change arbitrarily between releases.
    ///     </para>
    /// </summary>
    [EntityFrameworkInternal]
    public string DebugView
        => this.Print();

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

#if DEBUG
    internal bool IsMutable()
        => _mutable;

    internal IReadOnlyList<string> RemovedAliases()
        => _removedAliases!;
#endif
}
