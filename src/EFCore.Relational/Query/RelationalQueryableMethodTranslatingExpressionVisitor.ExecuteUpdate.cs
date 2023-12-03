// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

public partial class RelationalQueryableMethodTranslatingExpressionVisitor
{
    /// <summary>
    ///     Translates
    ///     <see
    ///         cref="RelationalQueryableExtensions.ExecuteUpdate{TSource}(IQueryable{TSource}, Expression{Func{SetPropertyCalls{TSource}, SetPropertyCalls{TSource}}})" />
    ///     method
    ///     over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="setPropertyCalls">
    ///     The lambda expression containing
    ///     <see
    ///         cref="SetPropertyCalls{TSource}.SetProperty{TProperty}(Func{TSource, TProperty}, Func{TSource, TProperty})" />
    ///     statements.
    /// </param>
    /// <returns>The non query after translation.</returns>
    protected virtual NonQueryExpression? TranslateExecuteUpdate(ShapedQueryExpression source, LambdaExpression setPropertyCalls)
    {
        // Our source may have IncludeExpressions because of owned entities or auto-include; unwrap these, as they're meaningless for
        // ExecuteUpdate's lambdas. Note that we don't currently support updates across tables.
        if (source.ShaperExpression is IncludeExpression includeExpression)
        {
            source = source.UpdateShaperExpression(PruneIncludes(includeExpression));
        }

        var propertyValueLambdaExpressions = new List<(LambdaExpression PropertySelector, Expression ValueExpression)>();
        PopulateSetPropertyCalls(setPropertyCalls.Body, propertyValueLambdaExpressions, setPropertyCalls.Parameters[0]);
        if (TranslationErrorDetails != null)
        {
            return null;
        }

        if (propertyValueLambdaExpressions.Count == 0)
        {
            AddTranslationErrorDetails(RelationalStrings.NoSetPropertyInvocation);
            return null;
        }

        // Go over the SetProperty calls, and translate the property selectors (left lambda).
        // The property selectors should get translated to ColumnExpressions (otherwise they' invalid - columns are what we need to update).
        // All columns must also refer to the same table (since that's how SQL UPDATE works), extract that target table from the translated
        // columns and validate that only one table is being referenced.
        // Note that we don't translate the value expressions in this pass, since if the query is complicated, we may need to do a pushdown
        // (see PushdownWithPkInnerJoinPredicate below); so we defer translation until we have the final source/select. For the property
        // selectors we need to translate now since we need the table.
        TableExpressionBase? targetTable = null;
        Expression? targetTablePropertySelector = null;
        var columns = new ColumnExpression[propertyValueLambdaExpressions.Count];
        for (var i = 0; i < propertyValueLambdaExpressions.Count; i++)
        {
            var (propertySelector, _) = propertyValueLambdaExpressions[i];
            var propertySelectorBody = RemapLambdaBody(source, propertySelector).UnwrapTypeConversion(out _);
            if (_sqlTranslator.Translate(propertySelectorBody) is not ColumnExpression column)
            {
                AddTranslationErrorDetails(RelationalStrings.InvalidPropertyInSetProperty(propertySelector.Print()));
                return null;
            }

            if (targetTable is null)
            {
                targetTable = column.Table;
                targetTablePropertySelector = propertySelector;
            }
            else if (!ReferenceEquals(column.Table, targetTable))
            {
                AddTranslationErrorDetails(
                    RelationalStrings.MultipleTablesInExecuteUpdate(propertySelector.Print(), targetTablePropertySelector!.Print()));
                return null;
            }

            columns[i] = column;
        }

        Check.DebugAssert(targetTable is not null, "Target table should have a value");

        if (targetTable is TpcTablesExpression tpcTablesExpression)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnTPC(
                    nameof(RelationalQueryableExtensions.ExecuteUpdate), tpcTablesExpression.EntityType.DisplayName()));
            return null;
        }

        // First, check if the provider has a native translation for the update represented by the select expression.
        // The default relational implementation handles simple, universally-supported cases (i.e. no operators except for predicate).
        // Providers may override IsValidSelectExpressionForExecuteUpdate to add support for more cases via provider-specific UPDATE syntax.
        var selectExpression = (SelectExpression)source.QueryExpression;
        return IsValidSelectExpressionForExecuteUpdate(selectExpression, targetTable, out var tableExpression)
            ? TranslateValueExpressions(this, source, selectExpression, tableExpression, propertyValueLambdaExpressions, columns)
            : PushdownWithPkInnerJoinPredicate();

        void PopulateSetPropertyCalls(
            Expression expression,
            List<(LambdaExpression, Expression)> list,
            ParameterExpression parameter)
        {
            switch (expression)
            {
                case ParameterExpression p
                    when parameter == p:
                    break;

                case MethodCallExpression
                    {
                        Method:
                        {
                            IsGenericMethod: true,
                            Name: nameof(SetPropertyCalls<int>.SetProperty),
                            DeclaringType.IsGenericType: true
                        }
                    } methodCallExpression
                    when methodCallExpression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(SetPropertyCalls<>):
                    list.Add(((LambdaExpression)methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]));

                    PopulateSetPropertyCalls(methodCallExpression.Object!, list, parameter);

                    break;

                default:
                    AddTranslationErrorDetails(RelationalStrings.InvalidArgumentToExecuteUpdate);
                    break;
            }
        }

        static NonQueryExpression? TranslateValueExpressions(
            RelationalQueryableMethodTranslatingExpressionVisitor visitor,
            ShapedQueryExpression source,
            SelectExpression selectExpression,
            TableExpression tableExpression,
            List<(LambdaExpression PropertySelector, Expression ValueExpression)> propertyValueLambdaExpression,
            ColumnExpression[] columns)
        {
            var setters = new ColumnValueSetter[columns.Length];

            for (var i = 0; i < propertyValueLambdaExpression.Count; i++)
            {
                var column = columns[i];
                var (_, valueSelector) = propertyValueLambdaExpression[i];

                var remappedValueSelector = valueSelector is LambdaExpression lambdaExpression
                    ? visitor.RemapLambdaBody(source, lambdaExpression)
                    : valueSelector;

                if (remappedValueSelector.Type != column.Type)
                {
                    remappedValueSelector = Expression.Convert(remappedValueSelector, column.Type);
                }

                if (visitor.TranslateExpression(remappedValueSelector, applyDefaultTypeMapping: false)
                    is not SqlExpression translatedValueSelector)
                {
                    visitor.AddTranslationErrorDetails(RelationalStrings.InvalidValueInSetProperty(valueSelector.Print()));
                    return null;
                }

                // Apply the type mapping of the column (translated from the property selector above) to the value,
                // and apply alias uniquification to it.
                translatedValueSelector = visitor._sqlExpressionFactory.ApplyTypeMapping(translatedValueSelector, column.TypeMapping);
                translatedValueSelector = selectExpression.AssignUniqueAliases(translatedValueSelector);

                setters[i] = new ColumnValueSetter(column, translatedValueSelector);
            }

            selectExpression.ReplaceProjection(new List<Expression>());
            selectExpression.ApplyProjection();

            return new NonQueryExpression(new UpdateExpression(tableExpression, selectExpression, setters));
        }

        NonQueryExpression? PushdownWithPkInnerJoinPredicate()
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

            var firstPropertySelector = propertyValueLambdaExpressions[0].PropertySelector;
            var shaper = RemapLambdaBody(source, firstPropertySelector).UnwrapTypeConversion(out _) switch
            {
                MemberExpression { Expression : not null } memberExpression
                    when memberExpression.Expression.UnwrapTypeConversion(out _) is StructuralTypeShaperExpression s
                    => s,

                MethodCallExpression mce when mce.TryGetEFPropertyArguments(out var source, out _)
                    && source.UnwrapTypeConversion(out _) is StructuralTypeShaperExpression s
                    => s,

                MethodCallExpression mce when mce.TryGetIndexerArguments(RelationalDependencies.Model, out var source2, out _)
                    && source2.UnwrapTypeConversion(out _) is StructuralTypeShaperExpression s
                    => s,

                _ => null
            };

            if (shaper is null)
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
                        nameof(RelationalQueryableExtensions.ExecuteUpdate),
                        entityType.DisplayName()));
                return null;
            }

            // Generate the INNER JOIN around the original query, on the PK properties.
            var outer = (ShapedQueryExpression)Visit(new EntityQueryRootExpression(entityType));
            var inner = source;
            var outerParameter = Expression.Parameter(entityType.ClrType);
            var outerKeySelector = Expression.Lambda(outerParameter.CreateKeyValuesExpression(pk.Properties), outerParameter);
            var firstPropertyLambdaExpression = propertyValueLambdaExpressions[0].Item1;
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
            for (var i = 0; i < propertyValueLambdaExpressions.Count; i++)
            {
                var (propertyExpression, valueExpression) = propertyValueLambdaExpressions[i];
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

                propertyValueLambdaExpressions[i] = (propertyExpression, valueExpression);
            }

            tableExpression = (TableExpression)outerSelectExpression.Tables[0];

            // Re-translate the property selectors to get column expressions pointing to the new outer select expression (the original one
            // has been pushed down into a subquery).
            for (var i = 0; i < propertyValueLambdaExpressions.Count; i++)
            {
                var (propertySelector, _) = propertyValueLambdaExpressions[i];
                var propertySelectorBody = RemapLambdaBody(outer, propertySelector).UnwrapTypeConversion(out _);

                if (TranslateExpression(propertySelectorBody) is not ColumnExpression column)
                {
                    AddTranslationErrorDetails(RelationalStrings.InvalidPropertyInSetProperty(propertySelector.Print()));
                    return null;
                }

                columns[i] = column;
            }

            return TranslateValueExpressions(this, outer, outerSelectExpression, tableExpression, propertyValueLambdaExpressions, columns);
        }

        static Expression GetEntitySource(IModel model, Expression propertyAccessExpression)
        {
            propertyAccessExpression = propertyAccessExpression.UnwrapTypeConversion(out _);
            if (propertyAccessExpression is MethodCallExpression mce)
            {
                if (mce.TryGetEFPropertyArguments(out var source, out _))
                {
                    return source;
                }

                if (mce.TryGetIndexerArguments(model, out var source2, out _))
                {
                    return source2;
                }
            }

            return ((MemberExpression)propertyAccessExpression).Expression!;
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
}
