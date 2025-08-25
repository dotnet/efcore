// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using static Microsoft.EntityFrameworkCore.Infrastructure.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.Query;

public partial class RelationalQueryableMethodTranslatingExpressionVisitor
{
    private const string ExecuteUpdateRuntimeParameterPrefix = "complex_type_";

    private static readonly MethodInfo ParameterValueExtractorMethod =
        typeof(RelationalSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterValueExtractor))!;

    /// <inheritdoc />
    protected override UpdateExpression? TranslateExecuteUpdate(ShapedQueryExpression source, IReadOnlyList<ExecuteUpdateSetter> setters)
    {
        if (setters.Count == 0)
        {
            throw new UnreachableException("Empty setters list");
        }

        // Our source may have IncludeExpressions because of owned entities or auto-include; unwrap these, as they're meaningless for
        // ExecuteUpdate's lambdas. Note that we don't currently support updates across tables.
        source = source.UpdateShaperExpression(new IncludePruner().Visit(source.ShaperExpression));

        if (TranslationErrorDetails != null)
        {
            return null;
        }

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
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (IsValidSelectExpressionForExecuteUpdate(selectExpression, targetTable, out var tableExpression))
        {
            selectExpression.ReplaceProjection(new List<Expression>());
            selectExpression.ApplyProjection();

            return new UpdateExpression(tableExpression, selectExpression, translatedSetters);
        }

        return PushdownWithPkInnerJoinPredicate();

        bool TryTranslateSetters(
            ShapedQueryExpression source,
            IReadOnlyList<ExecuteUpdateSetter> setters,
            [NotNullWhen(true)] out List<ColumnValueSetter>? translatedSetters,
            [NotNullWhen(true)] out TableExpressionBase? targetTable)
        {
            var select = (SelectExpression)source.QueryExpression;

            targetTable = null;
            string? targetTableAlias = null;
            var tempTranslatedSetters = new List<ColumnValueSetter>();
            translatedSetters = null;

            LambdaExpression? propertySelector;
            Expression? targetTablePropertySelector = null;

            foreach (var setter in setters)
            {
                (propertySelector, var valueSelector) = setter;
                var propertySelectorBody = RemapLambdaBody(source, propertySelector).UnwrapTypeConversion(out _);

                // The top-most node on the property selector must be a member access; chop it off to get the base expression and member.
                // We'll bind the member manually below, so as to get the IPropertyBase it represents - that's important for later.
                if (!IsMemberAccess(propertySelectorBody, QueryCompilationContext.Model, out var baseExpression, out var member))
                {
                    AddTranslationErrorDetails(RelationalStrings.InvalidPropertyInSetProperty(propertySelector.Print()));
                    return false;
                }

                if (!_sqlTranslator.TryBindMember(
                        _sqlTranslator.Visit(baseExpression), member, out var translatedBaseExpression, out var propertyBase))
                {
                    AddTranslationErrorDetails(RelationalStrings.InvalidPropertyInSetProperty(propertySelector.Print()));
                    return false;
                }

                // Hack: when returning a StructuralTypeShaperExpression, _sqlTranslator returns it wrapped by a
                // StructuralTypeReferenceExpression, which is supposed to be a private wrapper only with the SQL translator.
                // Call TranslateProjection to unwrap it (need to look into getting rid StructuralTypeReferenceExpression altogether).
                if (translatedBaseExpression is not CollectionResultExpression)
                {
                    translatedBaseExpression = _sqlTranslator.TranslateProjection(translatedBaseExpression);
                }

                switch (translatedBaseExpression)
                {
                    case ColumnExpression column:
                    {
                        Check.DebugAssert(column.TypeMapping is not null);

                        if (!TryProcessColumn(column)
                            || !TryTranslateScalarSetterValueSelector(
                                source, valueSelector, column.Type, column.TypeMapping, out var translatedValueSelector))
                        {
                            return false;
                        }

                        tempTranslatedSetters.Add(new ColumnValueSetter(column, translatedValueSelector));
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
                            propertyBase is IComplexProperty complexProperty && complexProperty.ComplexType == complexType,
                            "PropertyBase should be a complex property referring to the correct complex type");

                        if (complexType.IsMappedToJson())
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.ExecuteUpdateOverJsonIsNotSupported(complexType.DisplayName()));
                        }

                        if (!TryTranslateSetterValueSelector(source, valueSelector, shaper.Type, out var translatedValueSelector)
                            || !TryProcessComplexType(shaper, translatedValueSelector))
                        {
                            return false;
                        }

                        break;
                    }

                    case JsonScalarExpression { Json: ColumnExpression jsonColumn } jsonScalar:
                    {
                        Check.DebugAssert(jsonScalar.TypeMapping is not null);

                        if (!TryProcessColumn(jsonColumn)
                            || !TryTranslateScalarSetterValueSelector(source, valueSelector, jsonScalar.Type, jsonScalar.TypeMapping, out var translatedValueSelector))
                        {
                            return false;
                        }

                        // If the entire JSON column is being referenced as the target, remove the JsonScalarExpression altogether
                        // and just add a plain old setter updating the column as a whole; since this scenario doesn't involve any
                        // partial update, we can just add the setter directly without going through the provider's TranslateJsonSetter
                        // (see #30768 for stopping producing empty Json{Scalar,Query}Expressions).
                        // Otherwise, call the TranslateJsonSetter hook to produce the provider-specific syntax for JSON partial update.
                        tempTranslatedSetters.Add(
                            jsonScalar.Path is []
                                ? new ColumnValueSetter(jsonColumn, translatedValueSelector)
                                : GenerateJsonPartialUpdateSetter(jsonScalar, translatedValueSelector));

                        break;
                    }

                    case StructuralTypeShaperExpression { ValueBufferExpression: JsonQueryExpression jsonQuery }:
                        if (!TryProcessStructuralJsonSetter(jsonQuery))
                        {
                            return false;
                        }

                        break;

                    case CollectionResultExpression { QueryExpression: JsonQueryExpression jsonQuery }:
                        if (!TryProcessStructuralJsonSetter(jsonQuery))
                        {
                            return false;
                        }

                        break;

                    default:
                        AddTranslationErrorDetails(RelationalStrings.InvalidPropertyInSetProperty(propertySelector.Print()));
                        return false;

                        bool TryProcessStructuralJsonSetter(JsonQueryExpression jsonQuery)
                        {
                            var jsonColumn = jsonQuery.JsonColumn;
                            var complexType = (IComplexType)jsonQuery.StructuralType;

                            Check.DebugAssert(jsonColumn.TypeMapping is not null);

                            if (!TryProcessColumn(jsonColumn)
                                || !TryTranslateSetterValueSelector(
                                    source, valueSelector, jsonQuery.Type, out var translatedValueSelector))
                            {
                                return false;
                            }

                            SqlExpression? serializedValueSelector;

                            switch (translatedValueSelector)
                            {
                                // When an object is instantiated inline (e.g. SetProperty(c => c.ShippingAddress, c => new Address { ... })), we get a SqlConstantExpression
                                // with the .NET instance. Serialize it to JSON and replace the constant (note that the type mapping is inferred from the
                                // JSON column on other side - important for e.g. nvarchar vs. json columns)
                                case SqlConstantExpression { Value: var value }:
                                    serializedValueSelector = new SqlConstantExpression(
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

                                    serializedValueSelector = _sqlExpressionFactory.ApplyTypeMapping(
                                        _sqlTranslator.Translate(queryParameter, applyDefaultTypeMapping: false),
                                        jsonColumn.TypeMapping)!;
                                    break;
                                }

                                case RelationalStructuralTypeShaperExpression { ValueBufferExpression: JsonQueryExpression valueJsonQuery }:
                                    serializedValueSelector = ProcessJsonQuery(valueJsonQuery);
                                    break;

                                case CollectionResultExpression { QueryExpression: JsonQueryExpression valueJsonQuery }:
                                    serializedValueSelector = ProcessJsonQuery(valueJsonQuery);
                                    break;

                                default:
                                    throw new UnreachableException();

                                    // If the entire JSON column is being referenced, remove the JsonQueryExpression altogether and just return
                                    // the column (no need for special JSON modification functions/syntax).
                                    // See #30768 for stopping producing empty Json{Scalar,Query}Expressions.
                                    // Otherwise, convert the JsonQueryExpression to a JsonScalarExpression, which is our current representation for a complex
                                    // JSON in the SQL tree (as opposed to in the shaper) - see #36392.
                                    SqlExpression ProcessJsonQuery(JsonQueryExpression jsonQuery)
                                        => jsonQuery.Path is []
                                            ? jsonQuery.JsonColumn
                                            : new JsonScalarExpression(
                                                jsonQuery.JsonColumn,
                                                jsonQuery.Path,
                                                jsonQuery.Type,
                                                jsonQuery.JsonColumn.TypeMapping,
                                                jsonQuery.IsNullable);
                            }

                            // If the entire JSON column is being referenced as the target, remove the JsonQueryExpression altogether
                            // and just add a plain old setter updating the column as a whole; since this scenario doesn't involve any
                            // partial update, we can just add the setter directly without going through the provider's TranslateJsonSetter
                            // (see #30768 for stopping producing empty Json{Scalar,Query}Expressions).
                            // Otherwise, call the TranslateJsonSetter hook to produce the provider-specific syntax for JSON partial update.
                            tempTranslatedSetters.Add(
                                jsonQuery.Path is []
                                    ? new ColumnValueSetter(jsonColumn, serializedValueSelector)
                                    : GenerateJsonPartialUpdateSetter(jsonQuery, serializedValueSelector));
                            return true;
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

                                switch (propertyBase)
                                {
                                    case IProperty property:
                                        targetColumnModel = property.DeclaringType.GetTableMappings()
                                            .SelectMany(tm => tm.ColumnMappings)
                                            .Where(cm => cm.Property == property)
                                            .Select(cm => cm.Column)
                                            .SingleOrDefault();
                                        break;

                                    case IComplexProperty { ComplexType: var complexType } complexProperty:
                                    {
                                        // TODO: Make this better with #36646
                                        var containerColumnName = complexType.GetContainerColumnName();
                                        var containerColumnCandidates = complexType.ContainingEntityType.GetTableMappings()
                                            .SelectMany(m => m.Table.Columns)
                                            .Where(c => c.Name == containerColumnName)
                                            .ToList();

                                        targetColumnModel = containerColumnCandidates switch
                                        {
                                            [var c] => c,
                                            [] => throw new UnreachableException($"No container column found in relational model for {complexType.DisplayName()}"),
                                            _ => throw new InvalidOperationException(
                                                RelationalStrings.MultipleColumnsWithSameJsonContainerName(complexType.ContainingEntityType.DisplayName(), containerColumnName))
                                        };

                                        break;
                                    }

                                    default:
                                        throw new UnreachableException();
                                }

                                if (targetColumnModel is null)
                                {
                                    throw new InvalidOperationException(
                                        RelationalStrings.ExecuteUpdateDeleteOnEntityNotMappedToTable(propertyBase.DeclaringType.DisplayName()));
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
                }
            }

            translatedSetters = tempTranslatedSetters;

            Check.DebugAssert(targetTableAlias is not null, "Target table alias should have a value");
            var selectExpression = (SelectExpression)source.QueryExpression;
            targetTable = selectExpression.Tables.First(t => t.GetRequiredAlias() == targetTableAlias);

            return true;

            bool IsColumnOnSameTable(ColumnExpression column, LambdaExpression propertySelector)
            {
                if (targetTableAlias is null)
                {
                    targetTableAlias = column.TableAlias;
                    targetTablePropertySelector = propertySelector;
                }
                else if (!ReferenceEquals(column.TableAlias, targetTableAlias))
                {
                    AddTranslationErrorDetails(
                        RelationalStrings.MultipleTablesInExecuteUpdate(
                            propertySelector.Print(), targetTablePropertySelector!.Print()));
                    return false;
                }

                return true;
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

                    tempTranslatedSetters.Add(new ColumnValueSetter(column, translatedValueSelector));
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
            }

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
    ///     Provider extension point for implementing partial updates within JSON columns.
    /// </summary>
    /// <param name="target">
    ///     An expression representing the target to be updated; can be either <see cref="JsonScalarExpression" />
    ///     (when a scalar property is being updated within the JSON column), or a <see cref="JsonQueryExpression" />
    ///     (when an object or collection is being updated).
    /// </param>
    /// <param name="value">The JSON value to be set, ready for use as-is in <see cref="QuerySqlGenerator" />.</param>
    protected virtual ColumnValueSetter GenerateJsonPartialUpdateSetter(Expression target, SqlExpression value)
        => throw new InvalidOperationException(RelationalStrings.JsonPartialUpdateNotSupportedByProvider);

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

    private sealed class ParameterBasedComplexPropertyChainExpression(
        SqlParameterExpression parameterExpression,
        IComplexProperty firstComplexProperty)
        : Expression
    {
        public SqlParameterExpression ParameterExpression { get; } = parameterExpression;
        public List<IComplexProperty> ComplexPropertyChain { get; } = [firstComplexProperty];
    }
}
