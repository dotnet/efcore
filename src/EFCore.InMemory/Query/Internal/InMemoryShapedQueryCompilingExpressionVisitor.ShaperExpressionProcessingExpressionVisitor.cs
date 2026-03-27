// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using static System.Linq.Expressions.Expression;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Infrastructure.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal;

public partial class InMemoryShapedQueryCompilingExpressionVisitor
{
    private sealed class ShaperExpressionProcessingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo IncludeReferenceMethodInfo
            = typeof(ShaperExpressionProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(IncludeReference))!;

        private static readonly MethodInfo IncludeCollectionMethodInfo
            = typeof(ShaperExpressionProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(IncludeCollection))!;

        private static readonly MethodInfo MaterializeCollectionMethodInfo
            = typeof(ShaperExpressionProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(MaterializeCollection))!;

        private static readonly MethodInfo MaterializeSingleResultMethodInfo
            = typeof(ShaperExpressionProcessingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(MaterializeSingleResult))!;

        private static readonly MethodInfo CollectionAccessorAddMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo().GetDeclaredMethod(nameof(IClrCollectionAccessor.Add))!;

        private readonly InMemoryShapedQueryCompilingExpressionVisitor _inMemoryShapedQueryCompilingExpressionVisitor;
        private readonly bool _tracking;
        private ParameterExpression? _valueBufferParameter;

        private readonly Dictionary<Expression, ParameterExpression> _mapping = new();
        private readonly List<ParameterExpression> _variables = [];
        private readonly List<Expression> _expressions = [];
        private readonly Dictionary<ParameterExpression, Dictionary<IProperty, int>> _materializationContextBindings = new();

        public ShaperExpressionProcessingExpressionVisitor(
            InMemoryShapedQueryCompilingExpressionVisitor inMemoryShapedQueryCompilingExpressionVisitor,
            InMemoryQueryExpression inMemoryQueryExpression,
            bool tracking)
        {
            _inMemoryShapedQueryCompilingExpressionVisitor = inMemoryShapedQueryCompilingExpressionVisitor;
            _valueBufferParameter = inMemoryQueryExpression.CurrentParameter;
            _tracking = tracking;
        }

        private ShaperExpressionProcessingExpressionVisitor(
            InMemoryShapedQueryCompilingExpressionVisitor inMemoryShapedQueryCompilingExpressionVisitor,
            bool tracking)
        {
            _inMemoryShapedQueryCompilingExpressionVisitor = inMemoryShapedQueryCompilingExpressionVisitor;
            _tracking = tracking;
        }

