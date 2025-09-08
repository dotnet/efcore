// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage.Json;
using static Microsoft.EntityFrameworkCore.Infrastructure.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.Query;

public partial class RelationalQueryableMethodTranslatingExpressionVisitor
{
    private const string ExecuteUpdateRuntimeParameterPrefix = "complex_type_";

    private static readonly MethodInfo ParameterValueExtractorMethod =
        typeof(RelationalQueryableMethodTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterValueExtractor))!;

    private static readonly MethodInfo ParameterJsonSerializerMethod =
        typeof(RelationalQueryableMethodTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterJsonSerializer))!;

    /// <inheritdoc />
    protected override UpdateExpression? TranslateExecuteUpdate(ShapedQueryExpression source, IReadOnlyList<ExecuteUpdateSetter> setters)
    {
        Check.DebugAssert(setters.Count > 0, "Empty setters list");

        // Our source may have IncludeExpressions because of owned entities or auto-include; unwrap these, as they're meaningless for
        // ExecuteUpdate's lambdas. Note that we don't currently support updates across tables.
        source = source.UpdateShaperExpression(new IncludePruner().Visit(source.ShaperExpression));

        if (TranslationErrorDetails != null)
        {
            return null;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;

        // Translate the setters: the left (property) selectors get translated to ColumnExpressions, the right (value) selectors to
        // arbitrary SqlExpressions.
        // Note that if the query isn't natively supported, we'll do a pushdown (see PushdownWithPkInnerJoinPredicate below); if that
        // happens, we'll have to re-translate the setters over the new query (which includes a JOIN). However, we still translate here
        // since we need the target table in order to perform the check below.
        if (!TryTranslateSetters(source, setters, out var translatedSetters, out var targetTable))
        {
            return null;
        }

        if (targetTable is TpcTablesExpression tpcTablesExpression)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnTPC(
                    nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate), tpcTablesExpression.EntityType.DisplayName()));
            return null;
        }

        // Check if the provider has a native translation for the update represented by the select expression.
        // The default relational implementation handles simple, universally-supported cases (i.e. no operators except for predicate).
        // Providers may override IsValidSelectExpressionForExecuteUpdate to add support for more cases via provider-specific UPDATE syntax.
        if (IsValidSelectExpressionForExecuteUpdate(selectExpression, targetTable, out var tableExpression))
        {
            selectExpression.ReplaceProjection(new List<Expression>());
            selectExpression.ApplyProjection();

            return new UpdateExpression(tableExpression, selectExpression, translatedSetters);
        }

        return PushdownWithPkInnerJoinPredicate();

        UpdateExpression? PushdownWithPkInnerJoinPredicate()
        {
            // The provider doesn't natively support the update.
            // As a fallback, we place the original query in a subquery and user an INNER JOIN on the primary key columns.

            // Note that unlike with ExecuteDelete, we cannot use a Contains subquery (which would produce the simpler
            // WHERE Id IN (SELECT ...) syntax), since we allow projecting out to arbitrary shapes (e.g. anonymous types) before the
            // ExecuteUpdate.

            // To rewrite the query, we need to know the primary key properties, which requires getting the entity type.
            // Although there may be several entity types involved, we've already verified that they all map to the same table.
            // Since we don't support table sharing of multiple entity types with different keys, simply get the entity type and key from
            // the first property selector.

            // The following mechanism for extracting the entity type from property selectors only supports simple member access,
            // EF.Function, etc. We also unwrap casts to interface/base class (#29618). Note that owned IncludeExpressions have already
            // been pruned from the source before remapping the lambda (#28727).
            var firstPropertySelector = setters[0].PropertySelector;
            if (!IsMemberAccess(
                    RemapLambdaBody(source, firstPropertySelector).UnwrapTypeConversion(out _),
                    RelationalDependencies.Model,
                    out var baseExpression)
                || baseExpression.UnwrapTypeConversion(out _) is not StructuralTypeShaperExpression shaper)
            {
                AddTranslationErrorDetails(RelationalStrings.InvalidPropertyInSetProperty(firstPropertySelector));
                return null;
            }

            // TODO: #36336
            if (shaper.StructuralType is not IEntityType entityType)
            {
                AddTranslationErrorDetails(
                    RelationalStrings.ExecuteUpdateSubqueryNotSupportedOverComplexTypes(shaper.StructuralType.DisplayName()));
                return null;
            }

            if (entityType.FindPrimaryKey() is not { } pk)
            {
                AddTranslationErrorDetails(
                    RelationalStrings.ExecuteOperationOnKeylessEntityTypeWithUnsupportedOperator(
                        nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate),
                        entityType.DisplayName()));
                return null;
            }

            // Generate the INNER JOIN around the original query, on the PK properties.
            var outer = (ShapedQueryExpression)Visit(new EntityQueryRootExpression(entityType));
            var inner = source;
            var outerParameter = Expression.Parameter(entityType.ClrType);
            var outerKeySelector = Expression.Lambda(outerParameter.CreateKeyValuesExpression(pk.Properties), outerParameter);
            var firstPropertyLambdaExpression = setters[0].PropertySelector;
            var entitySource = GetEntitySource(RelationalDependencies.Model, firstPropertyLambdaExpression.Body);
            var innerKeySelector = Expression.Lambda(
                entitySource.CreateKeyValuesExpression(pk.Properties), firstPropertyLambdaExpression.Parameters);

            var joinPredicate = CreateJoinPredicate(outer, outerKeySelector, inner, innerKeySelector);

            Check.DebugAssert(joinPredicate != null, "Join predicate shouldn't be null");

            var outerSelectExpression = (SelectExpression)outer.QueryExpression;
            var outerShaperExpression = outerSelectExpression.AddInnerJoin(inner, joinPredicate, outer.ShaperExpression);
            outer = outer.UpdateShaperExpression(outerShaperExpression);
            var transparentIdentifierType = outer.ShaperExpression.Type;
            var transparentIdentifierParameter = Expression.Parameter(transparentIdentifierType);

            var propertyReplacement = AccessField(transparentIdentifierType, transparentIdentifierParameter, "Outer");
            var valueReplacement = AccessField(transparentIdentifierType, transparentIdentifierParameter, "Inner");
            var rewrittenSetters = new ExecuteUpdateSetter[setters.Count];
            for (var i = 0; i < setters.Count; i++)
            {
                var (propertyExpression, valueExpression) = setters[i];
                propertyExpression = Expression.Lambda(
                    ReplacingExpressionVisitor.Replace(
                        ReplacingExpressionVisitor.Replace(
                            firstPropertyLambdaExpression.Parameters[0],
                            propertyExpression.Parameters[0],
                            entitySource),
                        propertyReplacement, propertyExpression.Body),
                    transparentIdentifierParameter);

                valueExpression = valueExpression is LambdaExpression lambdaExpression
                    ? Expression.Lambda(
                        ReplacingExpressionVisitor.Replace(lambdaExpression.Parameters[0], valueReplacement, lambdaExpression.Body),
                        transparentIdentifierParameter)
                    : valueExpression;

                rewrittenSetters[i] = new ExecuteUpdateSetter(propertyExpression, valueExpression);
            }

            tableExpression = (TableExpression)outerSelectExpression.Tables[0];

            // Re-translate the property selectors to get column expressions pointing to the new outer select expression (the original one
            // has been pushed down into a subquery).
            if (!TryTranslateSetters(outer, rewrittenSetters, out var translatedSetters, out _))
            {
                return null;
            }

            outerSelectExpression.ReplaceProjection(new List<Expression>());
            outerSelectExpression.ApplyProjection();
            return new UpdateExpression(tableExpression, outerSelectExpression, translatedSetters);
        }

        static Expression GetEntitySource(IModel model, Expression propertyAccessExpression)
        {
            propertyAccessExpression = propertyAccessExpression.UnwrapTypeConversion(out _);
            return IsMemberAccess(propertyAccessExpression, model, out var source)
                ? source
                : ((MemberExpression)propertyAccessExpression).Expression!;
        }
    }

    /// <summary>
    ///     Validates if the current select expression can be used for execute update operation or it requires to be joined as a subquery.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, only multi-table select expressions are supported, and optionally with a predicate.
    ///     </para>
    ///     <para>
    ///         Providers can override this to allow more select expression features to be supported without pushing down into a subquery.
    ///         When doing this, VisitUpdate must also be overridden in the provider's QuerySqlGenerator to add SQL generation support for
    ///         the feature.
    ///     </para>
    /// </remarks>
    /// <param name="selectExpression">The select expression to validate.</param>
    /// <param name="targetTable">The target table containing the rows to be updated.</param>
    /// <param name="tableExpression">
    ///     The table expression corresponding to the provided <paramref name="targetTable" />, containing the rows to be updated.
    /// </param>
    /// <returns>
    ///     Returns <see langword="true" /> if the current select expression can be used for update as-is, <see langword="false" /> otherwise.
    /// </returns>
    protected virtual bool IsValidSelectExpressionForExecuteUpdate(
        SelectExpression selectExpression,
        TableExpressionBase targetTable,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        tableExpression = null;
        if (selectExpression is
            {
                Offset: null,
                Limit: null,
                IsDistinct: false,
                GroupBy: [],
                Having: null,
                Orderings: [],
                Tables.Count: > 0
            })
        {
            if (selectExpression.Tables.Count > 1)
            {
                // If the table we are looking for is the first table, then we need to verify whether we can lift the next table in FROM
                // clause
                if (ReferenceEquals(selectExpression.Tables[0], targetTable)
                    && selectExpression.Tables[1] is not InnerJoinExpression and not CrossJoinExpression)
                {
                    return false;
                }

                if (targetTable is JoinExpressionBase joinExpressionBase)
                {
                    targetTable = joinExpressionBase.Table;
                }
            }

            if (targetTable is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual bool TryTranslateSetters(
        ShapedQueryExpression source,
        IReadOnlyList<ExecuteUpdateSetter> setters,
        [NotNullWhen(true)] out IReadOnlyList<ColumnValueSetter>? columnSetters,
        [NotNullWhen(true)] out TableExpressionBase? targetTable)
    {
        var select = (SelectExpression)source.QueryExpression;

        targetTable = null;
        string? targetTableAlias = null;
        var mutableColumnSetters = new List<ColumnValueSetter>();
        columnSetters = null;

        Expression? targetTablePropertySelector = null;

        foreach (var setter in setters)
        {
            var (propertySelector, valueSelector) = setter;
            var propertySelectorBody = RemapLambdaBody(source, propertySelector).UnwrapTypeConversion(out _);

            // The top-most node on the property selector must be a member access; chop it off to get the base expression and member.
            // We'll bind the member manually below, so as to get the IPropertyBase it represents - that's important for later.
            if (!IsMemberAccess(propertySelectorBody, QueryCompilationContext.Model, out var baseExpression, out var member)
                || !_sqlTranslator.TryBindMember(
                    _sqlTranslator.Visit(baseExpression), member, out var target, out var targetProperty))
            {
                AddTranslationErrorDetails(RelationalStrings.InvalidPropertyInSetProperty(propertySelector.Print()));
                return false;
            }

            if (targetProperty.DeclaringType is IEntityType entityType && entityType.IsMappedToJson())
            {
                AddTranslationErrorDetails(RelationalStrings.ExecuteOperationOnOwnedJsonIsNotSupported("ExecuteUpdate", entityType.DisplayName()));
                return false;
            }

            // Hack: when returning a StructuralTypeShaperExpression, _sqlTranslator returns it wrapped by a
            // StructuralTypeReferenceExpression, which is supposed to be a private wrapper only with the SQL translator.
            // Call TranslateProjection to unwrap it (need to look into getting rid StructuralTypeReferenceExpression altogether).
            if (target is not CollectionResultExpression)
            {
                target = _sqlTranslator.TranslateProjection(target);
            }

            switch (target)
            {
                case ColumnExpression column:
                {
                    Check.DebugAssert(column.TypeMapping is not null);

                    if (!TryProcessColumn(column)
                        || !TryTranslateScalarSetterValueSelector(
                            source, valueSelector, column.Type, column.TypeMapping, out var translatedValue))
                    {
                        return false;
                    }

                    mutableColumnSetters.Add(new(column, translatedValue));
                    break;
                }

                // A table-split complex type is being assigned a new value.
                // Generate setters for each of the columns mapped to the comlex type.
                case StructuralTypeShaperExpression
                {
                    StructuralType: IComplexType complexType,
                    ValueBufferExpression: StructuralTypeProjectionExpression
                } shaper:
                {
                    Check.DebugAssert(
                        targetProperty is IComplexProperty complexProperty && complexProperty.ComplexType == complexType,
                        "PropertyBase should be a complex property referring to the correct complex type");

                    if (complexType.IsMappedToJson())
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.ExecuteUpdateOverJsonIsNotSupported(complexType.DisplayName()));
                    }

                    if (!TryTranslateSetterValueSelector(source, valueSelector, shaper.Type, out var translatedValue)
                        || !TryProcessComplexType(shaper, translatedValue))
                    {
                        return false;
                    }

                    break;
                }

                case JsonScalarExpression { Json: ColumnExpression jsonColumn } jsonScalar:
                {
                    var typeMapping = jsonScalar.TypeMapping;
                    Check.DebugAssert(typeMapping is not null);

                    // We should never see a JsonScalarExpression without a path - that means we're mapping a JSON scalar directly to a relational column.
                    // This is in theory possible (e.g. map a DateTime to a 'json' column with a single string timestamp representation inside, instead of to
                    // SQL Server datetime2), but contrived and unsupported.
                    Check.DebugAssert(jsonScalar.Path.Count > 0);

                    if (!TryProcessColumn(jsonColumn)
                        || !TryTranslateScalarSetterValueSelector(source, valueSelector, jsonScalar.Type, typeMapping, out var translatedValue))
                    {
                        return false;
                    }

                    // We now have the relational scalar expression for the value; but we need the JSON representation to pass to the provider's JSON modification
                    // function (e.g. SQL Server JSON_MODIFY()).
                    // For example, for a DateTime we'd have e.g. a SqlConstantExpression containing a DateTime instance, but we need a string containing
                    // the JSON-encoded ISO8601 representation.
                    if (!TrySerializeScalarToJson(jsonScalar, translatedValue, out var jsonValue))
                    {
                        throw new InvalidOperationException(
                            translatedValue is ColumnExpression
                                ? RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn
                                : RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToArbitraryExpression);
                    }

                    // We now have a serialized JSON value (number, string or bool) - generate a setter for it.
                    GenerateJsonPartialUpdateSetterWrapper(jsonScalar, jsonColumn, jsonValue);
                    continue;
                }

                case StructuralTypeShaperExpression { ValueBufferExpression: JsonQueryExpression jsonQuery }:
                    if (!TryProcessStructuralJsonSetter(jsonQuery))
                    {
                        return false;
                    }

                    continue;

                case CollectionResultExpression { QueryExpression: JsonQueryExpression jsonQuery }:
                    if (!TryProcessStructuralJsonSetter(jsonQuery))
                    {
                        return false;
                    }

                    continue;

                default:
                    AddTranslationErrorDetails(RelationalStrings.InvalidPropertyInSetProperty(propertySelector.Print()));
                    return false;
            }

            void GenerateJsonPartialUpdateSetterWrapper(Expression target, ColumnExpression jsonColumn, SqlExpression value)
            {
                var index = mutableColumnSetters.FindIndex(s => s.Column.Equals(jsonColumn));
                var origExistingSetterValue = index == -1 ? null : mutableColumnSetters[index].Value;
                var modifiedExistingSetterValue = index == -1 ? null : mutableColumnSetters[index].Value;
                var newSetter = GenerateJsonPartialUpdateSetter(target, value, ref modifiedExistingSetterValue);

                if (origExistingSetterValue is null ^ modifiedExistingSetterValue is null)
                {
                    throw new UnreachableException(
                        "existingSetterValue should only be used to compose additional setters on an existing setter");
                }

                if (!ReferenceEquals(modifiedExistingSetterValue, origExistingSetterValue))
                {
                    mutableColumnSetters[index] = new(jsonColumn, modifiedExistingSetterValue!);
                }

                if (newSetter is not null)
                {
                    mutableColumnSetters.Add(new(jsonColumn, newSetter));
                }
            }

            bool TryProcessColumn(ColumnExpression column)
            {
                var tableExpression = select.GetTable(column, out var tableIndex);
                if (tableExpression.UnwrapJoin() is TableExpression { Table: not ITable } unwrappedTableExpression)
                {
                    // If the entity is also mapped to a view, the SelectExpression will refer to the view instead, since
                    // translation happens with the assumption that we're querying, not deleting.
                    // For this case, we must replace the TableExpression in the SelectExpression - referring to the view - with the
                    // one that refers to the mutable table.

                    // Get the column on the (mutable) table which corresponds to the property being set
                    IColumn? targetColumnModel;

                    switch (targetProperty)
                    {
                        // Note that we've already validated in TranslateExecuteUpdate that there can't be properties mapped to
                        // multiple columns (e.g. TPC)
                        case IProperty property:
                            targetColumnModel = property.DeclaringType.GetTableMappings()
                                .SelectMany(tm => tm.ColumnMappings)
                                .Where(cm => cm.Property == property)
                                .Select(cm => cm.Column)
                                .SingleOrDefault();
                            break;

                        case IComplexProperty { ComplexType: var complexType } complexProperty:
                        {
                            // Find the container column in the relational model to get its type mapping
                            // Note that we assume exactly one column with the given name mapped to the entity (despite entity splitting).
                            // See #36647 and #36646 about improving this.
                            var containerColumnName = complexType.GetContainerColumnName();
                            targetColumnModel = complexType.ContainingEntityType.GetTableMappings()
                                .SelectMany(m => m.Table.Columns)
                                .Where(c => c.Name == containerColumnName)
                                .Single();

                            break;
                        }

                        default:
                            throw new UnreachableException();
                    }

                    if (targetColumnModel is null)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.ExecuteUpdateDeleteOnEntityNotMappedToTable(targetProperty.DeclaringType.DisplayName()));
                    }

                    unwrappedTableExpression = new TableExpression(unwrappedTableExpression.Alias, targetColumnModel.Table);
                    tableExpression = tableExpression is JoinExpressionBase join
                        ? join.Update(unwrappedTableExpression)
                        : unwrappedTableExpression;
                    var newTables = select.Tables.ToList();
                    newTables[tableIndex] = tableExpression;

                    // Note that we need to keep the select mutable, because if IsValidSelectExpressionForExecuteDelete below
                    // returns false, we need to compose on top of it.
                    select.SetTables(newTables);
                }

                return IsColumnOnSameTable(column, propertySelector);
            }

            // Recursively processes the complex types and all complex types referenced by it, adding setters fo all (non-complex)
            // properties.
            // Note that this only supports table splitting (where all columns are flattened to the table), but not JSON complex types (#28766).
            bool TryProcessComplexType(StructuralTypeShaperExpression shaperExpression, Expression valueExpression)
            {
                if (shaperExpression.StructuralType is not IComplexType complexType
                    || shaperExpression.ValueBufferExpression is not StructuralTypeProjectionExpression projection)
                {
                    return false;
                }

                foreach (var property in complexType.GetProperties())
                {
                    var column = projection.BindProperty(property);
                    if (!IsColumnOnSameTable(column, propertySelector))
                    {
                        return false;
                    }

                    var rewrittenValueSelector = CreatePropertyAccessExpression(valueExpression, property);
                    if (!TryTranslateScalarSetterValueSelector(
                        source, rewrittenValueSelector, column.Type, column.TypeMapping!, out var translatedValueSelector))
                    {
                        return false;
                    }

                    mutableColumnSetters.Add(new ColumnValueSetter(column, translatedValueSelector));
                }

                foreach (var complexProperty in complexType.GetComplexProperties())
                {
                    // Note that TranslateProjection currently returns null for StructuralTypeReferenceExpression with a subquery (as
                    // opposed to a parameter); this ensures that we don't generate an efficient translation where the subquery is
                    // duplicated for every property on the complex type.
                    // TODO: Make this work by using a common table expression (CTE)

                    if (complexProperty.ComplexType.IsMappedToJson())
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.ExecuteUpdateOverJsonIsNotSupported(complexProperty.ComplexType.DisplayName()));
                    }

                    var nestedShaperExpression = (StructuralTypeShaperExpression)projection.BindComplexProperty(complexProperty);
                    var nestedValueExpression = CreateComplexPropertyAccessExpression(valueExpression, complexProperty);
                    if (!TryProcessComplexType(nestedShaperExpression, nestedValueExpression))
                    {
                        return false;
                    }
                }

                return true;

                Expression CreatePropertyAccessExpression(Expression target, IProperty property)
                {
                    return target is LambdaExpression lambda
                        ? Expression.Lambda(Core(lambda.Body, property), lambda.Parameters[0])
                        : Core(target, property);

                    Expression Core(Expression target, IProperty property)
                    {
                        switch (target)
                        {
                            case SqlConstantExpression constantExpression:
                                return Expression.Constant(
                                    constantExpression.Value is null
                                        ? null
                                        : property.GetGetter().GetClrValue(constantExpression.Value),
                                    property.ClrType.MakeNullable());

                            case SqlParameterExpression parameterExpression:
                            {
                                var lambda = Expression.Lambda(
                                    Expression.Call(
                                        ParameterValueExtractorMethod.MakeGenericMethod(property.ClrType.MakeNullable()),
                                        QueryCompilationContext.QueryContextParameter,
                                        Expression.Constant(parameterExpression.Name, typeof(string)),
                                        Expression.Constant(null, typeof(List<IComplexProperty>)),
                                        Expression.Constant(property, typeof(IProperty))),
                                    QueryCompilationContext.QueryContextParameter);

                                var newParameterName =
                                    $"{ExecuteUpdateRuntimeParameterPrefix}{parameterExpression.Name}_{property.Name}";

                                return _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);
                            }

                            case ParameterBasedComplexPropertyChainExpression chainExpression:
                            {
                                var lambda = Expression.Lambda(
                                    Expression.Call(
                                        ParameterValueExtractorMethod.MakeGenericMethod(property.ClrType.MakeNullable()),
                                        QueryCompilationContext.QueryContextParameter,
                                        Expression.Constant(chainExpression.ParameterExpression.Name, typeof(string)),
                                        Expression.Constant(chainExpression.ComplexPropertyChain, typeof(List<IComplexProperty>)),
                                        Expression.Constant(property, typeof(IProperty))),
                                    QueryCompilationContext.QueryContextParameter);

                                var newParameterName =
                                    $"{ExecuteUpdateRuntimeParameterPrefix}{chainExpression.ParameterExpression.Name}_{property.Name}";

                                return _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);
                            }

                            case MemberInitExpression memberInitExpression
                                when memberInitExpression.Bindings.SingleOrDefault(mb => mb.Member.Name == property.Name) is MemberAssignment
                                    memberAssignment:
                                return memberAssignment.Expression;

                            default:
                                return target.CreateEFPropertyExpression(property);
                        }
                    }
                }

                Expression CreateComplexPropertyAccessExpression(Expression target, IComplexProperty complexProperty)
                {
                    return target is LambdaExpression lambda
                        ? Expression.Lambda(Core(lambda.Body, complexProperty), lambda.Parameters[0])
                        : Core(target, complexProperty);

                    Expression Core(Expression target, IComplexProperty complexProperty)
                        => target switch
                        {
                            SqlConstantExpression constant => _sqlExpressionFactory.Constant(
                                constant.Value is null ? null : complexProperty.GetGetter().GetClrValue(constant.Value),
                                complexProperty.ClrType.MakeNullable()),

                            SqlParameterExpression parameter
                                => new ParameterBasedComplexPropertyChainExpression(parameter, complexProperty),

                            StructuralTypeShaperExpression
                            {
                                StructuralType: IComplexType,
                                ValueBufferExpression: StructuralTypeProjectionExpression projection
                            }
                                => projection.BindComplexProperty(complexProperty),

                            _ => throw new UnreachableException()
                        };
                }
            }

            bool TryProcessStructuralJsonSetter(JsonQueryExpression jsonQuery)
            {
                var jsonColumn = jsonQuery.JsonColumn;

                if (jsonQuery.StructuralType is not IComplexType complexType)
                {
                    throw new InvalidOperationException(RelationalStrings.JsonExecuteUpdateNotSupportedWithOwnedEntities);
                }

                Check.DebugAssert(jsonColumn.TypeMapping is not null);

                if (!TryProcessColumn(jsonColumn)
                    || !TryTranslateSetterValueSelector(source, valueSelector, jsonQuery.Type, out var translatedValue))
                {
                    return false;
                }

                SqlExpression? serializedValue;

                switch (translatedValue)
                {
                    // When an object is instantiated inline (e.g. SetProperty(c => c.ShippingAddress, c => new Address { ... })), we get a SqlConstantExpression
                    // with the .NET instance. Serialize it to JSON and replace the constant (note that the type mapping is inferred from the
                    // JSON column on other side - important for e.g. nvarchar vs. json columns)
                    case SqlConstantExpression { Value: var value }:
                        serializedValue = new SqlConstantExpression(
                            RelationalJsonUtilities.SerializeComplexTypeToJson(complexType, value, jsonQuery.IsCollection),
                            typeof(string),
                            typeMapping: jsonColumn.TypeMapping);
                        break;

                    case SqlParameterExpression parameter:
                    {
                        var queryParameter = _queryCompilationContext.RegisterRuntimeParameter(
                            $"{ExecuteUpdateRuntimeParameterPrefix}{parameter.Name}",
                            Expression.Lambda(
                                Expression.Call(
                                    RelationalJsonUtilities.SerializeComplexTypeToJsonMethod,
                                    Expression.Constant(complexType),
                                    Expression.MakeIndex(
                                        Expression.Property(
                                            QueryCompilationContext.QueryContextParameter, nameof(QueryContext.Parameters)),
                                        indexer: typeof(Dictionary<string, object>).GetProperty("Item", [typeof(string)]),
                                        [Expression.Constant(parameter.Name, typeof(string))]),
                                    Expression.Constant(jsonQuery.IsCollection)),
                                QueryCompilationContext.QueryContextParameter));

                        serializedValue = _sqlExpressionFactory.ApplyTypeMapping(
                            _sqlTranslator.Translate(queryParameter, applyDefaultTypeMapping: false),
                            jsonColumn.TypeMapping)!;
                        break;
                    }

                    case RelationalStructuralTypeShaperExpression { ValueBufferExpression: JsonQueryExpression valueJsonQuery }:
                        serializedValue = ProcessJsonQuery(valueJsonQuery);
                        break;

                    case CollectionResultExpression { QueryExpression: JsonQueryExpression valueJsonQuery }:
                        serializedValue = ProcessJsonQuery(valueJsonQuery);
                        break;

                    default:
                        throw new UnreachableException();
                }

                // If the entire JSON column is being referenced as the target, remove the JsonQueryExpression altogether
                // and just add a plain old setter updating the column as a whole; since this scenario doesn't involve any
                // partial update, we can just add the setter directly without going through the provider's TranslateJsonSetter
                // (see #30768 for stopping producing empty Json{Scalar,Query}Expressions).
                // Otherwise, call the TranslateJsonSetter hook to produce the provider-specific syntax for JSON partial update.
                if (jsonQuery.Path is [])
                {
                    mutableColumnSetters.Add(new ColumnValueSetter(jsonColumn, serializedValue));
                }
                else
                {
                    GenerateJsonPartialUpdateSetterWrapper(jsonQuery, jsonColumn, serializedValue);
                }

                return true;
            }

            bool TryTranslateScalarSetterValueSelector(
                ShapedQueryExpression source,
                Expression valueSelector,
                Type type,
                RelationalTypeMapping typeMapping,
                [NotNullWhen(true)] out SqlExpression? result)
            {
                if (TryTranslateSetterValueSelector(source, valueSelector, type, out var tempResult)
                    && tempResult is SqlExpression translatedSelector)
                {
                    // Apply the type mapping of the column (translated from the property selector above) to the value
                    result = _sqlExpressionFactory.ApplyTypeMapping(translatedSelector, typeMapping);
                    return true;
                }

                AddTranslationErrorDetails(RelationalStrings.InvalidValueInSetProperty(valueSelector.Print()));
                result = null;
                return false;
            }

            bool TryTranslateSetterValueSelector(
                ShapedQueryExpression source,
                Expression valueSelector,
                Type propertyType,
                [NotNullWhen(true)] out Expression? result)
            {
                var remappedValueSelector = valueSelector is LambdaExpression lambdaExpression
                    ? RemapLambdaBody(source, lambdaExpression)
                    : valueSelector;

                if (remappedValueSelector.Type != propertyType)
                {
                    remappedValueSelector = Expression.Convert(remappedValueSelector, propertyType);
                }

                result = _sqlTranslator.TranslateProjection(remappedValueSelector, applyDefaultTypeMapping: false);

                if (result is null)
                {
                    AddTranslationErrorDetails(RelationalStrings.InvalidValueInSetProperty(valueSelector.Print()));
                    return false;
                }

                return true;
            }

            bool IsColumnOnSameTable(ColumnExpression column, LambdaExpression propertySelector)
            {
                if (targetTableAlias is null)
                {
                    targetTableAlias = column.TableAlias;
                    targetTablePropertySelector = propertySelector;
                }
                else if (column.TableAlias != targetTableAlias)
                {
                    AddTranslationErrorDetails(
                        RelationalStrings.MultipleTablesInExecuteUpdate(propertySelector.Print(), targetTablePropertySelector!.Print()));
                    return false;
                }

                return true;
            }

            // If the entire JSON column is being referenced, remove the JsonQueryExpression altogether and just return
            // the column (no need for special JSON modification functions/syntax).
            // See #30768 for stopping producing empty Json{Scalar,Query}Expressions.
            // Otherwise, convert the JsonQueryExpression to a JsonScalarExpression, which is our current representation for a complex
            // JSON in the SQL tree (as opposed to in the shaper) - see #36392.
            static SqlExpression ProcessJsonQuery(JsonQueryExpression jsonQuery)
                => jsonQuery.Path is []
                    ? jsonQuery.JsonColumn
                    : new JsonScalarExpression(
                        jsonQuery.JsonColumn,
                        jsonQuery.Path,
                        jsonQuery.Type,
                        jsonQuery.JsonColumn.TypeMapping,
                        jsonQuery.IsNullable);
        }

        Check.DebugAssert(targetTableAlias is not null, "Target table alias should have a value");
        var selectExpression = (SelectExpression)source.QueryExpression;
        targetTable = selectExpression.Tables.First(t => t.GetRequiredAlias() == targetTableAlias);
        columnSetters = mutableColumnSetters;

        return true;
    }

    /// <summary>
    ///     Serializes a relational scalar value to JSON for partial updating within a JSON column within
    ///     <see cref="TranslateExecuteUpdate" />.
    /// </summary>
    /// <param name="target">The expression representing the JSON scalar property to be updated.</param>
    /// <param name="value">A translated value (SqlConstantExpression, JsonScalarExpression) to serialize.</param>
    /// <param name="jsonValue">
    ///     The result expression representing a JSON expression ready to be passed to the provider's JSON partial
    ///     update function.
    /// </param>
    /// <returns>A scalar expression ready to be integrated into an UPDATE statement setter.</returns>
    protected virtual bool TrySerializeScalarToJson(
        JsonScalarExpression target,
        SqlExpression value,
        [NotNullWhen(true)] out SqlExpression? jsonValue)
    {
        var typeMapping = value.TypeMapping;
        Check.DebugAssert(typeMapping is not null);

        // First, for the types natively supported in JSON (int, string, bool), just pass these in as is, since the JSON functions support these
        // directly across databases.
        var providerClrType = (typeMapping.Converter?.ProviderClrType ?? value.Type).UnwrapNullableType();
        if (providerClrType.IsNumeric()
            || providerClrType == typeof(string)
            || providerClrType == typeof(bool))
        {
            jsonValue = value;
            return true;
        }

        Check.DebugAssert(typeMapping.JsonValueReaderWriter is not null, "Missing JsonValueReaderWriter on JSON property");
        var stringTypeMapping = _typeMappingSource.FindMapping(typeof(string))!;

        switch (value)
        {
            // When an object is instantiated inline (e.g. SetProperty(c => c.ShippingAddress, c => new Address { ... })), we get a SqlConstantExpression
            // with the .NET instance. Serialize it to JSON and replace the constant.
            case SqlConstantExpression { Value: var constantValue }:
            {
                string? jsonString;

                if (constantValue is null)
                {
                    jsonString = null;
                }
                else
                {
                    // We should only be here for things that get serialized to strings.
                    // Non-string JSON types (number, bool) should have been checked beforehand and handled differently.
                    jsonString = typeMapping.JsonValueReaderWriter.ToJsonString(constantValue);
                    Check.DebugAssert(jsonString.StartsWith('"') && jsonString.EndsWith('"'));
                    jsonString = jsonString[1..^1];
                }

                jsonValue = new SqlConstantExpression(jsonString, typeof(string), stringTypeMapping);
                return true;
            }

            case SqlParameterExpression sqlParameter:
            {
                var queryParameter = _queryCompilationContext.RegisterRuntimeParameter(
                    $"{ExecuteUpdateRuntimeParameterPrefix}{sqlParameter.Name}",
                    Expression.Lambda(
                        Expression.Call(
                            ParameterJsonSerializerMethod,
                            QueryCompilationContext.QueryContextParameter,
                            Expression.Constant(sqlParameter.Name, typeof(string)),
                            Expression.Constant(typeMapping.JsonValueReaderWriter, typeof(JsonValueReaderWriter))),
                        QueryCompilationContext.QueryContextParameter));
                jsonValue = (SqlParameterExpression)_sqlExpressionFactory.ApplyTypeMapping(
                    _sqlTranslator.Translate(queryParameter, applyDefaultTypeMapping: false),
                    stringTypeMapping)!;
                return true;
            }

            case JsonScalarExpression jsonScalarValue:
                // The JSON scalar property is being assigned to another JSON scalar property.
                // In principle this is easy - just copy the property value across; but the JsonScalarExpression
                // is typed for its relational value (e.g. datetime2), which we can't pass the the partial update
                // function.
                // Since int and bool have been handled above, simply retype the JsonScalarExpression to return
                // a string instead, to get the raw JSON representation.
                jsonValue = new JsonScalarExpression(
                    jsonScalarValue.Json,
                    jsonScalarValue.Path,
                    typeof(string),
                    stringTypeMapping,
                    jsonScalarValue.IsNullable);
                return true;

            default:
                jsonValue = null;
                return false;
        }
    }

    /// <summary>
    ///     Provider extension point for implementing partial updates within JSON columns.
    /// </summary>
    /// <param name="target">
    ///     An expression representing the target to be updated; can be either <see cref="JsonScalarExpression" />
    ///     (when a scalar property is being updated within the JSON column), or a <see cref="JsonQueryExpression" />
    ///     (when an object or collection is being updated).
    /// </param>
    /// <param name="value">The JSON value to be set, ready for use as-is in <see cref="QuerySqlGenerator" />.</param>
    /// <param name="existingSetterValue">
    ///     If a setter was previously created for this JSON column, it's value is passed here (this happens when e.g.
    ///     multiple properties are updated in the same JSON column). Implementations can compose the new setter into
    ///     the existing one (and return <see langword="null" />), or return a new one.
    /// </param>
    protected virtual SqlExpression? GenerateJsonPartialUpdateSetter(
        Expression target,
        SqlExpression value,
        ref SqlExpression? existingSetterValue)
        => throw new InvalidOperationException(RelationalStrings.JsonPartialExecuteUpdateNotSupportedByProvider);

    private static T? ParameterValueExtractor<T>(
        QueryContext context,
        string baseParameterName,
        List<IComplexProperty>? complexPropertyChain,
        IProperty property)
    {
        var baseValue = context.Parameters[baseParameterName];

        if (complexPropertyChain is not null)
        {
            foreach (var complexProperty in complexPropertyChain)
            {
                if (baseValue is null)
                {
                    break;
                }

                baseValue = complexProperty.GetGetter().GetClrValue(baseValue);
            }
        }

        return baseValue == null ? (T?)(object?)null : (T?)property.GetGetter().GetClrValue(baseValue);
    }

    private static string? ParameterJsonSerializer(QueryContext queryContext, string baseParameterName, JsonValueReaderWriter jsonValueReaderWriter)
    {
        var value = queryContext.Parameters[baseParameterName];

        if (value is null)
        {
            return null;
        }

        // We should only be here for things that get serialized to strings.
        // Non-string JSON types (number, bool, null) should have been checked beforehand and handled differently.
        var jsonString = jsonValueReaderWriter.ToJsonString(value);
        Check.DebugAssert(jsonString.StartsWith('"') && jsonString.EndsWith('"'));
        return jsonString[1..^1];
    }

    private sealed class ParameterBasedComplexPropertyChainExpression(
        SqlParameterExpression parameterExpression,
        IComplexProperty firstComplexProperty)
        : Expression
    {
        public SqlParameterExpression ParameterExpression { get; } = parameterExpression;
        public List<IComplexProperty> ComplexPropertyChain { get; } = [firstComplexProperty];
    }
}
