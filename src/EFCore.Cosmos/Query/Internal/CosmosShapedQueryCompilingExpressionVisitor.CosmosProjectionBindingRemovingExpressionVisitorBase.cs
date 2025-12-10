// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json.Linq;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    // Removes assignments of MaterializationContext
    // Rewrites usages of MaterializationContext to use JObject variable injected by JObjectInjectingExpressionVisitor instead.
    private abstract class CosmosProjectionBindingRemovingExpressionVisitorBase(ParameterExpression jTokenParameter, bool trackQueryResults)
        : ExpressionVisitor
    {
        private static readonly PropertyInfo JTokenTypePropertyInfo
            = typeof(JToken).GetRuntimeProperties()
                .Single(mi => mi.Name == nameof(JToken.Type));

        private static readonly MethodInfo JTokenToObjectWithSerializerMethodInfo
            = typeof(JToken).GetRuntimeMethods()
                .Single(mi => mi.Name == nameof(JToken.ToObject) && mi.GetParameters().Length == 1 && mi.IsGenericMethodDefinition);

        private static readonly MethodInfo GetItemMethodInfo
            = typeof(JToken).GetRuntimeProperties()
                .Single(pi => pi.Name == "Item" && pi.GetIndexParameters()[0].ParameterType == typeof(object))
                .GetMethod ?? throw new UnreachableException();

        private static readonly MethodInfo ToObjectWithSerializerMethodInfo
            = typeof(CosmosProjectionBindingRemovingExpressionVisitorBase)
                .GetRuntimeMethods().Single(mi => mi.Name == nameof(SafeToObjectWithSerializer));

        private ParameterExpression? _collectionBlockJArray;
        private ParameterExpression? _entityTypeBlockJObject;
        private ConcreteStructuralTypeBlock? _concreteStructuralTypeBlock;
        private List<IncludeExpression> _pendingIncludes = [];
        private int _currentComplexIndex = 1;

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.Assign)
            {
                if (binaryExpression.Left is ParameterExpression parameterExpression)
                {
                    if (parameterExpression.Type == typeof(JObject) ||
                        parameterExpression.Type == typeof(JArray))
                    {
                        var projectionExpression = ((UnaryExpression)binaryExpression.Right).Operand;

                        if (projectionExpression is UnaryExpression
                            {
                                NodeType: ExpressionType.Convert,
                                Operand: UnaryExpression operand
                            })
                        {
                            // Unwrap EntityProjectionExpression when the root entity is not projected
                            // That is, this is handling the projection of a non-root entity type.
                            projectionExpression = operand.Operand;
                        }

                        return MakeBinary(binaryExpression.NodeType, binaryExpression.Left, Visit(projectionExpression));
                    }

                    // Overwrite any creations of MaterializationContext
                    if (parameterExpression.Type == typeof(MaterializationContext))
                    {
                        var newExpression = (NewExpression)binaryExpression.Right;
                        Debug.Assert(newExpression.Constructor != null, "Materialization assignment must always be via constructor");
                        var updatedExpression = New(
                            newExpression.Constructor,
                            Constant(ValueBuffer.Empty),
                            newExpression.Arguments[1]);

                        return MakeBinary(ExpressionType.Assign, binaryExpression.Left, updatedExpression);
                    }
                }
                else if (binaryExpression.Left is MemberExpression memberExpression)
                {
                    Debug.Assert(_concreteStructuralTypeBlock != null, "Assignments to properties can only happen inside a structural type block.");
                    var complexProperty = _concreteStructuralTypeBlock.StructuralType.GetComplexProperties().FirstOrDefault(x => x.GetMemberInfo(true, true) == memberExpression.Member);
                    if (complexProperty != null)
                    {
                        return CreateComplexPropertyAssignmentBlock(memberExpression, binaryExpression.Right, complexProperty);
                    }

                    return memberExpression.Assign(Visit(binaryExpression.Right));
                }
            }

            return base.VisitBinary(binaryExpression);
        }

        private BlockExpression CreateComplexPropertyAssignmentBlock(MemberExpression memberExpression, Expression valueExpression, IComplexProperty complexProperty)
        {
            Debug.Assert(_concreteStructuralTypeBlock != null, "Complex property assignments can only happen inside a structural type block.");

            var complexJObjectVariableExpression = Variable(
                                typeof(JObject),
                                "complexJObject" + _currentComplexIndex++);
            var assignComplexJObjectVariableExpression = Assign(complexJObjectVariableExpression, Call( // @TODO: Can we reuse get property value?
                                    ToObjectWithSerializerMethodInfo.MakeGenericMethod(typeof(JObject)),
                                    Call(_concreteStructuralTypeBlock.JObject, GetItemMethodInfo,
                                        Constant(complexProperty.Name)
                                    )
                                ));

            if (complexProperty.IsNullable)
            {
                var condition = (ConditionalExpression)valueExpression;
                valueExpression = Condition(
                    Equal(complexJObjectVariableExpression, Constant(null)),
                    condition.IfTrue,
                    condition.IfFalse);
            }

            valueExpression = EnterScope(ref _concreteStructuralTypeBlock, new ConcreteStructuralTypeBlock(complexJObjectVariableExpression, complexProperty.ComplexType),
                () => Visit(valueExpression));

            return Block(
                [complexJObjectVariableExpression],
                assignComplexJObjectVariableExpression,
                memberExpression.Assign(valueExpression)
            );
        }

        /// <summary>
        /// Overwrites usages of MaterializationContext to get property values from JObject
        /// Handles IncludeExpressions to track included entities
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var method = methodCallExpression.Method;
            var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;

            // Use jObject instead of MaterializationContext to get property values
            if (genericMethod == EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod)
            {
                var property = methodCallExpression.Arguments[2].GetConstantValue<IProperty>();
                if (methodCallExpression.Arguments[0] is ProjectionBindingExpression projectionBindingExpression)
                {
                    var projection = GetProjection(projectionBindingExpression);
                    return CreateGetJTokenExpression(jTokenParameter, projection.Alias);
                }

                return CreateGetValueExpression(property, method.ReturnType);
            }

            if (method.DeclaringType == typeof(Enumerable)
                && method.Name == nameof(Enumerable.Select)
                && genericMethod == EnumerableMethods.Select)
            {
                var lambda = (LambdaExpression)methodCallExpression.Arguments[1];
                if (lambda.Body is IncludeExpression includeExpression)
                {
                    if (includeExpression.Navigation is not INavigation navigation
                        || navigation.IsOnDependent
                        || navigation.ForeignKey.DeclaringEntityType.IsDocumentRoot())
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.NonEmbeddedIncludeNotSupported(includeExpression.Navigation));
                    }

                    if (trackQueryResults)
                    {

                    }
                    _pendingIncludes.Add(includeExpression);

                    Visit(includeExpression.EntityExpression);

                    // Includes on collections are processed when visiting CollectionShaperExpression
                    return Visit(methodCallExpression.Arguments[0]);
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        #region Context
        protected override Expression VisitBlock(BlockExpression blockExpression)
        {
            var param = blockExpression.Variables.Count == 1 ? blockExpression.Variables[0] : null;
            if (param?.Type == typeof(JObject))
            {
                return EnterScope(ref _entityTypeBlockJObject, param, () => base.VisitBlock(blockExpression));
            }

            if (param?.Type == typeof(JArray))
            {
                return EnterScope(ref _collectionBlockJArray, param, () => base.VisitBlock(blockExpression));
            }

            return base.VisitBlock(blockExpression);
        }

        //protected override Expression VisitLambda<T>(Expression<T> node)
        //{
        //    if (node.Parameters.FirstOrDefault(x => x.Type == typeof(JObject)) is ParameterExpression jObject)
        //    {
        //        return EnterScope(ref _entityTypeBlockJObject, jObject, () => base.VisitLambda(node));
        //    }
        //    return base.VisitLambda(node);
        //}

        protected override SwitchCase VisitSwitchCase(SwitchCase switchCaseExpression)
        {
            if (switchCaseExpression.TestValues.SingleOrDefault() is ConstantExpression constantExpression
                     && constantExpression.Value is ITypeBase structuralType)
            {
                Debug.Assert(_entityTypeBlockJObject != null, "Concrete structural type swith case can not be outside of an entity type block.");
                var jObjectVariable = _entityTypeBlockJObject;
                return EnterScope(ref _concreteStructuralTypeBlock, new ConcreteStructuralTypeBlock(jObjectVariable, structuralType), () => base.VisitSwitchCase(switchCaseExpression));
            }

            return base.VisitSwitchCase(switchCaseExpression);
        }

        private class ConcreteStructuralTypeBlock
        {
            public ConcreteStructuralTypeBlock(ParameterExpression jObject, ITypeBase structuralType)
            {
                JObject = jObject;
                StructuralType = structuralType;
            }

            public ITypeBase StructuralType { get; }

            public ParameterExpression JObject { get; }
        }

        private static TReturn EnterScope<TScope, TReturn>(ref TScope scope, TScope newValue, Func<TReturn> action)
        {
            var oldValue = scope;
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            scope = newValue;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            var result = action();
            scope = oldValue;
            return result;
        }
        #endregion

        #region Include
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case ProjectionBindingExpression projectionBindingExpression:
                {
                    var projection = GetProjection(projectionBindingExpression);

                    return CreateGetValueExpression(jTokenParameter,
                        projection.IsValueProjection ? null : projection.Alias,
                        typeof(JObject),
                        false,
                        (projection.Expression as SqlExpression)?.TypeMapping);
                }

                case ObjectArrayAccessExpression objectArrayAccessExpression:
                {
                    return CreateGetValueExpression(
                        _entityTypeBlockJObject ?? throw new InvalidOperationException(),
                        objectArrayAccessExpression.PropertyName,
                        objectArrayAccessExpression.Type,
                        false,
                        null);
                }

                case EntityProjectionExpression entityProjectionExpression:
                {
                    Debug.Assert(_entityTypeBlockJObject != null, "Entity projection can only be inside an entity type block.");
                    return CreateGetValueExpression(
                        _entityTypeBlockJObject ?? throw new InvalidOperationException(),
                        entityProjectionExpression.PropertyName,
                        entityProjectionExpression.Type,
                        false,
                        null);
                }

                case CollectionShaperExpression collectionShaperExpression:
                {
                    Debug.Assert(collectionShaperExpression.Navigation != null);
                    Debug.Assert(_collectionBlockJArray != null, "Collection shaper can only be inside a collection block.");

                    var jObjectParameter = Parameter(typeof(JObject), _collectionBlockJArray.Name + "Object");
                    var ordinalParameter = Parameter(typeof(int), _collectionBlockJArray.Name + "Ordinal");

                    var innerShaper = EnterScope(ref _entityTypeBlockJObject, jObjectParameter, () => (BlockExpression)Visit(collectionShaperExpression.InnerShaper));

                    innerShaper = AddIncludes(innerShaper);

                    var entities = Call(
                        EnumerableMethods.SelectWithOrdinal.MakeGenericMethod(typeof(JObject), innerShaper.Type),
                        Call(
                            EnumerableMethods.Cast.MakeGenericMethod(typeof(JObject)),
                            _collectionBlockJArray),
                        Lambda(innerShaper, jObjectParameter, ordinalParameter));

                    var navigation = collectionShaperExpression.Navigation;
                    return Call(
                        PopulateCollectionMethodInfo.MakeGenericMethod(navigation.TargetEntityType.ClrType, navigation.ClrType),
                        Constant(navigation.GetCollectionAccessor()),
                        entities);
                }

                case IncludeExpression includeExpression:
                {
                    if (!(includeExpression.Navigation is INavigation navigation)
                        || navigation.IsOnDependent
                        || navigation.ForeignKey.DeclaringEntityType.IsDocumentRoot())
                    {
                        throw new InvalidOperationException(
                            CosmosStrings.NonEmbeddedIncludeNotSupported(includeExpression.Navigation));
                    }

                    var isFirstInclude = _pendingIncludes.Count == 0;
                    _pendingIncludes.Add(includeExpression);

                    var jObjectBlock = Visit(includeExpression.EntityExpression) as BlockExpression;

                    if (!isFirstInclude)
                    {
                        return jObjectBlock!;
                    }
                    Check.DebugAssert(jObjectBlock != null, "The first include must end up on a valid shaper block");

                    var jObjectParameter = jObjectBlock.Variables.Single();
                    return EnterScope(ref _entityTypeBlockJObject, jObjectParameter, () =>
                    {
                        // These are the expressions added by JObjectInjectingExpressionVisitor
                        var jObjectCondition = (ConditionalExpression)jObjectBlock.Expressions[^1];

                        var shaperBlock = (BlockExpression)jObjectCondition.IfFalse;
                        shaperBlock = AddIncludes(shaperBlock);

                        var jObjectExpressions = new List<Expression>(jObjectBlock.Expressions);
                        jObjectExpressions.RemoveAt(jObjectExpressions.Count - 1);

                        jObjectExpressions.Add(
                            jObjectCondition.Update(jObjectCondition.Test, jObjectCondition.IfTrue, shaperBlock));

                        return jObjectBlock.Update(jObjectBlock.Variables, jObjectExpressions);
                    });
                }
            }

            return base.VisitExtension(extensionExpression);
        }

        private BlockExpression AddIncludes(BlockExpression shaperBlock)
        {
            if (_pendingIncludes.Count == 0)
            {
                return shaperBlock;
            }

            var shaperExpressions = new List<Expression>(shaperBlock.Expressions);
            var instanceVariable = shaperExpressions[^1];
            shaperExpressions.RemoveAt(shaperExpressions.Count - 1);

            var includesToProcess = _pendingIncludes;
            _pendingIncludes = [];

            foreach (var include in includesToProcess)
            {
                AddInclude(shaperExpressions, include, shaperBlock, instanceVariable);
            }

            shaperExpressions.Add(instanceVariable);
            shaperBlock = shaperBlock.Update(shaperBlock.Variables, shaperExpressions);
            return shaperBlock;
        }

        private void AddInclude(
            List<Expression> shaperExpressions,
            IncludeExpression includeExpression,
            BlockExpression shaperBlock,
            Expression instanceVariable)
        {
            // Cosmos does not support Includes for ISkipNavigation
            var navigation = (INavigation)includeExpression.Navigation;
            var includeMethod = navigation.IsCollection ? IncludeCollectionMethodInfo : IncludeReferenceMethodInfo;
            var includingClrType = navigation.DeclaringEntityType.ClrType;
            var relatedEntityClrType = navigation.TargetEntityType.ClrType;
#pragma warning disable EF1001 // Internal EF Core API usage.
            var entityEntryVariable = trackQueryResults
                ? shaperBlock.Variables.Single(v => v.Type == typeof(InternalEntityEntry))
                : (Expression)Constant(null, typeof(InternalEntityEntry));
#pragma warning restore EF1001 // Internal EF Core API usage.

            var concreteEntityTypeVariable = shaperBlock.Variables.Single(v => v.Type == typeof(IEntityType));
            var inverseNavigation = navigation.Inverse;
            var fixup = GenerateFixup(
                includingClrType, relatedEntityClrType, navigation, inverseNavigation);
            var initialize = GenerateInitialize(includingClrType, navigation);

            var navigationExpression = Visit(includeExpression.NavigationExpression);

            shaperExpressions.Add(
                IfThen(
                    Call(
                        Constant(navigation.DeclaringEntityType, typeof(IReadOnlyEntityType)),
                        IsAssignableFromMethodInfo,
                        Convert(concreteEntityTypeVariable, typeof(IReadOnlyEntityType))),
                    Call(
                        includeMethod.MakeGenericMethod(includingClrType, relatedEntityClrType),
                        entityEntryVariable,
                        instanceVariable,
                        concreteEntityTypeVariable,
                        navigationExpression,
                        Constant(navigation),
                        Constant(inverseNavigation, typeof(INavigation)),
                        Constant(fixup),
                        Constant(initialize, typeof(Action<>).MakeGenericType(includingClrType)),
#pragma warning disable EF1001 // Internal EF Core API usage.
                        Constant(includeExpression.SetLoaded))));
#pragma warning restore EF1001 // Internal EF Core API usage.
        }

        private static readonly MethodInfo IncludeReferenceMethodInfo
            = typeof(CosmosProjectionBindingRemovingExpressionVisitorBase).GetTypeInfo()
                .GetDeclaredMethod(nameof(IncludeReference))!;

        private static void IncludeReference<TIncludingEntity, TIncludedEntity>(
#pragma warning disable EF1001 // Internal EF Core API usage.
            InternalEntityEntry entry,
#pragma warning restore EF1001 // Internal EF Core API usage.
            object entity,
            IEntityType entityType,
            TIncludedEntity relatedEntity,
            INavigation navigation,
            INavigation inverseNavigation,
            Action<TIncludingEntity, TIncludedEntity> fixup,
            Action<TIncludingEntity> _,
            bool __)
        {
            if (entity == null
                || !navigation.DeclaringEntityType.IsAssignableFrom(entityType))
            {
                return;
            }

            if (entry == null)
            {
                var includingEntity = (TIncludingEntity)entity;
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
            // For non-null relatedEntity StateManager will set the flag
            else if (relatedEntity == null)
            {
#pragma warning disable EF1001 // Internal EF Core API usage.
                entry.SetIsLoaded(navigation);
#pragma warning restore EF1001 // Internal EF Core API usage.
            }
        }

        private static readonly MethodInfo IncludeCollectionMethodInfo
            = typeof(CosmosProjectionBindingRemovingExpressionVisitorBase).GetTypeInfo()
                .GetDeclaredMethod(nameof(IncludeCollection))!;

        private static void IncludeCollection<TIncludingEntity, TIncludedEntity>(
#pragma warning disable EF1001 // Internal EF Core API usage.
            InternalEntityEntry entry,
#pragma warning restore EF1001 // Internal EF Core API usage.
            object entity,
            IEntityType entityType,
            IEnumerable<TIncludedEntity> relatedEntities,
            INavigation navigation,
            INavigation inverseNavigation,
            Action<TIncludingEntity, TIncludedEntity> fixup,
            Action<TIncludingEntity> initialize,
            bool setLoaded)
        {
            if (entity == null
                || !navigation.DeclaringEntityType.IsAssignableFrom(entityType))
            {
                return;
            }

            if (entry == null)
            {
                var includingEntity = (TIncludingEntity)entity;
                navigation.SetIsLoadedWhenNoTracking(includingEntity);

                if (relatedEntities != null)
                {
                    foreach (var relatedEntity in relatedEntities)
                    {
                        fixup(includingEntity, relatedEntity);
                        inverseNavigation?.SetIsLoadedWhenNoTracking(relatedEntity!);
                    }
                }
                else
                {
                    initialize(includingEntity);
                }
            }
            else
            {
                if (setLoaded)
                {
#pragma warning disable EF1001 // Internal EF Core API usage.
                    entry.SetIsLoaded(navigation);
#pragma warning restore EF1001 // Internal EF Core API usage.
                }

                if (relatedEntities != null)
                {
                    using var enumerator = relatedEntities.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                    }
                }
                else
                {
                    initialize((TIncludingEntity)entity);
                }
            }
        }

        private static Delegate GenerateFixup(
            Type entityType,
            Type relatedEntityType,
            INavigation navigation,
            INavigation? inverseNavigation)
        {
            var entityParameter = Parameter(entityType);
            var relatedEntityParameter = Parameter(relatedEntityType);
            var expressions = new List<Expression>
            {
                navigation.IsCollection
                    ? AddToCollectionNavigation(entityParameter, relatedEntityParameter, navigation)
                    : AssignReferenceNavigation(entityParameter, relatedEntityParameter, navigation)
            };

            if (inverseNavigation != null)
            {
                expressions.Add(
                    inverseNavigation.IsCollection
                        ? AddToCollectionNavigation(relatedEntityParameter, entityParameter, inverseNavigation)
                        : AssignReferenceNavigation(relatedEntityParameter, entityParameter, inverseNavigation));
            }

            return Lambda(Block(typeof(void), expressions), entityParameter, relatedEntityParameter)
                .Compile();
        }

        private static Delegate? GenerateInitialize(
            Type entityType,
            INavigation navigation)
        {
            if (!navigation.IsCollection)
            {
                return null;
            }

            var entityParameter = Parameter(entityType);

            var getOrCreateExpression = Call(
                Constant(navigation.GetCollectionAccessor()),
                CollectionAccessorGetOrCreateMethodInfo,
                entityParameter,
                Constant(true));

            return Lambda(Block(typeof(void), getOrCreateExpression), entityParameter)
                .Compile();
        }

        private static Expression AssignReferenceNavigation(
            ParameterExpression entity,
            ParameterExpression relatedEntity,
            INavigation navigation)
            => entity.MakeMemberAccess(navigation.GetMemberInfo(forMaterialization: true, forSet: true)).Assign(relatedEntity);

        private static Expression AddToCollectionNavigation(
            ParameterExpression entity,
            ParameterExpression relatedEntity,
            INavigation navigation)
            => Call(
                Constant(navigation.GetCollectionAccessor()),
                CollectionAccessorAddMethodInfo,
                entity,
                relatedEntity,
                Constant(true));

        private static readonly MethodInfo PopulateCollectionMethodInfo
            = typeof(CosmosProjectionBindingRemovingExpressionVisitorBase).GetTypeInfo()
                .GetDeclaredMethod(nameof(PopulateCollection))!;

        private static readonly MethodInfo IsAssignableFromMethodInfo
            = typeof(IReadOnlyEntityType).GetMethod(nameof(IReadOnlyEntityType.IsAssignableFrom), [typeof(IReadOnlyEntityType)])!;

        private static TCollection PopulateCollection<TEntity, TCollection>(
            IClrCollectionAccessor accessor,
            IEnumerable<TEntity> entities)
        {
            // TODO: throw a better exception for non ICollection navigations
            var collection = (ICollection<TEntity>)accessor.Create();
            foreach (var entity in entities)
            {
                collection.Add(entity);
            }

            return (TCollection)collection;
        }

        private static readonly MethodInfo CollectionAccessorAddMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo()
                .GetDeclaredMethod(nameof(IClrCollectionAccessor.Add)) ?? throw new UnreachableException();

        private static readonly MethodInfo CollectionAccessorGetOrCreateMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo()
                .GetDeclaredMethod(nameof(IClrCollectionAccessor.GetOrCreate)) ?? throw new UnreachableException();
        #endregion

        protected abstract ProjectionExpression GetProjection(ProjectionBindingExpression projectionBindingExpression);

        #region Create expression helpers
        /// <summary>
        /// Create expression to get a property's value from JObject
        /// </summary>
        private Expression CreateGetValueExpression(IProperty property, Type? type = null)
        {
            var currentJObject = _concreteStructuralTypeBlock?.JObject ?? _entityTypeBlockJObject;
            Debug.Assert(currentJObject != null, "Property value can only be retrieved inside an structural type block.");

            if (property.Name == CosmosPartitionKeyInPrimaryKeyConvention.JObjectPropertyName)
            {
                return currentJObject;
            }

            return CreateGetValueExpression(currentJObject, property.GetJsonPropertyName(), type ?? property.ClrType, !property.IsNullable && !property.IsKey(), property.GetTypeMapping());
        }

        private Expression CreateGetValueExpression(ParameterExpression jObject, string? property, Type type, bool isNonNullableScalar, CoreTypeMapping? typeMapping)
        {
            var valueExpression = property != null ? CreateGetJTokenExpression(jObject, property) : jObject;
            var converter = typeMapping?.Converter;
            if (converter != null)
            {
                var jTokenParameter = Parameter(typeof(JToken));

                var body
                    = ReplacingExpressionVisitor.Replace(
                        converter.ConvertFromProviderExpression.Parameters.Single(),
                        Call(
                            jTokenParameter,
                            JTokenToObjectWithSerializerMethodInfo.MakeGenericMethod(converter.ProviderClrType),
                            Constant(CosmosClientWrapper.Serializer)),
                        converter.ConvertFromProviderExpression.Body);

                var originalBodyType = body.Type;
                if (body.Type != type)
                {
                    body = Convert(body, type);
                }

                Expression replaceExpression;
                if (converter.ConvertsNulls)
                {
                    replaceExpression = ReplacingExpressionVisitor.Replace(
                        converter.ConvertFromProviderExpression.Parameters.Single(),
                        Default(converter.ProviderClrType),
                        converter.ConvertFromProviderExpression.Body);

                    if (replaceExpression.Type != type)
                    {
                        replaceExpression = Convert(replaceExpression, type);
                    }
                }
                else
                {
                    replaceExpression = isNonNullableScalar
                        ? Convert(
                            Default(originalBodyType),
                            type)
                        : Default(type);
                }

                body = Condition(
                    OrElse(
                        Equal(jTokenParameter, Default(typeof(JToken))),
                        Equal(
                            MakeMemberAccess(jTokenParameter, JTokenTypePropertyInfo),
                            Constant(JTokenType.Null))),
                    replaceExpression,
                    body);

                valueExpression = Invoke(Lambda(body, jTokenParameter), valueExpression);
            }
            else
            {
                valueExpression = CreateSerializeJTokenToTypeExpression(
                    valueExpression,
                    (isNonNullableScalar
                        ? typeMapping?.ClrType
                        : typeMapping?.ClrType.MakeNullable())
                    ?? type);

                if (valueExpression.Type != type)
                {
                    valueExpression = Convert(valueExpression, type);
                }
            }

            return valueExpression;
        }

        /// <summary>
        /// Create expression to get the JToken for a property from JObject
        /// </summary>
        private Expression CreateGetJTokenExpression(ParameterExpression jObject, IPropertyBase propertyBase)
            => CreateGetJTokenExpression(jObject, propertyBase is IProperty p ? p.GetJsonPropertyName() : propertyBase.Name);

        /// <summary>
        /// Create expression to get the JToken for a property from JObject
        /// </summary>
        private Expression CreateGetJTokenExpression(ParameterExpression jObject, string propertyName)
            => Call(jObject, GetItemMethodInfo,
                Constant(propertyName));

        /// <summary>
        /// Create expression to serialize JToken to given type
        /// </summary>
        private static Expression CreateSerializeJTokenToTypeExpression(Expression jTokenExpression, Type type)
            => type == typeof(JToken)
                ? jTokenExpression
                : Call(
                    ToObjectWithSerializerMethodInfo.MakeGenericMethod(type),
                    jTokenExpression);

        private static T? SafeToObjectWithSerializer<T>(JToken? token)
            => token == null || token.Type == JTokenType.Null ? default : token.ToObject<T>(CosmosClientWrapper.Serializer);
        #endregion
    }
}
