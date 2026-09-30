// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

public partial class RelationalQueryableMethodTranslatingExpressionVisitor
{
    /// <inheritdoc />
    protected override Expression TranslateExecuteMerge(
        ShapedQueryExpression source,
        ConstantExpression sourceRows,
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
        switch (entityType.GetTableMappings().ToList())
        {
            case []:
                throw new InvalidOperationException(
                    RelationalStrings.ExecuteUpdateDeleteOnEntityNotMappedToTable(entityType.DisplayName()));
            case [_]:
                break;
            default:
                throw new InvalidOperationException(
                    RelationalStrings.ExecuteOperationOnEntitySplitting(
                        nameof(EntityFrameworkQueryableExtensions.ExecuteMerge), entityType.DisplayName()));
        }

        var table = targetTable.Table;

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
        var conflictSourceGetters = new Dictionary<string, Func<object?, object?>>();
        var conflictProperties = new List<IProperty>();
        if (matches.Count == 0)
        {
            (conflictColumns, var pkProperties) = ResolvePrimaryKeyColumnsAndProperties(entityType, table);
            conflictProperties.AddRange(pkProperties);
        }
        else
        {
            var columns = new List<string>();
            foreach (var match in matches)
            {
                var (targetColumns, targetProperties) = ResolveColumnNamesAndProperties(match.TargetKeySelector, entityType, table);
                var sourceGetters = CompileKeyMemberGetters(match.SourceKeySelector);
                Check.DebugAssert(
                    targetColumns.Length == sourceGetters.Length, "Match target and source key selectors have different arities.");
                for (var i = 0; i < targetColumns.Length; i++)
                {
                    if (!conflictSourceGetters.TryAdd(targetColumns[i], sourceGetters[i]))
                    {
                        throw new InvalidOperationException(RelationalStrings.ExecuteMergeDuplicateMatchColumn(targetColumns[i]));
                    }

                    columns.Add(targetColumns[i]);
                    conflictProperties.Add(targetProperties[i]);
                }
            }

            conflictColumns = [.. columns];
        }

        // Determine the columns (and per-row values) to insert. When WhenNotMatched isn't specified, insert every mapped column using
        // the same-named source member (except for conflict columns, which use the match source selector); otherwise use the explicit
        // insert setters. Insert value selectors are over the source row only, so they can be evaluated client-side against the
        // (constant) source rows.
        var insertProperties = ResolveInsertProperties(entityType, table, whenNotMatchedSetters, conflictProperties);
        var sourceType = sourceRows.Type.TryGetSequenceType() ?? typeof(object);
        var insertColumns = new string[insertProperties.Count];
        var valueGetters = new Func<object?, object?>?[insertProperties.Count];
        var fixedValues = new SqlExpression?[insertProperties.Count];
        var columnTypeMappings = new RelationalTypeMapping[insertProperties.Count];

        // WhenMatched can only see source values through the inserted ("excluded") row, so track which source member, if any, each
        // insert column carries unchanged.
        var excludedColumnsBySourceMember = new Dictionary<string, (IColumnBase Column, IProperty Property)>();
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
                // A value selector over the source row: evaluate it per-row client-side against the (constant) source rows.
                case LambdaExpression valueSelector:
                    valueGetters[i] = CompileValueSelector(valueSelector);
                    sourceMember = valueSelector.Parameters is [var rowParam]
                        ? GetDirectSourceMemberName(valueSelector.Body, rowParam)
                        : null;
                    break;

                // A constant insert value from the SetProperty(property, value) overload is funcletized to a query parameter; emit
                // it as a SQL parameter (bound from the query context) rather than trying to evaluate it client-side.
                case QueryParameterExpression queryParameter:
                    fixedValues[i] = _sqlExpressionFactory.ApplyTypeMapping(
                        new SqlParameterExpression(queryParameter.Name, queryParameter.Type, typeMapping: null), column.StoreTypeMapping);
                    break;

                case ConstantExpression constant:
                    fixedValues[i] = new SqlConstantExpression(constant.Value, property.ClrType, column.StoreTypeMapping);
                    break;

                // No explicit insert setter: use the match source selector for conflict columns, otherwise the same-named source member.
                case null when conflictSourceGetters.TryGetValue(column.Name, out var sourceGetter):
                    valueGetters[i] = sourceGetter;
                    break;

                // Read the same-named member off the source row. The source type may differ from the target entity (e.g. a DTO), so
                // we can't use the target property's getter here — build the accessor against the actual source type instead.
                case null:
                    valueGetters[i] = CompileSourceMemberGetter(sourceType, property.Name);
                    sourceMember = property.Name;
                    break;

                default:
                    throw new InvalidOperationException(RelationalStrings.ExecuteMergeUnsupportedExpression(valueExpression.Print()));
            }

