// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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
///     for supported query shapes, producing a tree of <see cref="RelationalStructuralTypeMaterializer{TEntity}" />
///     instances wired together with include information.
/// </remarks>
public partial class RelationalMaterializerFactory(
    ICoreSingletonOptions coreSingletonOptions,
    IEnumerable<ISingletonInterceptor> singletonInterceptors)
{
    private static readonly MethodInfo ReadTypedValueMethod
        = typeof(RelationalMaterializerFactory).GetTypeInfo().GetDeclaredMethod(nameof(ReadTypedValue))!;

    private static readonly MethodInfo CreateGroupByEnumerableMaterializerMethod
        = typeof(RelationalMaterializerFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateGroupByEnumerableMaterializer))!;

    private readonly bool _detailedErrorsEnabled = coreSingletonOptions.AreDetailedErrorsEnabled;
    private readonly bool _threadSafetyChecksEnabled = coreSingletonOptions.AreThreadSafetyChecksEnabled;
    private readonly IInstantiationBindingInterceptor[] _bindingInterceptors =
        singletonInterceptors.OfType<IInstantiationBindingInterceptor>().ToArray();
    private readonly IMaterializationInterceptor? _materializationInterceptor =
        (IMaterializationInterceptor?)new MaterializationInterceptorAggregator().AggregateInterceptors(
            singletonInterceptors.OfType<IMaterializationInterceptor>().ToList());

    /// <summary>
    ///     Builds a non-generated query executor for an enumerable query where <typeparamref name="TElement" />
    ///     is the element type directly.
    /// </summary>
    public Func<QueryContext, IEnumerable<TElement>> CreateEnumerableMaterializer<TElement>(
        RelationalQueryCompilationContext queryCompilationContext,
        ShapedQueryExpression shapedQueryExpression)
    {
        var (select, shaper) = ((SelectExpression)shapedQueryExpression.QueryExpression, shapedQueryExpression.ShaperExpression);

        // For NoTrackingWithIdentityResolution, validate that JSON entity projections are in a safe
        // order. This mirrors the generated shaper's JsonCorrectOrderOfEntitiesForChangeTrackerValidator.
        // TODO: Probably move this out to ShaperValidator as a relational-specific check? Or it might make sense as a universal one.
        if (queryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution)
        {
            new RelationalShapedQueryCompilingExpressionVisitor.ShaperProcessingExpressionVisitor
                .JsonCorrectOrderOfEntitiesForChangeTrackerValidator(select).Validate(shaper);
        }

        return StripIgnorableConvert(shaper) switch
        {
            RelationalGroupByResultExpression groupByResultExpression
                => CreateFinalGroupByEnumerableMaterializer<TElement>(queryCompilationContext, select, groupByResultExpression),

            _ when queryCompilationContext.QuerySplittingBehavior == QuerySplittingBehavior.SplitQuery
                => CreateSplitQueryEnumerableMaterializer<TElement>(queryCompilationContext, select, shaper),

            _ when select.IsNonComposedFromSql()
                => CreateFromSqlEnumerableMaterializer<TElement>(queryCompilationContext, select, shaper),

            _ => CreateSingleQueryEnumerableMaterializer<TElement>(queryCompilationContext, select, shaper)
        };
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

        var rowMaterializer = BuildMaterializer<TElement>(
            shaper, select, isTracking, queryCompilationContext.QueryTrackingBehavior, ref nextCollectionId);

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
    ///     Builds a non-generated query executor for a non-composed FromSql query.
    ///     FromSql queries need runtime column-index remapping because the user's SQL may return
    ///     columns in any order. This mirrors the generated shaper's <c>FromSqlQueryingEnumerable</c> path.
    /// </summary>
    private Func<QueryContext, IEnumerable<TElement>> CreateFromSqlEnumerableMaterializer<TElement>(
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

        var rowMaterializer = BuildMaterializer<TElement>(
            shaper, select, isTracking, queryCompilationContext.QueryTrackingBehavior, ref nextCollectionId);

        var columnNames = select.Projection.Select(pe => ((ColumnExpression)pe.Expression).Name).ToArray();

        return qc => new FromSqlQueryingEnumerable<TElement>(
            (RelationalQueryContext)qc,
            relationalCommandResolver: parameters => relationalCommandCache.GetRelationalCommandTemplate(parameters),
            readerColumns: null,
            columnNames: columnNames,
            materializer: (queryContext, dbDataReader, indexMap) =>
            {
                // Wrap the DbDataReader to remap column ordinals using the FromSql index map
                var remappedReader = new IndexRemappingDbDataReader(dbDataReader, indexMap);
                return rowMaterializer!(queryContext, remappedReader, new ResultContext(), null!)!;
            },
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
        QueryTrackingBehavior? queryTrackingBehavior,
        ref int nextCollectionId,
        List<SplitCollectionIncludeInfo>? splitCollectionInfos = null,
        Type? resultType = null)
    {
        resultType ??= typeof(T);
        Check.DebugAssert(
            typeof(T).IsAssignableFrom(resultType),
            $"Materializer result type '{resultType.DisplayName()}' must be assignable to '{typeof(T).DisplayName()}'.");

        shaperExpression = StripIgnorableConvert(shaperExpression);

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
                        : (qc, reader, rc, coord) => ReadScalarValue<T>(reader, columnIndex, typedReader);
                }

                // Boxed path: scalar within e.g. an anonymous type projection (Select(b => new { b.Age })).
                // T is object, so boxing is unavoidable, but the typed reader still applies the type mapping
                // and any value converter correctly.
                return nullable
                    ? (qc, reader, rc, coord) => reader.IsDBNull(columnIndex) ? default : (T?)typedReader.Read<object>(reader)
                    : (qc, reader, rc, coord) => (T?)(object?)ReadScalarValue<object>(reader, columnIndex, typedReader);
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
                    shaperExpression, select, isTracking, queryTrackingBehavior, splitCollectionInfos, ref nextCollectionId);

                if (typeof(T) != typeof(object))
                {
                    // Cast/convert wrappers can make TResult differ from the structural shaper CLR type
                    // (e.g. Cast<Derived>() over a base structural shaper). In that case, use the boxed
                    // Materialize path and let the runtime cast enforce query semantics.
                    if (entityMaterializer.StructuralType.ClrType != typeof(T))
                    {
                        return (queryCtx, reader, rc, coord) => (T?)entityMaterializer.Materialize(queryCtx, reader, rc, coord);
                    }

                    // For nullable value type projections (e.g. Select(x => x.OptionalComplexProp) where the
                    // complex type is a struct), T is Nullable<TStruct> but the materializer's internal delegate
                    // returns TStruct (because TStructuralType? without a struct constraint doesn't produce
                    // Nullable<T>). The delegate types are incompatible, so we go through the boxed Materialize
                    // path which correctly handles the null check.
                    return Nullable.GetUnderlyingType(typeof(T)) is not null
                        ? ((queryCtx, reader, rc, coord) => (T?)entityMaterializer.Materialize(queryCtx, reader, rc, coord))
                        : entityMaterializer.GetTypedMaterializeDelegate<T>()!;
                }

                return (queryCtx, reader, rc, coord) => (T?)entityMaterializer.Materialize(queryCtx, reader, rc, coord);
            }

            // Single-query collection projection (e.g. Select(x => x.Posts) via correlated subquery).
            // Uses the multi-call protocol: InitializeCollection on first call, PopulateCollection on
            // every call, return collection when ResultReady.
            case RelationalCollectionShaperExpression collectionShaper:
                return BuildStandaloneCollectionMaterializer<T>(
                    collectionShaper, select, isTracking, queryTrackingBehavior,
                    ref nextCollectionId, static collection => collection);

            // Split-query collection projection (e.g. Select(c => new { Orders = c.Orders.ToList() })
            // with AsSplitQuery). The collection is loaded via a separate SQL query; we register it
            // with splitCollectionInfos and return an empty collection that gets populated in-place
            // by relatedDataLoaders.
            case RelationalSplitCollectionShaperExpression splitCollectionShaper
                when splitCollectionInfos is not null:
            {
                var collectionId = nextCollectionId++;

                var childSplitCollections = new List<SplitCollectionIncludeInfo>();
                var childMaterializer = BuildMaterializer<object>(
                    splitCollectionShaper.InnerShaper, splitCollectionShaper.SelectExpression,
                    isTracking, queryTrackingBehavior, ref nextCollectionId, childSplitCollections);

                var parentIdentifier = CompileIdentifierLambda(
                    splitCollectionShaper.ParentIdentifier, select);
                var childIdentifier = CompileIdentifierLambda(
                    splitCollectionShaper.ChildIdentifier, splitCollectionShaper.SelectExpression);
                var identifierComparers = splitCollectionShaper.IdentifierValueComparers
                    .Select<ValueComparer, Func<object, object, bool>>(vc => (a, b) => vc.Equals(a, b))
                    .ToArray();

                var navigation = splitCollectionShaper.Navigation;
                var collectionAccessor = navigation?.GetCollectionAccessor();
                var collectionAdd = CreateCollectionAddDelegate(splitCollectionShaper.ElementType);

                splitCollectionInfos.Add(new SplitCollectionIncludeInfo(
                    childMaterializer,
                    navigation: null, // standalone — not an include
                    inverseNavigation: null,
                    inverseNavigationSetter: null,
                    inverseNavigationCollectionAccessor: null,
                    collectionAccessor: collectionAccessor,
                    collectionAdd,
                    parentIdentifier,
                    childIdentifier,
                    identifierComparers,
                    collectionId,
                    splitCollectionShaper.SelectExpression,
                    childSplitCollections,
                    setLoaded: false,
                    parentEntityProvider: null));

                // Create and return the empty collection. It will be populated in-place by
                // relatedDataLoaders (PopulateSplitIncludeCollection) before the result is used.
                // ParentEntityProvider will be set up by BuildSplitEntityMaterializer to provide
                // this collection to InitializeSplitIncludeCollection for coordinator registration.
                var collectionType = splitCollectionShaper.Type;
                return (queryCtx, reader, rc, coord)
                    => (T?)(collectionAccessor?.Create() ?? Activator.CreateInstance(collectionType));
            }

            // Value type default construction (e.g. new DateTime()). Constructor is null for
            // parameterless value type NewExpressions; the result is always default(T).
            case NewExpression { Constructor: null } newExpression:
            {
                var type = newExpression.Type;
                return (_, _, _, _) => (T?)Activator.CreateInstance(type);
            }

            // Anonymous/named type projection (e.g. Select(x => new { x.Id, x.Name })).
            // Each constructor argument is materialized recursively.
            case NewExpression newExpression:
            {
                // TODO: For JIT mode, probably better to fall through to the default case, to just compile the expression tree.
                // But probably good to leave the optimization here for NativeAOT.
                var invoker = ConstructorInvoker.Create(newExpression.Constructor!);
                var parameterTypes = newExpression.Constructor.GetParameters()
                    .Select(p => p.ParameterType)
                    .ToArray();
                var subMaterializers = new Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, object?>[newExpression.Arguments.Count];
                var splitCollectionStartIndexes = splitCollectionInfos is null ? null : new int[subMaterializers.Length];
                object[]? currentValues = null;
                for (var i = 0; i < subMaterializers.Length; i++)
                {
                    if (splitCollectionStartIndexes is not null)
                    {
                        splitCollectionStartIndexes[i] = splitCollectionInfos!.Count;
                    }

                    subMaterializers[i] = BuildMaterializer<object>(
                        newExpression.Arguments[i], select, isTracking, queryTrackingBehavior,
                        ref nextCollectionId, splitCollectionInfos, parameterTypes[i]);
                }

                if (splitCollectionStartIndexes is not null)
                {
                    var splitCollectionInfosLocal = splitCollectionInfos!;

                    for (var i = 0; i < subMaterializers.Length; i++)
                    {
                        var subMaterializerIndex = i;
                        var startIndex = splitCollectionStartIndexes[i];
                        var endIndex = i + 1 < subMaterializers.Length ? splitCollectionStartIndexes[i + 1] : splitCollectionInfosLocal.Count;

                        for (var j = startIndex; j < endIndex; j++)
                        {
                            splitCollectionInfosLocal[j].ParentEntityProvider ??= () => currentValues?[subMaterializerIndex];
                        }
                    }
                }

                var composedMaterializer = ComposeWithMultiCallProtocol<T>(
                    subMaterializers,
                    (_, _, values) =>
                    {
                        var invokerArgs = new object?[values.Length];
                        values.CopyTo(invokerArgs, 0);

                        return (T?)invoker.Invoke(invokerArgs.AsSpan());
                    });

                return (queryCtx, reader, rc, coord) =>
                {
                    var result = composedMaterializer(queryCtx, reader, rc, coord);
                    currentValues = rc.Values;

                    return result;
                };
            }

            // Common collection terminal operators (ToList, ToArray...) over correlated collection projections.
            // Handling these directly keeps NativeAOT on the handwritten materializer path instead of falling back to interpreted client
            // evaluation.
            case MethodCallExpression methodCallExpression
                when TryGetCollectionTerminal(methodCallExpression, out var terminalCollectionShaper, out var collectionResultFinalizer):
            {
                return BuildStandaloneCollectionMaterializer<T>(
                    terminalCollectionShaper, select, isTracking, queryTrackingBehavior,
                    ref nextCollectionId, collectionResultFinalizer);
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
                var splitCollectionStartIndexes = splitCollectionInfos is null ? null : new int[subMaterializers.Length];
                object[]? currentValues = null;
                for (var i = 0; i < subMaterializers.Length; i++)
                {
                    if (splitCollectionStartIndexes is not null)
                    {
                        splitCollectionStartIndexes[i] = splitCollectionInfos!.Count;
                    }

                    subMaterializers[i] = BuildMaterializer<object>(
                        rewriter.ExtractedSubExpressions[i], select, isTracking, queryTrackingBehavior,
                        ref nextCollectionId, splitCollectionInfos);
                }

                if (splitCollectionStartIndexes is not null)
                {
                    var splitCollectionInfosLocal = splitCollectionInfos!;

                    for (var i = 0; i < subMaterializers.Length; i++)
                    {
                        var subMaterializerIndex = i;
                        var startIndex = splitCollectionStartIndexes[i];
                        var endIndex = i + 1 < subMaterializers.Length ? splitCollectionStartIndexes[i + 1] : splitCollectionInfosLocal.Count;

                        for (var j = startIndex; j < endIndex; j++)
                        {
                            splitCollectionInfosLocal[j].ParentEntityProvider ??= () => currentValues?[subMaterializerIndex];
                        }
                    }
                }

                var projectionFunc = Lambda<Func<QueryContext, DbDataReader, object[], T?>>(
                    rewritten, queryContextParam, dataReaderParam, entityValuesParam).Compile();

                if (subMaterializers.Length == 0)
                {
                    return (queryCtx, reader, rc, coord) => projectionFunc(queryCtx, reader, []);
                }

                var composedMaterializer = ComposeWithMultiCallProtocol<T>(subMaterializers, (queryCtx, reader, values)
                    => projectionFunc(queryCtx, reader, values));

                return (queryCtx, reader, rc, coord) =>
                {
                    var result = composedMaterializer(queryCtx, reader, rc, coord);
                    currentValues = rc.Values;

                    return result;
                };
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
        return (queryCtx, reader, rc, coord) =>
        {
            var subContexts = rc.GetNestedResultContexts(subMaterializers.Length);

            if (rc.Values is null)
            {
                for (var i = 0; i < subContexts.Length; i++)
                {
                    subContexts[i].Values = null;
                }

                rc.Values = new object[subMaterializers.Length];
                var anyNotReady = false;
                for (var i = 0; i < subMaterializers.Length; i++)
                {
                    // Reset ResultReady before each sub-materializer so we can detect whether
                    // THIS sub-materializer changed it to false (collection population mode).
                    coord.RowState.MarkResultReady();
                    var result = subMaterializers[i](queryCtx, reader, subContexts[i], coord);

                    if (result is not null)
                    {
                        rc.Values[i] = result;
                    }
                    else if (!coord.ResultReady)
                    {
                        // Entity materializer set ResultReady=false → collection population in
                        // progress, entity is cached in subContexts[i].Values[0].
                        rc.Values[i] = subContexts[i].Values?[0]!;
                    }

                    // else: ResultReady is still true — null is a legitimate result (e.g. a
                    // conditional expression evaluating to null). Do NOT fall back to
                    // subContexts[i].Values, which may contain unrelated cached state.

                    anyNotReady |= !coord.ResultReady;
                }

                if (anyNotReady)
                {
                    coord.RowState.MarkResultPending();
                }
                else
                {
                    coord.RowState.MarkResultReady();
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
                        var result = subMaterializers[i](queryCtx, reader, subContexts[i], coord);
                        if (coord.ResultReady)
                        {
                            rc.Values[i] = result!;
                        }
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
        ///     each entry and wire them via <c>ComposeWithMultiCallProtocol</c>.
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
        QueryTrackingBehavior? queryTrackingBehavior,
        ref int nextCollectionId,
        Func<object, object?> resultFinalizer)
    {
        var collectionId = nextCollectionId++;

        var innerElementMaterializer = BuildMaterializer<object>(
            collectionShaper.InnerShaper, select, isTracking, queryTrackingBehavior, ref nextCollectionId);

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

        var collectionAdd = CreateCollectionAddDelegate(collectionShaper.ElementType);

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

            return resultCoordinator.ResultReady ? (T?)resultFinalizer(resultContext.Values[0]) : default;
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

        if (resultCoordinator.RowState.IsCurrentResultReaderExhausted)
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
                resultCoordinator.RowState.MarkRowForNextResult();
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

                resultCoordinator.RowState.MarkResultPending();
                return;
            }

            GenerateCurrentElementIfPending();
            resultCoordinator.RowState.MarkCurrentRowConsumed();
            collectionContext.UpdateSelfIdentifier(innerKey);
        }
        else
        {
            collectionContext.UpdateSelfIdentifier(innerKey);
        }

        ProcessCurrentElementRow();
        resultCoordinator.RowState.MarkResultPending();

        void ProcessCurrentElementRow()
        {
            var previousResultReady = resultCoordinator.ResultReady;
            resultCoordinator.RowState.MarkResultReady();

            var element = innerShaper(
                queryContext, dataReader, collectionContext.ResultContext, resultCoordinator);

            if (resultCoordinator.ResultReady)
            {
                collectionContext.ResultContext.Values = null;
                collectionAdd(collectionContext.Collection!, element);
            }

            if (!previousResultReady)
            {
                resultCoordinator.RowState.MarkResultPending();
            }
        }

        void GenerateCurrentElementIfPending()
        {
            if (collectionContext.ResultContext.Values is not null)
            {
                resultCoordinator.RowState.MarkNoMoreRowsForCurrentResult();
                ProcessCurrentElementRow();
            }

            collectionContext.UpdateSelfIdentifier(null);
        }
    }

    /// <summary>
    ///     Recursively builds a <see cref="RelationalStructuralTypeMaterializer{TEntity}" /> from a shaper expression.
    ///     When <paramref name="splitCollectionInfos" /> is non-null, operates in split-query mode:
    ///     <see cref="RelationalSplitCollectionShaperExpression" /> nodes are collected into the list
    ///     instead of being added as <see cref="CollectionIncludeInfo" /> on the entity materializer.
    /// </summary>
    private RelationalStructuralTypeMaterializer BuildMaterializerFromShaper(
        Expression shaperExpression,
        SelectExpression selectExpression,
        bool isTracking,
        QueryTrackingBehavior? queryTrackingBehavior,
        List<SplitCollectionIncludeInfo>? splitCollectionInfos,
        ref int nextCollectionId)
    {
        shaperExpression = StripIgnorableConvert(shaperExpression);

        switch (shaperExpression)
        {
            case RelationalStructuralTypeShaperExpression
            {
                ValueBufferExpression: ProjectionBindingExpression projectionBinding
            } shaper:
            {
                return BuildStructuralTypeMaterializer(
                    shaper.StructuralType, projectionBinding, selectExpression, isTracking, shaper.IsNullable, queryTrackingBehavior);
            }

            case IncludeExpression includeExpression:
            {
                // Recurse into the entity expression to build the inner materializer (which may have further includes)
                var innerMaterializer = BuildMaterializerFromShaper(
                    includeExpression.EntityExpression, selectExpression, isTracking, queryTrackingBehavior,
                    splitCollectionInfos, ref nextCollectionId);

                var navigation = includeExpression.Navigation;
                var inverseNavigation = navigation.Inverse;
                IClrPropertySetter? inverseNavSetter = null;
                IClrCollectionAccessor? inverseNavCollectionAccessor = null;
                if (inverseNavigation is { IsCollection: false })
                {
                    inverseNavSetter = ((IRuntimePropertyBase)inverseNavigation).MaterializationSetter;
                }
                else if (inverseNavigation is { IsCollection: true })
                {
                    inverseNavCollectionAccessor = inverseNavigation.GetCollectionAccessor();
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
                            includeExpression.NavigationExpression, selectExpression, isTracking, queryTrackingBehavior,
                            splitCollectionInfos, ref nextCollectionId);

                        var navSetter = ((IRuntimePropertyBase)navigation).MaterializationSetter;

                        var refInclude = new ReferenceIncludeInfo(
                            includedMaterializer,
                            navigation,
                            navSetter,
                            inverseNavigation,
                            inverseNavSetter,
                            inverseNavCollectionAccessor,
                            isKeylessEntityType: navigation.DeclaringEntityType.FindPrimaryKey() is null);
                        innerMaterializer.AddReferenceInclude(refInclude);

                        if (splitCollectionInfos is not null)
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
                            queryTrackingBehavior, splitCollectionInfos: null, ref nextCollectionId);

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
                            inverseNavCollectionAccessor,
                            collectionAccessor,
                            parentIdentifier,
                            outerIdentifier,
                            selfIdentifier,
                            parentComparers,
                            outerComparers,
                            selfComparers,
                            collectionId,
                            isKeylessEntityType: navigation.DeclaringEntityType.FindPrimaryKey() is null,
                            includeExpression.SetLoaded));

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
                            isTracking, queryTrackingBehavior, childSplitCollections, ref nextCollectionId);

                        var parentIdentifier = CompileIdentifierLambda(
                            splitCollectionShaper.ParentIdentifier, selectExpression);
                        var childIdentifier = CompileIdentifierLambda(
                            splitCollectionShaper.ChildIdentifier, splitCollectionShaper.SelectExpression);
                        var identifierComparers = splitCollectionShaper.IdentifierValueComparers
                            .Select<ValueComparer, Func<object, object, bool>>(vc => (a, b) => vc.Equals(a, b))
                            .ToArray();

                        var collectionAccessor = navigation.GetCollectionAccessor()!;

                        splitCollectionInfos.Add(new SplitCollectionIncludeInfo(
                            childMaterializer.Materialize,
                            navigation,
                            inverseNavigation,
                            inverseNavSetter,
                            inverseNavCollectionAccessor,
                            collectionAccessor,
                            collectionAdd: null,
                            parentIdentifier,
                            childIdentifier,
                            identifierComparers,
                            collectionId,
                            splitCollectionShaper.SelectExpression,
                            childSplitCollections,
                            includeExpression.SetLoaded,
                            childMaterializer,
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
                                targetEntityType, isTracking, nullable: true, _materializationInterceptor, queryTrackingBehavior);

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

            // FetchJoinEntity(joinShaper, targetShaper) — used for tracking many-to-many skip
            // navigation includes. Inserted by NavigationExpandingExpressionVisitor into the Join
            // result selector to ensure the join entity (e.g. PostTag) is materialized alongside
            // the target entity (e.g. Tag). The method itself is an identity function that returns
            // only the target, but both entity shapers survive in the expression tree so the
            // generated shaper materializes both via recursive VisitExtension. We replicate that
            // by building materializers for both and storing the join materializer on the target
            // so it gets invoked during MaterializeEntity for tracking side effects.

            // TODO: Rather than a wrapper MethodCallExpression, this should probably just be something we add on the shaper
            // expression itself.
            case MethodCallExpression
            {
                Method.Name: nameof(NavigationExpandingExpressionVisitor.FetchJoinEntity),
                Arguments: [var joinShaper, var targetShaper]
            }:
            {
                // Build the target entity materializer (this is what gets returned)
                var targetMaterializer = BuildMaterializerFromShaper(
                    targetShaper, selectExpression, isTracking, queryTrackingBehavior,
                    splitCollectionInfos, ref nextCollectionId);

                // Build and store the join entity materializer so it gets materialized for tracking.
                targetMaterializer.JoinEntityMaterializer = BuildMaterializerFromShaper(
                    joinShaper, selectExpression, isTracking, queryTrackingBehavior,
                    splitCollectionInfos, ref nextCollectionId);

                return targetMaterializer;
            }

            default:
                throw new NotImplementedException(
                    $"The non-generated materializer does not yet support shaper expression type '{shaperExpression.GetType().Name}': {shaperExpression}.");
        }
    }

    private static Expression StripIgnorableConvert(Expression expression)
    {
        while (expression is UnaryExpression
               {
                   NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked
               } unaryExpression
               && CanIgnoreConvertWrapper(unaryExpression))
        {
            expression = unaryExpression.Operand;
        }

        return expression;

        static bool CanIgnoreConvertWrapper(UnaryExpression unaryExpression)
        {
            // Ignore only built-in reference/identity casts used for expression typing; these
            // wrappers don't carry additional materialization semantics.
            if (unaryExpression.Method is not null)
            {
                return false;
            }

            var operandType = unaryExpression.Operand.Type;
            var targetType = unaryExpression.Type;

            return !operandType.IsValueType
                && !targetType.IsValueType
                && (targetType.IsAssignableFrom(operandType) || operandType.IsAssignableFrom(targetType));
        }
    }

    private RelationalStructuralTypeMaterializer BuildStructuralTypeMaterializer(
        ITypeBase structuralType,
        ProjectionBindingExpression projectionBinding,
        SelectExpression selectExpression,
        bool isTracking,
        bool isNullable,
        QueryTrackingBehavior? queryTrackingBehavior)
    {
        var projectionIndex = selectExpression.GetProjection(projectionBinding).GetConstantValue<object>();
        if (projectionIndex is not IDictionary<IPropertyBase, int> propertyIndexMap)
        {
            throw new NotImplementedException(
                $"The non-generated materializer does not support projection index type '{projectionIndex?.GetType().Name}' for type '{structuralType.DisplayName()}'.");
        }

        var materializerType = typeof(RelationalStructuralTypeMaterializer<>).MakeGenericType(structuralType.ClrType);
        return (RelationalStructuralTypeMaterializer)Activator.CreateInstance(
            materializerType, structuralType, propertyIndexMap, isTracking, isNullable,
            queryTrackingBehavior, _bindingInterceptors, _materializationInterceptor)!;
    }

    private static Action<object, object?> CreateCollectionAddDelegate(Type elementType)
    {
        var addMethod = typeof(ICollection<>).MakeGenericType(elementType)
            .GetMethod(nameof(ICollection<object>.Add))!;

        return (collection, element) => addMethod.Invoke(collection, [element]);
    }

    private static bool TryGetCollectionTerminal(
        MethodCallExpression methodCallExpression,
        [NotNullWhen(true)] out RelationalCollectionShaperExpression? collectionShaper,
        [NotNullWhen(true)] out Func<object, object?>? resultFinalizer)
    {
        collectionShaper = null;
        resultFinalizer = null;

        if (methodCallExpression.Method.DeclaringType != typeof(Enumerable)
            || !methodCallExpression.Method.IsGenericMethod
            || methodCallExpression.Arguments is not [var source]
            || StripIgnorableConvert(source) is not RelationalCollectionShaperExpression relationalCollectionShaper)
        {
            return false;
        }

        collectionShaper = relationalCollectionShaper;

        switch (methodCallExpression.Method.Name)
        {
            case nameof(Enumerable.ToArray):
            {
                var terminalMethod = methodCallExpression.Method;
                resultFinalizer = collection => terminalMethod.Invoke(null, [collection]);
                return true;
            }

            case nameof(Enumerable.ToList):
            {
                var resultType = methodCallExpression.Type;
                resultFinalizer = collection =>
                {
                    Check.DebugAssert(
                        resultType.IsInstanceOfType(collection),
                        $"Collection accumulator type '{collection.GetType().DisplayName()}' must match ToList result type '{resultType.DisplayName()}'.");

                    return collection;
                };

                return true;
            }

            case nameof(Enumerable.ToHashSet) when methodCallExpression.Arguments.Count == 1:
            {
                var terminalMethod = methodCallExpression.Method;
                resultFinalizer = collection => terminalMethod.Invoke(null, [collection]);
                return true;
            }

            default:
                return false;
        }
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
                var projectionExpression = SelectExpression.Projection[columnIndex].Expression;

                return CreateReaderGetValue(
                    columnIndex,
                    pbe.Type,
                    projectionExpression,
                    projectionExpression is not ColumnExpression columnExpression || columnExpression.IsNullable);
            }

            return base.VisitExtension(node);
        }

        private Expression CreateReaderGetValue(
            int columnIndex,
            Type targetType,
            SqlExpression projectionExpression,
            bool nullable)
        {
            var indexExpr = Constant(columnIndex);
            var typeMapping = (RelationalTypeMapping)projectionExpression.TypeMapping!;
            Check.DebugAssert(
                typeMapping is not null,
                $"Projection expression '{projectionExpression}' must have a relational type mapping.");

            var typedReader = typeMapping.CreateReader(columnIndex);
            Expression valueExpression = Call(
                ReadTypedValueMethod,
                DataReaderParam,
                Constant(typedReader, typeof(ITypedValueReader<DbDataReader>)));

            if (valueExpression.Type != targetType.UnwrapNullableType())
            {
                valueExpression = Convert(valueExpression, targetType.UnwrapNullableType());
            }

            if (nullable)
            {
                var nullValue = targetType.IsValueType && Nullable.GetUnderlyingType(targetType) is null
                    ? Throw(
                        New(
                            typeof(InvalidOperationException).GetConstructor([typeof(string)])!,
                            Constant("Nullable object must have a value.")),
                        targetType)
                    : (Expression)Default(targetType);

                if (valueExpression.Type != targetType)
                {
                    valueExpression = Convert(valueExpression, targetType);
                }

                return Condition(
                    Call(DataReaderParam, IsDbNullMethod, indexExpr),
                    nullValue,
                    valueExpression);
            }

            return valueExpression;
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

    /// <summary>
    ///     Reads a scalar value from the <see cref="DbDataReader" />, wrapping any exception in a
    ///     friendly error message. Mirrors the generated shaper's try/catch around column reads.
    /// </summary>
    private static T ReadScalarValue<T>(DbDataReader reader, int columnIndex, ITypedValueReader<DbDataReader> typedReader)
    {
        try
        {
            return typedReader.Read<T>(reader);
        }
        catch (Exception e)
        {
            var value = reader.GetFieldValue<object>(columnIndex);
            var expectedType = typeof(T);
            var actualType = value?.GetType();

            var message = e is NullReferenceException || Equals(value, DBNull.Value)
                ? RelationalStrings.ErrorMaterializingValueNullReference(expectedType)
                : e is InvalidCastException
                    ? RelationalStrings.ErrorMaterializingValueInvalidCast(expectedType, actualType)
                    : RelationalStrings.ErrorMaterializingValue;

            throw new InvalidOperationException(message, e);
        }
    }

    private static object? ReadTypedValue(DbDataReader reader, ITypedValueReader<DbDataReader> typedReader)
        => typedReader.Read<object>(reader);

    private Func<QueryContext, IEnumerable<TElement>> CreateFinalGroupByEnumerableMaterializer<TElement>(
        RelationalQueryCompilationContext queryCompilationContext,
        SelectExpression select,
        RelationalGroupByResultExpression groupByResultExpression)
    {
        // Final GroupBy executes through GroupBySingleQueryingEnumerable<TKey, TElement> or
        // GroupBySplitQueryingEnumerable<TKey, TElement>, both of which require the key and element types as
        // generic arguments. This method only has the grouping type statically, so extracting those arguments
        // requires runtime generic construction, which NativeAOT does not support.
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            throw new NotSupportedException(RelationalStrings.FinalGroupByNotSupportedInNativeAot);
        }

        var groupingType = groupByResultExpression.Type;
        Check.DebugAssert(
            groupingType.IsGenericType && groupingType.GetGenericTypeDefinition() == typeof(IGrouping<,>),
            $"Final GroupBy result type '{groupingType.DisplayName()}' must be an IGrouping type.");

        var groupingTypeArguments = groupingType.GetGenericArguments();

        return (Func<QueryContext, IEnumerable<TElement>>)CreateGroupByEnumerableMaterializerMethod
            .MakeGenericMethod(typeof(TElement), groupingTypeArguments[0], groupingTypeArguments[1])
            .Invoke(this, [queryCompilationContext, select, groupByResultExpression])!;
    }

    private Func<QueryContext, IEnumerable<TGrouping>> CreateGroupByEnumerableMaterializer<TGrouping, TKey, TElement>(
        RelationalQueryCompilationContext queryCompilationContext,
        SelectExpression select,
        RelationalGroupByResultExpression groupByResultExpression)
    {
        var relationalCommandCache = CreateCommandCache(queryCompilationContext, select);
        var isTracking = queryCompilationContext.QueryTrackingBehavior is QueryTrackingBehavior.TrackAll;
        var nextCollectionId = 0;

        var keyMaterializer = BuildMaterializer<TKey>(
            groupByResultExpression.KeyShaper, select, isTracking,
            queryCompilationContext.QueryTrackingBehavior, ref nextCollectionId);
        var keyIdentifier = CompileIdentifierLambda(groupByResultExpression.KeyIdentifier, select);
        var keyIdentifierValueComparers = groupByResultExpression.KeyIdentifierValueComparers
            .Select<ValueComparer, Func<object, object, bool>>(vc => (a, b) => vc.Equals(a, b))
            .ToArray();

        TKey KeySelector(QueryContext queryContext, DbDataReader dataReader)
            => keyMaterializer(queryContext, dataReader, new ResultContext(), new SingleQueryResultCoordinator())!;

        if (queryCompilationContext.QuerySplittingBehavior == QuerySplittingBehavior.SplitQuery)
        {
            var splitShaper = BuildSplitQueryShaper<TElement>(
                queryCompilationContext,
                select,
                groupByResultExpression.ElementShaper,
                isTracking,
                out var relatedDataLoaders,
                out var relatedDataLoadersAsync);

            return qc => (IEnumerable<TGrouping>)(object)new GroupBySplitQueryingEnumerable<TKey, TElement>(
                (RelationalQueryContext)qc,
                relationalCommandResolver: parameters => relationalCommandCache.GetRelationalCommandTemplate(parameters),
                readerColumns: null,
                keySelector: KeySelector,
                keyIdentifier: keyIdentifier,
                keyIdentifierValueComparers: keyIdentifierValueComparers,
                elementSelector: (queryContext, dataReader, resultContext, resultCoordinator)
                    => splitShaper(queryContext, dataReader, resultContext, resultCoordinator)!,
                relatedDataLoaders: relatedDataLoaders,
                relatedDataLoadersAsync: relatedDataLoadersAsync,
                contextType: queryCompilationContext.ContextType,
                standAloneStateManager: queryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution,
                detailedErrorsEnabled: _detailedErrorsEnabled,
                threadSafetyChecksEnabled: _threadSafetyChecksEnabled);
        }

        var elementMaterializer = BuildMaterializer<TElement>(
            groupByResultExpression.ElementShaper, select, isTracking,
            queryCompilationContext.QueryTrackingBehavior, ref nextCollectionId);

        return qc => (IEnumerable<TGrouping>)(object)new GroupBySingleQueryingEnumerable<TKey, TElement>(
            (RelationalQueryContext)qc,
            relationalCommandResolver: parameters => relationalCommandCache.GetRelationalCommandTemplate(parameters),
            readerColumns: null,
            keySelector: KeySelector,
            keyIdentifier: keyIdentifier,
            keyIdentifierValueComparers: keyIdentifierValueComparers,
            elementSelector: (queryContext, dataReader, resultContext, resultCoordinator)
                => elementMaterializer(queryContext, dataReader, resultContext, resultCoordinator)!,
            contextType: queryCompilationContext.ContextType,
            standAloneStateManager: queryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution,
            detailedErrorsEnabled: _detailedErrorsEnabled,
            threadSafetyChecksEnabled: _threadSafetyChecksEnabled);
    }
}
