// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal;

public class XGQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
{
    private const string Issue1792SkipWithParameterFlagName = "Microsoft.EntityFrameworkCore.XuGu.Issue1792.SkipWithParameter";

    private static readonly bool _xg8EngineCrashWhenUsingJsonTableWithPrimitiveCollectionInParametersSkip
        = AppContext.TryGetSwitch(Issue1792SkipWithParameterFlagName, out var enabled) && enabled;

    private readonly IXGOptions _options;
    private readonly XGSqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly SqlAliasManager _sqlAliasManager;

    public XGQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        RelationalQueryCompilationContext relationalQueryCompilationContext,
        IXGOptions options)
        : base(dependencies, relationalDependencies, relationalQueryCompilationContext)
    {
        _sqlExpressionFactory = (XGSqlExpressionFactory)relationalDependencies.SqlExpressionFactory;
        _typeMappingSource = relationalDependencies.TypeMappingSource;
        _sqlAliasManager = relationalQueryCompilationContext.SqlAliasManager;
        _options = options;
    }

    protected XGQueryableMethodTranslatingExpressionVisitor(
        XGQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor)
    {
        _sqlExpressionFactory = parentVisitor._sqlExpressionFactory;
        _typeMappingSource = parentVisitor._typeMappingSource;
        _options = parentVisitor._options;
    }

    protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new XGQueryableMethodTranslatingExpressionVisitor(this);

    protected override bool IsNaturallyOrdered(SelectExpression selectExpression)
    {
        return selectExpression is
               {
                   Tables: [var mainTable, ..],
                   Orderings:
                   [
                       {
                           Expression: ColumnExpression { Name: "key", TableAlias: var orderingTable } orderingColumn,
                           IsAscending: true
                       }
                   ]
               }
               && orderingTable == mainTable.Alias
               && IsJsonEachKeyColumn(selectExpression, orderingColumn);

        bool IsJsonEachKeyColumn(SelectExpression selectExpression, ColumnExpression orderingColumn)
            => selectExpression.Tables.FirstOrDefault(t => t.Alias == orderingColumn.TableAlias)?.UnwrapJoin() is TableExpressionBase table
               && (table is XGJsonTableExpression
                   || (table is SelectExpression subquery
                       && subquery.Projection.FirstOrDefault(p => p.Alias == "key")?.Expression is ColumnExpression projectedColumn
                       && IsJsonEachKeyColumn(subquery, projectedColumn)));
    }

    protected override bool IsValidSelectExpressionForExecuteDelete(
        SelectExpression selectExpression,
        StructuralTypeShaperExpression shaper,
        [NotNullWhen(true)] out TableExpression tableExpression)
    {
        if (selectExpression.Offset == null
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Having == null
            && (selectExpression.Tables.Count == 1 || selectExpression.Orderings.Count == 0))
        {
            TableExpressionBase table;
            if (selectExpression.Tables.Count == 1)
            {
                table = selectExpression.Tables[0];
            }
            else
            {
                var projectionBindingExpression = (ProjectionBindingExpression)shaper.ValueBufferExpression;
                var entityProjectionExpression = (StructuralTypeProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
                var column = entityProjectionExpression.BindProperty(shaper.StructuralType.GetProperties().First());
                table = selectExpression.GetTable(column).UnwrapJoin();
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        tableExpression = null;
        return false;
    }

    protected override bool IsValidSelectExpressionForExecuteUpdate(
        SelectExpression selectExpression,
        TableExpressionBase targetTable,
        [NotNullWhen(true)] out TableExpression tableExpression)
    {
        if (selectExpression is
            {
                Offset: null,
                IsDistinct: false,
                GroupBy: [],
                Having: null,
                Orderings: []
            })
        {
            TableExpressionBase table;
            if (selectExpression.Tables.Count == 1)
            {
                table = selectExpression.Tables[0];
            }
            else
            {
                table = targetTable;

                if (selectExpression.Tables.Count > 1 &&
                    table is JoinExpressionBase joinExpressionBase)
                {
                    table = joinExpressionBase.Table;
                }
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        tableExpression = null;
        return false;
    }

    protected override ShapedQueryExpression TranslateAny(ShapedQueryExpression source, LambdaExpression predicate)
    {
        // Simplify x.Array.Any() => JSON_LENGTH(x.Array) > 0 instead of WHERE EXISTS (SELECT 1 FROM JSON_TABLE(x.Array))
        if (predicate is null
            && source.QueryExpression is SelectExpression
            {
                Tables: [TableValuedFunctionExpression { Name: "JSON_TABLE", Schema: null, IsBuiltIn: true, Arguments: [var array] }],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null
            })
        {
            var translation =
                _sqlExpressionFactory.GreaterThan(
                    _sqlExpressionFactory.NullableFunction(
                        "JSON_LENGTH",
                        new[] { array },
                        typeof(int)),
                    _sqlExpressionFactory.Constant(0));

            return source.UpdateQueryExpression(new SelectExpression(translation, _sqlAliasManager));
        }

        return base.TranslateAny(source, predicate);
    }

    protected override ShapedQueryExpression TranslateElementAtOrDefault(
        ShapedQueryExpression source,
        Expression index,
        bool returnDefault)
    {
        if (!returnDefault
            && source.QueryExpression is SelectExpression
            {
                Tables:
                [
                    XGJsonTableExpression
                    {
                        Name: "JSON_TABLE", Schema: null, IsBuiltIn: true, JsonExpression: var jsonArrayColumn
                    } jsonEachExpression
                ],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Orderings: [{ Expression: ColumnExpression { Name: "key" } orderingColumn, IsAscending: true }],
                Limit: null,
                Offset: null
            } selectExpression
            && orderingColumn.TableAlias == jsonEachExpression.Alias
            && TranslateExpression(index) is { } translatedIndex)
        {
            // Index on JSON array

            // Extract the column projected out of the source, and simplify the subquery to a simple JsonScalarExpression
            var shaperExpression = source.ShaperExpression;
            if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
                && unaryExpression.Operand.Type.IsNullableType()
                && unaryExpression.Operand.Type.UnwrapNullableType() == unaryExpression.Type)
            {
                shaperExpression = unaryExpression.Operand;
            }

            if (shaperExpression is ProjectionBindingExpression projectionBindingExpression
                && selectExpression.GetProjection(projectionBindingExpression) is ColumnExpression projectionColumn)
            {
                SqlExpression translation = new JsonScalarExpression(
                    jsonArrayColumn,
                    new[] { new PathSegment(translatedIndex) },
                    projectionColumn.Type,
                    projectionColumn.TypeMapping,
                    projectionColumn.IsNullable);

                // If we have a type mapping (i.e. translating over a column rather than a parameter), apply any necessary server-side
                // conversions.
                if (projectionColumn.TypeMapping is not null)
                {
                    translation = ApplyJsonSqlConversion(
                        translation, _sqlExpressionFactory, projectionColumn.TypeMapping, projectionColumn.IsNullable);
                }

                return source.UpdateQueryExpression(new SelectExpression(translation, _sqlAliasManager));
            }
        }

        return base.TranslateElementAtOrDefault(source, index, returnDefault);
    }

    // TODO: Implement for EF Core 7 JSON support.
    protected override ShapedQueryExpression TransformJsonQueryToTable(JsonQueryExpression jsonQueryExpression)
    {
        return base.TransformJsonQueryToTable(jsonQueryExpression);
    }

    protected override ShapedQueryExpression TranslatePrimitiveCollection(SqlExpression sqlExpression, IProperty property, string tableAlias)
    {
        if (!_options.PrimitiveCollectionsSupport)
        {
            throw new InvalidOperationException(
                CoreStrings.TranslationFailedWithDetails(
                    sqlExpression.Print(),
                    "Primitive collections support has not been enabled."));
        }

        // if (!_options.ServerVersion.Supports.JsonTableImplementationUsesImplicitLateralJoin &&
        //     sqlExpression is ColumnExpression)
        // {
        //     // MariaDB will just return wrong results if the column of an outer table is being referenced.
        //     return null;
        // }

        // Generate the JSON_TABLE() function expression, and wrap it in a SelectExpression.

        // Note that where the elementTypeMapping is known (i.e. collection columns), we immediately generate JSON_TABLE() with a COLUMNS clause
        // (i.e. with a columnInfo), which determines the type conversion to apply to the JSON elements coming out.
        // For parameter collections, the element type mapping will only be inferred and applied later (see
        // XGInferredTypeMappingApplier below), at which point the we'll apply it to add the COLUMNS clause.
        var elementTypeMapping = (RelationalTypeMapping)sqlExpression.TypeMapping?.ElementTypeMapping;

        var jsonTableExpression = new XGJsonTableExpression(
            tableAlias,
            sqlExpression,
            new[] { new PathSegment(_sqlExpressionFactory.Constant("*", RelationalTypeMapping.NullMapping)) },
            elementTypeMapping is not null
                ? new[]
                {
                    new XGJsonTableExpression.ColumnInfo
                    {
                        Name = "value",
                        TypeMapping = elementTypeMapping,
                        Path = new[] { new PathSegment(_sqlExpressionFactory.Constant(0, _typeMappingSource.FindMapping(typeof(int)))) },
                    }
                }
                : null);


        // Using primitive collections in parameters that are used as the JSON source argument for JSON_TABLE(source, ...) can crash
        // MySQL 8 somewhere later down the line. We mitigate this by inlining those parameters.
        // There are however other scenarios that can still crash MySQL 8 (e.g. `NorthwindSelectQueryXGTest.Correlated_collection_after_distinct_not_containing_original_identifier`).
        // For those cases, we implement a flag to skip skip the JSON_TABLE generation.
        if (elementTypeMapping is null &&
            !_options.ServerVersion.Supports.JsonTableImplementationUsingParameterAsSourceWithoutEngineCrash &&
            _xg8EngineCrashWhenUsingJsonTableWithPrimitiveCollectionInParametersSkip)
        {
            AddTranslationErrorDetails($"JSON_TABLE() has been disabled by the '{Issue1792SkipWithParameterFlagName}' AppContext switch, because it can crash MySQL 8.");
            return null;
        }

        var elementClrType = sqlExpression.Type.GetSequenceType();

        // If this is a collection property, get the element's nullability out of metadata. Otherwise, this is a parameter property, in
        // which case we only have the CLR type (note that we cannot produce different SQLs based on the nullability of an *element* in
        // a parameter collection - our caching mechanism only supports varying by the nullability of the parameter itself (i.e. the
        // collection).
        var isElementNullable = property?.GetElementType()!.IsNullable;

        var keyColumnTypeMapping = _typeMappingSource.FindMapping(typeof(int))!;

#pragma warning disable EF1001 // Internal EF Core API usage.
        var selectExpression = new SelectExpression(
            [jsonTableExpression],
            new ColumnExpression(
                "value",
                tableAlias,
                elementClrType.UnwrapNullableType(),
                elementTypeMapping,
                isElementNullable ?? elementClrType.IsNullableType()),
            identifier: [(new ColumnExpression("key", tableAlias, typeof(int), keyColumnTypeMapping, nullable: false), keyColumnTypeMapping.Comparer)],
            _sqlAliasManager);
#pragma warning restore EF1001 // Internal EF Core API usage.

        // JSON_TABLE() doesn't guarantee the ordering of the elements coming out; when using JSON_TABLE() without COLUMNS, a [key] column is returned
        // with the JSON array's ordering, which we can ORDER BY; this option doesn't exist with JSON_TABLE() with COLUMNS, unfortunately.
        // However, JSON_TABLE() with COLUMNS has better performance, and also applies JSON-specific conversions we cannot be done otherwise
        // (e.g. JSON_TABLE() with COLUMNS does base64 decoding for VARBINARY).
        // Here we generate JSON_TABLE() with COLUMNS, but also add an ordering by [key] - this is a temporary invalid representation.
        // In XGQueryTranslationPostprocessor, we'll post-process the expression; if the ORDER BY was stripped (e.g. because of
        // IN, EXISTS or a set operation), we'll just leave the JSON_TABLE() with COLUMNS. If not, we'll convert the JSON_TABLE() with COLUMNS to an
        // JSON_TABLE() without COLUMNS.
        // Note that the JSON_TABLE() 'key' column is an nvarchar - we convert it to an int before sorting.
        selectExpression.AppendOrdering(
            new OrderingExpression(
                selectExpression.CreateColumnExpression(
                    jsonTableExpression,
                    "key",
                    typeof(uint),
                    typeMapping: _typeMappingSource.FindMapping(typeof(uint)),
                    columnNullable: false),
                ascending: true));

        var shaperExpression = (Expression)new ProjectionBindingExpression(selectExpression, new ProjectionMember(), elementClrType.MakeNullable());
        if (shaperExpression.Type != elementClrType)
        {
            Check.DebugAssert(
                elementClrType.MakeNullable() == shaperExpression.Type,
                "expression.Type must be nullable of targetType");

            shaperExpression = Expression.Convert(shaperExpression, elementClrType);
        }

        return new ShapedQueryExpression(selectExpression, shaperExpression);
    }

    /// <summary>
    /// Wraps the given expression with any SQL logic necessary to convert a value coming out of a JSON document into the relational value
    /// represented by the given type mapping.
    /// </summary>
    private static SqlExpression ApplyJsonSqlConversion(
        SqlExpression expression,
        ISqlExpressionFactory sqlExpressionFactory,
        RelationalTypeMapping typeMapping,
        bool isNullable)
        => typeMapping switch
        {
            // TODO: EF Core 8 - Decide between UNHEX() and FROM_BASE64().
            ByteArrayTypeMapping => sqlExpressionFactory.Function("FROM_BASE64", new[] { expression }, isNullable, new[] { true }, typeof(byte[]), typeMapping),
            _ => expression
        };

    private sealed class FakeMemberInfo : MemberInfo
    {
        public FakeMemberInfo(string name)
            => Name = name;

        public override string Name { get; }

        public override object[] GetCustomAttributes(bool inherit)
            => throw new NotSupportedException();
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            => throw new NotSupportedException();
        public override bool IsDefined(Type attributeType, bool inherit)
            => throw new NotSupportedException();
        public override Type DeclaringType => throw new NotSupportedException();
        public override MemberTypes MemberType => throw new NotSupportedException();
        public override Type ReflectedType => throw new NotSupportedException();
    }
}
