// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

public partial class RelationalQueryableMethodTranslatingExpressionVisitor
{
    /// <summary>
    ///     Used to create a root <see cref="SelectExpression" /> representing a query of the given entity type.
    /// </summary>
    protected virtual SelectExpression CreateSelect(IEntityType entityType)
    {
        var select = CreateRootSelectExpressionCore(entityType);
        AddEntitySelectConditions(select, entityType);
        return select;

        SelectExpression CreateRootSelectExpressionCore(IEntityType entityType)
        {
            switch (entityType.GetMappingStrategy())
            {
                case RelationalAnnotationNames.TptMappingStrategy:
                {
                    var keyProperties = entityType.FindPrimaryKey()!.Properties;
                    List<ColumnExpression> joinColumns = default!;
                    var tableMap = new Dictionary<ITableBase, string>();
                    var propertyMap = new Dictionary<IProperty, ColumnExpression>();
                    var complexPropertyMap = new Dictionary<IComplexProperty, Expression>();
                    var tables = new List<TableExpressionBase>();
                    var identifier = new List<(ColumnExpression Column, ValueComparer Comparer)>();

                    foreach (var baseType in entityType.GetAllBaseTypesInclusive())
                    {
                        var table = GetTableBaseFiltered(baseType, tableMap);
                        var alias = _sqlAliasManager.GenerateTableAlias(table);
                        var tableExpression = new TableExpression(alias, table);
                        tableMap.Add(table, alias);

                        foreach (var property in baseType.GetDeclaredProperties())
                        {
                            propertyMap[property] = CreateColumnExpression(property, table, alias, nullable: false);
                        }

                        foreach (var complexProperty in baseType.GetDeclaredComplexProperties())
                        {
                            complexPropertyMap[complexProperty] = ProcessComplexProperty(complexProperty, table, alias, containerNullable: false);
                        }

                        if (tables.Count == 0)
                        {
                            tables.Add(tableExpression);
                            joinColumns = [];
                            foreach (var property in keyProperties)
                            {
                                var columnExpression = propertyMap[property];
                                joinColumns.Add(columnExpression);
                                identifier.Add((columnExpression, property.GetKeyValueComparer()));
                            }
                        }
                        else
                        {
                            var innerColumns = keyProperties.Select(p => CreateColumnExpression(p, table, alias, nullable: false));

                            var joinPredicate = joinColumns
                                .Zip(innerColumns, _sqlExpressionFactory.Equal)
                                .Aggregate(_sqlExpressionFactory.AndAlso);

                            tables.Add(new InnerJoinExpression(tableExpression, joinPredicate));
                        }
                    }

                    var caseWhenClauses = new List<CaseWhenClause>();
                    foreach (var derivedType in entityType.GetDerivedTypes())
                    {
                        var table = GetTableBaseFiltered(derivedType, tableMap);
                        var alias = _sqlAliasManager.GenerateTableAlias(table);
                        var tableExpression = new TableExpression(alias, table);
                        tableMap.Add(table, alias);

                        foreach (var property in derivedType.GetDeclaredProperties())
                        {
                            propertyMap[property] = CreateColumnExpression(property, table, alias, nullable: true);
                        }

                        foreach (var complexProperty in derivedType.GetDeclaredComplexProperties())
                        {
                            complexPropertyMap[complexProperty] = ProcessComplexProperty(complexProperty, table, alias, containerNullable: true);
                        }

                        var keyColumns = keyProperties.Select(p => CreateColumnExpression(p, table, alias, nullable: true)).ToArray();

                        if (!derivedType.IsAbstract())
                        {
                            caseWhenClauses.Add(
                                new CaseWhenClause(
                                    _sqlExpressionFactory.IsNotNull(keyColumns[0]),
                                    _sqlExpressionFactory.Constant(derivedType.ShortName())));
                        }

                        var joinPredicate = joinColumns
                            .Zip(keyColumns, _sqlExpressionFactory.Equal)
                            .Aggregate(_sqlExpressionFactory.AndAlso);

                        tables.Add(new LeftJoinExpression(tableExpression, joinPredicate, prunable: true));
                    }

                    caseWhenClauses.Reverse();
                    var discriminatorExpression = caseWhenClauses.Count == 0
                        ? null
                        : _sqlExpressionFactory.ApplyDefaultTypeMapping(
                            _sqlExpressionFactory.Case(caseWhenClauses, elseResult: null));

                    var projection = new StructuralTypeProjectionExpression(
                        entityType, propertyMap, complexPropertyMap, nullable: false, discriminatorExpression);

                    return new SelectExpression(tables, projection, identifier, _sqlAliasManager);
                }

                case RelationalAnnotationNames.TpcMappingStrategy:
                {
                    // Drop additional table if ofType/is operator used Issue#27957
                    var concreteEntityTypes = entityType.GetDerivedTypesInclusive().Where(e => !e.IsAbstract()).ToArray();

                    // If we happen to be dealing with a TPC leaf, handle it like a regular, non-TPC entity type
                    // (no UNION needed, no discriminator needed)
                    if (concreteEntityTypes is [var singleEntityType])
                    {
                        var table = singleEntityType.GetViewOrTableMappings().Single().Table;
                        var alias = _sqlAliasManager.GenerateTableAlias(table);
                        var tableExpression = new TableExpression(alias, table);

                        return GenerateSingleTableSelect(entityType, table, tableExpression);
                    }

                    // Multiple TPC concrete types
                    var maxIdentifierLength = concreteEntityTypes[0].Model.GetMaxIdentifierLength();

                    // The maps of scalar and complex properties that will be projected from the UNION of all TPC tables
                    var propertyMap = new Dictionary<IProperty, ColumnExpression>();
                    var complexPropertyMap = new Dictionary<IComplexProperty, Expression>();

                    // A flattened map of all scalar properties and their columns, which will all need to be projected out
                    var allPropertyMap = new List<(IPropertyBase Property, ColumnExpression ProjectedColumnExpression)>();

                    var processedEntityTypes = new HashSet<IEntityType>();
                    var projectedColumnNames = new HashSet<string>(); // For uniquification

                    var tpcTableAlias = _sqlAliasManager.GenerateTableAlias("union");

                    foreach (var concreteEntityType in concreteEntityTypes)
                    {
                        var table = concreteEntityType.GetViewOrTableMappings().Single().Table;

                        foreach (var currentType in concreteEntityType.GetAllBaseTypesInclusive())
                        {
                            // Skip base types that have already been processed via another concrete entity type
                            if (!processedEntityTypes.Add(currentType))
                            {
                                continue;
                            }

                            foreach (var property in currentType.GetDeclaredProperties())
                            {
                                var columnExpression = ProcessPropertyTpc(property, containerNullable: false);
                                propertyMap[property] = columnExpression;
                            }

                            foreach (var complexProperty in currentType.GetDeclaredComplexProperties())
                            {
                                complexPropertyMap[complexProperty] =
                                    ProcessComplexPropertyTpc(complexProperty, containerNullable: false);
                            }

                            ColumnExpression ProcessPropertyTpc(IProperty property, bool containerNullable)
                            {
                                // We'll create the column projected out of the UNION of the *entire* hierarchy (columns for each
                                // individual subquery of that UNION are created below).
                                // For the name of this projected column, we'll just use its name on the table mapped to our concrete
                                // entity type.
                                // Note that at least in theory, the same property can be mapped to different column names on different TPC
                                // tables in the same hierarchy; but we require a single name to project out of the UNION, so we pick the
                                // one from the first concrete entity type we happen to process.
                                // However, since multiple properties from different entity types may have the same column names in their
                                // respective table, we must uniquify the projected column name.
                                var concreteColumn = table.FindColumn(property)!;
                                var projectedColumnName = Uniquifier.Uniquify(
                                    concreteColumn.Name, projectedColumnNames, maxIdentifierLength);
                                projectedColumnNames.Add(projectedColumnName);

                                var columnExpression = new ColumnExpression(
                                    projectedColumnName,
                                    tpcTableAlias,
                                    property.ClrType.UnwrapNullableType(),
                                    property.GetRelationalTypeMapping(),
                                    // Note that we're creating projected columns for the UNION of the entire hierarchy.
                                    // So the moment this property is not on the root projected entity type (or one of its base types),
                                    // it must be nullable since there's going to be some concrete type which doesn't have it.
                                    nullable: containerNullable
                                        || property.IsNullable
                                        || !entityType.IsAssignableTo(property.DeclaringType.ContainingEntityType));

                                allPropertyMap.Add((property, columnExpression));

                                return columnExpression;
                            }

                            Expression ProcessComplexPropertyTpc(
                                IComplexProperty complexProperty,
                                bool containerNullable)
                            {
                                var complexType = complexProperty.ComplexType;
                                var isNullable = containerNullable || complexProperty.IsNullable;
                                var propertyMap = new Dictionary<IProperty, ColumnExpression>();
                                var complexPropertyMap = new Dictionary<IComplexProperty, Expression>();

                                if (complexType.IsMappedToJson())
                                {
                                    var containerColumnName = complexProperty.ComplexType.GetContainerColumnName();
                                    Check.DebugAssert(containerColumnName is not null, "Complex JSON type without a container column");

                                    var containerColumn = table.FindColumn(containerColumnName);
                                    Check.DebugAssert(containerColumn is not null, "Complex JSON container table not found on relational table");

                                    // Since multiple properties from different entity types may have the same column names in their
                                    // respective table, we must uniquify the projected container column name.
                                    var projectedColumnName = Uniquifier.Uniquify(
                                        containerColumn.Name, projectedColumnNames, maxIdentifierLength);
                                    projectedColumnNames.Add(projectedColumnName);

                                    var shaper = GenerateComplexJsonShaper(
                                        complexProperty,
                                        containerColumn: null,
                                        projectedColumnName,
                                        containerColumn.ProviderClrType,
                                        containerColumn.StoreTypeMapping,
                                        tpcTableAlias,
                                        containerNullable,
                                        out var containerColumnExpression);

                                    allPropertyMap.Add((complexProperty, containerColumnExpression));

                                    return shaper;
                                }

                                foreach (var property in complexType.GetProperties())
                                {
                                    propertyMap[property] = ProcessPropertyTpc(property, isNullable);
                                }

                                foreach (var nestedComplexProperty in complexType.GetComplexProperties())
                                {
                                    complexPropertyMap[nestedComplexProperty] =
                                        ProcessComplexPropertyTpc(nestedComplexProperty, isNullable)!;
                                }

                                return new RelationalStructuralTypeShaperExpression(
                                    complexType,
                                    new StructuralTypeProjectionExpression(complexType, propertyMap, complexPropertyMap, isNullable),
                                    isNullable);
                            }
                        }
                    }

                    // We need to add a discriminator column in addition to all the property columns; its name also needs to be uniquified.
                    var discriminatorColumnName = Uniquifier.Uniquify(
                        SelectExpression.DiscriminatorColumnAlias, projectedColumnNames, maxIdentifierLength);

                    var subSelectExpressions = new List<SelectExpression>(concreteEntityTypes.Length);
                    var discriminatorValues = new List<string>(concreteEntityTypes.Length);
                    foreach (var concreteEntityType in concreteEntityTypes)
                    {
                        var table = concreteEntityType.GetViewOrTableMappings().Single().Table;
                        var tableAlias = _sqlAliasManager.GenerateTableAlias(table);
                        var tableExpression = new TableExpression(tableAlias, table);

                        var projections = new List<ProjectionExpression>(allPropertyMap.Count);
                        foreach (var (property, projectedColumn) in allPropertyMap)
                        {
                            var declaringEntityType = property.DeclaringType.ContainingEntityType;

                            // We first check whether this property exists on our concrete entity type (or is it a sibling's);
                            // if not, we add a NULL projection out.
                            if (!concreteEntityType.IsAssignableTo(declaringEntityType))
                            {
                                projections.Add(
                                    new ProjectionExpression(
                                        _sqlExpressionFactory.Constant(value: null, property.ClrType.MakeNullable(), projectedColumn.TypeMapping),
                                        projectedColumn.Name));
                                continue;
                            }

                            var column = property switch
                            {
                                IProperty p => table.FindColumn(p),
                                IComplexProperty p => p.ComplexType.GetContainerColumnName() is string columnName ? table.FindColumn(columnName) : null,
                                _ => throw new UnreachableException()
                            };

                            Debug.Assert(column is not null, "Column not found for property " + property.Name);

                            // Note that the projected name may differ from the column name on the entity's concrete table because of
                            // uniquification (i.e. two TPC entities in the same hierarchy have two properties mapped columns with the
                            // same name)
                            projections.Add(
                                new ProjectionExpression(
                                    new ColumnExpression(
                                        column.Name,
                                        tableAlias,
                                        column.ProviderClrType.UnwrapNullableType(),
                                        column.StoreTypeMapping,
                                        column.IsNullable),
                                    projectedColumn.Name));
                        }

                        // Add a constant projection for the discriminator value
                        projections.Add(
                            new ProjectionExpression(
                                _sqlExpressionFactory.ApplyDefaultTypeMapping(
                                    _sqlExpressionFactory.Constant(concreteEntityType.ShortName())),
                                discriminatorColumnName));
                        discriminatorValues.Add(concreteEntityType.ShortName());

                        subSelectExpressions.Add(
                            SelectExpression.CreateImmutable(alias: null!, [tableExpression], projections, _sqlAliasManager));
                    }

                    var identifier = new List<(ColumnExpression Column, ValueComparer Comparer)>();
                    foreach (var property in entityType.FindPrimaryKey()!.Properties)
                    {
                        var columnExpression = propertyMap[property];
                        identifier.Add((columnExpression, property.GetKeyValueComparer()));
                    }

                    var discriminatorColumn = CreateColumnExpression(subSelectExpressions[0].Projection[^1], tpcTableAlias);
                    var tpcTablesExpression = new TpcTablesExpression(
                        tpcTableAlias, entityType, subSelectExpressions, discriminatorColumn, discriminatorValues);

                    return new SelectExpression(
                        [tpcTablesExpression],
                        new StructuralTypeProjectionExpression(entityType, propertyMap, complexPropertyMap, nullable: false, discriminatorColumn),
                        identifier,
                        _sqlAliasManager);
                }

                case RelationalAnnotationNames.TphMappingStrategy:
                case null:
                {
                    if (entityType.GetFunctionMappings().SingleOrDefault(e => e.IsDefaultFunctionMapping) is { } functionMapping)
                    {
                        var storeFunction = functionMapping.Table;

                        var alias = _sqlAliasManager.GenerateTableAlias(storeFunction);
                        return GenerateSingleTableSelect(
                            entityType, storeFunction, new TableValuedFunctionExpression(alias, (IStoreFunction)storeFunction, []));
                    }

                    var mappings = entityType.GetViewOrTableMappings().ToList();
                    if (mappings is [{ Table: var singleTable }])
                    {
                        var alias = _sqlAliasManager.GenerateTableAlias(singleTable);

                        return GenerateSingleTableSelect(entityType, singleTable, new TableExpression(alias, singleTable));
                    }

                    // Single entity type mapped to multiple tables - this is entity splitting
                    var keyProperties = entityType.FindPrimaryKey()!.Properties;
                    List<ColumnExpression> joinColumns = default!;
                    var propertyMap = new Dictionary<IProperty, ColumnExpression>();
                    var tableMap = new Dictionary<ITableBase, string>();
                    var tables = new List<TableExpressionBase>();
                    var identifier = new List<(ColumnExpression Column, ValueComparer Comparer)>();
                    foreach (var mapping in mappings)
                    {
                        var table = mapping.Table;
                        var alias = _sqlAliasManager.GenerateTableAlias(table);
                        var tableExpression = new TableExpression(alias, table);
                        tableMap[table] = alias;

                        if (tables.Count == 0)
                        {
                            tables.Add(tableExpression);
                            joinColumns = [];
                            foreach (var property in keyProperties)
                            {
                                var columnExpression = CreateColumnExpression(property, table, alias, nullable: false);
                                propertyMap[property] = columnExpression;
                                joinColumns.Add(columnExpression);
                                identifier.Add((columnExpression, property.GetKeyValueComparer()));
                            }
                        }
                        else
                        {
                            var innerColumns = keyProperties.Select(p => CreateColumnExpression(p, table, alias, nullable: false));

                            var joinPredicate = joinColumns
                                .Zip(innerColumns, _sqlExpressionFactory.Equal)
                                .Aggregate(_sqlExpressionFactory.AndAlso);

                            tables.Add(new InnerJoinExpression(tableExpression, joinPredicate, prunable: true));
                        }
                    }

                    foreach (var property in entityType.GetProperties())
                    {
                        if (property.IsPrimaryKey())
                        {
                            continue;
                        }

                        var columnBase = mappings.Select(e => e.Table.FindColumn(property)).First(e => e != null)!;
                        propertyMap[property] = CreateColumnExpression(property, columnBase, tableMap[columnBase.Table], nullable: false);
                    }

                    var complexPropertyMap = new Dictionary<IComplexProperty, Expression>();
                    foreach (var complexProperty in entityType.GetComplexProperties())
                    {
                        var table = complexProperty.ComplexType.GetViewOrTableMappings().Single().Table;
                        complexPropertyMap[complexProperty] = ProcessComplexProperty(complexProperty, table, tableMap[table], containerNullable: false);
                    }

                    var projection = new StructuralTypeProjectionExpression(entityType, propertyMap, complexPropertyMap);
                    AddJsonNavigationBindings(entityType, projection, propertyMap, tableMap);

                    return new SelectExpression(tables, projection, identifier, _sqlAliasManager);
                }

                default:
                    throw new UnreachableException();
            }

            static ITableBase GetTableBaseFiltered(IEntityType entityType, Dictionary<ITableBase, string> existingTables)
                => entityType.GetViewOrTableMappings().Single(m => !existingTables.ContainsKey(m.Table)).Table;
        }
    }

