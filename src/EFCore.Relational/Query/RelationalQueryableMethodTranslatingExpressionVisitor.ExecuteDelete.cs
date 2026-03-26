// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

public partial class RelationalQueryableMethodTranslatingExpressionVisitor
{
    /// <inheritdoc />
    protected override DeleteExpression? TranslateExecuteDelete(ShapedQueryExpression source)
    {
        source = source.UpdateShaperExpression(new IncludePruner().Visit(source.ShaperExpression));

        if (source.ShaperExpression is not StructuralTypeShaperExpression { StructuralType: IEntityType entityType } shaper)
        {
            AddTranslationErrorDetails(RelationalStrings.ExecuteDeleteOnNonEntityType);
            return null;
        }

        if (entityType.IsMappedToJson())
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnOwnedJsonIsNotSupported("ExecuteDelete", entityType.DisplayName()));
            return null;
        }

        switch (entityType.GetMappingStrategy())
        {
            case RelationalAnnotationNames.TptMappingStrategy:
                AddTranslationErrorDetails(
                    RelationalStrings.ExecuteOperationOnTPT(
                        nameof(EntityFrameworkQueryableExtensions.ExecuteDelete), entityType.DisplayName()));
                return null;

            // Note that we do allow TPC if the target is a leaf type
            case RelationalAnnotationNames.TpcMappingStrategy when entityType.GetDirectlyDerivedTypes().Any():
                AddTranslationErrorDetails(
                    RelationalStrings.ExecuteOperationOnTPC(
                        nameof(EntityFrameworkQueryableExtensions.ExecuteDelete), entityType.DisplayName()));
                return null;
        }

        // Find the table model that maps to the entity type; there must be exactly one (e.g. no entity splitting).
        ITable targetTable;
        switch (entityType.GetTableMappings().ToList())
        {
            case []:
                throw new InvalidOperationException(
                    RelationalStrings.ExecuteUpdateDeleteOnEntityNotMappedToTable(entityType.DisplayName()));

            case [var singleTableMapping]:
                targetTable = singleTableMapping.Table;
                break;

            default:
                AddTranslationErrorDetails(
                    RelationalStrings.ExecuteOperationOnEntitySplitting(
                        nameof(EntityFrameworkQueryableExtensions.ExecuteDelete), entityType.DisplayName()));
                return null;
        }

        var selectExpression = (SelectExpression)source.QueryExpression;

        // Find the table expression in the SelectExpression that corresponds to the projected entity type.
        var projectionBindingExpression = (ProjectionBindingExpression)shaper.ValueBufferExpression;
        var projection = (StructuralTypeProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
        var column = projection.BindProperty(shaper.StructuralType.GetProperties().First());
        var tableExpression = selectExpression.GetTable(column, out var tableIndex);

        // If the projected table expression (the thing to be deleted) isn't a TableExpression (e.g. it's a set operation), we can't
        // translate to a simple DELETE (which requires a simple target table), and must fall back to rewriting as a subquery.
        if (tableExpression.UnwrapJoin() is TableExpression unwrappedTableExpression)
        {
            // In normal cases, the table expression will be refer to the same table model we found above for the entity type.
            if (unwrappedTableExpression.Table is ITable)
            {
                Check.DebugAssert(
                    unwrappedTableExpression.Table == targetTable,
                    "Projected table is a table, but not the same one mapped to the entity type");
            }
            else
            {
                // If the entity is also mapped to a view, the SelectExpression will refer to the view instead, since translation happens
                // with the assumption that we're querying, not deleting.
                // For this case, we must replace the TableExpression in the SelectExpression - referring to the view - with the one that
                // refers to the mutable table.
                Check.DebugAssert(
                    unwrappedTableExpression.Table.EntityTypeMappings.Any(etm => etm.TypeBase == entityType),
                    "Projected table is not mapped to the entity type projected by the shaper");

                unwrappedTableExpression = new TableExpression(unwrappedTableExpression.Alias, targetTable);
                tableExpression = tableExpression is JoinExpressionBase join
                    ? join.Update(unwrappedTableExpression)
                    : unwrappedTableExpression;
                var newTables = selectExpression.Tables.ToList();
                newTables[tableIndex] = tableExpression;

                // Note that we need to keep the select mutable, because if IsValidSelectExpressionForExecuteDelete below returns false,
                // we need to compose on top of it.
                selectExpression.SetTables(newTables);
            }

            // Finally, check if the provider has a native translation for the delete represented by the select expression.
            // The default relational implementation handles simple, universally-supported cases (i.e. no operators except for predicate).
            // Providers may override IsValidSelectExpressionForExecuteDelete to add support for more cases via provider-specific DELETE syntax.
            if (IsValidSelectExpressionForExecuteDelete(selectExpression))
            {
                if (AreOtherNonOwnedEntityTypesInTheTable(entityType.GetRootType(), targetTable))
                {
                    AddTranslationErrorDetails(
                        RelationalStrings.ExecuteDeleteOnTableSplitting(unwrappedTableExpression.Table.SchemaQualifiedName));

                    return null;
                }

                selectExpression.ReplaceProjection(new List<Expression>());
                selectExpression.ApplyProjection();

                return new DeleteExpression(unwrappedTableExpression, selectExpression);
            }
        }

        // We can't translate to a simple delete (e.g. the provider doesn't support one of the clauses).
        // As a fallback, we place the original query in a Contains subquery, which will get translated via the regular entity equality/
        // containment mechanism (InExpression for non-composite keys, Any for composite keys)
        var pk = entityType.FindPrimaryKey();
        if (pk == null)
        {
            AddTranslationErrorDetails(
                RelationalStrings.ExecuteOperationOnKeylessEntityTypeWithUnsupportedOperator(
                    nameof(EntityFrameworkQueryableExtensions.ExecuteDelete),
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
    /// <returns>
    ///     Returns <see langword="true" /> if the current select expression can be used for delete as-is, <see langword="false" /> otherwise.
    /// </returns>
    protected virtual bool IsValidSelectExpressionForExecuteDelete(SelectExpression selectExpression)
        => selectExpression is
        {
            Tables: [TableExpression],
            Orderings: [],
            Offset: null,
            Limit: null,
            GroupBy: [],
            Having: null
        };

    /// <summary>
    ///     This method has been obsoleted, use the method accepting a single SelectExpression parameter instead.
    /// </summary>
    [Obsolete("This method has been obsoleted, use the method accepting a single SelectExpression parameter instead.", error: true)]
    protected virtual bool IsValidSelectExpressionForExecuteDelete(
        SelectExpression selectExpression,
        StructuralTypeShaperExpression shaper,
        [NotNullWhen(true)] out TableExpression? tableExpression)
        => throw new UnreachableException();
}