            if (sourceMember is not null)
            {
                excludedColumnsBySourceMember.TryAdd(sourceMember, (column, property));
            }
        }

        var sourceRowValues = new List<RowValueExpression>();
        foreach (var row in (IEnumerable)sourceRows.Value!)
        {
            var values = new SqlExpression[insertProperties.Count];
            for (var i = 0; i < insertProperties.Count; i++)
            {
                // Source values are user data, so they're redacted from logs unless sensitive data logging is enabled.
                values[i] = fixedValues[i]
                    ?? new SqlConstantExpression(
                        valueGetters[i]!(row), insertProperties[i].Property.ClrType, sensitive: true, columnTypeMappings[i]);
            }

            sourceRowValues.Add(new RowValueExpression(values));
        }

        // WhenMatched value selectors reference the existing target row (and the incoming source row), so they must be translated to
        // SQL rather than evaluated: target references become target columns, source references become the "excluded" pseudo-row.
        IReadOnlyList<MergeColumnSetter>? updateSetters = null;
        if (whenMatchedSetters is not null)
        {
            var setters = new List<MergeColumnSetter>(whenMatchedSetters.Count);
            foreach (var setter in whenMatchedSetters)
            {
                var property = ResolveProperty(setter.PropertySelector, entityType);
                var column = table.FindColumn(property)
                    ?? throw new InvalidOperationException(RelationalStrings.ExecuteMergePropertyNotMapped(property.Name, table.Name));

                SqlExpression value;
                if (setter.ValueExpression is LambdaExpression valueLambda)
                {
                    var targetParam = valueLambda.Parameters[0];
                    var sourceParam = valueLambda.Parameters.Count > 1 ? valueLambda.Parameters[1] : null;
                    value = TranslateMergeUpdateValue(
                        valueLambda.Body, targetParam, sourceParam, entityType, table, excludedColumnsBySourceMember);
                }
                else
                {
                    value = TranslateMergeUpdateValue(
                        setter.ValueExpression, targetParam: null, sourceParam: null, entityType, table, excludedColumnsBySourceMember);
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

        return new MergeExpression(targetTable, insertColumns, sourceRowValues, conflictColumns, updateSetters, returning, returningShaper);
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
        Expression value = Expression.Call(
            getMethod.DeclaringType != typeof(DbDataReader) ? Expression.Convert(reader, getMethod.DeclaringType!) : reader,
            getMethod,
            Expression.Constant(ordinal));

        value = typeMapping.CustomizeDataReaderExpression(value);

        if (typeMapping.Converter is ValueConverter converter)
        {
            var convert = converter.ConvertFromProviderExpression;
            value = ReplacingExpressionVisitor.Replace(convert.Parameters[0], value, convert.Body);
        }

        if (value.Type != resultType)
        {
            value = Expression.Convert(value, resultType);
        }

        return Expression.Condition(
            Expression.Call(reader, IsDbNullMethod, Expression.Constant(ordinal)),
            Expression.Default(resultType),
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
        protected override Expression VisitMember(MemberExpression node)
            => GetParameterRoot(node) == targetParam ? ReadColumn(node, node.Type) : base.VisitMember(node);

        protected override Expression VisitMethodCall(MethodCallExpression node)
            => node is { Method.Name: "Property", Arguments: [var source, ConstantExpression { Value: string }] }
                && source is ParameterExpression p && p == targetParam
                    ? ReadColumn(node, node.Type)
                    : base.VisitMethodCall(node);

        private Expression ReadColumn(Expression selector, Type resultType)
        {
            var property = ResolveProperty(selector, entityType);
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

            return visitor.CreateReturningColumnRead(readerParam, ordinal, column.StoreTypeMapping, resultType);
        }
    }

    private static List<(IProperty Property, Expression? ValueExpression)> ResolveInsertProperties(
        IEntityType entityType,
        ITableBase table,
        IReadOnlyList<ExecuteUpdateSetter>? whenNotMatchedSetters,
        IReadOnlyList<IProperty> conflictProperties)
    {
        if (whenNotMatchedSetters is null)
        {
            // Default: insert every property that maps to a column on the target table. Computed columns are populated by the database
            // and cannot be inserted into, so they're excluded.
            var storeObject = StoreObjectIdentifier.Table(table.Name, table.Schema);
            return entityType.GetProperties()
                .Where(p => table.FindColumn(p) is not null && p.GetComputedColumnSql(storeObject) is null)
                .Select(p => (p, (Expression?)null))
                .ToList();
        }

        var result = new List<(IProperty, Expression?)>(whenNotMatchedSetters.Count);
        var explicitProperties = new HashSet<IProperty>();
        foreach (var setter in whenNotMatchedSetters)
        {
            var property = ResolveProperty(setter.PropertySelector, entityType);
            result.Add((property, setter.ValueExpression));
            explicitProperties.Add(property);
        }

        // The conflict (match) columns must always be part of the insert row, even when WhenNotMatched doesn't set them explicitly:
        // otherwise a not-matched row is inserted without its conflict-column value, so it can never actually be matched by a
        // subsequent merge. Falling through with a null ValueExpression makes the caller source the value from the match's source
        // key selector, same as the implicit-insert path.
        foreach (var property in conflictProperties)
        {
            if (explicitProperties.Add(property))
            {
                result.Add((property, null));
            }
        }

        return result;
    }

    // Translates a WhenMatched value selector into SQL. Target-row references become "<table>"."<col>"; a source-row member becomes the
    // SQLite/PostgreSQL "excluded" pseudo-row column it was inserted into unchanged. Supports member access, binary operators and
    // constants (the common upsert grammar).
    private SqlExpression TranslateMergeUpdateValue(
        Expression expression,
        ParameterExpression? targetParam,
        ParameterExpression? sourceParam,
        IEntityType entityType,
        ITableBase table,
        IReadOnlyDictionary<string, (IColumnBase Column, IProperty Property)> excludedColumnsBySourceMember)
    {
        expression = expression.UnwrapTypeConversion(out _);

        switch (expression)
        {
            case MemberExpression member when sourceParam is not null && GetParameterRoot(member) == sourceParam:
            {
                if (member.Expression != sourceParam
                    || !excludedColumnsBySourceMember.TryGetValue(member.Member.Name, out var excluded))
                {
                    throw new InvalidOperationException(RelationalStrings.ExecuteMergeSourceMemberNotInserted(member.Print()));
                }

                return new MergeColumnReferenceExpression(
                    excluded.Column.Name, fromExcluded: true, excluded.Property.ClrType, excluded.Column.StoreTypeMapping);
            }

            case MemberExpression member:
            {
                var property = ResolveProperty(member, entityType);
                var column = table.FindColumn(property)
                    ?? throw new InvalidOperationException(RelationalStrings.ExecuteMergePropertyNotMapped(property.Name, table.Name));

                return new MergeColumnReferenceExpression(column.Name, fromExcluded: false, property.ClrType, column.StoreTypeMapping);
            }

            case BinaryExpression binary:
            {
                var left = TranslateMergeUpdateValue(
                    binary.Left, targetParam, sourceParam, entityType, table, excludedColumnsBySourceMember);
                var right = TranslateMergeUpdateValue(
                    binary.Right, targetParam, sourceParam, entityType, table, excludedColumnsBySourceMember);
                return _sqlExpressionFactory.MakeBinary(binary.NodeType, left, right, typeMapping: null)
                    ?? throw new InvalidOperationException(RelationalStrings.ExecuteMergeUnsupportedOperator(binary.NodeType));
            }

            case ConstantExpression constant:
                return _sqlExpressionFactory.Constant(constant.Value, constant.Type, typeMapping: null);

            // A constant WhenMatched value from the SetProperty(property, value) overload is funcletized to a query parameter;
            // emit it as a SQL parameter (bound from the query context).
            case QueryParameterExpression queryParameter:
                return new SqlParameterExpression(queryParameter.Name, queryParameter.Type, typeMapping: null);

            default:
                throw new InvalidOperationException(RelationalStrings.ExecuteMergeUnsupportedExpression(expression.Print()));
        }
    }

    private static ParameterExpression? GetParameterRoot(Expression expression)
    {
        while (expression is MemberExpression member)
        {
            expression = member.Expression!;
        }

        return expression as ParameterExpression;
    }

    private static Func<object?, object?> CompileValueSelector(LambdaExpression valueSelector)
    {
        var compiled = valueSelector.Compile();
        return valueSelector.Parameters.Count == 0
            ? _ => compiled.DynamicInvoke()
            : row => compiled.DynamicInvoke(row);
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
            var property = ResolveProperty(members[i], entityType);
            properties[i] = property;
            names[i] = (table.FindColumn(property)
                ?? throw new InvalidOperationException(
                    RelationalStrings.ExecuteMergePropertyNotMapped(property.Name, table.Name))).Name;
        }

        return (names, properties);
    }

    private static Func<object?, object?> CompileSourceMemberGetter(Type sourceType, string memberName)
    {
        var member = (MemberInfo?)sourceType.GetProperty(memberName) ?? sourceType.GetField(memberName);
        if (member is null)
        {
            throw new InvalidOperationException(
                RelationalStrings.ExecuteMergePropertyNotFound(memberName, sourceType.DisplayName(fullName: false)));
        }

        var parameter = Expression.Parameter(typeof(object), "row");
        Expression body = Expression.MakeMemberAccess(Expression.Convert(parameter, sourceType), member);
        if (body.Type != typeof(object))
        {
            body = Expression.Convert(body, typeof(object));
        }

        return (Func<object?, object?>)Expression.Lambda(typeof(Func<object?, object?>), body, parameter).Compile();
    }

    private static Func<object?, object?>[] CompileKeyMemberGetters(LambdaExpression selector)
    {
        var parameter = selector.Parameters[0];
        IReadOnlyList<Expression> members = selector.Body switch
        {
            NewExpression @new => @new.Arguments,
            _ => [selector.Body]
        };

        var getters = new Func<object?, object?>[members.Count];
        for (var i = 0; i < members.Count; i++)
        {
            var body = members[i];
            if (body.Type.IsValueType)
            {
                body = Expression.Convert(body, typeof(object));
            }

            var compiled = Expression.Lambda(body, parameter).Compile();
            getters[i] = row => compiled.DynamicInvoke(row);
        }

        return getters;
    }

    private static string? GetDirectSourceMemberName(Expression expression, ParameterExpression sourceParam)
        => expression.UnwrapTypeConversion(out _) is MemberExpression { Member.Name: var name } member
            && member.Expression == sourceParam
                ? name
                : null;

    private static IProperty ResolveProperty(Expression selector, IEntityType entityType)
    {
        var expression = selector is LambdaExpression lambda ? lambda.Body : selector;
        expression = expression.UnwrapTypeConversion(out _);

        return expression switch
        {
            MemberExpression { Member.Name: var name } => entityType.FindProperty(name)
                ?? throw new InvalidOperationException(
                    RelationalStrings.ExecuteMergePropertyNotFound(name, entityType.DisplayName())),
            MethodCallExpression { Method.Name: "Property", Arguments: [_, ConstantExpression { Value: string shadowName }] }
                => entityType.FindProperty(shadowName)
                    ?? throw new InvalidOperationException(
                        RelationalStrings.ExecuteMergePropertyNotFound(shadowName, entityType.DisplayName())),
            _ => throw new InvalidOperationException(RelationalStrings.ExecuteMergeInvalidPropertySelector(selector.Print()))
        };
    }
}
