// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using static Microsoft.EntityFrameworkCore.Query.QueryHelpers;

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
        if (!TranslateSetters(source, setters, out var translatedSetters, out var targetTable))
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

        bool TranslateSetters(
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

                if (!_sqlTranslator.TryBindMember(_sqlTranslator.Visit(baseExpression), member, out var translatedBaseExpression, out var propertyBase))
                {
                    AddTranslationErrorDetails(RelationalStrings.InvalidPropertyInSetProperty(propertySelector.Print()));
                    return false;
                }

                // Hack: when returning a StructuralTypeShaperExpression, _sqlTranslator returns it wrapped by a
                // StructuralTypeReferenceExpression, which is supposed to be a private wrapper only with the SQL translator.
                // Call TranslateProjection to unwrap it (need to look into getting rid StructuralTypeReferenceExpression altogether).
                translatedBaseExpression = _sqlTranslator.TranslateProjection(translatedBaseExpression);

                switch (translatedBaseExpression)
                {
                    case ColumnExpression column:
                    {
                        if (propertyBase is not IProperty property)
                        {
                            throw new UnreachableException("Property selector translated to ColumnExpression but no IProperty");
                        }

                        var tableExpression = select.GetTable(column, out var tableIndex);
                        if (tableExpression.UnwrapJoin() is TableExpression { Table: not ITable } unwrappedTableExpression)
                        {
                            // If the entity is also mapped to a view, the SelectExpression will refer to the view instead, since
                            // translation happens with the assumption that we're querying, not deleting.
                            // For this case, we must replace the TableExpression in the SelectExpression - referring to the view - with the
                            // one that refers to the mutable table.

                            // Get the column on the (mutable) table which corresponds to the property being set
                            var targetColumnModel = property.DeclaringType.GetTableMappings()
                                .SelectMany(tm => tm.ColumnMappings)
                                .Where(cm => cm.Property == property)
                                .Select(cm => cm.Column)
                                .SingleOrDefault();

                            if (targetColumnModel is null)
                            {
                                throw new InvalidOperationException(
                                    RelationalStrings.ExecuteUpdateDeleteOnEntityNotMappedToTable(property.DeclaringType.DisplayName()));
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

                        if (!IsColumnOnSameTable(column, propertySelector)
                            || TranslateSqlSetterValueSelector(source, valueSelector, column) is not SqlExpression translatedValueSelector)
                        {
                            return false;
                        }

                        tempTranslatedSetters.Add(new ColumnValueSetter(column, translatedValueSelector));
                        break;
                    }

                    // TODO: This is for column flattening; implement JSON complex type support as well.
                    case StructuralTypeShaperExpression
                    {
                        StructuralType: IComplexType complexType,
                        ValueBufferExpression: StructuralTypeProjectionExpression
                    } shaper:
                    {
                        Check.DebugAssert(
                            propertyBase is IComplexProperty complexProperty && complexProperty.ComplexType == complexType,
                            "PropertyBase should be a complex property referring to the correct complex type");

                        if (TranslateSetterValueSelector(source, valueSelector, shaper.Type) is not Expression translatedValueSelector
                            || !TryProcessComplexType(shaper, translatedValueSelector))
                        {
                            return false;
                        }

                        break;
                    }

                    default:
                        AddTranslationErrorDetails(RelationalStrings.InvalidPropertyInSetProperty(propertySelector.Print()));
                        return false;
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
                    if (TranslateSqlSetterValueSelector(
                            source, rewrittenValueSelector, column) is not SqlExpression translatedValueSelector)
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

                    var nestedShaperExpression = projection.BindComplexProperty(complexProperty);
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
                            when memberInitExpression.Bindings.SingleOrDefault(
                                mb => mb.Member.Name == property.Name) is MemberAssignment memberAssignment:
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

        SqlExpression? TranslateSqlSetterValueSelector(
            ShapedQueryExpression source,
            Expression valueSelector,
            ColumnExpression column)
        {
            if (TranslateSetterValueSelector(source, valueSelector, column.Type) is SqlExpression translatedSelector)
            {
                // Apply the type mapping of the column (translated from the property selector above) to the value
                translatedSelector = _sqlExpressionFactory.ApplyTypeMapping(translatedSelector, column.TypeMapping);
                return translatedSelector;
            }

            AddTranslationErrorDetails(RelationalStrings.InvalidValueInSetProperty(valueSelector.Print()));
            return null;
        }

        Expression? TranslateSetterValueSelector(ShapedQueryExpression source, Expression valueSelector, Type propertyType)
        {
            var remappedValueSelector = valueSelector is LambdaExpression lambdaExpression
                ? RemapLambdaBody(source, lambdaExpression)
                : valueSelector;

            if (remappedValueSelector.Type != propertyType)
            {
                remappedValueSelector = Expression.Convert(remappedValueSelector, propertyType);
            }

            if (_sqlTranslator.TranslateProjection(remappedValueSelector, applyDefaultTypeMapping: false) is not Expression
                translatedValueSelector)
            {
                AddTranslationErrorDetails(RelationalStrings.InvalidValueInSetProperty(valueSelector.Print()));
                return null;
            }

            return translatedValueSelector;
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

            if (shaper.StructuralType is not IEntityType entityType)
            {
                AddTranslationErrorDetails(
                    RelationalStrings.ExecuteUpdateSubqueryNotSupportedOverComplexTypes(shaper.StructuralType.DisplayName()));
                return null;
            }

            if (entityType.FindPrimaryKey() is not IKey pk)
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

                rewrittenSetters[i] = new(propertyExpression, valueExpression);
            }

            tableExpression = (TableExpression)outerSelectExpression.Tables[0];

            // Re-translate the property selectors to get column expressions pointing to the new outer select expression (the original one
            // has been pushed down into a subquery).
            if (!TranslateSetters(outer, rewrittenSetters, out var translatedSetters, out _))
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

    private static T? ParameterValueExtractor<T>(
        QueryContext context,
        string baseParameterName,
        List<IComplexProperty>? complexPropertyChain,
        IProperty property)
    {
        var baseValue = context.ParameterValues[baseParameterName];

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
        public List<IComplexProperty> ComplexPropertyChain { get; } = new() { firstComplexProperty };
    }
}
