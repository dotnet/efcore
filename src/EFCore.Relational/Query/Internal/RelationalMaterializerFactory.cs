// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
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
public partial class RelationalMaterializerFactory(ICoreSingletonOptions coreSingletonOptions)
{
    private readonly bool _detailedErrorsEnabled = coreSingletonOptions.AreDetailedErrorsEnabled;
    private readonly bool _threadSafetyChecksEnabled = coreSingletonOptions.AreThreadSafetyChecksEnabled;

    /// <summary>
    ///     Builds a non-generated query executor for an enumerable query where <typeparamref name="TElement" />
    ///     is the element type directly. This eliminates the need for <c>MakeGenericMethod</c>, making the
    ///     entire path NativeAOT-compatible.
    /// </summary>
    public Func<QueryContext, IEnumerable<TElement>> CreateEnumerableMaterializer<TElement>(
        RelationalQueryCompilationContext queryCompilationContext,
        ShapedQueryExpression shapedQueryExpression)
    {
        var (select, shaper) = ((SelectExpression)shapedQueryExpression.QueryExpression, shapedQueryExpression.ShaperExpression);

        // For NoTrackingWithIdentityResolution, validate that JSON entity projections are in a safe
        // order. This mirrors the generated shaper's JsonCorrectOrderOfEntitiesForChangeTrackerValidator.
        if (queryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution)
        {
            new RelationalShapedQueryCompilingExpressionVisitor.ShaperProcessingExpressionVisitor
                .JsonCorrectOrderOfEntitiesForChangeTrackerValidator(select).Validate(shaper);
        }

        if (queryCompilationContext.QuerySplittingBehavior == QuerySplittingBehavior.SplitQuery)
        {
            return CreateSplitQueryEnumerableMaterializer<TElement>(queryCompilationContext, select, shaper);
        }

        if (select.IsNonComposedFromSql())
        {
            throw new NotImplementedException("The non-generated materializer does not yet support FromSql queries.");
        }

        return CreateSingleQueryEnumerableMaterializer<TElement>(queryCompilationContext, select, shaper);
    }