    private SelectExpression GenerateSingleTableSelect(IEntityType entityType, ITableBase table, TableExpressionBase tableExpression)
    {
        var alias = tableExpression.Alias!;

        var propertyMap = new Dictionary<IProperty, ColumnExpression>();
        foreach (var property in entityType.GetPropertiesInHierarchy())
        {
            propertyMap[property] = CreateColumnExpression(property, table, alias, nullable: false);
        }

        var complexPropertyMap = new Dictionary<IComplexProperty, Expression>();
        foreach (var complexProperty in entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
            .SelectMany(t => t.GetDeclaredComplexProperties()))
        {
            complexPropertyMap[complexProperty] = ProcessComplexProperty(complexProperty, table, alias, containerNullable: false);
        }

        var tableMap = new Dictionary<ITableBase, string> { [table] = alias };
        var projection = new StructuralTypeProjectionExpression(entityType, propertyMap, complexPropertyMap);
        AddJsonNavigationBindings(entityType, projection, propertyMap, tableMap);

        var identifier = new List<(ColumnExpression Column, ValueComparer Comparer)>();

        if (entityType.FindPrimaryKey() is { } primaryKey)
        {
            foreach (var property in primaryKey.Properties)
            {
                identifier.Add((propertyMap[property], property.GetKeyValueComparer()));
            }
        }

        return new SelectExpression([tableExpression], projection, identifier, _sqlAliasManager);
    }