        public LambdaExpression ProcessShaper(Expression shaperExpression)
        {
            var result = Visit(shaperExpression);
            _expressions.Add(result);
            result = Block(_variables, _expressions);

            // If parameter is null then the projection is not really server correlated so we can just put anything.
            _valueBufferParameter ??= Parameter(typeof(ValueBuffer));

            return Lambda(result, QueryCompilationContext.QueryContextParameter, _valueBufferParameter);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case StructuralTypeShaperExpression shaper:
                {
                    var key = shaper.ValueBufferExpression;
                    if (!_mapping.TryGetValue(key, out var variable))
                    {
                        variable = Parameter(shaper.StructuralType.ClrType);
                        _variables.Add(variable);
                        var innerShaper =
                            _inMemoryShapedQueryCompilingExpressionVisitor.InjectEntityMaterializers(shaper);
                        innerShaper = Visit(innerShaper);
                        _expressions.Add(Assign(variable, innerShaper));
                        _mapping[key] = variable;
                    }

                    return variable;
                }

                case ProjectionBindingExpression projectionBindingExpression:
                {
                    var key = projectionBindingExpression;
                    if (!_mapping.TryGetValue(key, out var variable))
                    {
                        variable = Parameter(projectionBindingExpression.Type);
                        _variables.Add(variable);
                        var queryExpression = (InMemoryQueryExpression)projectionBindingExpression.QueryExpression;
                        _valueBufferParameter ??= queryExpression.CurrentParameter;

                        var projectionIndex = queryExpression.GetProjection(projectionBindingExpression).GetConstantValue<int>();

                        // We don't need to pass property when reading at top-level
                        _expressions.Add(
                            Assign(
                                variable, queryExpression.CurrentParameter.CreateValueBufferReadValueExpression(
                                    projectionBindingExpression.Type, projectionIndex, property: null)));
                        _mapping[key] = variable;
                    }

                    return variable;
                }

                case IncludeExpression includeExpression:
                {
                    var entity = Visit(includeExpression.EntityExpression);
                    var entityClrType = includeExpression.EntityExpression.Type;
                    var includingClrType = includeExpression.Navigation.DeclaringEntityType.ClrType;
                    var inverseNavigation = includeExpression.Navigation.Inverse;
                    var relatedEntityClrType = includeExpression.Navigation.TargetEntityType.ClrType;
                    if (includingClrType != entityClrType
                        && includingClrType.IsAssignableFrom(entityClrType))
                    {
                        includingClrType = entityClrType;
                    }

                    if (includeExpression.Navigation.IsCollection)
                    {
                        var collectionResultShaperExpression = (CollectionResultShaperExpression)includeExpression.NavigationExpression;
                        var shaperLambda = new ShaperExpressionProcessingExpressionVisitor(
                                _inMemoryShapedQueryCompilingExpressionVisitor, _tracking)
                            .ProcessShaper(collectionResultShaperExpression.InnerShaper);
                        _expressions.Add(
                            Call(
                                IncludeCollectionMethodInfo.MakeGenericMethod(entityClrType, includingClrType, relatedEntityClrType),
                                QueryCompilationContext.QueryContextParameter,
                                Visit(collectionResultShaperExpression.Projection),
                                Constant(shaperLambda.Compile()),
                                entity,
                                Constant(includeExpression.Navigation),
                                Constant(inverseNavigation, typeof(INavigationBase)),
                                Constant(
                                    GenerateFixup(
                                            includingClrType, relatedEntityClrType, includeExpression.Navigation, inverseNavigation)
                                        .Compile()),
                                Constant(_tracking),
#pragma warning disable EF1001 // Internal EF Core API usage.
                                Constant(includeExpression.SetLoaded)));
#pragma warning restore EF1001 // Internal EF Core API usage.
                    }
                    else
                    {
                        _expressions.Add(
                            Call(
                                IncludeReferenceMethodInfo.MakeGenericMethod(entityClrType, includingClrType, relatedEntityClrType),
                                QueryCompilationContext.QueryContextParameter,
                                entity,
                                Visit(includeExpression.NavigationExpression),
                                Constant(includeExpression.Navigation),
                                Constant(inverseNavigation, typeof(INavigationBase)),
                                Constant(
                                    GenerateFixup(
                                            includingClrType, relatedEntityClrType, includeExpression.Navigation, inverseNavigation)
                                        .Compile()),
                                Constant(_tracking)));
                    }

                    return entity;
                }

                case CollectionResultShaperExpression collectionResultShaperExpression:
                {
                    var navigation = collectionResultShaperExpression.Navigation;
                    var collectionAccessor = navigation?.GetCollectionAccessor();
                    var collectionType = collectionAccessor?.CollectionType ?? collectionResultShaperExpression.Type;
                    var elementType = collectionResultShaperExpression.ElementType;
                    var shaperLambda = new ShaperExpressionProcessingExpressionVisitor(
                            _inMemoryShapedQueryCompilingExpressionVisitor, _tracking)
                        .ProcessShaper(collectionResultShaperExpression.InnerShaper);

                    return Call(
                        MaterializeCollectionMethodInfo.MakeGenericMethod(elementType, collectionType),
                        QueryCompilationContext.QueryContextParameter,
                        Visit(collectionResultShaperExpression.Projection),
                        Constant(shaperLambda.Compile()),
                        Constant(collectionAccessor, typeof(IClrCollectionAccessor)));
                }

                case SingleResultShaperExpression singleResultShaperExpression:
                {
                    var shaperLambda = new ShaperExpressionProcessingExpressionVisitor(
                            _inMemoryShapedQueryCompilingExpressionVisitor, _tracking)
                        .ProcessShaper(singleResultShaperExpression.InnerShaper);

                    return Call(
                        MaterializeSingleResultMethodInfo.MakeGenericMethod(singleResultShaperExpression.Type),
                        QueryCompilationContext.QueryContextParameter,
                        Visit(singleResultShaperExpression.Projection),
                        Constant(shaperLambda.Compile()));
                }
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression is { NodeType: ExpressionType.Assign, Left: ParameterExpression parameterExpression }
                && parameterExpression.Type == typeof(MaterializationContext))
            {
                var newExpression = (NewExpression)binaryExpression.Right;

                var projectionBindingExpression = (ProjectionBindingExpression)newExpression.Arguments[0];
                var queryExpression = (InMemoryQueryExpression)projectionBindingExpression.QueryExpression;
                _valueBufferParameter ??= queryExpression.CurrentParameter;

                _materializationContextBindings[parameterExpression]
                    = queryExpression.GetProjection(projectionBindingExpression).GetConstantValue<Dictionary<IProperty, int>>();

                var updatedExpression = newExpression.Update(
                    new[] { Constant(ValueBuffer.Empty), newExpression.Arguments[1] });

                return MakeBinary(ExpressionType.Assign, binaryExpression.Left, updatedExpression);
            }

