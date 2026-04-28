// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Caching.Memory;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Query;

#pragma warning disable EF1001 // Internal EF Core API usage

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     Builds non-generated entity materializers from shaper expressions. This replaces
///     <see cref="RelationalShapedQueryCompilingExpressionVisitor.ShaperProcessingExpressionVisitor" />
///     for supported query shapes, producing a tree of <see cref="RelationalEntityMaterializer{TEntity}" />
///     instances wired together with include information.
/// </remarks>
public class RelationalMaterializerFactory(
    RelationalQueryCompilationContext queryCompilationContext,
    IMemoryCache memoryCache,
    IQuerySqlGeneratorFactory querySqlGeneratorFactory,
    IRelationalParameterBasedSqlProcessorFactory relationalParameterBasedSqlProcessorFactory,
    bool detailedErrorsEnabled,
    bool threadSafetyChecksEnabled)
{
    private readonly Type _contextType = queryCompilationContext.ContextType;
    private readonly bool _useRelationalNulls
        = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).UseRelationalNulls;
    private readonly ParameterTranslationMode _collectionParameterTranslationMode
        = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).ParameterizedCollectionMode;

    private int _nextCollectionId;

    /// <summary>
    ///     Builds a non-generated query executor for an enumerable query where <typeparamref name="TElement" />
    ///     is the element type directly. This eliminates the need for <c>MakeGenericMethod</c>, making the
    ///     entire path NativeAOT-compatible.
    /// </summary>
    public Func<QueryContext, IEnumerable<TElement>> CreateEnumerableMaterializer<TElement>(ShapedQueryExpression shapedQueryExpression)
    {
        _nextCollectionId = 0;

        var (select, shaper) = ((SelectExpression)shapedQueryExpression.QueryExpression, shapedQueryExpression.ShaperExpression);

        var querySplittingBehavior = queryCompilationContext.QuerySplittingBehavior;
        if (querySplittingBehavior == QuerySplittingBehavior.SplitQuery)
        {
            throw new NotImplementedException("The non-generated materializer does not yet support split queries.");
        }

        if (select.IsNonComposedFromSql())
        {
            throw new NotImplementedException("The non-generated materializer does not yet support FromSql queries.");
        }

        var isTracking = queryCompilationContext.QueryTrackingBehavior is QueryTrackingBehavior.TrackAll;

        var relationalCommandCache = new RelationalCommandCache(
            memoryCache,
            querySqlGeneratorFactory,
            relationalParameterBasedSqlProcessorFactory,
            select,
            _useRelationalNulls,
            _collectionParameterTranslationMode);

        if (shaper is UnaryExpression { NodeType: ExpressionType.Convert } convert)
        {
            shaper = convert.Operand;
        }

        var rowMaterializer = BuildMaterializer<TElement>(shaper, select, isTracking);

        return qc => new SingleQueryingEnumerable<TElement>(
            (RelationalQueryContext)qc,
            relationalCommandResolver: parameters => relationalCommandCache.GetRelationalCommandTemplate(parameters),
            readerColumns: null,
            materializer: rowMaterializer!,
            contextType: _contextType,
            standAloneStateManager: queryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution,
            detailedErrorsEnabled: detailedErrorsEnabled,
            threadSafetyChecksEnabled: threadSafetyChecksEnabled);
    }

    /// <summary>
    ///     Builds a materializer delegate for any shaper expression. When <typeparamref name="T" /> is the actual
    ///     CLR type (called from <see cref="CreateEnumerableMaterializer{TElement}" />), typed reader methods are
    ///     used to avoid boxing. When <typeparamref name="T" /> is <c>object</c> (called recursively for
    ///     <see cref="NewExpression" /> constructor arguments), boxing is unavoidable for
    ///     <see cref="ConstructorInvoker" />, but the typed reader still applies the type mapping and converter.
    /// </summary>
    private Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T?> BuildMaterializer<T>(
        Expression shaperExpression,
        SelectExpression select,
        bool isTracking)
    {
        if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } convert)
        {
            shaperExpression = convert.Operand;
        }

        switch (shaperExpression)
        {
            case ProjectionBindingExpression scalarProjection:
            {
                var projectionIndex = select.GetProjection(scalarProjection).GetConstantValue<object>();
                if (projectionIndex is not int columnIndex)
                {
                    throw new NotImplementedException(
                        $"The non-generated materializer does not support projection index type '{projectionIndex?.GetType().Name}'.");
                }

                var nullable = select.Projection[columnIndex].Expression is not ColumnExpression col || col.IsNullable;
                var typeMapping = select.Projection[columnIndex].Expression.TypeMapping!;
                var typedReader = typeMapping.CreateReader(columnIndex);

                if (typeof(T) != typeof(object))
                {
                    // Top-level scalar projection (Select(b => b.Age)): T is known statically, so Read<T> dispatches
                    // to the correct typed reader method (GetInt32, GetString, etc.) with zero boxing.
                    return nullable
                        ? (qc, reader, rc, coord) => reader.IsDBNull(columnIndex) ? default : typedReader.Read<T>(reader)
                        : (qc, reader, rc, coord) => typedReader.Read<T>(reader);
                }

                // Boxed path: scalar within e.g. an anonymous type projection (Select(b => new { b.Age })).
                // T is object, so boxing is unavoidable, but the typed reader still applies the type mapping
                // and any value converter correctly.
                return nullable
                    ? (qc, reader, rc, coord) => reader.IsDBNull(columnIndex) ? default : (T?)typedReader.Read<object>(reader)
                    : (qc, reader, rc, coord) => (T?)typedReader.Read<object>(reader);
            }

            case NewExpression newExpression:
            {
                var invoker = ConstructorInvoker.Create(newExpression.Constructor!);
                var m = newExpression.Arguments
                    .Select(arg => BuildMaterializer<object>(arg, select, isTracking))
                    .ToArray();

                // Use individual-arg overloads (0–4 args) to avoid array allocation on each row.
                return m.Length switch
                {
                    0 => (queryCtx, reader, rc, coord)
                        => (T?)invoker.Invoke(),
                    1 => (queryCtx, reader, rc, coord)
                        => (T?)invoker.Invoke(m[0](queryCtx, reader, rc, coord)),
                    2 => (queryCtx, reader, rc, coord)
                        => (T?)invoker.Invoke(m[0](queryCtx, reader, rc, coord), m[1](queryCtx, reader, rc, coord)),
                    3 => (queryCtx, reader, rc, coord)
                        => (T?)invoker.Invoke(
                            m[0](queryCtx, reader, rc, coord),
                            m[1](queryCtx, reader, rc, coord),
                            m[2](queryCtx, reader, rc, coord)),
                    4 => (queryCtx, reader, rc, coord)
                        => (T?)invoker.Invoke(
                            m[0](queryCtx, reader, rc, coord),
                            m[1](queryCtx, reader, rc, coord),
                            m[2](queryCtx, reader, rc, coord),
                            m[3](queryCtx, reader, rc, coord)),
                    _ => (queryCtx, reader, rc, coord) =>
                    {
                        var args = new object?[m.Length];
                        for (var i = 0; i < args.Length; i++)
                        {
                            args[i] = m[i](queryCtx, reader, rc, coord);
                        }

                        return (T?)invoker.Invoke(args.AsSpan());
                    }
                };
            }

            case RelationalStructuralTypeShaperExpression:
            case IncludeExpression:
            {
                var entityMaterializer = TryBuildMaterializerFromShaper(shaperExpression, select, isTracking)
                    ?? throw new NotImplementedException(
                        $"The non-generated materializer does not yet support shaper expression type '{shaperExpression.GetType().Name}'.");

                if (typeof(T) != typeof(object))
                {
                    // Typed path: use GetTypedMaterializeDelegate to avoid boxing.
                    return entityMaterializer.GetTypedMaterializeDelegate<T>()!;
                }

                // Boxed path: use Materialize() returning object?.
                return (queryCtx, reader, rc, coord) => (T?)entityMaterializer.Materialize(queryCtx, reader, rc, coord);
            }

            default:
                throw new UnreachableException($"Unexpected shaper expression type '{shaperExpression.GetType().Name}'.");
        }
    }

    /// <summary>
    ///     Recursively builds a <see cref="RelationalEntityMaterializer{TEntity}" /> from a shaper expression.
    /// </summary>
    private RelationalEntityMaterializer? TryBuildMaterializerFromShaper(
        Expression shaperExpression,
        SelectExpression selectExpression,
        bool isTracking)
    {
        switch (shaperExpression)
        {
            case RelationalStructuralTypeShaperExpression
            {
                StructuralType: IEntityType entityType,
                ValueBufferExpression: ProjectionBindingExpression projectionBinding
            } shaper:
            {
                return TryBuildEntityMaterializer(
                    entityType, projectionBinding, selectExpression, isTracking, shaper.IsNullable);
            }

            case IncludeExpression includeExpression:
            {
                // Recurse into the entity expression to build the inner materializer (which may have further includes)
                var innerMaterializer = TryBuildMaterializerFromShaper(
                    includeExpression.EntityExpression, selectExpression, isTracking);
                if (innerMaterializer is null)
                {
                    return null;
                }

                var navigation = includeExpression.Navigation;
                var inverseNavigation = navigation.Inverse;
                IClrPropertySetter? inverseNavSetter = null;
                if (inverseNavigation is { IsCollection: false })
                {
                    inverseNavSetter = ((IRuntimePropertyBase)inverseNavigation).GetSetter();
                }

                switch (includeExpression.NavigationExpression)
                {
                    // Reference include (many-to-one / one-to-one), with or without further nested includes.
                    // When NavigationExpression is a plain RelationalStructuralTypeShaperExpression the entity has
                    // no further includes; when it is itself an IncludeExpression the nested include tree is built
                    // recursively by the TryBuildMaterializerFromShaper call below.
                    case RelationalStructuralTypeShaperExpression
                    {
                        StructuralType: IEntityType,
                        ValueBufferExpression: ProjectionBindingExpression
                    }:
                    case IncludeExpression:
                    {
                        var includedMaterializer = TryBuildMaterializerFromShaper(
                            includeExpression.NavigationExpression, selectExpression, isTracking);
                        if (includedMaterializer is null)
                        {
                            return null;
                        }

                        var navSetter = ((IRuntimePropertyBase)navigation).GetSetter();

                        innerMaterializer.AddReferenceInclude(new ReferenceIncludeInfo(
                            includedMaterializer,
                            navigation,
                            navSetter,
                            inverseNavigation,
                            inverseNavSetter,
                            isKeylessEntityType: navigation.DeclaringEntityType.FindPrimaryKey() is null));

                        return innerMaterializer;
                    }

                    // Collection include (one-to-many)
                    case RelationalCollectionShaperExpression collectionShaper:
                    {
                        var childMaterializer = TryBuildMaterializerFromShaper(
                            collectionShaper.InnerShaper, selectExpression, isTracking);
                        if (childMaterializer is null)
                        {
                            return null;
                        }

                        var parentIdentifier = CompileIdentifierLambda(
                            collectionShaper.ParentIdentifier, selectExpression);
                        var outerIdentifier = CompileIdentifierLambda(
                            collectionShaper.OuterIdentifier, selectExpression);
                        var selfIdentifier = CompileIdentifierLambda(
                            collectionShaper.SelfIdentifier, selectExpression);

                        var parentComparers = collectionShaper.ParentIdentifierValueComparers
                            .Select<ValueComparer, Func<object, object, bool>>(vc => (a, b) => vc.Equals(a, b))
                            .ToArray();
                        var outerComparers = collectionShaper.OuterIdentifierValueComparers
                            .Select<ValueComparer, Func<object, object, bool>>(vc => (a, b) => vc.Equals(a, b))
                            .ToArray();
                        var selfComparers = collectionShaper.SelfIdentifierValueComparers
                            .Select<ValueComparer, Func<object, object, bool>>(vc => (a, b) => vc.Equals(a, b))
                            .ToArray();

                        var collectionAccessor = navigation.GetCollectionAccessor()!;
                        var collectionId = _nextCollectionId++;

                        innerMaterializer.AddCollectionInclude(new CollectionIncludeInfo(
                            childMaterializer,
                            navigation,
                            inverseNavigation,
                            inverseNavSetter,
                            collectionAccessor,
                            parentIdentifier,
                            outerIdentifier,
                            selfIdentifier,
                            parentComparers,
                            outerComparers,
                            selfComparers,
                            collectionId,
                            isKeylessEntityType: navigation.DeclaringEntityType.FindPrimaryKey() is null));

                        return innerMaterializer;
                    }

                    default:
                        return null;
                }
            }

            default:
                return null;
        }
    }

    private static RelationalEntityMaterializer? TryBuildEntityMaterializer(
        IEntityType entityType,
        ProjectionBindingExpression projectionBinding,
        SelectExpression selectExpression,
        bool isTracking,
        bool isNullable)
    {
        var projectionIndex = selectExpression.GetProjection(projectionBinding).GetConstantValue<object>();
        if (projectionIndex is not IDictionary<IPropertyBase, int> propertyIndexMap)
        {
            return null;
        }

        foreach (var concreteType in entityType.GetConcreteDerivedTypesInclusive())
        {
            if (concreteType.ClrType.GetConstructor(Type.EmptyTypes) == null)
            {
                return null;
            }
        }

        var materializerType = typeof(RelationalEntityMaterializer<>).MakeGenericType(entityType.ClrType);
        return (RelationalEntityMaterializer)Activator.CreateInstance(
            materializerType, entityType, propertyIndexMap, isTracking, isNullable)!;
    }

    private static Func<QueryContext, DbDataReader, object[]> CompileIdentifierLambda(
        Expression identifierExpression,
        SelectExpression selectExpression)
    {
        var queryContextParam = Parameter(typeof(QueryContext), "queryContext");
        var dataReaderParam = Parameter(typeof(DbDataReader), "dataReader");

        var rewriter = new IdentifierExpressionRewriter(selectExpression, dataReaderParam);
        var rewritten = rewriter.Visit(identifierExpression);

        return Lambda<Func<QueryContext, DbDataReader, object[]>>(
            rewritten, queryContextParam, dataReaderParam).Compile();
    }

    /// <summary>
    ///     Rewrites identifier expressions for collection includes.
    /// </summary>
    private sealed class IdentifierExpressionRewriter(SelectExpression selectExpression, ParameterExpression dataReaderParam) : ExpressionVisitor
    {
        private static readonly MethodInfo GetValueMethod
            = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetValue))!;

        private static readonly MethodInfo IsDbNullMethod
            = typeof(DbDataReader).GetMethod(nameof(DbDataReader.IsDBNull))!;

        protected override Expression VisitExtension(Expression node)
        {
            if (node is ProjectionBindingExpression pbe)
            {
                var columnIndex = (int)selectExpression.GetProjection(pbe).GetConstantValue<object>();
                return CreateReaderGetValue(columnIndex, pbe.Type);
            }

            return base.VisitExtension(node);
        }

        private Expression CreateReaderGetValue(int columnIndex, Type targetType)
        {
            var indexExpr = Constant(columnIndex);

            var getFieldValueMethod = typeof(DbDataReader)
                .GetMethod(nameof(DbDataReader.GetFieldValue))!
                .MakeGenericMethod(targetType.UnwrapNullableType());

            if (targetType.IsNullableType())
            {
                return Condition(
                    Call(dataReaderParam, IsDbNullMethod, indexExpr),
                    Default(targetType),
                    Convert(
                        Call(dataReaderParam, getFieldValueMethod, indexExpr),
                        targetType));
            }

            return Call(dataReaderParam, getFieldValueMethod, indexExpr);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "ValueBufferTryReadValue" && node.Arguments.Count == 3)
            {
                var indexExpr = Visit(node.Arguments[1]);
                return Condition(
                    Call(dataReaderParam, IsDbNullMethod, indexExpr),
                    Constant(null, typeof(object)),
                    Call(dataReaderParam, GetValueMethod, indexExpr));
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked)
            {
                var operand = Visit(node.Operand);

                if (operand.Type != node.Operand.Type)
                {
                    if (node.Type == typeof(object) && operand.Type == typeof(object))
                    {
                        return operand;
                    }

                    return Convert(operand, node.Type);
                }
            }

            return base.VisitUnary(node);
        }
    }
}