    private static Expression ProcessComplexProperty(
        IComplexProperty complexProperty,
        ITableBase table,
        string tableAlias,
        bool containerNullable)
    {
        var complexType = complexProperty.ComplexType;
        var isNullable = containerNullable || complexProperty.IsNullable;
        if (complexType.IsMappedToJson())
        {
            return GenerateComplexJsonShaper(complexProperty, table, tableAlias, containerNullable);
        }

        var propertyMap = new Dictionary<IProperty, ColumnExpression>();
        foreach (var property in complexType.GetProperties())
        {
            propertyMap[property] = CreateColumnExpression(property, table, tableAlias, isNullable);
        }

        var complexPropertyMap = new Dictionary<IComplexProperty, Expression>();
        foreach (var nestedComplexProperty in complexType.GetComplexProperties())
        {
            complexPropertyMap[nestedComplexProperty] = ProcessComplexProperty(nestedComplexProperty, table, tableAlias, isNullable);
        }

        return new RelationalStructuralTypeShaperExpression(
            complexType,
            new StructuralTypeProjectionExpression(complexType, propertyMap, complexPropertyMap, isNullable),
            isNullable);
    }

    private static Expression GenerateComplexJsonShaper(
        IComplexProperty complexProperty,
        ITableBase table,
        string tableAlias,
        bool containerNullable)
    {
        Check.DebugAssert(complexProperty.ComplexType.IsMappedToJson());

        var containerColumnName = complexProperty.ComplexType.GetContainerColumnName();
        Check.DebugAssert(containerColumnName is not null, "Complex JSON type without a container column");

        var containerColumn = table.FindColumn(containerColumnName);
        Check.DebugAssert(containerColumn is not null, "Complex JSON container table not found on relational table");

        return GenerateComplexJsonShaper(
            complexProperty,
            containerColumn,
            containerColumn.Name,
            containerColumn.ProviderClrType,
            containerColumn.StoreTypeMapping,
            tableAlias,
            containerNullable,
            out _);
    }

