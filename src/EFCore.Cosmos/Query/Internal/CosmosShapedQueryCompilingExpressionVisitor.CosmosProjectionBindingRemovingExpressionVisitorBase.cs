// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

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
    private abstract class CosmosProjectionBindingRemovingExpressionVisitorBase(
        ParameterExpression jTokenParameter,
        bool trackQueryResults)
        : ExpressionVisitor
    {
        private static readonly MethodInfo GetItemMethodInfo
            = typeof(JObject).GetRuntimeProperties()
                .Single(pi => pi.Name == "Item" && pi.GetIndexParameters()[0].ParameterType == typeof(string))
                .GetMethod;

        private static readonly PropertyInfo JTokenTypePropertyInfo
            = typeof(JToken).GetRuntimeProperties()
                .Single(mi => mi.Name == nameof(JToken.Type));

        private static readonly MethodInfo JTokenToObjectWithSerializerMethodInfo
            = typeof(JToken).GetRuntimeMethods()
                .Single(mi => mi.Name == nameof(JToken.ToObject) && mi.GetParameters().Length == 1 && mi.IsGenericMethodDefinition);

        private static readonly MethodInfo CollectionAccessorAddMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo()
                .GetDeclaredMethod(nameof(IClrCollectionAccessor.Add));

        private static readonly MethodInfo CollectionAccessorGetOrCreateMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo()
                .GetDeclaredMethod(nameof(IClrCollectionAccessor.GetOrCreate));

        private readonly IDictionary<ParameterExpression, Expression> _materializationContextBindings
            = new Dictionary<ParameterExpression, Expression>();

        private readonly IDictionary<Expression, ParameterExpression> _projectionBindings
            = new Dictionary<Expression, ParameterExpression>();

        private readonly IDictionary<Expression, (IEntityType EntityType, Expression JObjectExpression)> _ownerMappings
            = new Dictionary<Expression, (IEntityType, Expression)>();

        private readonly IDictionary<Expression, Expression> _ordinalParameterBindings
            = new Dictionary<Expression, Expression>();

        private List<IncludeExpression> _pendingIncludes
            = [];

        private static readonly MethodInfo ToObjectWithSerializerMethodInfo
            = typeof(CosmosProjectionBindingRemovingExpressionVisitorBase)
                .GetRuntimeMethods().Single(mi => mi.Name == nameof(SafeToObjectWithSerializer));

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.Assign)
            {
                if (binaryExpression.Left is ParameterExpression parameterExpression)
                {
                    if (parameterExpression.Type == typeof(JObject)
                        || parameterExpression.Type == typeof(JArray))
                    {
                        string storeName = null;

                        // Values injected by JObjectInjectingExpressionVisitor
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

                        switch (projectionExpression)
                        {
                            // ProjectionBindingExpression may represent a named token to be obtained from a containing JObject, or
                            // it may be that the token is not nested in a JObject if the query was generated using the SQL VALUE clause.
                            case ProjectionBindingExpression projectionBindingExpression:
                            {
                                var projection = GetProjection(projectionBindingExpression);
                                projectionExpression = projection.Expression;
                                if (!projection.IsValueProjection)
                                {
                                    storeName = projection.Alias;
                                }

                                break;
                            }

                            case ObjectArrayAccessExpression e:
                                storeName = e.PropertyName;
                                break;

                            case EntityProjectionExpression e:
                                storeName = e.PropertyName;
                                break;
                        }

                        Expression valueExpression;
                        switch (projectionExpression)
                        {
                            case ObjectArrayAccessExpression objectArrayProjectionExpression:
                                _projectionBindings[objectArrayProjectionExpression] = parameterExpression;
                                valueExpression = CreateGetValueExpression(
                                    objectArrayProjectionExpression.Object, storeName, parameterExpression.Type);
                                break;

                            case EntityProjectionExpression entityProjectionExpression:
                                var accessExpression = entityProjectionExpression.Object;
                                _projectionBindings[accessExpression] = parameterExpression;

                                switch (accessExpression)
                                {
                                    case ObjectReferenceExpression:
                                        valueExpression = CreateGetValueExpression(jTokenParameter, storeName, parameterExpression.Type);
                                        break;

                                    case ObjectAccessExpression:
                                        // Access to an owned type may be nested inside another owned type, so collect the store names
                                        // and add owner mappings for each.
                                        var storeNames = new List<string>();
                                        while (accessExpression is ObjectAccessExpression objectAccessExpression)
                                        {
                                            accessExpression = objectAccessExpression.Object;
                                            storeNames.Add(objectAccessExpression.PropertyName);
                                            _ownerMappings[objectAccessExpression]
                                                = (objectAccessExpression.Navigation.DeclaringEntityType, accessExpression);
                                        }

                                        valueExpression = CreateGetValueExpression(accessExpression, (string)null, typeof(JObject));
                                        for (var i = storeNames.Count - 1; i >= 0; i--)
                                        {
                                            valueExpression = CreateGetValueExpression(valueExpression, storeNames[i], typeof(JObject));
                                        }

                                        break;
                                    default:
                                        throw new InvalidOperationException(
                                            CoreStrings.TranslationFailed(binaryExpression.Print()));
                                }

                                break;

                            default:
                                throw new UnreachableException();
                        }

                        return MakeBinary(ExpressionType.Assign, binaryExpression.Left, valueExpression);
                    }

                    if (parameterExpression.Type == typeof(MaterializationContext))
                    {
                        var newExpression = (NewExpression)binaryExpression.Right;

                        EntityProjectionExpression entityProjectionExpression;
                        if (newExpression.Arguments[0] is ProjectionBindingExpression projectionBindingExpression)
                        {
                            var projection = GetProjection(projectionBindingExpression);
                            entityProjectionExpression = (EntityProjectionExpression)projection.Expression;
                        }
                        else
                        {
                            var projection = ((UnaryExpression)((UnaryExpression)newExpression.Arguments[0]).Operand).Operand;
                            entityProjectionExpression = (EntityProjectionExpression)projection;
                        }

                        _materializationContextBindings[parameterExpression] = entityProjectionExpression.Object;

                        var updatedExpression = New(
                            newExpression.Constructor,
                            Constant(ValueBuffer.Empty),
                            newExpression.Arguments[1]);

                        return MakeBinary(ExpressionType.Assign, binaryExpression.Left, updatedExpression);
                    }
                }

                if (binaryExpression.Left is MemberExpression { Member: FieldInfo { IsInitOnly: true } } memberExpression)
                {
                    return memberExpression.Assign(Visit(binaryExpression.Right));
                }
            }

            return base.VisitBinary(binaryExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var method = methodCallExpression.Method;
            var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;
            if (genericMethod == EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod)
            {
                var property = methodCallExpression.Arguments[2].GetConstantValue<IProperty>();
                Expression innerExpression;
                if (methodCallExpression.Arguments[0] is ProjectionBindingExpression projectionBindingExpression)
                {
                    var projection = GetProjection(projectionBindingExpression);

                    innerExpression = Convert(
                        CreateReadJTokenExpression(jTokenParameter, projection.Alias),
                        typeof(JObject));
                }
                else
                {
                    innerExpression = _materializationContextBindings[
                        (ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object];
                }

                return CreateGetValueExpression(innerExpression, property, methodCallExpression.Type);
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

                    _pendingIncludes.Add(includeExpression);

                    Visit(includeExpression.EntityExpression);

                    // Includes on collections are processed when visiting CollectionShaperExpression
                    return Visit(methodCallExpression.Arguments[0]);
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case ProjectionBindingExpression projectionBindingExpression:
                {
                    var projection = GetProjection(projectionBindingExpression);

                    return CreateGetValueExpression(
                        jTokenParameter,
                        projection.IsValueProjection ? null : projection.Alias,
                        projectionBindingExpression.Type,
                        (projection.Expression as SqlExpression)?.TypeMapping);
                }

                case CollectionShaperExpression collectionShaperExpression:
                {
                    ObjectArrayAccessExpression objectArrayAccess;
                    switch (collectionShaperExpression.Projection)
                    {
                        case ProjectionBindingExpression projectionBindingExpression:
                            var projection = GetProjection(projectionBindingExpression);
                            objectArrayAccess = (ObjectArrayAccessExpression)projection.Expression;
                            break;
                        case ObjectArrayAccessExpression objectArrayProjectionExpression:
                            objectArrayAccess = objectArrayProjectionExpression;
                            break;
                        default:
                            throw new InvalidOperationException(CoreStrings.TranslationFailed(extensionExpression.Print()));
                    }

                    var jArray = _projectionBindings[objectArrayAccess];
                    var jObjectParameter = Parameter(typeof(JObject), jArray.Name + "Object");
                    var ordinalParameter = Parameter(typeof(int), jArray.Name + "Ordinal");

                    var accessExpression = objectArrayAccess.InnerProjection.Object;
                    _projectionBindings[accessExpression] = jObjectParameter;
                    _ownerMappings[accessExpression] =
                        (objectArrayAccess.Navigation.DeclaringEntityType, objectArrayAccess.Object);
                    _ordinalParameterBindings[accessExpression] = Add(
                        ordinalParameter, Constant(1, typeof(int)));

                    var innerShaper = (BlockExpression)Visit(collectionShaperExpression.InnerShaper);

                    innerShaper = AddIncludes(innerShaper);

                    var entities = Call(
                        EnumerableMethods.SelectWithOrdinal.MakeGenericMethod(typeof(JObject), innerShaper.Type),
                        Call(
                            EnumerableMethods.Cast.MakeGenericMethod(typeof(JObject)),
                            jArray),
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
                        return jObjectBlock;
                    }

                    Check.DebugAssert(jObjectBlock != null, "The first include must end up on a valid shaper block");

                    // These are the expressions added by JObjectInjectingExpressionVisitor
                    var jObjectCondition = (ConditionalExpression)jObjectBlock.Expressions[^1];

                    var shaperBlock = (BlockExpression)jObjectCondition.IfFalse;
                    shaperBlock = AddIncludes(shaperBlock);

                    var jObjectExpressions = new List<Expression>(jObjectBlock.Expressions);
                    jObjectExpressions.RemoveAt(jObjectExpressions.Count - 1);

                    jObjectExpressions.Add(
                        jObjectCondition.Update(jObjectCondition.Test, jObjectCondition.IfTrue, shaperBlock));

                    return jObjectBlock.Update(jObjectBlock.Variables, jObjectExpressions);
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
                .GetDeclaredMethod(nameof(IncludeReference));

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
                .GetDeclaredMethod(nameof(IncludeCollection));

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
                        inverseNavigation?.SetIsLoadedWhenNoTracking(relatedEntity);
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
            INavigation inverseNavigation)
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

        private static Delegate GenerateInitialize(
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
                .GetDeclaredMethod(nameof(PopulateCollection));

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

        protected abstract ProjectionExpression GetProjection(ProjectionBindingExpression projectionBindingExpression);

        private static Expression CreateReadJTokenExpression(Expression jObjectExpression, string propertyName)
            => Call(jObjectExpression, GetItemMethodInfo, Constant(propertyName));

        private Expression CreateGetValueExpression(
            Expression jTokenExpression,
            IProperty property,
            Type type)
        {
            if (property.Name == CosmosPartitionKeyInPrimaryKeyConvention.JObjectPropertyName)
            {
                return _projectionBindings[jTokenExpression];
            }

            var entityType = property.DeclaringType as IEntityType;
            var ownership = entityType?.FindOwnership();
            var storeName = property.GetJsonPropertyName();
            if (storeName.Length == 0)
            {
                if (entityType == null
                    || !entityType.IsDocumentRoot())
                {
                    if (ownership is { IsUnique: false } && property.IsOrdinalKeyProperty())
                    {
                        var ordinalExpression = _ordinalParameterBindings[jTokenExpression];
                        if (ordinalExpression.Type != type)
                        {
                            ordinalExpression = Convert(ordinalExpression, type);
                        }

                        return ordinalExpression;
                    }

                    var principalProperty = property.FindFirstPrincipal();
                    if (principalProperty != null)
                    {
                        Expression ownerJObjectExpression = null;
                        if (_ownerMappings.TryGetValue(jTokenExpression, out var ownerInfo))
                        {
                            Check.DebugAssert(
                                principalProperty.DeclaringType.IsAssignableFrom(ownerInfo.EntityType),
                                $"{principalProperty.DeclaringType} is not assignable from {ownerInfo.EntityType}");

                            ownerJObjectExpression = ownerInfo.JObjectExpression;
                        }
                        else if (jTokenExpression is ObjectReferenceExpression objectReferenceExpression)
                        {
                            ownerJObjectExpression = objectReferenceExpression;
                        }
                        else if (jTokenExpression is ObjectAccessExpression objectAccessExpression)
                        {
                            ownerJObjectExpression = objectAccessExpression.Object;
                        }

                        if (ownerJObjectExpression != null)
                        {
                            return CreateGetValueExpression(ownerJObjectExpression, principalProperty, type);
                        }
                    }
                }

                return Default(type);
            }

            // Workaround for old databases that didn't store the key property
            if (ownership is { IsUnique: false }
                && !entityType.IsDocumentRoot()
                && property.ClrType == typeof(int)
                && !property.IsForeignKey()
                && property.FindContainingPrimaryKey() is { Properties.Count: > 1 }
                && property.GetJsonPropertyName().Length != 0
                && !property.IsShadowProperty())
            {
                var readExpression = CreateGetValueExpression(
                    jTokenExpression,
                    storeName,
                    type.MakeNullable(),
                    property.GetTypeMapping(),
                    isNonNullableScalar: false);

                var nonNullReadExpression = readExpression;
                if (nonNullReadExpression.Type != type)
                {
                    nonNullReadExpression = Convert(nonNullReadExpression, type);
                }

                var ordinalExpression = _ordinalParameterBindings[jTokenExpression];
                if (ordinalExpression.Type != type)
                {
                    ordinalExpression = Convert(ordinalExpression, type);
                }

                return Condition(
                    Equal(readExpression, Constant(null, readExpression.Type)),
                    ordinalExpression,
                    nonNullReadExpression);
            }

            return Convert(
                CreateGetValueExpression(
                    jTokenExpression,
                    storeName,
                    type.MakeNullable(),
                    property.GetTypeMapping(),
                    // special case keys - we check them for null to see if the entity needs to be materialized, so we want to keep the null, rather than non-nullable default
                    // returning defaults is supposed to help with evolving the schema - so this doesn't concern keys anyway (they shouldn't evolve)
                    isNonNullableScalar: !property.IsNullable && !property.IsKey()),
                type);
        }

        private Expression CreateGetValueExpression(
            Expression jTokenExpression,
            string storeName,
            Type type,
            CoreTypeMapping typeMapping = null,
            bool isNonNullableScalar = false)
        {
            Check.DebugAssert(type.IsNullableType(), "Must read nullable type from JObject.");

            var innerExpression = jTokenExpression switch
            {
                _ when _projectionBindings.TryGetValue(jTokenExpression, out var innerVariable)
                    => innerVariable,

                ObjectReferenceExpression
                    => jTokenParameter,

                ObjectAccessExpression objectAccessExpression
                    => CreateGetValueExpression(
                        objectAccessExpression.Object,
                        ((IAccessExpression)objectAccessExpression.Object).PropertyName,
                        typeof(JObject)),

                _ => jTokenExpression
            };

            jTokenExpression = storeName == null
                ? innerExpression
                : CreateReadJTokenExpression(
                    innerExpression.Type == typeof(JObject)
                        ? innerExpression
                        : Convert(innerExpression, typeof(JObject)), storeName);

            Expression valueExpression;
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
                        ? Expression.Convert(
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

                valueExpression = Invoke(Lambda(body, jTokenParameter), jTokenExpression);
            }
            else
            {
                valueExpression = ConvertJTokenToType(
                    jTokenExpression,
                    (isNonNullableScalar
                        ? typeMapping?.ClrType
                        : typeMapping?.ClrType.MakeNullable()) ?? type);

                if (valueExpression.Type != type)
                {
                    valueExpression = Convert(valueExpression, type);
                }
            }

            return valueExpression;
        }

        private static Expression ConvertJTokenToType(Expression jTokenExpression, Type type)
            => type == typeof(JToken)
                ? jTokenExpression
                : Call(
                    ToObjectWithSerializerMethodInfo.MakeGenericMethod(type),
                    jTokenExpression);

        private static T SafeToObjectWithSerializer<T>(JToken token)
            => token == null || token.Type == JTokenType.Null ? default : token.ToObject<T>(CosmosClientWrapper.Serializer);
    }
}