    /// <summary>
    ///     Builds a non-generated query executor for a single (non-split) enumerable query.
    /// </summary>
    private Func<QueryContext, IEnumerable<TElement>> CreateSingleQueryEnumerableMaterializer<TElement>(
        RelationalQueryCompilationContext queryCompilationContext,
        SelectExpression select,
        Expression shaper)
    {
        var nextCollectionId = 0;
        var isTracking = queryCompilationContext.QueryTrackingBehavior is QueryTrackingBehavior.TrackAll;
        var relationalCommandCache = CreateCommandCache(queryCompilationContext, select);

        if (shaper is UnaryExpression { NodeType: ExpressionType.Convert } convert)
        {
            shaper = convert.Operand;
        }

        var rowMaterializer = BuildMaterializer<TElement>(shaper, select, isTracking, ref nextCollectionId);

        return qc => new SingleQueryingEnumerable<TElement>(
            (RelationalQueryContext)qc,
            relationalCommandResolver: parameters => relationalCommandCache.GetRelationalCommandTemplate(parameters),
            readerColumns: null,
            materializer: rowMaterializer!,
            contextType: queryCompilationContext.ContextType,
            standAloneStateManager: queryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution,
            detailedErrorsEnabled: _detailedErrorsEnabled,
            threadSafetyChecksEnabled: _threadSafetyChecksEnabled);
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
        bool isTracking,
        ref int nextCollectionId,
        List<SplitCollectionIncludeInfo>? splitCollectionInfos = null)
    {
        // Strip top-level Convert nodes that are just boxing/widening (e.g. Convert(entity, object)).
        // Do NOT strip if the Convert is doing actual value conversion (e.g. string → DateTimeOffset)
        // — those need to flow through the default/fallback expression compilation path.
        if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } convert
            && (convert.Type == typeof(object) || convert.Type.IsAssignableFrom(convert.Operand.Type)))
        {
            shaperExpression = convert.Operand;
        }

        switch (shaperExpression)
        {
            // Scalar column projection (e.g. Select(x => x.Name), or a scalar within an anonymous type).
            // Reads a single column from the DbDataReader using the type mapping.
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
                    if (nullable && typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) is null)
                    {
                        // Non-nullable value type projected from a nullable column (e.g. LEFT JOIN).
                        // Mirrors the generated shaper: read as Nullable<T>, call .Value to throw
                        // InvalidOperationException ("Nullable object must have a value") when NULL.
                        return (qc, reader, rc, coord)
                            => reader.IsDBNull(columnIndex)
                                ? throw new InvalidOperationException("Nullable object must have a value.")
                                : typedReader.Read<T>(reader);
                    }

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

            // JSON structural type projection (e.g. Select(x => x.JsonComplexProp) or Select(x => x.OwnedJsonNav)).
            // Reads a JSON column from the DbDataReader and materializes the structural type from the JSON stream.
            case RelationalStructuralTypeShaperExpression
            {
                ValueBufferExpression: ProjectionBindingExpression jsonPbe
            } jsonShaper
                when select.GetProjection(jsonPbe).GetConstantValue<object>() is JsonProjectionInfo jsonProjectionInfo:
            {
                return BuildTopLevelJsonMaterializer<T>(
                    jsonShaper.StructuralType, jsonProjectionInfo, isTracking, jsonShaper.IsNullable);
            }

            // JSON collection projection (e.g. Select(x => x.JsonCollectionProp)).
            // Reads a JSON column and materializes an array of elements from a JSON array.
            case CollectionResultExpression
            {
                QueryExpression: ProjectionBindingExpression jsonCollectionPbe,
                StructuralProperty: { } jsonCollectionProperty
            } when select.GetProjection(jsonCollectionPbe).GetConstantValue<object>() is JsonProjectionInfo jsonCollProjInfo:
            {
                return BuildJsonCollectionProjectionMaterializer<T>(jsonCollectionProperty, jsonCollProjInfo, isTracking);
            }

            // Entity, complex type, or include projection (e.g. ctx.Blogs, ctx.Blogs.Include(b => b.Posts),
            // Select(x => x.MyComplexProp)). Builds a RelationalEntityMaterializer tree.
            case RelationalStructuralTypeShaperExpression:
            case IncludeExpression:
            {
                var entityMaterializer = BuildMaterializerFromShaper(
                    shaperExpression, select, isTracking, splitCollectionInfos, ref nextCollectionId);

                if (typeof(T) != typeof(object))
                {
                    return entityMaterializer.GetTypedMaterializeDelegate<T>()!;
                }

                return (queryCtx, reader, rc, coord) => (T?)entityMaterializer.Materialize(queryCtx, reader, rc, coord);
            }

            // Single-query collection projection (e.g. Select(x => x.Posts) via correlated subquery).
            // Uses the multi-call protocol: InitializeCollection on first call, PopulateCollection on
            // every call, return collection when ResultReady.
            case RelationalCollectionShaperExpression collectionShaper:
                return BuildStandaloneCollectionMaterializer<T>(collectionShaper, select, isTracking, ref nextCollectionId);

            // Split-query collection projection (e.g. Select(c => new { Orders = c.Orders.ToList() })
            // with AsSplitQuery). The collection is loaded via a separate SQL query; we register it
            // with splitCollectionInfos and return an empty collection that gets populated in-place
            // by relatedDataLoaders.
            case RelationalSplitCollectionShaperExpression splitCollectionShaper
                when splitCollectionInfos is not null:
            {
                var collectionId = nextCollectionId++;

                var childSplitCollections = new List<SplitCollectionIncludeInfo>();
                var childMaterializer = BuildMaterializerFromShaper(
                    splitCollectionShaper.InnerShaper, splitCollectionShaper.SelectExpression,
                    isTracking, childSplitCollections, ref nextCollectionId);

                var parentIdentifier = CompileIdentifierLambda(
                    splitCollectionShaper.ParentIdentifier, select);
                var childIdentifier = CompileIdentifierLambda(
                    splitCollectionShaper.ChildIdentifier, splitCollectionShaper.SelectExpression);
                var identifierComparers = splitCollectionShaper.IdentifierValueComparers
                    .Select<ValueComparer, Func<object, object, bool>>(vc => (a, b) => vc.Equals(a, b))
                    .ToArray();

                var navigation = splitCollectionShaper.Navigation;
                var collectionAccessor = navigation?.GetCollectionAccessor();

                splitCollectionInfos.Add(new SplitCollectionIncludeInfo(
                    childMaterializer,
                    navigation: null, // standalone — not an include
                    inverseNavigation: null,
                    inverseNavigationSetter: null,
                    collectionAccessor: collectionAccessor,
                    parentIdentifier,
                    childIdentifier,
                    identifierComparers,
                    collectionId,
                    splitCollectionShaper.SelectExpression,
                    childSplitCollections,
                    parentEntityProvider: null));

                // Create and return the empty collection. It will be populated in-place by
                // relatedDataLoaders (PopulateSplitIncludeCollection) before the result is used.
                // ParentEntityProvider will be set up by BuildSplitEntityMaterializer to provide
                // this collection to InitializeSplitIncludeCollection for coordinator registration.
                var collectionType = splitCollectionShaper.Type;
                return (queryCtx, reader, rc, coord)
                    => (T?)(collectionAccessor?.Create() ?? Activator.CreateInstance(collectionType));
            }

            // Anonymous/named type projection (e.g. Select(x => new { x.Id, x.Name })).
            // Each constructor argument is materialized recursively.
            case NewExpression newExpression:
            {
                // TODO: For JIT mode, probably better to fall through to the default case, to just compile the expression tree.
                // But probably good to leave the optimization here for NativeAOT.
                var invoker = ConstructorInvoker.Create(newExpression.Constructor!);
                var subMaterializers = new Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, object?>[newExpression.Arguments.Count];
                for (var i = 0; i < subMaterializers.Length; i++)
                {
                    subMaterializers[i] = BuildMaterializer<object>(newExpression.Arguments[i], select, isTracking, ref nextCollectionId, splitCollectionInfos);
                }

                var invokerArgs = new object?[subMaterializers.Length];

                return ComposeWithMultiCallProtocol<T>(
                    subMaterializers,
                    (_, _, values) =>
                    {
                        values.CopyTo(invokerArgs.AsSpan());
                        return (T?)invoker.Invoke(invokerArgs.AsSpan());
                    });
            }

            // Client-evaluated expression fallback (e.g. method calls, member accesses, conditionals
            // containing EF-specific extension nodes). Sub-expressions are extracted and materialized
            // separately, then the remaining pure CLR expression is compiled against the values.
            default:
            {
                // Single-pass rewrite: replaces ProjectionBindingExpression nodes with typed reader
                // calls and entity/include nodes with array element accesses on an entityValues
                // parameter. Collects the original entity expressions for materializer building.
                // Use the shared QueryContextParameter so references in the shaper tree
                // (e.g. from funcletized closure variables) match the lambda parameter.
                var queryContextParam = QueryCompilationContext.QueryContextParameter;
                var dataReaderParam = Parameter(typeof(DbDataReader), "dataReader");
                var entityValuesParam = Parameter(typeof(object[]), "entityValues");

                var rewriter = new ShaperExpressionRewritingVisitor(select, dataReaderParam, entityValuesParam);
                var rewritten = rewriter.Visit(shaperExpression);

                if (rewritten.Type != typeof(T))
                {
                    rewritten = Convert(rewritten, typeof(T));
                }

                // Build materializers for extracted sub-expressions (entities and scalars).
                var subMaterializers = new Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, object?>[rewriter.ExtractedSubExpressions.Count];
                for (var i = 0; i < subMaterializers.Length; i++)
                {
                    subMaterializers[i] = BuildMaterializer<object>(
                        rewriter.ExtractedSubExpressions[i], select, isTracking, ref nextCollectionId, splitCollectionInfos);
                }

                var projectionFunc = Lambda<Func<QueryContext, DbDataReader, object[], T?>>(
                    rewritten, queryContextParam, dataReaderParam, entityValuesParam).Compile();

                return subMaterializers.Length == 0
                    ? (queryCtx, reader, rc, coord) => projectionFunc(queryCtx, reader, [])
                    : ComposeWithMultiCallProtocol(subMaterializers, (queryCtx, reader, values)
                        => projectionFunc(queryCtx, reader, values));
            }
        }
    }

    /// <summary>
    ///     Wraps an array of sub-materializers with the multi-call protocol used by
    ///     <see cref="SingleQueryingEnumerable{T}" /> for collection population.
    ///     On first call (rc.Values == null): materializes all sub-expressions, stores in rc.Values.
    ///     On subsequent calls: drives sub-materializers for collection population.
    ///     When ResultReady: composes the final result from rc.Values via <paramref name="resultComposer" />.
    /// </summary>
    private static Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T?> ComposeWithMultiCallProtocol<T>(
        Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, object?>[] subMaterializers,
        Func<QueryContext, DbDataReader, object[], T?> resultComposer)
    {
        var subContexts = new ResultContext[subMaterializers.Length];
        for (var i = 0; i < subContexts.Length; i++)
        {
            subContexts[i] = new ResultContext();
        }

        return (queryCtx, reader, rc, coord) =>
        {
            if (rc.Values is null)
            {
                for (var i = 0; i < subContexts.Length; i++)
                {
                    subContexts[i].Values = null;
                }

                rc.Values = new object[subMaterializers.Length];
                for (var i = 0; i < subMaterializers.Length; i++)
                {
                    var result = subMaterializers[i](queryCtx, reader, subContexts[i], coord);

                    // For entity materializers with collection includes, the return value may be
                    // null (ResultReady=false during collection population), but the entity itself
                    // is cached in subContexts[i].Values[0]. For everything else (scalars, nested
                    // NewExpressions), the return value is the actual value.
                    rc.Values[i] = (result ?? subContexts[i].Values?[0])!;
                }
            }
            else
            {
                // Only drive sub-materializers that have internal state (entity materializers with
                // collection includes). Scalars and simple entities don't need subsequent calls —
                // their values were cached in rc.Values during init.
                for (var i = 0; i < subMaterializers.Length; i++)
                {
                    if (subContexts[i].Values is not null)
                    {
                        subMaterializers[i](queryCtx, reader, subContexts[i], coord);
                    }
                }
            }

            return coord.ResultReady ? resultComposer(queryCtx, reader, rc.Values) : default;
        };
    }

    /// <summary>
    ///     Rewrites a shaper expression tree by replacing <see cref="ProjectionBindingExpression" /> nodes
    ///     with type-mapping-aware <see cref="DbDataReader" /> access calls (including value converters).
    ///     Extends <see cref="IdentifierExpressionRewriter" /> to reuse its <c>VisitMethodCall</c> and
    ///     <c>VisitUnary</c> handling.
    /// </summary>
    private sealed class ShaperExpressionRewritingVisitor(
        SelectExpression selectExpression,
        ParameterExpression dataReaderParameter,
        ParameterExpression? entityValuesParam = null)
        : IdentifierExpressionRewriter(selectExpression, dataReaderParameter)
    {
        private static readonly MethodInfo IsDbNullMethod
            = typeof(DbDataReader).GetMethod(nameof(DbDataReader.IsDBNull))!;

        private static readonly PropertyInfo _parametersProperty
            = typeof(QueryContext).GetProperty(nameof(QueryContext.Parameters))!;

        private static readonly PropertyInfo _dictionaryIndexer
            = typeof(Dictionary<string, object?>).GetProperty("Item")!;

        private readonly ParameterExpression? _entityValuesParam = entityValuesParam;

        /// <summary>
        ///     Sub-expressions extracted during visitation (entities, includes, and scalars when in
        ///     multi-call mode). Each is replaced with a typed array access on
        ///     <c>_entityValuesParam[index]</c>. After visitation, callers build materializers for
        ///     each entry and wire them via <see cref="ComposeWithMultiCallProtocol{T}" />.
        /// </summary>
        public List<Expression> ExtractedSubExpressions { get; } = [];

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case ProjectionBindingExpression projectionBinding
                    when _entityValuesParam is not null:
                {
                    // In multi-call mode, extract scalar reads into the values array so they're
                    // cached during init and not re-read from the reader on subsequent calls.
                    var index = ExtractedSubExpressions.Count;
                    ExtractedSubExpressions.Add(node);
                    return Convert(
                        ArrayIndex(_entityValuesParam, Constant(index)),
                        projectionBinding.Type);
                }

                case ProjectionBindingExpression projectionBinding:
                {
                    var projectionIndex = SelectExpression.GetProjection(projectionBinding).GetConstantValue<object>();
                    if (projectionIndex is not int columnIndex)
                    {
                        return base.VisitExtension(node);
                    }

                    var projection = SelectExpression.Projection[columnIndex];
                    var nullable = projection.Expression is not ColumnExpression col || col.IsNullable;
                    var typeMapping = (RelationalTypeMapping)projection.Expression.TypeMapping!;
                    var getMethod = typeMapping.GetDataReaderMethod();

                    Expression valueExpression = Call(
                        getMethod.DeclaringType != typeof(DbDataReader)
                            ? Convert(DataReaderParam, getMethod.DeclaringType!)
                            : DataReaderParam,
                        getMethod,
                        Constant(columnIndex));

                    valueExpression = typeMapping.CustomizeDataReaderExpression(valueExpression);

                    var converter = typeMapping.Converter;
                    if (converter is not null)
                    {
                        if (valueExpression.Type != converter.ProviderClrType)
                        {
                            valueExpression = Convert(valueExpression, converter.ProviderClrType);
                        }

                        valueExpression = ReplacingExpressionVisitor.Replace(
                            converter.ConvertFromProviderExpression.Parameters.Single(),
                            valueExpression,
                            converter.ConvertFromProviderExpression.Body);
                    }

                    if (valueExpression.Type != projectionBinding.Type)
                    {
                        valueExpression = Convert(valueExpression, projectionBinding.Type);
                    }

                    if (nullable)
                    {
                        var targetType = projectionBinding.Type;

                        // For non-nullable value types from nullable columns, throw
                        // InvalidOperationException (matching Nullable<T>.Value behavior)
                        // instead of returning default.
                        var nullValue = targetType.IsValueType && Nullable.GetUnderlyingType(targetType) is null
                            ? Throw(
                                New(
                                    typeof(InvalidOperationException).GetConstructor([typeof(string)])!,
                                    Constant("Nullable object must have a value.")),
                                targetType)
                            : (Expression)Default(targetType);

                        valueExpression = Condition(
                            Call(DataReaderParam, IsDbNullMethod, Constant(columnIndex)),
                            nullValue,
                            valueExpression);
                    }

                    return valueExpression;
                }

                case RelationalStructuralTypeShaperExpression or IncludeExpression
                    when _entityValuesParam is not null:
                {
                    var index = ExtractedSubExpressions.Count;
                    ExtractedSubExpressions.Add(node);
                    return Convert(
                        ArrayIndex(_entityValuesParam, Constant(index)),
                        node.Type);
                }

                // Collection shapers nested inside client-evaluated expressions (e.g. `.ToArray()`,
                // `.AsEnumerable()`) need to be extracted and materialized separately via
                // BuildStandaloneCollectionMaterializer / BuildMaterializer.
                case RelationalCollectionShaperExpression or RelationalSplitCollectionShaperExpression
                    when _entityValuesParam is not null:
                {
                    var index = ExtractedSubExpressions.Count;
                    ExtractedSubExpressions.Add(node);
                    return Convert(
                        ArrayIndex(_entityValuesParam, Constant(index)),
                        node.Type);
                }

                // Runtime query parameters: replace with dictionary lookup on QueryContext.Parameters.
                // These appear in client-evaluated expressions that reference funcletized closure variables.
                case QueryParameterExpression queryParameter:
                    return Convert(
                        Property(
                            Property(QueryCompilationContext.QueryContextParameter, _parametersProperty),
                            _dictionaryIndexer,
                            Constant(queryParameter.Name)),
                        queryParameter.Type);

                // Liftable constants: in non-generated mode, resolve to the original expression directly.
                case LiftableConstantExpression liftableConstant:
                    return liftableConstant.OriginalExpression;

                default:
                    return base.VisitExtension(node);
            }
        }
    }

    /// <summary>
    ///     Builds a materializer for a standalone collection projection (e.g. <c>Select(x => x.Posts)</c>).
    ///     This mirrors the generated shaper's <c>InitializeCollection</c> / <c>PopulateCollection</c> protocol:
    ///     on first call the collection is created and stored in <see cref="ResultContext.Values" />;
    ///     on every call (including first) elements are populated from the current row;
    ///     when <see cref="SingleQueryResultCoordinator.ResultReady" /> is true the collection is returned.
    /// </summary>
    private Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T?> BuildStandaloneCollectionMaterializer<T>(
        RelationalCollectionShaperExpression collectionShaper,
        SelectExpression select,
        bool isTracking,
        ref int nextCollectionId)
    {
        var collectionId = nextCollectionId++;

        var innerElementMaterializer = BuildMaterializer<object>(
            collectionShaper.InnerShaper, select, isTracking, ref nextCollectionId);

        var parentIdentifier = CompileIdentifierLambda(collectionShaper.ParentIdentifier, select);
        var outerIdentifier = CompileIdentifierLambda(collectionShaper.OuterIdentifier, select);
        var selfIdentifier = CompileIdentifierLambda(collectionShaper.SelfIdentifier, select);

        var parentComparers = collectionShaper.ParentIdentifierValueComparers
            .Select<ValueComparer, Func<object, object, bool>>(vc => (a, b) => vc.Equals(a, b))
            .ToArray();
        var outerComparers = collectionShaper.OuterIdentifierValueComparers
            .Select<ValueComparer, Func<object, object, bool>>(vc => (a, b) => vc.Equals(a, b))
            .ToArray();
        var selfComparers = collectionShaper.SelfIdentifierValueComparers
            .Select<ValueComparer, Func<object, object, bool>>(vc => (a, b) => vc.Equals(a, b))
            .ToArray();

        var navigation = collectionShaper.Navigation;
        var collectionAccessor = navigation?.GetCollectionAccessor();

        // Build a typed add delegate: (collection, element) => ((ICollection<TElement>)collection).Add((TElement)element).
        // This works for any ICollection<T> including HashSet<T>, unlike IList.Add.
        var addMethod = typeof(ICollection<>).MakeGenericType(collectionShaper.ElementType)
            .GetMethod(nameof(ICollection<object>.Add))!;
        Action<object, object?> collectionAdd = (collection, element) =>
            addMethod.Invoke(collection, [element]);

        // Mirrors the generated shaper structure: InitializeCollection on first call,
        // PopulateCollection on every call, return collection when ResultReady.
        return (queryContext, dataReader, resultContext, resultCoordinator) =>
        {
            if (resultContext.Values is null)
            {
                var collection = collectionAccessor?.Create()
                    ?? Activator.CreateInstance(collectionShaper.Type)!;

                resultCoordinator.SetSingleQueryCollectionContext(
                    collectionId,
                    new SingleQueryCollectionContext(
                        null, collection,
                        parentIdentifier(queryContext, dataReader),
                        outerIdentifier(queryContext, dataReader)));

                resultContext.Values = [collection];
            }

            PopulateCollection(
                queryContext, dataReader, resultCoordinator, collectionId,
                parentIdentifier, outerIdentifier, selfIdentifier,
                parentComparers, outerComparers, selfComparers,
                innerElementMaterializer,
                collectionAdd);

            return resultCoordinator.ResultReady ? (T)resultContext.Values[0] : default;
        };
    }

    /// <summary>
    ///     Populates a standalone collection from the current <see cref="DbDataReader" /> row.
    ///     This is the non-generated equivalent of
    ///     <see cref="RelationalShapedQueryCompilingExpressionVisitor.ShaperProcessingExpressionVisitor.PopulateCollection{TCollection, TElement, TRelatedEntity}" />.
    /// </summary>
    internal static void PopulateCollection(
        QueryContext queryContext,
        DbDataReader dataReader,
        SingleQueryResultCoordinator resultCoordinator,
        int collectionId,
        Func<QueryContext, DbDataReader, object[]> parentIdentifier,
        Func<QueryContext, DbDataReader, object[]> outerIdentifier,
        Func<QueryContext, DbDataReader, object[]> selfIdentifier,
        IReadOnlyList<Func<object, object, bool>> parentIdentifierValueComparers,
        IReadOnlyList<Func<object, object, bool>> outerIdentifierValueComparers,
        IReadOnlyList<Func<object, object, bool>> selfIdentifierValueComparers,
        Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, object?> innerShaper,
        Action<object, object?> collectionAdd)
    {
        var collectionContext = resultCoordinator.Collections[collectionId]!;
        if (collectionContext.Collection is null)
        {
            return;
        }

        if (resultCoordinator.HasNext == false)
        {
            GenerateCurrentElementIfPending();
            return;
        }

        if (!CompareIdentifiers(
                outerIdentifierValueComparers,
                outerIdentifier(queryContext, dataReader),
                collectionContext.OuterIdentifier))
        {
            GenerateCurrentElementIfPending();

            if (!CompareIdentifiers(
                    parentIdentifierValueComparers,
                    parentIdentifier(queryContext, dataReader),
                    collectionContext.ParentIdentifier))
            {
                resultCoordinator.HasNext = true;
            }

            return;
        }

        var innerKey = selfIdentifier(queryContext, dataReader);
        if (innerKey.Length > 0 && innerKey.All(e => e == null))
        {
            return;
        }

        if (collectionContext.SelfIdentifier is not null)
        {
            if (CompareIdentifiers(selfIdentifierValueComparers, innerKey, collectionContext.SelfIdentifier))
            {
                if (collectionContext.ResultContext.Values is not null)
                {
                    ProcessCurrentElementRow();
                }

                resultCoordinator.ResultReady = false;
                return;
            }

            GenerateCurrentElementIfPending();
            resultCoordinator.HasNext = null;
            collectionContext.UpdateSelfIdentifier(innerKey);
        }
        else
        {
            collectionContext.UpdateSelfIdentifier(innerKey);
        }

        ProcessCurrentElementRow();
        resultCoordinator.ResultReady = false;

        void ProcessCurrentElementRow()
        {
            var previousResultReady = resultCoordinator.ResultReady;
            resultCoordinator.ResultReady = true;

            var element = innerShaper(
                queryContext, dataReader, collectionContext.ResultContext, resultCoordinator);

            if (resultCoordinator.ResultReady)
            {
                collectionContext.ResultContext.Values = null;
                collectionAdd(collectionContext.Collection!, element);
            }

            resultCoordinator.ResultReady &= previousResultReady;
        }

        void GenerateCurrentElementIfPending()
        {
            if (collectionContext.ResultContext.Values is not null)
            {
                resultCoordinator.HasNext = false;
                ProcessCurrentElementRow();
            }

            collectionContext.UpdateSelfIdentifier(null);
        }
    }

    /// <summary>
    ///     Recursively builds a <see cref="RelationalEntityMaterializer{TEntity}" /> from a shaper expression.
    ///     When <paramref name="splitCollectionInfos" /> is non-null, operates in split-query mode:
    ///     <see cref="RelationalSplitCollectionShaperExpression" /> nodes are collected into the list
    ///     instead of being added as <see cref="CollectionIncludeInfo" /> on the entity materializer.
    /// </summary>
    private static RelationalEntityMaterializer BuildMaterializerFromShaper(
        Expression shaperExpression,
        SelectExpression selectExpression,
        bool isTracking,
        List<SplitCollectionIncludeInfo>? splitCollectionInfos,
        ref int nextCollectionId)
    {
        switch (shaperExpression)
        {
            case RelationalStructuralTypeShaperExpression
            {
                ValueBufferExpression: ProjectionBindingExpression projectionBinding
            } shaper:
            {
                return BuildStructuralTypeMaterializer(
                    shaper.StructuralType, projectionBinding, selectExpression, isTracking, shaper.IsNullable);
            }

            case IncludeExpression includeExpression:
            {
                // Recurse into the entity expression to build the inner materializer (which may have further includes)
                var innerMaterializer = BuildMaterializerFromShaper(
                    includeExpression.EntityExpression, selectExpression, isTracking,
                    splitCollectionInfos, ref nextCollectionId);

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
                    // recursively by the BuildMaterializerFromShaper call below.
                    case RelationalStructuralTypeShaperExpression
                    {
                        StructuralType: IEntityType,
                        ValueBufferExpression: ProjectionBindingExpression
                    }:
                    case IncludeExpression:
                    {
                        // Check if this is a JSON include before attempting the column-based path.
                        // JSON owned entities have ProjectionBindingExpressions that resolve to
                        // JsonProjectionInfo, which BuildStructuralTypeMaterializer cannot handle.
                        if (IsJsonIncludeNavigation(includeExpression.NavigationExpression, selectExpression))
                        {
                            goto default;
                        }

                        var splitCountBefore = splitCollectionInfos?.Count ?? 0;

                        var includedMaterializer = BuildMaterializerFromShaper(
                            includeExpression.NavigationExpression, selectExpression, isTracking,
                            splitCollectionInfos, ref nextCollectionId);

                        var navSetter = ((IRuntimePropertyBase)navigation).GetSetter();

                        var refInclude = new ReferenceIncludeInfo(
                            includedMaterializer,
                            navigation,
                            navSetter,
                            inverseNavigation,
                            inverseNavSetter,
                            isKeylessEntityType: navigation.DeclaringEntityType.FindPrimaryKey() is null);
                        innerMaterializer.AddReferenceInclude(refInclude);

                        // For single queries, flatten collection includes from the referenced entity
                        // up to the parent. For split queries, this isn't needed — collections are
                        // loaded via separate queries and don't participate in the multi-call protocol.
                        if (splitCollectionInfos is null)
                        {
                            FlattenCollectionIncludes(includedMaterializer, refInclude.ResultContext, innerMaterializer);
                        }
                        else
                        {
                            // For split queries, any split collections added by the recursive call
                            // need their ParentEntityProvider set to return the referenced entity
                            // (e.g. Customer from OrderDetail → Order → Customer → Orders).
                            // Only set ParentEntityProvider if it hasn't already been set by a deeper
                            // level in the reference include chain — deeper levels set it to the
                            // correct entity closest to the collection.
                            var capturedContext = refInclude.ResultContext;
                            for (var i = splitCountBefore; i < splitCollectionInfos.Count; i++)
                            {
                                splitCollectionInfos[i].ParentEntityProvider ??= () => capturedContext.Values?[0];
                            }
                        }

                        return innerMaterializer;
                    }

                    // Single-query collection include (one-to-many)
                    case RelationalCollectionShaperExpression collectionShaper
                        when splitCollectionInfos is null:
                    {
                        var childMaterializer = BuildMaterializerFromShaper(
                            collectionShaper.InnerShaper, selectExpression, isTracking,
                            splitCollectionInfos: null, ref nextCollectionId);

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
                        var collectionId = nextCollectionId++;

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

                    // Split-query collection include (one-to-many via separate SQL query)
                    case RelationalSplitCollectionShaperExpression splitCollectionShaper
                        when splitCollectionInfos is not null:
                    {
                        var collectionId = nextCollectionId++;

                        var childSplitCollections = new List<SplitCollectionIncludeInfo>();
                        var childMaterializer = BuildMaterializerFromShaper(
                            splitCollectionShaper.InnerShaper, splitCollectionShaper.SelectExpression,
                            isTracking, childSplitCollections, ref nextCollectionId);

                        var parentIdentifier = CompileIdentifierLambda(
                            splitCollectionShaper.ParentIdentifier, selectExpression);
                        var childIdentifier = CompileIdentifierLambda(
                            splitCollectionShaper.ChildIdentifier, splitCollectionShaper.SelectExpression);
                        var identifierComparers = splitCollectionShaper.IdentifierValueComparers
                            .Select<ValueComparer, Func<object, object, bool>>(vc => (a, b) => vc.Equals(a, b))
                            .ToArray();

                        var collectionAccessor = navigation.GetCollectionAccessor()!;

                        splitCollectionInfos.Add(new SplitCollectionIncludeInfo(
                            childMaterializer,
                            navigation,
                            inverseNavigation,
                            inverseNavSetter,
                            collectionAccessor,
                            parentIdentifier,
                            childIdentifier,
                            identifierComparers,
                            collectionId,
                            splitCollectionShaper.SelectExpression,
                            childSplitCollections,
                            parentEntityProvider: null));

                        return innerMaterializer;
                    }

                    default:
                    {
                        // JSON include case: navigation expression is a RelationalStructuralTypeShaperExpression
                        // or CollectionResultExpression with a ProjectionBindingExpression that resolves to JsonProjectionInfo.
                        var jsonPbe = (includeExpression.NavigationExpression as CollectionResultExpression)
                            ?.QueryExpression as ProjectionBindingExpression
                            ?? (includeExpression.NavigationExpression as RelationalStructuralTypeShaperExpression)
                            ?.ValueBufferExpression as ProjectionBindingExpression;

                        if (jsonPbe is not null
                            && selectExpression.GetProjection(jsonPbe).GetConstantValue<object>() is JsonProjectionInfo jsonProjectionInfo)
                        {
                            var targetEntityType = navigation.TargetEntityType;
                            var jsonMaterializer = BuildJsonStructuralTypeMaterializer(
                                targetEntityType, isTracking, nullable: true);

                            // Find the JSON column type mapping for correct column reading
                            var jsonColumnName = targetEntityType.GetContainerColumnName()!;
                            var jsonColumn = targetEntityType.ContainingEntityType.GetViewOrTableMappings()
                                .Select(m => m.Table.FindColumn(jsonColumnName))
                                .FirstOrDefault(c => c is not null)
                                ?? throw new UnreachableException(
                                    $"Could not find JSON container column '{jsonColumnName}'.");

                            var jsonColumnTypeMapping = (RelationalTypeMapping)jsonColumn.StoreTypeMapping;
                            var jsonStreamReader = BuildJsonColumnReader(
                                jsonColumnTypeMapping, jsonProjectionInfo.JsonColumnIndex);

                            innerMaterializer.AddJsonInclude(new JsonIncludeInfo(
                                navigation,
                                inverseNavigation,
                                inverseNavSetter,
                                jsonMaterializer,
                                jsonProjectionInfo,
                                jsonStreamReader,
                                navigation.IsCollection));

                            return innerMaterializer;
                        }

                        throw new NotImplementedException(
                            $"The non-generated materializer does not yet support include navigation expression type '{includeExpression.NavigationExpression.GetType().Name}'.");
                    }
                }
            }

            default:
                throw new NotImplementedException(
                    $"The non-generated materializer does not yet support shaper expression type '{shaperExpression.GetType().Name}'.");
        }
    }

    private static RelationalEntityMaterializer BuildStructuralTypeMaterializer(
        ITypeBase structuralType,
        ProjectionBindingExpression projectionBinding,
        SelectExpression selectExpression,
        bool isTracking,
        bool isNullable)
    {
        var projectionIndex = selectExpression.GetProjection(projectionBinding).GetConstantValue<object>();
        if (projectionIndex is not IDictionary<IPropertyBase, int> propertyIndexMap)
        {
            throw new NotImplementedException(
                $"The non-generated materializer does not support projection index type '{projectionIndex?.GetType().Name}' for type '{structuralType.DisplayName()}'.");
        }

        if (structuralType is IEntityType entityType)
        {
            foreach (var concreteType in entityType.GetConcreteDerivedTypesInclusive())
            {
                if (concreteType.ClrType.GetConstructor(Type.EmptyTypes) == null)
                {
                    throw new NotImplementedException(
                        $"The non-generated materializer does not yet support entity type '{concreteType.DisplayName()}' which has no parameterless constructor.");
                }
            }
        }
        else if (structuralType.ClrType.GetConstructor(Type.EmptyTypes) == null
            && !structuralType.ClrType.IsValueType)
        {
            throw new NotImplementedException(
                $"The non-generated materializer does not yet support type '{structuralType.DisplayName()}' which has no parameterless constructor.");
        }

        var materializerType = typeof(RelationalEntityMaterializer<>).MakeGenericType(structuralType.ClrType);
        return (RelationalEntityMaterializer)Activator.CreateInstance(
            materializerType, structuralType, propertyIndexMap, isTracking, isNullable)!;
    }

    /// <summary>
    ///     Returns whether the given include navigation expression resolves to a JSON projection
    ///     (i.e. its <see cref="ProjectionBindingExpression" /> resolves to <see cref="JsonProjectionInfo" />).
    /// </summary>
    private static bool IsJsonIncludeNavigation(Expression navigationExpression, SelectExpression selectExpression)
    {
        var pbe = (navigationExpression as CollectionResultExpression)?.QueryExpression as ProjectionBindingExpression
            ?? (navigationExpression as RelationalStructuralTypeShaperExpression)?.ValueBufferExpression as ProjectionBindingExpression;

        return pbe is not null
            && selectExpression.GetProjection(pbe).GetConstantValue<object>() is JsonProjectionInfo;
    }

    /// <summary>
    ///     Recursively extracts collection includes from a reference include's materializer (and any
    ///     nested reference includes) and adds them to the <paramref name="target" /> materializer.
    ///     This mirrors the generated shaper's behavior where all collection populations are driven
    ///     at the top level rather than nested inside reference include materializers.
    /// </summary>
    private static void FlattenCollectionIncludes(
        RelationalEntityMaterializer materializer,
        ResultContext materializerResultContext,
        RelationalEntityMaterializer target)
    {
        // First, recursively flatten collections from this materializer's own reference includes.
        if (materializer.ReferenceIncludes is not null)
        {
            for (var i = 0; i < materializer.ReferenceIncludes.Count; i++)
            {
                var refInclude = materializer.ReferenceIncludes[i];
                FlattenCollectionIncludes(refInclude.Materializer, refInclude.ResultContext, target);
            }
        }

        // Then extract this materializer's direct collection includes and re-parent them.
        if (materializer.CollectionIncludes is not null)
        {
            for (var i = 0; i < materializer.CollectionIncludes.Count; i++)
            {
                var ci = materializer.CollectionIncludes[i];

                // Capture the ResultContext so the parent entity can be retrieved after
                // ProcessReferenceIncludes has materialized it.
                var capturedContext = materializerResultContext;
                target.AddCollectionInclude(new CollectionIncludeInfo(
                    ci.InnerMaterializer,
                    ci.Navigation,
                    ci.InverseNavigation,
                    ci.InverseNavigationSetter,
                    ci.CollectionAccessor,
                    ci.ParentIdentifier,
                    ci.OuterIdentifier,
                    ci.SelfIdentifier,
                    ci.ParentIdentifierValueComparers,
                    ci.OuterIdentifierValueComparers,
                    ci.SelfIdentifierValueComparers,
                    ci.CollectionId,
                    ci.IsKeylessEntityType,
                    parentEntityProvider: () => capturedContext.Values?[0]));
            }

            materializer.ClearCollectionIncludes();
        }
    }

    private static bool CompareIdentifiers(
        IReadOnlyList<Func<object, object, bool>> valueComparers,
        object[] left,
        object[] right)
    {
        for (var i = 0; i < left.Length; i++)
        {
            if (!valueComparers[i](left[i], right[i]))
            {
                return false;
            }
        }

        return true;
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
    ///     Rewrites expressions by replacing <see cref="ProjectionBindingExpression" /> nodes with
    ///     <see cref="DbDataReader" /> access calls. Used for collection identifier expressions.
    /// </summary>
    private class IdentifierExpressionRewriter : ExpressionVisitor
    {
        private static readonly MethodInfo GetValueMethod
            = typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetValue))!;

        private static readonly MethodInfo IsDbNullMethod
            = typeof(DbDataReader).GetMethod(nameof(DbDataReader.IsDBNull))!;

        protected readonly SelectExpression SelectExpression;
        protected readonly ParameterExpression DataReaderParam;

        public IdentifierExpressionRewriter(SelectExpression selectExpression, ParameterExpression dataReaderParam)
        {
            SelectExpression = selectExpression;
            DataReaderParam = dataReaderParam;
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (node is ProjectionBindingExpression pbe)
            {
                var columnIndex = (int)SelectExpression.GetProjection(pbe).GetConstantValue<object>();
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
                    Call(DataReaderParam, IsDbNullMethod, indexExpr),
                    Default(targetType),
                    Convert(
                        Call(DataReaderParam, getFieldValueMethod, indexExpr),
                        targetType));
            }

            return Call(DataReaderParam, getFieldValueMethod, indexExpr);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "ValueBufferTryReadValue" && node.Arguments.Count == 3)
            {
                var indexExpr = Visit(node.Arguments[1]);
                return Condition(
                    Call(DataReaderParam, IsDbNullMethod, indexExpr),
                    Constant(null, typeof(object)),
                    Call(DataReaderParam, GetValueMethod, indexExpr));
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

    #region Non-query execution (ExecuteDelete / ExecuteUpdate)

    /// <summary>
    ///     Builds a non-generated executor for a non-query operation (ExecuteDelete / ExecuteUpdate).
    ///     These don't involve entity materialization — they just execute a SQL command and return the
    ///     affected row count.
    /// </summary>
    public Func<QueryContext, int> CreateNonQueryExecutor(
        RelationalQueryCompilationContext queryCompilationContext,
        Expression nonQueryExpression)
    {
        var commandCache = CreateNonQueryCommandCache(queryCompilationContext, nonQueryExpression);
        var contextType = queryCompilationContext.ContextType;

        return qc => ExecuteNonQuery(
            (RelationalQueryContext)qc, commandCache.GetRelationalCommandTemplate, contextType,
            CommandSource.ExecuteUpdate, _threadSafetyChecksEnabled);
    }

    /// <summary>
    ///     Builds a non-generated async executor for a non-query operation (ExecuteDelete / ExecuteUpdate).
    /// </summary>
    public Func<QueryContext, Task<int>> CreateNonQueryAsyncExecutor(
        RelationalQueryCompilationContext queryCompilationContext,
        Expression nonQueryExpression)
    {
        var commandCache = CreateNonQueryCommandCache(queryCompilationContext, nonQueryExpression);
        var contextType = queryCompilationContext.ContextType;

        return async qc => await ExecuteNonQueryAsync(
            (RelationalQueryContext)qc, commandCache.GetRelationalCommandTemplate, contextType,
            CommandSource.ExecuteUpdate, _threadSafetyChecksEnabled).ConfigureAwait(false);
    }

    private RelationalCommandCache CreateNonQueryCommandCache(
        RelationalQueryCompilationContext queryCompilationContext,
        Expression nonQueryExpression)
    {
        if (nonQueryExpression is DeleteExpression deleteExpression)
        {
            nonQueryExpression = deleteExpression.ApplyTags(queryCompilationContext.Tags);
        }
        else if (nonQueryExpression is UpdateExpression updateExpression)
        {
            nonQueryExpression = updateExpression.ApplyTags(queryCompilationContext.Tags);
        }

        return CreateCommandCache(queryCompilationContext, nonQueryExpression);
    }

    private static int ExecuteNonQuery(
        RelationalQueryContext relationalQueryContext,
        RelationalCommandResolver relationalCommandResolver,
        Type contextType,
        CommandSource commandSource,
        bool threadSafetyChecksEnabled)
    {
        try
        {
            using var _ = threadSafetyChecksEnabled
                ? relationalQueryContext.ConcurrencyDetector.EnterCriticalSection()
                : default(ConcurrencyDetectorCriticalSectionDisposer?);

            return relationalQueryContext.ExecutionStrategy.Execute(
                (relationalQueryContext, relationalCommandResolver, commandSource),
                static (_, state) =>
                {
                    EntityFrameworkMetricsData.ReportQueryExecuting();

                    var relationalCommand = state.relationalCommandResolver.RentAndPopulateRelationalCommand(state.relationalQueryContext);

                    return relationalCommand.ExecuteNonQuery(
                        new RelationalCommandParameterObject(
                            state.relationalQueryContext.Connection,
                            state.relationalQueryContext.Parameters,
                            null,
                            state.relationalQueryContext.Context,
                            state.relationalQueryContext.CommandLogger,
                            state.commandSource));
                },
                null);
        }
        catch (Exception exception)
        {
            HandleNonQueryException(relationalQueryContext, contextType, commandSource, exception);
            throw;
        }
    }

    private static Task<int> ExecuteNonQueryAsync(
        RelationalQueryContext relationalQueryContext,
        RelationalCommandResolver relationalCommandResolver,
        Type contextType,
        CommandSource commandSource,
        bool threadSafetyChecksEnabled)
    {
        try
        {
            using var _ = threadSafetyChecksEnabled
                ? relationalQueryContext.ConcurrencyDetector.EnterCriticalSection()
                : default(ConcurrencyDetectorCriticalSectionDisposer?);

            return relationalQueryContext.ExecutionStrategy.ExecuteAsync(
                (relationalQueryContext, relationalCommandResolver, commandSource),
                static (_, state, cancellationToken) =>
                {
                    EntityFrameworkMetricsData.ReportQueryExecuting();

                    var relationalCommand = state.relationalCommandResolver.RentAndPopulateRelationalCommand(state.relationalQueryContext);

                    return relationalCommand.ExecuteNonQueryAsync(
                        new RelationalCommandParameterObject(
                            state.relationalQueryContext.Connection,
                            state.relationalQueryContext.Parameters,
                            null,
                            state.relationalQueryContext.Context,
                            state.relationalQueryContext.CommandLogger,
                            state.commandSource),
                        cancellationToken);
                },
                null,
                relationalQueryContext.CancellationToken);
        }
        catch (Exception exception)
        {
            HandleNonQueryException(relationalQueryContext, contextType, commandSource, exception);
            throw;
        }
    }

    private static void HandleNonQueryException(
        RelationalQueryContext relationalQueryContext,
        Type contextType,
        CommandSource commandSource,
        Exception exception)
    {
        if (relationalQueryContext.ExceptionDetector.IsCancellation(exception))
        {
            relationalQueryContext.QueryLogger.QueryCanceled(contextType);
        }
        else
        {
            switch (commandSource)
            {
                case CommandSource.ExecuteDelete:
                    relationalQueryContext.QueryLogger.ExecuteDeleteFailed(contextType, exception);
                    break;

                case CommandSource.ExecuteUpdate:
                    relationalQueryContext.QueryLogger.ExecuteUpdateFailed(contextType, exception);
                    break;

                default:
                    relationalQueryContext.QueryLogger.NonQueryOperationFailed(contextType, exception);
                    break;
            }
        }
    }

    #endregion Non-query execution

    private RelationalCommandCache CreateCommandCache(
        RelationalQueryCompilationContext queryCompilationContext,
        Expression queryExpression)
    {
        var useRelationalNulls = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).UseRelationalNulls;
        var collectionParameterTranslationMode
            = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).ParameterizedCollectionMode;
        var relationalDependencies = queryCompilationContext.RelationalDependencies;

        return new RelationalCommandCache(
            relationalDependencies.MemoryCache,
            relationalDependencies.QuerySqlGeneratorFactory,
            relationalDependencies.RelationalParameterBasedSqlProcessorFactory,
            queryExpression,
            useRelationalNulls,
            collectionParameterTranslationMode);
    }
}