    private static Expression GenerateComplexJsonShaper(
        IComplexProperty complexProperty,
        IColumnBase? containerColumn,
        string containerColumnName,
        Type containerColumnClrType,
        RelationalTypeMapping containerColumnTypeMapping,
        string tableAlias,
        bool containerNullable,
        out ColumnExpression containerColumnExpression)
    {
        // This method is for JSON complex properties on non-JSON types only; see CreateSelect() which accepts a JsonQueryExpression
        // for JSON-within-JSON.
        Check.DebugAssert(complexProperty.ComplexType.IsMappedToJson());
        Check.DebugAssert(!complexProperty.DeclaringType.IsMappedToJson());

        var complexType = complexProperty.ComplexType;
        var isNullable = containerNullable || complexProperty.IsNullable;

        // If the source type is a JSON complex type; since we're binding over StructuralTypeProjectionExpression - which represents a relational
        // table-like thing - this means that an internal JSON collection has been converted to a relational table (e.g. OPENJSON on SQL Server)
        // and we're now binding over that table.
        // Otherwise, if the source type isn't mapped to JSON, we're just binding to an actual JSON column in a relational table, and not within it.
        containerColumnExpression = new ColumnExpression(
            containerColumnName,
            tableAlias,
            containerColumn,
            containerColumnClrType,
            containerColumnTypeMapping,
            isNullable);

        var jsonQuery = new JsonQueryExpression(
            complexType,
            containerColumnExpression,
            keyPropertyMap: null,
            complexProperty.ClrType,
            complexProperty.IsCollection);

        return complexProperty.IsCollection
            ? new CollectionResultExpression(jsonQuery, complexProperty, elementType: complexType.ClrType)
            : new RelationalStructuralTypeShaperExpression(complexType, jsonQuery, isNullable);
    }

