// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

public partial class RelationalQueryableMethodTranslatingExpressionVisitor
{
    /// <inheritdoc />
    [Experimental(EFDiagnostics.ExecuteMergeExperimental)]
    protected override Expression TranslateExecuteMerge(
        ShapedQueryExpression source,
        QueryParameterExpression sourceRows,
        IReadOnlyList<ExecuteMergeMatch> matches,
        IReadOnlyList<ExecuteUpdateSetter>? whenMatchedSetters,
        IReadOnlyList<ExecuteUpdateSetter>? whenNotMatchedSetters,
        LambdaExpression? returningSelector)
    {
        if (source.QueryExpression is not SelectExpression
            {
                Tables: [TableExpression targetTable],
                Predicate: null,
                Offset: null,
                Limit: null,
                IsDistinct: false,
                GroupBy: [],
                Having: null,
                Orderings: []
            })
        {
            throw new InvalidOperationException(RelationalStrings.ExecuteMergeOnComplexQuery);
        }

        if (source.ShaperExpression is not StructuralTypeShaperExpression { StructuralType: IEntityType entityType })
        {
            throw new InvalidOperationException(RelationalStrings.ExecuteMergeOnNonEntityType);
        }

        switch (entityType.GetMappingStrategy())
        {
            case RelationalAnnotationNames.TptMappingStrategy:
                throw new InvalidOperationException(
                    RelationalStrings.ExecuteOperationOnTPT(
                        nameof(EntityFrameworkQueryableExtensions.ExecuteMerge), entityType.DisplayName()));

            // Note that we do allow TPC if the target is a leaf type
            case RelationalAnnotationNames.TpcMappingStrategy when entityType.GetDirectlyDerivedTypes().Any():
                throw new InvalidOperationException(
                    RelationalStrings.ExecuteOperationOnTPC(
                        nameof(EntityFrameworkQueryableExtensions.ExecuteMerge), entityType.DisplayName()));
        }

        // TPH with more than one type in the hierarchy isn't supported: entityType.GetProperties() below only returns this type's own
        // (inherited) properties, so a default insert can't populate the discriminator or sibling types' columns correctly.
        if (entityType.FindDiscriminatorProperty() != null)
        {
            throw new InvalidOperationException(RelationalStrings.ExecuteMergeOnTph(entityType.DisplayName()));
        }

        // Find the table model that maps to the entity type; there must be exactly one (e.g. no entity splitting).
        var table = entityType.GetTableMappings().ToList() switch
        {
            [] => throw new InvalidOperationException(
                RelationalStrings.ExecuteUpdateDeleteOnEntityNotMappedToTable(entityType.DisplayName())),
            [var singleTableMapping] => singleTableMapping.Table,
            _ => throw new InvalidOperationException(
                RelationalStrings.ExecuteOperationOnEntitySplitting(
                    nameof(EntityFrameworkQueryableExtensions.ExecuteMerge), entityType.DisplayName()))
        };

        // If the entity is also mapped to a view, the query refers to the view (translation assumes we're querying); target the
        // mutable table instead, as ExecuteDelete does.
        if (targetTable.Table != table)
        {
            targetTable = new TableExpression(targetTable.Alias, table);
        }

        // Table splitting: other, unrelated entity types mapped to the same table would have required columns this merge doesn't know
        // to populate.
        if (AreOtherNonOwnedEntityTypesInTheTable(entityType.GetRootType(), table))
        {
            throw new InvalidOperationException(RelationalStrings.ExecuteMergeOnTableSplitting(table.SchemaQualifiedName));
        }

        // Complex property columns can't be populated by the insert, which only sees the entity type's own properties. (Owned types
        // don't get here: their auto-included navigations fail the single entity type check above.)
        if (entityType.GetComplexProperties().Any())
        {
            throw new InvalidOperationException(RelationalStrings.ExecuteMergeOnComplexProperties(entityType.DisplayName()));
        }

        // Determine the conflict (match) columns and, for each, how to read its value from a source row. No Match() call => default to
        // the primary key, matched by the same-named source member. The conflict-column value must come from the source-side selector of
        // the match, not the same-named source member, so that Match(t => t.Id, s => s.ExternalId) actually matches on ExternalId.
        string[] conflictColumns;
        var conflictSourceMembers = new Dictionary<string, MemberInfo>();
        var conflictProperties = new List<(IProperty Property, string SourceMember)>();
        if (matches.Count == 0)
        {
            (conflictColumns, var pkProperties) = ResolvePrimaryKeyColumnsAndProperties(entityType, table);
            conflictProperties.AddRange(pkProperties.Select(p => (p, p.Name)));
        }
        else
        {
            var columns = new List<string>();
            foreach (var match in matches)
            {
                var (targetColumns, targetProperties) = ResolveColumnNamesAndProperties(match.TargetKeySelector, entityType, table);
                var sourceMembers = ResolveKeySourceMembers(match.SourceKeySelector);
                Check.DebugAssert(
                    targetColumns.Length == sourceMembers.Length, "Match target and source key selectors have different arities.");
                for (var i = 0; i < targetColumns.Length; i++)
                {
                    if (!conflictSourceMembers.TryAdd(targetColumns[i], sourceMembers[i]))
                    {
                        throw new InvalidOperationException(RelationalStrings.ExecuteMergeDuplicateMatchColumn(targetColumns[i]));
                    }

                    columns.Add(targetColumns[i]);
                    conflictProperties.Add((targetProperties[i], sourceMembers[i].Name));
                }
            }

            conflictColumns = [.. columns];
        }

        // The conflict check compares the value the database computes for the inserted row, not the source's match value, so a computed
        // match column would silently match on a different value.
        var storeObject = StoreObjectIdentifier.Table(table.Name, table.Schema);
        foreach (var (property, _) in conflictProperties)
        {
            if (property.GetComputedColumnSql(storeObject) is not null)
            {
                throw new InvalidOperationException(RelationalStrings.ExecuteMergeComputedMatchColumn(property.Name));
            }
        }

        // Determine the columns (and per-row values) to insert. When WhenNotMatched isn't specified, insert every mapped column using
        // the same-named source member (except for conflict columns, which use the match source selector); otherwise use the explicit
        // insert setters. The source rows are a query parameter, so a single template row is built here and expanded into the actual
        // rows when the query is executed.
        var insertProperties = ResolveInsertProperties(entityType, table, storeObject, whenNotMatchedSetters, conflictProperties);
        var sourceType = sourceRows.Type.TryGetSequenceType() ?? typeof(object);
        var insertColumns = new string[insertProperties.Count];
        var valueMembers = new MemberInfo?[insertProperties.Count];
        var fixedValues = new SqlExpression?[insertProperties.Count];
        var columnTypeMappings = new RelationalTypeMapping[insertProperties.Count];

        // WhenMatched can only see source values through the inserted ("excluded") row, so track which insert columns, if any, carry
        // each source member unchanged.
        var excludedColumnsBySourceMember = new Dictionary<string, List<(IColumnBase Column, IProperty Property)>>();
        for (var i = 0; i < insertProperties.Count; i++)
        {
            var (property, valueExpression) = insertProperties[i];
            var column = table.FindColumn(property)
                ?? throw new InvalidOperationException(RelationalStrings.ExecuteMergePropertyNotMapped(property.Name, table.Name));
            insertColumns[i] = column.Name;
            columnTypeMappings[i] = column.StoreTypeMapping;
            string? sourceMember = null;
            switch (valueExpression)
            {
                // Only a direct source member is supported: compiling an arbitrary selector would fail on (or bake in the value of)
                // funcletized parameters such as a captured variable.
                case LambdaExpression valueSelector:
                    sourceMember = GetDirectSourceMemberName(valueSelector.Body, valueSelector.Parameters[0])
                        ?? throw new InvalidOperationException(RelationalStrings.ExecuteMergeUnsupportedExpression(valueSelector.Print()));
                    valueMembers[i] = FindSourceMember(sourceType, sourceMember);
                    break;

                // A constant insert value from the SetProperty(property, value) overload is funcletized to a query parameter; emit
                // it as a SQL parameter (bound from the query context) rather than trying to evaluate it client-side.
                case QueryParameterExpression queryParameter:
                    fixedValues[i] = _sqlExpressionFactory.ApplyTypeMapping(
                        new SqlParameterExpression(queryParameter.Name, queryParameter.Type, typeMapping: null), column.StoreTypeMapping);
                    break;

                case ConstantExpression constant:
                    if (constant.Value is not null
                        && !property.ClrType.UnwrapNullableType().IsInstanceOfType(constant.Value))
                    {
                        throw new InvalidOperationException(RelationalStrings.ExecuteMergeUnsupportedExpression(constant.Print()));
                    }

                    fixedValues[i] = new SqlConstantExpression(constant.Value, property.ClrType, column.StoreTypeMapping);
                    break;

                // No explicit insert setter: use the match source selector for conflict columns, otherwise the same-named source member.
                case null when conflictSourceMembers.TryGetValue(column.Name, out var conflictSourceMember):
                    valueMembers[i] = conflictSourceMember;
                    sourceMember = conflictSourceMember.Name;
                    break;

                // Read the same-named member off the source row. The source type may differ from the target entity (e.g. a DTO), so
                // we can't use the target property's getter here — look the member up on the actual source type instead.
                case null:
                    valueMembers[i] = FindSourceMember(sourceType, property.Name);
                    sourceMember = property.Name;
                    break;

                default:
                    throw new InvalidOperationException(RelationalStrings.ExecuteMergeUnsupportedExpression(valueExpression.Print()));
            }

            if (sourceMember is not null)
            {
                if (!excludedColumnsBySourceMember.TryGetValue(sourceMember, out var excludedColumns))
                {
                    excludedColumnsBySourceMember[sourceMember] = excludedColumns = [];
                }

                excludedColumns.Add((column, property));
            }
        }

        var templateValues = new SqlExpression[insertProperties.Count];
        for (var i = 0; i < insertProperties.Count; i++)
        {
            templateValues[i] = fixedValues[i]
                ?? new MergeSourceMemberExpression(valueMembers[i]!, insertProperties[i].Property.ClrType, columnTypeMappings[i]);
        }

        // WhenMatched value selectors reference the existing target row (and the incoming source row), so they must be translated to
        // SQL rather than evaluated: target references become target columns, source references become the "excluded" pseudo-row.
        // A WhenMatched without setters leaves matched rows unchanged, as when it's omitted.
        IReadOnlyList<MergeColumnSetter>? updateSetters = null;
        if (whenMatchedSetters is { Count: > 0 })
        {
            var setters = new List<MergeColumnSetter>(whenMatchedSetters.Count);
            var setProperties = new HashSet<IProperty>();
            foreach (var setter in whenMatchedSetters)
            {
                var property = ResolveProperty(setter.PropertySelector, entityType);
                if (!setProperties.Add(property))
                {
                    throw new InvalidOperationException(RelationalStrings.ExecuteMergeDuplicateUpdateProperty(property.Name));
                }

                var column = table.FindColumn(property)
                    ?? throw new InvalidOperationException(RelationalStrings.ExecuteMergePropertyNotMapped(property.Name, table.Name));

                SqlExpression value;
                if (setter.ValueExpression is LambdaExpression valueLambda)
                {
                    var targetParam = valueLambda.Parameters[0];
                    var sourceParam = valueLambda.Parameters.Count > 1 ? valueLambda.Parameters[1] : null;
                    value = TranslateMergeUpdateValue(
                        valueLambda.Body, targetParam, sourceParam, property, column, entityType, table, excludedColumnsBySourceMember);
                }
                else
                {
                    value = TranslateMergeUpdateValue(
                        setter.ValueExpression, targetParam: null, sourceParam: null, property, column, entityType, table,
                        excludedColumnsBySourceMember);
                }

                value = _sqlExpressionFactory.ApplyTypeMapping(value, column.StoreTypeMapping)!;
                setters.Add(new MergeColumnSetter(column.Name, value));
            }

            updateSetters = setters;
        }

        IReadOnlyList<ProjectionExpression>? returning = null;
        LambdaExpression? returningShaper = null;
        if (returningSelector is not null)
        {
            (returning, returningShaper) = BuildReturning(returningSelector, entityType, table);
        }

        return new MergeExpression(
            targetTable, insertColumns, [new RowValueExpression(templateValues)], conflictColumns, updateSetters, returning, returningShaper,
            sourceRows.Name);
    }

