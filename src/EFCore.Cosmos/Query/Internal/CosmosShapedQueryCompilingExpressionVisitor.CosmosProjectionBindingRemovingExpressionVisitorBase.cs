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
        CosmosShapedQueryCompilingExpressionVisitor shapedQueryCompiler,
        ParameterExpression jTokenParameter,
        bool trackQueryResults)
        : ExpressionVisitor
    {
        public static readonly MethodInfo ToObjectWithSerializerMethodInfo
            = typeof(CosmosProjectionBindingRemovingExpressionVisitorBase)
                .GetRuntimeMethods().Single(mi => mi.Name == nameof(SafeToObjectWithSerializer));

        public static readonly MethodInfo GetItemMethodInfo
            = typeof(JToken).GetRuntimeProperties()
                .Single(pi => pi.Name == "Item" && pi.GetIndexParameters()[0].ParameterType == typeof(object))
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

        private readonly ParameterExpression _jTokenParameter = jTokenParameter;
        private ParameterExpression _parentJObject = jTokenParameter;
        private int _currentStructuralIndex;

        public Expression Translate(Expression expression) => Visit(shapedQueryCompiler.InjectStructuralTypeMaterializers(expression));

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            switch (binaryExpression)
            {
                case { NodeType: ExpressionType.Assign, Left: ParameterExpression parameterExpression, Right: MethodCallExpression { Method.IsGenericMethod: true } jObjectMethodCallExpression } 
                     when jObjectMethodCallExpression.Method.GetGenericMethodDefinition() == ToObjectWithSerializerMethodInfo
                        && (parameterExpression.Type == typeof(JObject) || parameterExpression.Type == typeof(JArray)):
                    // JObject assignment already uses ToObjectWithSerializerMethodInfo. This can happen because code was generated for nested structural properties that already leverage JObject correctly.
                    return binaryExpression;
                case { NodeType: ExpressionType.Assign, Left: ParameterExpression parameterExpression }
                    when parameterExpression.Type == typeof(JObject) || parameterExpression.Type == typeof(JArray):
                    
                        // Values injected by JObjectInjectingExpressionVisitor
                        var projectionExpression = (ProjectionBindingExpression)binaryExpression.Right.UnwrapTypeConversion(out _);

                        var projection = GetProjection(projectionExpression);
                        if (projection.Expression is StructuralTypeProjectionExpression stp)
                        {
                            _projectionBindings[stp.Object] = parameterExpression;
                        }

                        var storeName = projection.IsValueProjection ? null : projection.Alias;

                        var valueExpression = CreateGetValueExpression(
                            _jTokenParameter, storeName, parameterExpression.Type);

                        return MakeBinary(ExpressionType.Assign, binaryExpression.Left, valueExpression);
                case { NodeType: ExpressionType.Assign, Left: ParameterExpression parameterExpression }
                    when parameterExpression.Type == typeof(MaterializationContext):
                    var newExpression = (NewExpression)binaryExpression.Right;

                    if (newExpression.Arguments[0] is StructuralPropertyBindingExpression structuralPropertyBindingExpression)
                    {
                        _materializationContextBindings[parameterExpression] = structuralPropertyBindingExpression;
                        _projectionBindings[structuralPropertyBindingExpression] = structuralPropertyBindingExpression.JObjectParameter;
                    }
                    else
                    {
                        StructuralTypeProjectionExpression structuralTypeProjectionExpression;
                        if (newExpression.Arguments[0] is ProjectionBindingExpression projectionBindingExpression)
                        {
                            structuralTypeProjectionExpression = (StructuralTypeProjectionExpression)
                                GetProjection(projectionBindingExpression).Expression;
                        }
                        else
                        {
                            structuralTypeProjectionExpression = (StructuralTypeProjectionExpression)
                                ((UnaryExpression)((UnaryExpression)newExpression.Arguments[0]).Operand).Operand;
                        }

                        _materializationContextBindings[parameterExpression] = structuralTypeProjectionExpression.Object;
                    }

                    var updatedExpression = New(
                        newExpression.Constructor,
                        Constant(ValueBuffer.Empty),
                        newExpression.Arguments[1]);

                    return MakeBinary(ExpressionType.Assign, binaryExpression.Left, updatedExpression);
                case { NodeType: ExpressionType.Assign, Left: MemberExpression { Member: FieldInfo { IsInitOnly: true } } memberExpression }:
                    return memberExpression.Assign(Visit(binaryExpression.Right));
                default:
                    return base.VisitBinary(binaryExpression);
            }
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
                        CreateReadJTokenExpression(_jTokenParameter, projection.Alias),
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
                        _jTokenParameter,
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
                    // The ShapedQueryCompilingExpressionVisitor will not generate CollectionShaperExpression for complex collection properties, so
                    // an ObjectArrayAccessExpression in a CollectionShaperExpression can only be generated for owned collections
                    // Complex properties are handled by CosmosShapedQueryCompilingExpressionVisitor.AddStructuralTypeInitialization
                    _ownerMappings[accessExpression] =
                        ((IEntityType)objectArrayAccess.StructuralProperty.DeclaringType, objectArrayAccess.Object);
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
            bool _)
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
            bool setLoaded)
        {
            if (entity == null
                || !navigation.DeclaringEntityType.IsAssignableFrom(entityType))
            {
                return;
            }

            navigation.GetCollectionAccessor()!.GetOrCreate(entity, forMaterialization: true);

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
                    // Enumerator contains logic for tracking the entities, so we need to make sure to enumerate it
                    using var enumerator = relatedEntities.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                    }
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

        public static readonly MethodInfo PopulateCollectionMethodInfo
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
                    => _jTokenParameter,

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

                valueExpression = Invoke(Lambda(body, jTokenParameter), jTokenExpression);
            }
            else
            {
                valueExpression = ConvertJTokenToType(
                    jTokenExpression,
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

        private static Expression ConvertJTokenToType(Expression jTokenExpression, Type type)
            => type == typeof(JToken)
                ? jTokenExpression
                : Call(
                    ToObjectWithSerializerMethodInfo.MakeGenericMethod(type),
                    jTokenExpression);

        private static T SafeToObjectWithSerializer<T>(JToken token)
            => token == null || token.Type == JTokenType.Null ? default : token.ToObject<T>(CosmosClientWrapper.Serializer);

        public void ProcessStructuralProperties(
            StructuralTypeShaperExpression shaper,
            ParameterExpression instanceVariable,
            List<Expression> expressions)
        {
            if (shaper.StructuralType is IEntityType entityType)
            {
                foreach (var navigation in entityType.GetNavigations())
                {
                    var member = MakeMemberAccess(instanceVariable, navigation.GetMemberInfo(true, true));
                    expressions.Add(navigation.IsCollection
                        ? CreateStructuralCollectionAssignmentBlock(member, navigation, navigation.TargetEntityType, navigation.TargetEntityType.GetContainingPropertyName()!, isNullable: false)
                        : CreateStructuralPropertyAssignmentBlock(member, navigation.TargetEntityType, navigation.TargetEntityType.GetContainingPropertyName()!, false));
                }
            }

            foreach (var complexProperty in shaper.StructuralType.GetComplexProperties())
            {
                var member = MakeMemberAccess(instanceVariable, complexProperty.GetMemberInfo(true, true));
                expressions.Add(complexProperty.IsCollection
                    ? CreateStructuralCollectionAssignmentBlock(member, complexProperty, complexProperty.ComplexType, complexProperty.GetJsonPropertyName()!, complexProperty.IsNullable)
                    : CreateStructuralPropertyAssignmentBlock(member, complexProperty.ComplexType, complexProperty.GetJsonPropertyName(), complexProperty.IsNullable));
            }
        }

        private BlockExpression CreateStructuralPropertyAssignmentBlock(
            MemberExpression memberExpression,
            ITypeBase structuralType,
            string jsonPropertyName,
            bool isNullable)
        {
            var jObjectVariable = Parameter(typeof(JObject), "structuralJObject" + ++_currentStructuralIndex);
            var assignJObjectVariable = Assign(jObjectVariable,
                Call(
                    ToObjectWithSerializerMethodInfo.MakeGenericMethod(typeof(JObject)),
                    Call(_parentJObject, GetItemMethodInfo, Constant(jsonPropertyName))));

            var materializeExpression = CreateStructuralTypeMaterializeExpression(structuralType, jObjectVariable);
            if (isNullable)
            {
                materializeExpression = Condition(Equal(jObjectVariable, Constant(null)),
                    Default(structuralType.ClrType.MakeNullable()),
                    ConvertChecked(materializeExpression, structuralType.ClrType.MakeNullable()));
            }

            return Block(
                [jObjectVariable],
                [
                    assignJObjectVariable,
                    memberExpression.Assign(materializeExpression)
                ]
            );
        }

        private BlockExpression CreateStructuralCollectionAssignmentBlock(
            MemberExpression memberExpression,
            IPropertyBase structuralProperty,
            ITypeBase structuralType,
            string jsonPropertyName,
            bool isNullable)
        {
            var structuralJArrayVariable = Variable(
                typeof(JArray),
                "structuralJArray" + ++_currentStructuralIndex);

            var assignJArrayVariable = Assign(structuralJArrayVariable,
                Call(
                    ToObjectWithSerializerMethodInfo.MakeGenericMethod(typeof(JArray)),
                    Call(_parentJObject, GetItemMethodInfo, Constant(jsonPropertyName))));

            var jObjectParameter = Parameter(typeof(JObject), "structuralJObject" + _currentStructuralIndex);
            var ordinalParameter = Parameter(typeof(int), "ordinal" + _currentStructuralIndex);
            var materializeExpression = CreateStructuralTypeMaterializeExpression(structuralType, jObjectParameter, ordinalParameter);

            var select = Call(
                EnumerableMethods.SelectWithOrdinal.MakeGenericMethod(typeof(JObject), structuralType.ClrType),
                Call(
                    EnumerableMethods.Cast.MakeGenericMethod(typeof(JObject)),
                    structuralJArrayVariable),
                Lambda(materializeExpression, jObjectParameter, ordinalParameter));

            Expression populateExpression =
                Call(
                    PopulateCollectionMethodInfo.MakeGenericMethod(structuralType.ClrType, structuralProperty.ClrType),
                    Constant(structuralProperty.GetCollectionAccessor()),
                    select);

            if (isNullable)
            {
                populateExpression = Condition(Equal(structuralJArrayVariable, Constant(null)),
                    Default(structuralProperty.ClrType.MakeNullable()),
                    ConvertChecked(populateExpression, structuralProperty.ClrType.MakeNullable()));
            }

            return Block(
                [structuralJArrayVariable],
                [
                    assignJArrayVariable,
                    memberExpression.Assign(populateExpression)
                ]
            );
        }

        private Expression CreateStructuralTypeMaterializeExpression(
            ITypeBase structuralType,
            ParameterExpression jObjectParameter,
            ParameterExpression ordinalParameter = null)
        {
            var tempValueBuffer = new StructuralPropertyBindingExpression(jObjectParameter);
            var structuralTypeShaperExpression = new StructuralTypeShaperExpression(
                structuralType,
                tempValueBuffer,
                false);

            var oldParentJObject = _parentJObject;
            _parentJObject = jObjectParameter;

            // For owned collections, register ordinal parameter bindings
            if (ordinalParameter != null && structuralType is IEntityType entityType)
            {
                var ordinalExpression = Add(ordinalParameter, Constant(1, typeof(int)));
                _ordinalParameterBindings[tempValueBuffer] = ordinalExpression;

                var ownership = entityType.FindOwnership();
                if (ownership != null)
                {
                    _ownerMappings[tempValueBuffer] =
                        ((IEntityType)ownership.PrincipalEntityType, oldParentJObject);
                }
            }

            var materializeExpression = shapedQueryCompiler.InjectStructuralTypeMaterializers(structuralTypeShaperExpression);
            _parentJObject = oldParentJObject;

            if (structuralType.ClrType.IsNullableType())
            {
                materializeExpression = Condition(Equal(jObjectParameter, Constant(null)),
                    Default(structuralType.ClrType),
                    materializeExpression);
            }

            return materializeExpression;
        }

        protected sealed class StructuralPropertyBindingExpression(ParameterExpression jObjectParameter) : Expression
        {
            public override Type Type => typeof(ValueBuffer);

            public override ExpressionType NodeType => ExpressionType.Extension;

            public ParameterExpression JObjectParameter { get; } = jObjectParameter;
        }
    }
}