    /// <summary>
    ///     Used to create a <see cref="SelectExpression" /> representing a query of the given entity type, when its table expression has
    ///     already been constructed externally. This overload is used for cases such as <see cref="FromSqlExpression" />,
    ///     <see cref="TableValuedFunctionExpression" />, etc.
    /// </summary>
    private SelectExpression CreateSelect(IEntityType entityType, TableExpressionBase tableExpressionBase)
    {
        if ((entityType.BaseType != null || entityType.GetDirectlyDerivedTypes().Any())
            && entityType.FindDiscriminatorProperty() == null)
        {
            throw new InvalidOperationException(RelationalStrings.SelectExpressionNonTphWithCustomTable(entityType.DisplayName()));
        }

        if (tableExpressionBase is not ITableBasedExpression { Table: ITableBase table })
        {
            throw new UnreachableException("SelectExpression with unexpected missing table");
        }

        var select = GenerateSingleTableSelect(entityType, table, tableExpressionBase);
        AddEntitySelectConditions(select, entityType);

        return select;
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
    private void AddEntitySelectConditions(SelectExpression selectExpression, IEntityType entityType)
    {
        // First add condition for discriminator mapping
        var discriminatorProperty = entityType.FindDiscriminatorProperty();
        if (discriminatorProperty != null
            && (!entityType.GetRootType().GetIsDiscriminatorMappingComplete()
                || !entityType.GetAllBaseTypesInclusiveAscending()
                    .All(e => (e == entityType || e.IsAbstract()) && !HasSiblings(e))))
        {
            var discriminatorColumn = GetMappedProjection(selectExpression).BindProperty(discriminatorProperty);
            var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToList();
            var predicate = _sqlExpressionFactory.In(
                discriminatorColumn,
                concreteEntityTypes
                    .Select(et => _sqlExpressionFactory.Constant(et.GetDiscriminatorValue(), discriminatorColumn.Type))
                    .ToArray());

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
            var projection = GetMappedProjection(selectExpression);
            var requiredNonPkProperties = entityType.GetProperties().Where(p => !p.IsNullable && !p.IsPrimaryKey()).ToList();
            if (requiredNonPkProperties.Count > 0)
            {
                predicate = requiredNonPkProperties.Select(e => IsNotNull(e, projection))
                    .Aggregate(_sqlExpressionFactory.AndAlso);
            }

            var allNonSharedNonPkProperties = entityType.GetNonPrincipalSharedNonPkProperties(table);
            // We don't need condition for nullable property if there exist at least one required property which is non shared.
            if (allNonSharedNonPkProperties.Count != 0
                && allNonSharedNonPkProperties.All(p => p.IsNullable))
            {
                var atLeastOneNonNullValueInNullablePropertyCondition = allNonSharedNonPkProperties
                    .Select(p => IsNotNull(p, projection))
                    .Aggregate(_sqlExpressionFactory.OrElse);

                predicate = predicate == null
                    ? atLeastOneNonNullValueInNullablePropertyCondition
                    : _sqlExpressionFactory.AndAlso(predicate, atLeastOneNonNullValueInNullablePropertyCondition);
            }

            if (predicate != null)
            {
                selectExpression.ApplyPredicate(predicate);
            }
        }

        return;

        bool HasSiblings(IEntityType entityType)
            => entityType.BaseType?.GetDirectlyDerivedTypes().Any(i => i != entityType) == true;

        static StructuralTypeProjectionExpression GetMappedProjection(SelectExpression selectExpression)
            => (StructuralTypeProjectionExpression)selectExpression.GetProjection(
                new ProjectionBindingExpression(selectExpression, new ProjectionMember(), typeof(ValueBuffer)));

        SqlExpression IsNotNull(IProperty property, StructuralTypeProjectionExpression projection)
            => _sqlExpressionFactory.IsNotNull(projection.BindProperty(property));
    }

    private void AddJsonNavigationBindings(
        IEntityType entityType,
        StructuralTypeProjectionExpression projection,
        Dictionary<IProperty, ColumnExpression> propertyExpressions,
        Dictionary<ITableBase, string> tableMap)
    {
        foreach (var ownedJsonNavigation in entityType.GetNavigationsInHierarchy()
                     .Where(n => n.ForeignKey.IsOwnership
                         && n.TargetEntityType.IsMappedToJson()
                         && n.ForeignKey.PrincipalToDependent == n))
        {
            // Find the containing column for the owned JSON entity type, and then the table in the table map that
            // contains that column.
            var targetEntityType = ownedJsonNavigation.TargetEntityType;
            var containerColumnName = targetEntityType.GetContainerColumnName() ?? throw new UnreachableException();
            var (containerColumn, tableAlias) = tableMap
                .Select(kvp => (Column: kvp.Key.FindColumn(containerColumnName), TableAlias: kvp.Value))
                .SingleOrDefault(c => c.Column is not null);

            Check.DebugAssert(
                containerColumn is not null,
                $"JSON container column '{containerColumnName}' not found in table map for owned JSON entity type '{targetEntityType.DisplayName()}' on '{entityType.DisplayName()}'");

            var containerColumnTypeMapping = containerColumn!.StoreTypeMapping;
            var isNullable = containerColumn.IsNullable
                || !ownedJsonNavigation.ForeignKey.IsRequiredDependent
                || ownedJsonNavigation.IsCollection;

            var column = new ColumnExpression(
                containerColumnName,
                tableAlias,
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual SelectExpression CreateSelect(
        JsonQueryExpression jsonQueryExpression,
        TableExpressionBase tableExpressionBase,
        string identifierColumnName,
        Type identifierColumnType,
        RelationalTypeMapping identifierColumnTypeMapping)
    {
        if (!jsonQueryExpression.IsCollection)
        {
            throw new ArgumentException(RelationalStrings.SelectCanOnlyBeBuiltOnCollectionJsonQuery, nameof(jsonQueryExpression));
        }

        var structuralType = jsonQueryExpression.StructuralType;
        var jsonColumn = jsonQueryExpression.JsonColumn;
        var tableAlias = tableExpressionBase.Alias!;

        Check.DebugAssert(
            structuralType.BaseType is null && !structuralType.GetDirectlyDerivedTypes().Any(),
            "Inheritance encountered inside a JSON document");

        // Create a dictionary mapping all properties to their ColumnExpressions, for the SelectExpression's projection.
        var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
        foreach (var property in structuralType.GetPropertiesInHierarchy())
        {
            // For owned JSON mapping, add column(s) representing key of the parent (non-JSON) entity, on top of all the projections from OPENJSON/json_each/etc.
            if (jsonQueryExpression.KeyPropertyMap?.TryGetValue(property, out var ownerKeyColumn) == true)
            {
                propertyExpressions[property] = ownerKeyColumn;
                continue;
            }

            // Skip also properties with no JSON name (i.e. shadow keys containing the index in the collection, which don't actually exist
            // in the JSON document and can't be bound to)
            if (property.GetJsonPropertyName() is { } jsonPropertyName)
            {
                propertyExpressions[property] = CreateColumnExpression(
                    tableExpressionBase, jsonPropertyName, property.ClrType, property.GetRelationalTypeMapping(),
                    /* jsonQueryExpression.IsNullable || */ property.IsNullable); // TODO:
            }
        }

        var complexPropertyExpressions = new Dictionary<IComplexProperty, Expression>();
        foreach (var complexProperty in structuralType.GetComplexProperties())
        {
            var complexType = complexProperty.ComplexType;
            Check.DebugAssert(complexType.IsMappedToJson());
            Check.DebugAssert(complexProperty.DeclaringType.IsMappedToJson());

            var isNullable = jsonQueryExpression.IsNullable || complexProperty.IsNullable;

            var containerColumnExpression = new ColumnExpression(
                complexType.GetJsonPropertyName()
                ?? throw new UnreachableException($"No JSON property name for complex property {complexProperty.Name}"),
                tableAlias,
                jsonColumn.Type,
                jsonColumn.TypeMapping,
                isNullable);

            var jsonQuery = new JsonQueryExpression(
                complexType,
                containerColumnExpression,
                keyPropertyMap: null,
                complexProperty.ClrType,
                complexProperty.IsCollection);

            complexPropertyExpressions[complexProperty] = complexProperty.IsCollection
                ? new CollectionResultExpression(jsonQuery, complexProperty, elementType: complexType.ClrType)
                : new RelationalStructuralTypeShaperExpression(complexType, jsonQuery, isNullable);
        }

        var projection = new StructuralTypeProjectionExpression(structuralType, propertyExpressions, complexPropertyExpressions);

        // Go over all owned JSON navigations and pre-populate bindings for them - these get used later if the LINQ query binds to them.
        if (structuralType is IEntityType entityType)
        {
            foreach (var ownedJsonNavigation in entityType.GetNavigationsInHierarchy()
                         .Where(n => n.ForeignKey.IsOwnership
                             && n.TargetEntityType.IsMappedToJson()
                             && n.ForeignKey.PrincipalToDependent == n))
            {
                var targetEntityType = ownedJsonNavigation.TargetEntityType;
                var jsonNavigationName = ownedJsonNavigation.TargetEntityType.GetJsonPropertyName();
                Check.DebugAssert(jsonNavigationName is not null, "Invalid navigation found on JSON-mapped entity");
                var isNullable = jsonQueryExpression.IsNullable
                    || !ownedJsonNavigation.ForeignKey.IsRequiredDependent
                    || ownedJsonNavigation.IsCollection;

                // The TableExpressionBase represents a relational expansion of the JSON collection. We now need a ColumnExpression to represent
                // the specific JSON property (projected as a relational column) which holds the JSON subtree for the target entity.
                var column = new ColumnExpression(jsonNavigationName, tableAlias, jsonColumn.Type, jsonColumn.TypeMapping, isNullable);

                // need to remap key property map to use target entity key properties
                var newKeyPropertyMap = new Dictionary<IProperty, ColumnExpression>();
                var targetPrimaryKeyProperties =
                    targetEntityType.FindPrimaryKey()!.Properties.Take(jsonQueryExpression.KeyPropertyMap!.Count);
                var sourcePrimaryKeyProperties =
                    entityType.FindPrimaryKey()!.Properties.Take(jsonQueryExpression.KeyPropertyMap.Count);
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
        }

        var identifierColumn = new ColumnExpression(
            identifierColumnName,
            tableAlias,
            identifierColumnType.UnwrapNullableType(),
            identifierColumnTypeMapping,
            identifierColumnType.IsNullableType());

        return new SelectExpression(
            [tableExpressionBase],
            projection,
            [(identifierColumn, identifierColumnTypeMapping.Comparer)],
            _sqlAliasManager);
    }

    private static ColumnExpression CreateColumnExpression(
        IProperty property,
        ITableBase table,
        string tableAlias,
        bool nullable)
        => CreateColumnExpression(property, table.FindColumn(property)!, tableAlias, nullable);

    private static ColumnExpression CreateColumnExpression(
        IProperty property,
        IColumnBase column,
        string tableAlias,
        bool nullable)
        => new(
            column.Name,
            tableAlias,
            column,
            property.ClrType.UnwrapNullableType(),
            column.PropertyMappings.First(m => m.Property == property).TypeMapping,
            nullable || column.IsNullable);

    private static ColumnExpression CreateColumnExpression(ProjectionExpression subqueryProjection, string tableAlias)
        => new(
            subqueryProjection.Alias,
            tableAlias,
            column: subqueryProjection.Expression is ColumnExpression { Column: { } column } ? column : null,
            subqueryProjection.Type,
            subqueryProjection.Expression.TypeMapping!,
            subqueryProjection.Expression switch
            {
                ColumnExpression c => c.IsNullable,
                SqlConstantExpression c => c.Value is null,
                _ => true
            });

    private static ColumnExpression CreateColumnExpression(
        TableExpressionBase tableExpression,
        string columnName,
        Type type,
        RelationalTypeMapping? typeMapping,
        bool? columnNullable = null)
        => new(
            columnName,
            tableExpression.GetRequiredAlias(),
            type.UnwrapNullableType(),
            typeMapping,
            columnNullable ?? type.IsNullableType());
}