    private static readonly MethodInfo IsDbNullMethod =
        typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.IsDBNull), [typeof(int)])!;

    // Builds the RETURNING projection columns and a shaper that materializes each returned target row into the result type. The shaper
    // reads the returned columns positionally (matching the RETURNING order) and evaluates the original returning selector over them.
    private (IReadOnlyList<ProjectionExpression>, LambdaExpression) BuildReturning(
        LambdaExpression returningSelector,
        IEntityType entityType,
        ITableBase table)
    {
        var targetParam = returningSelector.Parameters[0];
        var readerParam = Expression.Parameter(typeof(DbDataReader), "dataReader");
        var projections = new List<ProjectionExpression>();
        var ordinals = new Dictionary<IProperty, int>();

        var rewriter = new MergeReturningShaperRewriter(this, targetParam, readerParam, entityType, table, projections, ordinals);
        var body = rewriter.Visit(returningSelector.Body);

        // A selector that reads no target column (e.g. t => 1) would produce an empty RETURNING clause.
        if (projections.Count == 0)
        {
            throw new InvalidOperationException(RelationalStrings.ExecuteMergeReturningUnsupportedSelector(returningSelector.Print()));
        }

        var delegateType = typeof(Func<,,,,>).MakeGenericType(
            typeof(QueryContext), typeof(DbDataReader), typeof(ResultContext), typeof(SingleQueryResultCoordinator),
            returningSelector.ReturnType);

        var shaper = Expression.Lambda(
            delegateType,
            body,
            Expression.Parameter(typeof(QueryContext), "queryContext"),
            readerParam,
            Expression.Parameter(typeof(ResultContext), "resultContext"),
            Expression.Parameter(typeof(SingleQueryResultCoordinator), "resultCoordinator"));

        return (projections, shaper);
    }

    private Expression CreateReturningColumnRead(
        ParameterExpression reader,
        int ordinal,
        RelationalTypeMapping typeMapping,
        Type resultType)
    {
        var getMethod = typeMapping.GetDataReaderMethod();

        // When buffering, the reader is a buffered one rather than the provider's, but it returns any column through the DbDataReader
        // methods (see RelationalShapedQueryCompilingExpressionVisitor.CreateMergeReturningReaderColumns).
        Expression value = QueryCompilationContext.IsBuffering
            ? Expression.Call(reader, RelationalTypeMapping.GetDataReaderMethod(getMethod.ReturnType), Expression.Constant(ordinal))
            : Expression.Call(
                getMethod.DeclaringType != typeof(DbDataReader) ? Expression.Convert(reader, getMethod.DeclaringType!) : reader,
                getMethod,
                Expression.Constant(ordinal));

        value = typeMapping.CustomizeDataReaderExpression(value);

        Expression nullValue = Expression.Default(resultType);
        if (typeMapping.Converter is ValueConverter converter)
        {
            if (value.Type != converter.ProviderClrType)
            {
                value = Expression.Convert(value, converter.ProviderClrType);
            }

            var convert = converter.ConvertFromProviderExpression;
            value = ReplacingExpressionVisitor.Replace(convert.Parameters[0], value, convert.Body);

            if (converter.ConvertsNulls)
            {
                nullValue = ReplacingExpressionVisitor.Replace(
                    convert.Parameters[0], Expression.Default(converter.ProviderClrType), convert.Body);
                if (nullValue.Type != resultType)
                {
                    nullValue = Expression.Convert(nullValue, resultType);
                }
            }
        }

        if (value.Type != resultType)
        {
            value = Expression.Convert(value, resultType);
        }

        return Expression.Condition(
            Expression.Call(reader, IsDbNullMethod, Expression.Constant(ordinal)),
            nullValue,
            value);
    }

    private sealed class MergeReturningShaperRewriter(
        RelationalQueryableMethodTranslatingExpressionVisitor visitor,
        ParameterExpression targetParam,
        ParameterExpression readerParam,
        IEntityType entityType,
        ITableBase table,
        List<ProjectionExpression> projections,
        Dictionary<IProperty, int> ordinals) : ExpressionVisitor
    {
        // Only target properties, optionally composed into new/member-init expressions with conversions and constants, are supported;
        // anything else, e.g. a captured variable, can't be evaluated by the shaper.
        [return: NotNullIfNotNull(nameof(node))]
        public override Expression? Visit(Expression? node)
            => node switch
            {
                null => null,
                MemberExpression member when GetParameterRoot(member) == targetParam => ReadColumn(member),
                MethodCallExpression { Arguments: [var instance, ConstantExpression { Value: string }] } methodCall
                    when methodCall.Method.IsEFPropertyMethod() && instance.UnwrapTypeConversion(out _) == targetParam
                    => ReadColumn(methodCall),
                NewExpression or MemberInitExpression or ConstantExpression
                    or UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked }
                    => base.Visit(node),
                _ => throw new InvalidOperationException(RelationalStrings.ExecuteMergeReturningUnsupportedExpression(node.Print()))
            };

        private Expression ReadColumn(Expression selector)
        {
            var property = ResolveProperty(selector, targetParam, entityType);
            var column = table.FindColumn(property)
                ?? throw new InvalidOperationException(RelationalStrings.ExecuteMergePropertyNotMapped(property.Name, table.Name));

            if (!ordinals.TryGetValue(property, out var ordinal))
            {
                ordinal = projections.Count;
                ordinals[property] = ordinal;
                projections.Add(
                    new ProjectionExpression(
                        new MergeColumnReferenceExpression(column.Name, fromExcluded: false, property.ClrType, column.StoreTypeMapping),
                        column.Name));
            }

            return visitor.CreateReturningColumnRead(readerParam, ordinal, column.StoreTypeMapping, selector.Type);
        }
    }

    private static List<(IProperty Property, Expression? ValueExpression)> ResolveInsertProperties(
        IEntityType entityType,
        ITableBase table,
        StoreObjectIdentifier storeObject,
        IReadOnlyList<ExecuteUpdateSetter>? whenNotMatchedSetters,
        IReadOnlyList<(IProperty Property, string SourceMember)> conflictProperties)
    {
        if (whenNotMatchedSetters is null)
        {
            // Default: insert every property that maps to a column on the target table, except those generated by the database.
            return entityType.GetProperties()
                .Where(p => table.FindColumn(p) is not null && !IsGeneratedOnInsert(p, storeObject, conflictProperties))
                .Select(p => (p, (Expression?)null))
                .ToList();
        }

        if (whenNotMatchedSetters.Count == 0)
        {
            throw new InvalidOperationException(RelationalStrings.ExecuteMergeEmptyWhenNotMatched);
        }

        var result = new List<(IProperty, Expression?)>(whenNotMatchedSetters.Count);
        var explicitValues = new Dictionary<IProperty, Expression>();
        foreach (var setter in whenNotMatchedSetters)
        {
            var property = ResolveProperty(setter.PropertySelector, entityType);
            if (!explicitValues.TryAdd(property, setter.ValueExpression))
            {
                throw new InvalidOperationException(RelationalStrings.ExecuteMergeDuplicateInsertProperty(property.Name));
            }

            if (IsGeneratedOnInsert(property, storeObject, conflictProperties))
            {
                throw new InvalidOperationException(RelationalStrings.ExecuteMergeGeneratedInsertProperty(property.Name));
            }

            result.Add((property, setter.ValueExpression));
        }

        // Without its conflict-column values, an inserted row could never be matched by a subsequent merge. The value must also be the
        // match's source member: ON CONFLICT compares the inserted value, so anything else would silently match on a different value.
        foreach (var (property, sourceMember) in conflictProperties)
        {
            if (!explicitValues.TryGetValue(property, out var value))
            {
                throw new InvalidOperationException(RelationalStrings.ExecuteMergeMatchColumnNotInserted(property.Name));
            }

            if (value is not LambdaExpression valueSelector
                || GetDirectSourceMemberName(valueSelector.Body, valueSelector.Parameters[0]) != sourceMember)
            {
                throw new InvalidOperationException(
                    RelationalStrings.ExecuteMergeMatchColumnInsertMismatch(property.Name, sourceMember));
            }
        }

        return result;
    }

    // Computed columns are populated by the database and can't be inserted into; as in SaveChanges, neither are properties ignored before
    // save (e.g. a row version), unless they're match columns, whose values ON CONFLICT needs.
    private static bool IsGeneratedOnInsert(
        IProperty property,
        StoreObjectIdentifier storeObject,
        IReadOnlyList<(IProperty Property, string SourceMember)> conflictProperties)
        => property.GetComputedColumnSql(storeObject) is not null
            || (property.GetBeforeSaveBehavior() == PropertySaveBehavior.Ignore && conflictProperties.All(c => c.Property != property));

    // Translates a WhenMatched value selector into SQL. A target-row reference becomes "<table>"."<col>"; a source-row member becomes the
    // SQLite/PostgreSQL "excluded" pseudo-row column it was inserted into unchanged. Only member access and constants are supported:
    // operators and functions translate differently across providers, so they need the regular translation pipeline.
    private SqlExpression TranslateMergeUpdateValue(
        Expression expression,
        ParameterExpression? targetParam,
        ParameterExpression? sourceParam,
        IProperty property,
        IColumnBase column,
        IEntityType entityType,
        ITableBase table,
        IReadOnlyDictionary<string, List<(IColumnBase Column, IProperty Property)>> excludedColumnsBySourceMember)
    {
        expression = UnwrapValuePreservingConversions(expression);

        switch (expression)
        {
            case MemberExpression member when sourceParam is not null && GetParameterRoot(member) == sourceParam:
            {
                if (member.Expression != sourceParam
                    || !excludedColumnsBySourceMember.TryGetValue(member.Member.Name, out var excludedColumns))
                {
                    throw new InvalidOperationException(RelationalStrings.ExecuteMergeSourceMemberNotInserted(member.Print()));
                }

                // The excluded value has been converted by its own column's value converter, so it can only be assigned to a column
                // that converts the same way; prefer the column being set.
                var (excludedColumn, excludedProperty) = excludedColumns
                    .Where(c => c.Column.StoreTypeMapping.Converter == column.StoreTypeMapping.Converter)
                    .OrderBy(c => c.Column != column)
                    .FirstOrDefault();
                if (excludedColumn is null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ExecuteMergeSourceMemberConverterMismatch(member.Print(), property.Name));
                }

                return new MergeColumnReferenceExpression(
                    excludedColumn.Name, fromExcluded: true, excludedProperty.ClrType, excludedColumn.StoreTypeMapping);
            }

            case MemberExpression member:
            {
                var targetProperty = ResolveProperty(member, targetParam, entityType);
                var targetColumn = table.FindColumn(targetProperty)
                    ?? throw new InvalidOperationException(
                        RelationalStrings.ExecuteMergePropertyNotMapped(targetProperty.Name, table.Name));

                // The column holds its own converter's provider value, so it can only be copied to a column that converts the same way.
                if (targetColumn.StoreTypeMapping.Converter != column.StoreTypeMapping.Converter)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ExecuteMergeTargetPropertyConverterMismatch(targetProperty.Name, property.Name));
                }

                return new MergeColumnReferenceExpression(
                    targetColumn.Name, fromExcluded: false, targetProperty.ClrType, targetColumn.StoreTypeMapping);
            }

            case ConstantExpression constant:
                if (constant.Value is not null
                    && !property.ClrType.UnwrapNullableType().IsInstanceOfType(constant.Value))
                {
                    throw new InvalidOperationException(RelationalStrings.ExecuteMergeUnsupportedExpression(constant.Print()));
                }

                return _sqlExpressionFactory.Constant(constant.Value, property.ClrType, column.StoreTypeMapping);

            // A constant WhenMatched value from the SetProperty(property, value) overload is funcletized to a query parameter;
            // emit it as a SQL parameter (bound from the query context).
            case QueryParameterExpression queryParameter:
                return new SqlParameterExpression(queryParameter.Name, queryParameter.Type, typeMapping: null);

            default:
                throw new InvalidOperationException(RelationalStrings.ExecuteMergeUnsupportedExpression(expression.Print()));
        }
    }

    // Casts that only change nullability, box, or cast to a base type keep the value; others (e.g. narrowing or checked casts) would
    // need translating, which isn't supported yet.
    private static Expression UnwrapValuePreservingConversions(Expression expression)
    {
        while (expression is UnaryExpression
            {
                NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked or ExpressionType.TypeAs
            } convert)
        {
            if (convert.Type != typeof(object)
                && !convert.Type.IsAssignableFrom(convert.Operand.Type)
                && convert.Type.UnwrapNullableType() != convert.Operand.Type.UnwrapNullableType())
            {
                throw new InvalidOperationException(RelationalStrings.ExecuteMergeUnsupportedExpression(convert.Print()));
            }

            expression = convert.Operand;
        }

        return expression;
    }

    private static ParameterExpression? GetParameterRoot(Expression expression)
    {
        while (expression is MemberExpression member)
        {
            expression = member.Expression!;
        }

        return expression as ParameterExpression;
    }

    private static (string[] Columns, IProperty[] Properties) ResolvePrimaryKeyColumnsAndProperties(
        IEntityType entityType,
        ITableBase table)
    {
        var primaryKey = entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException(RelationalStrings.ExecuteMergeNoPrimaryKey(entityType.DisplayName()));

        var properties = primaryKey.Properties.ToArray();
        var columns = properties
            .Select(
                p => (table.FindColumn(p)
                    ?? throw new InvalidOperationException(
                        RelationalStrings.ExecuteMergePropertyNotMapped(p.Name, table.Name))).Name)
            .ToArray();

        return (columns, properties);
    }

    private static (string[] Columns, IProperty[] Properties) ResolveColumnNamesAndProperties(
        LambdaExpression selector,
        IEntityType entityType,
        ITableBase table)
    {
        IReadOnlyList<Expression> members = selector.Body switch
        {
            NewExpression @new => @new.Arguments,
            _ => [selector.Body]
        };

        var names = new string[members.Count];
        var properties = new IProperty[members.Count];
        for (var i = 0; i < members.Count; i++)
        {
            var property = ResolveProperty(members[i], selector.Parameters[0], entityType);
            properties[i] = property;
            names[i] = (table.FindColumn(property)
                ?? throw new InvalidOperationException(
                    RelationalStrings.ExecuteMergePropertyNotMapped(property.Name, table.Name))).Name;
        }

        return (names, properties);
    }

    private static MemberInfo FindSourceMember(Type sourceType, string memberName)
    {
        var member = (MemberInfo?)sourceType.GetProperty(memberName) ?? sourceType.GetField(memberName);
        if (member is null)
        {
            throw new InvalidOperationException(
                RelationalStrings.ExecuteMergePropertyNotFound(memberName, sourceType.DisplayName(fullName: false)));
        }

        return member;
    }

    private static MemberInfo[] ResolveKeySourceMembers(LambdaExpression selector)
    {
        var parameter = selector.Parameters[0];
        IReadOnlyList<Expression> members = selector.Body switch
        {
            NewExpression @new => @new.Arguments,
            _ => [selector.Body]
        };

        var sourceMembers = new MemberInfo[members.Count];
        for (var i = 0; i < members.Count; i++)
        {
            var memberName = GetDirectSourceMemberName(members[i], parameter)
                ?? throw new InvalidOperationException(RelationalStrings.ExecuteMergeUnsupportedExpression(selector.Print()));
            sourceMembers[i] = FindSourceMember(parameter.Type, memberName);
        }

        return sourceMembers;
    }

    private static string? GetDirectSourceMemberName(Expression expression, ParameterExpression sourceParam)
        => UnwrapValuePreservingConversions(expression) is MemberExpression { Member.Name: var name } member
            && member.Expression == sourceParam
                ? name
                : null;

    private static IProperty ResolveProperty(LambdaExpression selector, IEntityType entityType)
        => ResolveProperty(selector.Body, selector.Parameters[0], entityType);

    // Only properties declared directly on the target are supported; a nested member such as t.Owned.Name must not resolve to a
    // same-named property on the target itself.
    private static IProperty ResolveProperty(Expression selector, ParameterExpression? targetParam, IEntityType entityType)
    {
        var expression = UnwrapValuePreservingConversions(selector);

        return expression switch
        {
            MemberExpression { Member.Name: var name, Expression: var instance }
                when instance is not null && instance.UnwrapTypeConversion(out _) == targetParam
                => entityType.FindProperty(name)
                    ?? throw new InvalidOperationException(
                        RelationalStrings.ExecuteMergePropertyNotFound(name, entityType.DisplayName())),
            MethodCallExpression { Arguments: [var instance, ConstantExpression { Value: string shadowName }] } methodCall
                when methodCall.Method.IsEFPropertyMethod() && instance.UnwrapTypeConversion(out _) == targetParam
                => entityType.FindProperty(shadowName)
                    ?? throw new InvalidOperationException(
                        RelationalStrings.ExecuteMergePropertyNotFound(shadowName, entityType.DisplayName())),
            _ => throw new InvalidOperationException(RelationalStrings.ExecuteMergeInvalidPropertySelector(selector.Print()))
        };
    }
}
