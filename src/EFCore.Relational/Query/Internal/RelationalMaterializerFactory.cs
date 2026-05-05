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
public class RelationalMaterializerFactory(ICoreSingletonOptions coreSingletonOptions)
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
        var nextCollectionId = 0;

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

        var contextType = queryCompilationContext.ContextType;
        var isTracking = queryCompilationContext.QueryTrackingBehavior is QueryTrackingBehavior.TrackAll;
        var useRelationalNulls = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).UseRelationalNulls;
        var collectionParameterTranslationMode
            = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).ParameterizedCollectionMode;
        var relationalDependencies = queryCompilationContext.RelationalDependencies;

        var relationalCommandCache = new RelationalCommandCache(
            relationalDependencies.MemoryCache,
            relationalDependencies.QuerySqlGeneratorFactory,
            relationalDependencies.RelationalParameterBasedSqlProcessorFactory,
            select,
            useRelationalNulls,
            collectionParameterTranslationMode);

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
            contextType: contextType,
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
        ref int nextCollectionId)
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
                var m = new Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, object?>[newExpression.Arguments.Count];
                for (var i = 0; i < m.Length; i++)
                {
                    m[i] = BuildMaterializer<object>(newExpression.Arguments[i], select, isTracking, ref nextCollectionId);
                }

                // Each argument gets its own ResultContext so entity materializers within a
                // NewExpression don't share the Values init guard. This mirrors the generated
                // shaper where each entity occupies a separate slot in the values array.
                var argContexts = new ResultContext[m.Length];
                for (var i = 0; i < argContexts.Length; i++)
                {
                    argContexts[i] = new ResultContext();
                }

                // Reusable buffer for passing args to the ConstructorInvoker.
                var invokerArgs = new object?[m.Length];

                // Mirrors the generated shaper's structure:
                // - Init (rc.Values == null): materialize all args once, store in rc.Values
                // - Every call: drive collection populations by calling all arg materializers
                // - Return: when ResultReady, construct result from rc.Values
                //
                // This ensures scalars/entities are read from the reader only once (during init)
                // and reused from the values array on subsequent calls during collection population,
                // matching how the generated shaper stores all values in _valuesArrayInitializers.
                return (queryCtx, reader, rc, coord) =>
                {
                    if (rc.Values is null)
                    {
                        for (var i = 0; i < argContexts.Length; i++)
                        {
                            argContexts[i].Values = null;
                        }

                        rc.Values = new object[m.Length];
                        for (var i = 0; i < m.Length; i++)
                        {
                            var result = m[i](queryCtx, reader, argContexts[i], coord);

                            // For entity materializers with collection includes, the return value may be
                            // null (ResultReady=false during collection population), but the entity itself
                            // is cached in argContexts[i].Values[0]. For everything else (scalars, nested
                            // NewExpressions), the return value is the actual value.
                            rc.Values[i] = (result ?? argContexts[i].Values?[0])!;
                        }
                    }
                    else
                    {
                        for (var i = 0; i < m.Length; i++)
                        {
                            m[i](queryCtx, reader, argContexts[i], coord);
                        }
                    }

                    if (!coord.ResultReady)
                    {
                        return default;
                    }

                    rc.Values.CopyTo(invokerArgs.AsSpan());
                    return (T?)invoker.Invoke(invokerArgs.AsSpan());
                };
            }

            case RelationalStructuralTypeShaperExpression:
            case IncludeExpression:
            {
                var entityMaterializer = TryBuildMaterializerFromShaper(shaperExpression, select, isTracking, ref nextCollectionId)
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

            case RelationalCollectionShaperExpression collectionShaper:
            {
                return BuildStandaloneCollectionMaterializer<T>(collectionShaper, select, isTracking, ref nextCollectionId);
            }

            default:
                throw new UnreachableException($"Unexpected shaper expression type '{shaperExpression.GetType().Name}'.");
        }
    }

    /// <summary>
    ///     Builds a materializer for a standalone collection projection (e.g. <c>Select(x => x.Posts)</c>).
    ///     This mirrors the generated shaper's <c>InitializeCollection</c> / <c>PopulateCollection</c> protocol:
    ///     on first call the collection is created and stored in <see cref="ResultContext.Values" />;
    ///     on every call (including first) elements are populated from the current row;
    ///     when <see cref="SingleQueryResultCoordinator.ResultReady" /> is true the collection is returned.
    /// </summary>
    private Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, T?>
        BuildStandaloneCollectionMaterializer<T>(
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

        // Mirrors the generated shaper structure: InitializeCollection on first call,
        // PopulateCollection on every call, return collection when ResultReady.
        return (queryContext, dataReader, resultContext, resultCoordinator) =>
        {
            if (resultContext.Values is null)
            {
                var collection = collectionAccessor?.Create() ?? Activator.CreateInstance(typeof(T))!;

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
                innerElementMaterializer);

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
        Func<QueryContext, DbDataReader, ResultContext, SingleQueryResultCoordinator, object?> innerShaper)
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
                ((IList)collectionContext.Collection!).Add(element);
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
    /// </summary>
    private RelationalEntityMaterializer? TryBuildMaterializerFromShaper(
        Expression shaperExpression,
        SelectExpression selectExpression,
        bool isTracking,
        ref int nextCollectionId)
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
                    includeExpression.EntityExpression, selectExpression, isTracking, ref nextCollectionId);
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
                            includeExpression.NavigationExpression, selectExpression, isTracking, ref nextCollectionId);
                        if (includedMaterializer is null)
                        {
                            return null;
                        }

                        var navSetter = ((IRuntimePropertyBase)navigation).GetSetter();

                        var refInclude = new ReferenceIncludeInfo(
                            includedMaterializer,
                            navigation,
                            navSetter,
                            inverseNavigation,
                            inverseNavSetter,
                            isKeylessEntityType: navigation.DeclaringEntityType.FindPrimaryKey() is null);
                        innerMaterializer.AddReferenceInclude(refInclude);

                        // Flatten collection includes from the referenced entity up to the parent.
                        // In the generated shaper, ALL collection populations are driven at the top
                        // level; nesting them inside a reference include's MaterializeTyped causes
                        // ResultReady/HasNext interference.
                        FlattenCollectionIncludes(includedMaterializer, refInclude.ResultContext, innerMaterializer);

                        return innerMaterializer;
                    }

                    // Collection include (one-to-many)
                    case RelationalCollectionShaperExpression collectionShaper:
                    {
                        var childMaterializer = TryBuildMaterializerFromShaper(
                            collectionShaper.InnerShaper, selectExpression, isTracking, ref nextCollectionId);
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

    private static RelationalCommandCache CreateNonQueryCommandCache(
        RelationalQueryCompilationContext queryCompilationContext,
        Expression nonQueryExpression)
    {
        var useRelationalNulls = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).UseRelationalNulls;
        var collectionParameterTranslationMode
            = RelationalOptionsExtension.Extract(queryCompilationContext.ContextOptions).ParameterizedCollectionMode;
        var relationalDependencies = queryCompilationContext.RelationalDependencies;

        if (nonQueryExpression is DeleteExpression deleteExpression)
        {
            nonQueryExpression = deleteExpression.ApplyTags(queryCompilationContext.Tags);
        }
        else if (nonQueryExpression is UpdateExpression updateExpression)
        {
            nonQueryExpression = updateExpression.ApplyTags(queryCompilationContext.Tags);
        }

        return new RelationalCommandCache(
            relationalDependencies.MemoryCache,
            relationalDependencies.QuerySqlGeneratorFactory,
            relationalDependencies.RelationalParameterBasedSqlProcessorFactory,
            nonQueryExpression,
            useRelationalNulls,
            collectionParameterTranslationMode);
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
}
