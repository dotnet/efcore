// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
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
                    var columns = new Dictionary<IProperty, ColumnExpression>();
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
                            columns[property] = CreateColumnExpression(property, table, alias, nullable: false);
                        }

                        if (tables.Count == 0)
                        {
                            tables.Add(tableExpression);
                            joinColumns = [];
                            foreach (var property in keyProperties)
                            {
                                var columnExpression = columns[property];
                                joinColumns.Add(columnExpression);
                                identifier.Add((columnExpression, property.GetKeyValueComparer()));
                            }
                        }
                        else
                        {
                            var innerColumns = keyProperties.Select(
                                p => CreateColumnExpression(p, table, alias, nullable: false));

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
                            columns[property] = CreateColumnExpression(property, table, alias, nullable: true);
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
                        entityType, columns, tableMap, nullable: false, discriminatorExpression);

                    return new SelectExpression(tables, projection, identifier, _sqlAliasManager);
                }

                case RelationalAnnotationNames.TpcMappingStrategy:
                {
                    // Drop additional table if ofType/is operator used Issue#27957
                    var entityTypes = entityType.GetDerivedTypesInclusive().Where(e => !e.IsAbstract()).ToArray();
                    if (entityTypes.Length == 1)
                    {
                        // For single entity case, we don't need discriminator.
                        var table = entityTypes[0].GetViewOrTableMappings().Single().Table;
                        var alias = _sqlAliasManager.GenerateTableAlias(table);
                        var tableMap = new Dictionary<ITableBase, string> { [table] = alias };

                        var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
                        foreach (var property in entityType.GetPropertiesInHierarchy())
                        {
                            propertyExpressions[property] = CreateColumnExpression(property, table, alias, nullable: false);
                        }

                        var identifier = new List<(ColumnExpression Column, ValueComparer Comparer)>();
                        var primaryKey = entityType.FindPrimaryKey();
                        if (primaryKey != null)
                        {
                            foreach (var property in primaryKey.Properties)
                            {
                                identifier.Add((propertyExpressions[property], property.GetKeyValueComparer()));
                            }
                        }

                        return new SelectExpression(
                            [new TableExpression(alias, table)],
                            new StructuralTypeProjectionExpression(entityType, propertyExpressions, tableMap),
                            identifier,
                            _sqlAliasManager);
                    }
                    else
                    {
                        var tables = entityTypes.Select(e => e.GetViewOrTableMappings().Single().Table).ToArray();
                        var properties = entityType.GetFlattenedPropertiesInHierarchy().ToArray();
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
                        var discriminatorColumnName = SelectExpression.DiscriminatorColumnAlias;
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
                            var tableAlias = _sqlAliasManager.GenerateTableAlias(table);
                            var tableExpression = new TableExpression(tableAlias, table);

                            var projections = new List<ProjectionExpression>();
                            for (var j = 0; j < properties.Length; j++)
                            {
                                var property = properties[j];
                                var declaringEntityType = property.DeclaringType.ContainingEntityType;
                                var projection = declaringEntityType.IsAssignableFrom(concreteEntityType)
                                    ? CreateColumnExpression(property, table, tableAlias, declaringEntityType != entityType)
                                    : (SqlExpression)_sqlExpressionFactory.Constant(
                                        null, property.ClrType.MakeNullable(), property.GetRelationalTypeMapping());
                                projections.Add(new ProjectionExpression(projection, propertyNames[j]));
                            }

                            projections.Add(
                                new ProjectionExpression(
                                    _sqlExpressionFactory.ApplyDefaultTypeMapping(
                                        _sqlExpressionFactory.Constant(concreteEntityType.ShortName())),
                                    discriminatorColumnName));
                            discriminatorValues.Add(concreteEntityType.ShortName());

                            subSelectExpressions.Add(SelectExpression.CreateImmutable(alias: null!, [tableExpression], projections));
                        }

                        var tpcTableAlias = _sqlAliasManager.GenerateTableAlias("union");

                        var firstSelectExpression = subSelectExpressions[0];
                        var columns = new Dictionary<IProperty, ColumnExpression>();
                        for (var i = 0; i < properties.Length; i++)
                        {
                            columns[properties[i]] = CreateColumnExpression(firstSelectExpression.Projection[i], tpcTableAlias);
                        }

                        var identifier = new List<(ColumnExpression Column, ValueComparer Comparer)>();
                        foreach (var property in entityType.FindPrimaryKey()!.Properties)
                        {
                            var columnExpression = columns[property];
                            identifier.Add((columnExpression, property.GetKeyValueComparer()));
                        }

                        var discriminatorColumn = CreateColumnExpression(firstSelectExpression.Projection[^1], tpcTableAlias);
                        var tpcTables = new TpcTablesExpression(
                            tpcTableAlias, entityType, subSelectExpressions, discriminatorColumn, discriminatorValues);
                        var tableMap = tables.ToDictionary(t => t, _ => tpcTableAlias);

                        return new SelectExpression(
                            [tpcTables],
                            new StructuralTypeProjectionExpression(entityType, columns, tableMap, nullable: false, discriminatorColumn),
                            identifier,
                            _sqlAliasManager);
                    }
                }

                default:
                {
                    // Also covers TPH
                    if (entityType.GetFunctionMappings().SingleOrDefault(e => e.IsDefaultFunctionMapping) is IFunctionMapping
                        functionMapping)
                    {
                        var storeFunction = functionMapping.Table;

                        var alias = _sqlAliasManager.GenerateTableAlias(storeFunction);
                        return GenerateNonHierarchyNonSplittingEntityType(
                            storeFunction, new TableValuedFunctionExpression(alias, (IStoreFunction)storeFunction, []));
                    }

                    var mappings = entityType.GetViewOrTableMappings().ToList();
                    if (mappings.Count == 1)
                    {
                        var table = mappings[0].Table;
                        var alias = _sqlAliasManager.GenerateTableAlias(table);

                        return GenerateNonHierarchyNonSplittingEntityType(table, new TableExpression(alias, table));
                    }

                    // entity splitting
                    var keyProperties = entityType.FindPrimaryKey()!.Properties;
                    List<ColumnExpression> joinColumns = default!;
                    var columns = new Dictionary<IProperty, ColumnExpression>();
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
                                columns[property] = columnExpression;
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
                        columns[property] = CreateColumnExpression(
                            property, columnBase, tableMap[columnBase.Table], nullable: false);
                    }

                    return new SelectExpression(
                        tables,
                        new StructuralTypeProjectionExpression(entityType, columns, tableMap),
                        identifier,
                        _sqlAliasManager);
                }
            }

            SelectExpression GenerateNonHierarchyNonSplittingEntityType(ITableBase table, TableExpressionBase tableExpression)
            {
                var alias = tableExpression.Alias!;

                var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
                foreach (var property in entityType.GetPropertiesInHierarchy())
                {
                    propertyExpressions[property] = CreateColumnExpression(property, table, alias, nullable: false);
                }

                var projection = new StructuralTypeProjectionExpression(
                    entityType,
                    propertyExpressions,
                    new Dictionary<ITableBase, string> { [table] = alias });
                AddJsonNavigationBindings(entityType, projection, propertyExpressions, alias);

                var identifier = new List<(ColumnExpression Column, ValueComparer Comparer)>();
                var primaryKey = entityType.FindPrimaryKey();
                if (primaryKey != null)
                {
                    foreach (var property in primaryKey.Properties)
                    {
                        identifier.Add((propertyExpressions[property], property.GetKeyValueComparer()));
                    }
                }

                return new SelectExpression([tableExpression], projection, identifier, _sqlAliasManager);
            }

            static ITableBase GetTableBaseFiltered(IEntityType entityType, Dictionary<ITableBase, string> existingTables)
                => entityType.GetViewOrTableMappings().Single(m => !existingTables.ContainsKey(m.Table)).Table;
        }
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

        var table = (tableExpressionBase as ITableBasedExpression)?.Table;
        Check.DebugAssert(table is not null, "SelectExpression with unexpected missing table");

        var alias = tableExpressionBase.Alias!;
        var tableMap = new Dictionary<ITableBase, string> { [table] = alias };

        var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();

        foreach (var property in entityType.GetPropertiesInHierarchy())
        {
            propertyExpressions[property] = CreateColumnExpression(property, table, alias, nullable: false);
        }

        var projection = new StructuralTypeProjectionExpression(entityType, propertyExpressions, tableMap);
        AddJsonNavigationBindings(entityType, projection, propertyExpressions, alias);

        var identifier = new List<(ColumnExpression Column, ValueComparer Comparer)>();
        var primaryKey = entityType.FindPrimaryKey();
        if (primaryKey != null)
        {
            foreach (var property in primaryKey.Properties)
            {
                identifier.Add((propertyExpressions[property], property.GetKeyValueComparer()));
            }
        }

        var select = new SelectExpression([tableExpressionBase], projection, identifier, _sqlAliasManager);

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
            var predicate = concreteEntityTypes.Count == 1
                ? (SqlExpression)_sqlExpressionFactory.Equal(
                    discriminatorColumn,
                    _sqlExpressionFactory.Constant(concreteEntityTypes[0].GetDiscriminatorValue(), discriminatorColumn.Type))
                : _sqlExpressionFactory.In(
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
                    .Select(e => IsNotNull(e, projection))
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
        string tableAlias)
    {
        foreach (var ownedJsonNavigation in entityType.GetNavigationsInHierarchy()
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

        var entityType = jsonQueryExpression.EntityType;

        Check.DebugAssert(
            entityType.BaseType is null && !entityType.GetDirectlyDerivedTypes().Any(),
            "Inheritance encountered inside a JSON document");

        // Create a dictionary mapping all properties to their ColumnExpressions, for the SelectExpression's projection.
        var propertyExpressions = new Dictionary<IProperty, ColumnExpression>();

        foreach (var property in entityType.GetPropertiesInHierarchy())
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
        var tableAlias = tableExpressionBase.Alias!;

        // TODO: We'll need to make sure this is correct when we add support for JSON complex types.
        var tableMap = new Dictionary<ITableBase, string> { [table] = tableAlias };

        var projection = new StructuralTypeProjectionExpression(
            entityType,
            propertyExpressions,
            tableMap);

        var containerColumnName = entityType.GetContainerColumnName()!;
        var containerColumn = table.FindColumn(containerColumnName)!;
        var containerColumnTypeMapping = containerColumn.StoreTypeMapping;
        foreach (var ownedJsonNavigation in entityType.GetNavigationsInHierarchy()
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
            var column = new ColumnExpression(
                jsonNavigationName,
                tableAlias,
                containerColumnTypeMapping.ClrType,
                containerColumnTypeMapping,
                isNullable);

            // need to remap key property map to use target entity key properties
            var newKeyPropertyMap = new Dictionary<IProperty, ColumnExpression>();
            var targetPrimaryKeyProperties = targetEntityType.FindPrimaryKey()!.Properties.Take(jsonQueryExpression.KeyPropertyMap.Count);
            var sourcePrimaryKeyProperties =
                jsonQueryExpression.EntityType.FindPrimaryKey()!.Properties.Take(jsonQueryExpression.KeyPropertyMap.Count);
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
        => new(column.Name,
            tableAlias,
            property.ClrType.UnwrapNullableType(),
            column.PropertyMappings.First(m => m.Property == property).TypeMapping,
            nullable || column.IsNullable);

    private static ColumnExpression CreateColumnExpression(ProjectionExpression subqueryProjection, string tableAlias)
        => new(
            subqueryProjection.Alias,
            tableAlias,
            subqueryProjection.Type,
            subqueryProjection.Expression.TypeMapping!,
            subqueryProjection.Expression switch
            {
                ColumnExpression columnExpression => columnExpression.IsNullable,
                SqlConstantExpression sqlConstantExpression => sqlConstantExpression.Value == null,
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
