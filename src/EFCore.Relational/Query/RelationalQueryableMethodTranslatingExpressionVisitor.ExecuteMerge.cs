// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static Microsoft.EntityFrameworkCore.Infrastructure.ExpressionExtensions;

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
        if (source.QueryExpression is not SelectExpression { Tables: [TableExpression targetTable], Predicate: null })
        {
            throw new InvalidOperationException(RelationalStrings.ExecuteMergeOnComplexQuery);
        }

        if (source.ShaperExpression is not StructuralTypeShaperExpression { StructuralType: IEntityType entityType })
        {
            throw new InvalidOperationException(RelationalStrings.ExecuteMergeOnNonEntityType);
        }

        var table = targetTable.Table;

        // Determine the conflict (match) columns and, for each, how to read its value from a source row. No Match() call => default to
        // the primary key, matched by the same-named source member. The conflict-column value must come from the source-side selector of
        // the match, not the same-named source member, so that Match(t => t.Id, s => s.ExternalId) actually matches on ExternalId.
        string[] conflictColumns;
        var conflictSourceGetters = new Dictionary<string, Func<object?, object?>>();
        if (matches.Count == 0)
        {
            conflictColumns = ResolvePrimaryKeyColumns(entityType, table);
        }
        else
        {
            var columns = new List<string>();
            foreach (var match in matches)
            {
                var targetColumns = ResolveColumnNames(match.TargetKeySelector, entityType, table);
                var sourceGetters = CompileKeyMemberGetters(match.SourceKeySelector);
                Check.DebugAssert(
                    targetColumns.Length == sourceGetters.Length, "Match target and source key selectors have different arities.");
                for (var i = 0; i < targetColumns.Length; i++)
                {
                    columns.Add(targetColumns[i]);
                    conflictSourceGetters[targetColumns[i]] = sourceGetters[i];
                }
            }

            conflictColumns = [.. columns];
        }

        // Determine the columns (and per-row values) to insert. When WhenNotMatched isn't specified, insert every mapped column using
        // the same-named source member (except for conflict columns, which use the match source selector); otherwise use the explicit
        // insert setters. Insert value selectors are over the source row only, so they can be evaluated client-side against the
        // (constant) source rows.
        var insertProperties = ResolveInsertProperties(entityType, table, whenNotMatchedSetters);
        var insertColumns = new string[insertProperties.Count];
        var valueGetters = new Func<object?, object?>?[insertProperties.Count];
        var fixedValues = new SqlExpression?[insertProperties.Count];
        var columnTypeMappings = new RelationalTypeMapping[insertProperties.Count];
        for (var i = 0; i < insertProperties.Count; i++)
        {
            var (property, valueExpression) = insertProperties[i];
            var column = table.FindColumn(property)
                ?? throw new InvalidOperationException(RelationalStrings.ExecuteMergePropertyNotMapped(property.Name, table.Name));
            insertColumns[i] = column.Name;
            columnTypeMappings[i] = column.StoreTypeMapping;
            switch (valueExpression)
            {
                // A value selector over the source row: evaluate it per-row client-side against the (constant) source rows.
                case LambdaExpression valueSelector:
                    valueGetters[i] = CompileValueSelector(valueSelector);
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

                case null:
                    var getter = property.GetGetter();
                    valueGetters[i] = row => getter.GetClrValue(row!);
                    break;

                default:
                    throw new InvalidOperationException(RelationalStrings.ExecuteMergeUnsupportedExpression(valueExpression.Print()));
            }
        }

        var sourceRowValues = new List<RowValueExpression>();
        foreach (var row in (IEnumerable)sourceRows.Value!)
        {
            var values = new SqlExpression[insertProperties.Count];
            for (var i = 0; i < insertProperties.Count; i++)
            {
                values[i] = fixedValues[i]
                    ?? new SqlConstantExpression(valueGetters[i]!(row), insertProperties[i].Property.ClrType, columnTypeMappings[i]);
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
                    value = TranslateMergeUpdateValue(valueLambda.Body, targetParam, sourceParam, entityType, table);
                }
                else
                {
                    value = TranslateMergeUpdateValue(setter.ValueExpression, targetParam: null, sourceParam: null, entityType, table);
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
        IReadOnlyList<ExecuteUpdateSetter>? whenNotMatchedSetters)
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
        foreach (var setter in whenNotMatchedSetters)
        {
            var property = ResolveProperty(setter.PropertySelector, entityType);
            result.Add((property, setter.ValueExpression));
        }

        return result;
    }

    // Translates a WhenMatched value selector into SQL. Target-row references become "<table>"."<col>"; source-row references become
    // the SQLite/PostgreSQL "excluded" pseudo-row. Supports member access, binary operators and constants (the common upsert grammar).
    private SqlExpression TranslateMergeUpdateValue(
        Expression expression,
        ParameterExpression? targetParam,
        ParameterExpression? sourceParam,
        IEntityType entityType,
        ITableBase table)
    {
        expression = expression.UnwrapTypeConversion(out _);

        switch (expression)
        {
            case MemberExpression member:
            {
                var property = ResolveProperty(member, entityType);
                var column = table.FindColumn(property)
                    ?? throw new InvalidOperationException(RelationalStrings.ExecuteMergePropertyNotMapped(property.Name, table.Name));
                var fromExcluded = GetParameterRoot(member) == sourceParam && sourceParam is not null;

                return new MergeColumnReferenceExpression(column.Name, fromExcluded, property.ClrType, column.StoreTypeMapping);
            }

            case BinaryExpression binary:
            {
                var left = TranslateMergeUpdateValue(binary.Left, targetParam, sourceParam, entityType, table);
                var right = TranslateMergeUpdateValue(binary.Right, targetParam, sourceParam, entityType, table);
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

    private static string[] ResolvePrimaryKeyColumns(IEntityType entityType, ITableBase table)
    {
        var primaryKey = entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException(RelationalStrings.ExecuteMergeNoPrimaryKey(entityType.DisplayName()));

        return primaryKey.Properties
            .Select(
                p => (table.FindColumn(p)
                    ?? throw new InvalidOperationException(
                        RelationalStrings.ExecuteMergePropertyNotMapped(p.Name, table.Name))).Name)
            .ToArray();
    }

    private static string[] ResolveColumnNames(LambdaExpression selector, IEntityType entityType, ITableBase table)
    {
        IReadOnlyList<Expression> members = selector.Body switch
        {
            NewExpression @new => @new.Arguments,
            _ => [selector.Body]
        };

        var names = new string[members.Count];
        for (var i = 0; i < members.Count; i++)
        {
            var property = ResolveProperty(members[i], entityType);
            names[i] = (table.FindColumn(property)
                ?? throw new InvalidOperationException(
                    RelationalStrings.ExecuteMergePropertyNotMapped(property.Name, table.Name))).Name;
        }

        return names;
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