            if (binaryExpression is
                { NodeType: ExpressionType.Assign, Left: MemberExpression { Member: FieldInfo { IsInitOnly: true } } memberExpression })
            {
                return memberExpression.Assign(Visit(binaryExpression.Right));
            }

            return base.VisitBinary(binaryExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == ExpressionExtensions.ValueBufferTryReadValueMethod)
            {
                var property = methodCallExpression.Arguments[2].GetConstantValue<IProperty?>();
                var indexMap = _materializationContextBindings[
                    (ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object!];

                Check.DebugAssert(
                    property != null || methodCallExpression.Type.IsNullableType(), "Must read nullable value without property");

                return Call(
                    methodCallExpression.Method,
                    _valueBufferParameter!,
                    Constant(indexMap[property!]),
                    methodCallExpression.Arguments[2]);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private static void IncludeReference<TEntity, TIncludingEntity, TIncludedEntity>(
            QueryContext queryContext,
            TEntity entity,
            TIncludedEntity? relatedEntity,
            INavigationBase navigation,
            INavigationBase? inverseNavigation,
            Action<TIncludingEntity, TIncludedEntity> fixup,
            bool trackingQuery)
            where TIncludingEntity : class, TEntity
            where TEntity : class
            where TIncludedEntity : class
        {
            if (entity is TIncludingEntity includingEntity)
            {
                if (trackingQuery
                    && navigation.DeclaringEntityType.FindPrimaryKey() != null)
                {
                    // For non-null relatedEntity StateManager will set the flag
                    if (relatedEntity == null)
                    {
                        queryContext.SetNavigationIsLoaded(includingEntity, navigation);
                    }
                }
                else
                {
                    navigation.SetIsLoadedWhenNoTracking(includingEntity);
                    if (relatedEntity != null)
                    {
                        fixup(includingEntity, relatedEntity);
                        if (inverseNavigation is { IsCollection: false })
                        {
                            inverseNavigation.SetIsLoadedWhenNoTracking(relatedEntity);
                        }
                    }
                }
            }
        }

        private static void IncludeCollection<TEntity, TIncludingEntity, TIncludedEntity>(
            QueryContext queryContext,
            IEnumerable<ValueBuffer> innerValueBuffers,
            Func<QueryContext, ValueBuffer, TIncludedEntity> innerShaper,
            TEntity entity,
            INavigationBase navigation,
            INavigationBase? inverseNavigation,
            Action<TIncludingEntity, TIncludedEntity> fixup,
            bool trackingQuery,
            bool setLoaded)
            where TIncludingEntity : class, TEntity
            where TEntity : class
            where TIncludedEntity : class
        {
            if (entity is TIncludingEntity includingEntity)
            {
                if (!navigation.IsShadowProperty())
                {
                    navigation.GetCollectionAccessor()!.GetOrCreate(includingEntity, forMaterialization: true);
                }

                if (setLoaded)
                {
                    if (trackingQuery)
                    {
                        queryContext.SetNavigationIsLoaded(entity, navigation);
                    }
                    else
                    {
                        navigation.SetIsLoadedWhenNoTracking(entity);
                    }
                }

                foreach (var valueBuffer in innerValueBuffers)
                {
                    var relatedEntity = innerShaper(queryContext, valueBuffer);

                    if (!trackingQuery)
                    {
                        fixup(includingEntity, relatedEntity);
                        inverseNavigation?.SetIsLoadedWhenNoTracking(relatedEntity);
                    }
                }
            }
        }

        private static TCollection MaterializeCollection<TElement, TCollection>(
            QueryContext queryContext,
            IEnumerable<ValueBuffer> innerValueBuffers,
            Func<QueryContext, ValueBuffer, TElement> innerShaper,
            IClrCollectionAccessor? clrCollectionAccessor)
            where TCollection : class, ICollection<TElement>
        {
            var collection = (TCollection)(clrCollectionAccessor?.Create() ?? new List<TElement>());

            foreach (var valueBuffer in innerValueBuffers)
            {
                var element = innerShaper(queryContext, valueBuffer);
                collection.Add(element);
            }

            return collection;
        }

        private static TResult? MaterializeSingleResult<TResult>(
            QueryContext queryContext,
            ValueBuffer valueBuffer,
            Func<QueryContext, ValueBuffer, TResult> innerShaper)
            => valueBuffer.IsEmpty
                ? default
                : innerShaper(queryContext, valueBuffer);

        private static LambdaExpression GenerateFixup(
            Type entityType,
            Type relatedEntityType,
            INavigationBase navigation,
            INavigationBase? inverseNavigation)
        {
            var entityParameter = Parameter(entityType);
            var relatedEntityParameter = Parameter(relatedEntityType);
            var expressions = new List<Expression>();

            if (!navigation.IsShadowProperty())
            {
                expressions.Add(
                    navigation.IsCollection
                        ? AddToCollectionNavigation(entityParameter, relatedEntityParameter, navigation)
                        : AssignReferenceNavigation(entityParameter, relatedEntityParameter, navigation));
            }

            if (inverseNavigation != null
                && !inverseNavigation.IsShadowProperty())
            {
                expressions.Add(
                    inverseNavigation.IsCollection
                        ? AddToCollectionNavigation(relatedEntityParameter, entityParameter, inverseNavigation)
                        : AssignReferenceNavigation(relatedEntityParameter, entityParameter, inverseNavigation));
            }

            return Lambda(Block(typeof(void), expressions), entityParameter, relatedEntityParameter);
        }

        private static Expression AssignReferenceNavigation(
            ParameterExpression entity,
            ParameterExpression relatedEntity,
            INavigationBase navigation)
            => entity.MakeMemberAccess(navigation.GetMemberInfo(forMaterialization: true, forSet: true)).Assign(relatedEntity);

        private static Expression AddToCollectionNavigation(
            ParameterExpression entity,
            ParameterExpression relatedEntity,
            INavigationBase navigation)
            => Call(
                Constant(navigation.GetCollectionAccessor()),
                CollectionAccessorAddMethodInfo,
                entity,
                relatedEntity,
                Constant(true));
    }
}
