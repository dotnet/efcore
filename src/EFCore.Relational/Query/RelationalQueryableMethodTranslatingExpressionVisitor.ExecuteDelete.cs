// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

public partial class RelationalQueryableMethodTranslatingExpressionVisitor
{
    /// <summary>
    ///     Translates <see cref="RelationalQueryableExtensions.ExecuteDelete{TSource}(IQueryable{TSource})" /> method
    ///     over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <returns>The non query after translation.</returns>
    protected virtual NonQueryExpression? TranslateExecuteDelete(ShapedQueryExpression source)
    {
        source = source.UpdateShaperExpression(new IncludePruner().Visit(source.ShaperExpression));

        if (source.ShaperExpression is not StructuralTypeShaperExpression { StructuralType: IEntityType entityType } shaper)
        {
            AddTranslationErrorDetails(RelationalStrings.ExecuteDeleteOnNonEntityType);
            return null;
        }

        var mappingStrategy = entityType.GetMappingStrategy();
        if (mappingStrategy == RelationalAnnotationNames.TptMappingStrategy)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnTPT(nameof(RelationalQueryableExtensions.ExecuteDelete), entityType.DisplayName()));
            return null;
        }

        if (mappingStrategy == RelationalAnnotationNames.TpcMappingStrategy
            && entityType.GetDirectlyDerivedTypes().Any())
        {
            // We allow TPC is it is leaf type
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnTPC(nameof(RelationalQueryableExtensions.ExecuteDelete), entityType.DisplayName()));
            return null;
        }

        if (entityType.GetViewOrTableMappings().Count() != 1)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnEntitySplitting(
                    nameof(RelationalQueryableExtensions.ExecuteDelete), entityType.DisplayName()));
            return null;
        }

        // First, check if the provider has a native translation for the delete represented by the select expression.
        // The default relational implementation handles simple, universally-supported cases (i.e. no operators except for predicate).
        // Providers may override IsValidSelectExpressionForExecuteDelete to add support for more cases via provider-specific DELETE syntax.
        var selectExpression = (SelectExpression)source.QueryExpression;
        if (IsValidSelectExpressionForExecuteDelete(selectExpression, shaper, out var tableExpression))
        {
            if (AreOtherNonOwnedEntityTypesInTheTable(entityType.GetRootType(), tableExpression.Table))
            {
                AddTranslationErrorDetails(
                    RelationalStrings.ExecuteDeleteOnTableSplitting(tableExpression.Table.SchemaQualifiedName));

                return null;
            }

            selectExpression.ReplaceProjection(new List<Expression>());
            selectExpression.ApplyProjection();

            return new NonQueryExpression(new DeleteExpression(tableExpression, selectExpression));

            static bool AreOtherNonOwnedEntityTypesInTheTable(IEntityType rootType, ITableBase table)
            {
                foreach (var entityTypeMapping in table.EntityTypeMappings)
                {
                    var typeBase = entityTypeMapping.TypeBase;
                    if ((entityTypeMapping.IsSharedTablePrincipal == true
                            && typeBase != rootType)
                        || (entityTypeMapping.IsSharedTablePrincipal == false
                            && typeBase is IEntityType entityType
                            && entityType.GetRootType() != rootType
                            && !entityType.IsOwned()))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // The provider doesn't natively support the delete.
        // As a fallback, we place the original query in a Contains subquery, which will get translated via the regular entity equality/
        // containment mechanism (InExpression for non-composite keys, Any for composite keys)
        var pk = entityType.FindPrimaryKey();
        if (pk == null)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnKeylessEntityTypeWithUnsupportedOperator(
                    nameof(RelationalQueryableExtensions.ExecuteDelete),
                    entityType.DisplayName()));
            return null;
        }

        var clrType = entityType.ClrType;
        var entityParameter = Expression.Parameter(clrType);
        var predicateBody = Expression.Call(QueryableMethods.Contains.MakeGenericMethod(clrType), source, entityParameter);

        var newSource = Expression.Call(
            QueryableMethods.Where.MakeGenericMethod(clrType),
            new EntityQueryRootExpression(entityType),
            Expression.Quote(Expression.Lambda(predicateBody, entityParameter)));

        return TranslateExecuteDelete((ShapedQueryExpression)Visit(newSource));
    }

    /// <summary>
    ///     Checks weather the current select expression can be used as-is for executing a delete operation, or whether it must be pushed
    ///     down into a subquery.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default, only single-table select expressions are supported, and optionally with a predicate.
    ///     </para>
    ///     <para>
    ///         Providers can override this to allow more select expression features to be supported without pushing down into a subquery.
    ///         When doing this, VisitDelete must also be overridden in the provider's QuerySqlGenerator to add SQL generation support for
    ///         the feature.
    ///     </para>
    /// </remarks>
    /// <param name="selectExpression">The select expression to validate.</param>
    /// <param name="shaper">The structural type shaper expression on which the delete operation is being applied.</param>
    /// <param name="tableExpression">The table expression from which rows are being deleted.</param>
    /// <returns>
    ///     Returns <see langword="true" /> if the current select expression can be used for delete as-is, <see langword="false" /> otherwise.
    /// </returns>
    protected virtual bool IsValidSelectExpressionForExecuteDelete(
        SelectExpression selectExpression,
        StructuralTypeShaperExpression shaper,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (selectExpression is
            {
                Tables: [TableExpression expression],
                Orderings: [],
                Offset: null,
                Limit: null,
                GroupBy: [],
                Having: null
            })
        {
            tableExpression = expression;

            return true;
        }

        tableExpression = null;
        return false;
    }
}
