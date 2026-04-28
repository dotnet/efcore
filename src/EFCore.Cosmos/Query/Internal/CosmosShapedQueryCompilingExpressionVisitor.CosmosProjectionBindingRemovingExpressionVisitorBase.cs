// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
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

        private readonly IDictionary<Expression, Expression> _ordinalParameterBindings
            = new Dictionary<Expression, Expression>();

        private readonly Dictionary<Expression, ParameterExpression> _ownerMappings
            = new();

        public static readonly MethodInfo ToObjectWithSerializerMethodInfo
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
                        var projectionExpression = binaryExpression.Right.UnwrapTypeConversion(out _);

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

                            case StructuralTypeProjectionExpression e:
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

                            case StructuralTypeProjectionExpression structuralTypeProjectionExpression:
                                var accessExpression = structuralTypeProjectionExpression.Object;
                                _projectionBindings[accessExpression] = parameterExpression;

                                valueExpression = accessExpression switch
                                {
                                    ObjectReferenceExpression => CreateGetValueExpression(jTokenParameter, storeName, parameterExpression.Type),
                                    ObjectAccessExpression => CreateGetValueExpression(jTokenParameter, storeName, typeof(JObject)),
                                    ObjectArrayAccessExpression => CreateGetValueExpression(jTokenParameter, storeName, typeof(JArray)),
                                    _ => throw new InvalidOperationException(CoreStrings.TranslationFailed(binaryExpression.Print())),
                                };
                                break;
                            case MethodCallExpression { Method.IsGenericMethod: true } jObjectMethodCallExpression
                                when jObjectMethodCallExpression.Method.GetGenericMethodDefinition() == ToObjectWithSerializerMethodInfo:
                                // JObject assignment already uses ToObjectWithSerializerMethodInfo. This can happen because code was generated for complex properties that already leverages JObject correctly.
                                return binaryExpression;
                            default:
                                throw new UnreachableException();
                        }

                        return MakeBinary(ExpressionType.Assign, binaryExpression.Left, valueExpression);
                    }

                    if (parameterExpression.Type == typeof(MaterializationContext))
                    {
                        var newExpression = (NewExpression)binaryExpression.Right;

                        if (newExpression.Arguments[0] is StructuralPropertyBindingExpression complexPropertyBindingExpression)
                        {
                            _materializationContextBindings[parameterExpression] = complexPropertyBindingExpression;
                            _projectionBindings[complexPropertyBindingExpression] = complexPropertyBindingExpression.JObjectParameter;
                            if (_ordinalParameterBindings.TryGetValue(complexPropertyBindingExpression, out var ordinal))
                            {
                                _ordinalParameterBindings[complexPropertyBindingExpression.JObjectParameter] = ordinal;
                            }
                        }
                        else
                        {
                            StructuralTypeProjectionExpression structuralTypeProjectionExpression;
                            if (newExpression.Arguments[0] is ProjectionBindingExpression projectionBindingExpression)
                            {
                                var projection = GetProjection(projectionBindingExpression);
                                structuralTypeProjectionExpression = (StructuralTypeProjectionExpression)projection.Expression;
                            }
                            else
                            {
                                var projection = ((UnaryExpression)((UnaryExpression)newExpression.Arguments[0]).Operand).Operand;
                                structuralTypeProjectionExpression = (StructuralTypeProjectionExpression)projection;
                            }

                            _materializationContextBindings[parameterExpression] = structuralTypeProjectionExpression.Object;
                        }

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
                            objectArrayAccess = (ObjectArrayAccessExpression)((StructuralTypeProjectionExpression)projection.Expression).Object;
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
                    _projectionBindings[objectArrayAccess] = jObjectParameter;
                    // The ShapedQueryCompilingExpressionVisitor will not generate CollectionShaperExpression for complex collection properties, so
                    // an ObjectArrayAccessExpression in a CollectionShaperExpression can only be generated for owned collections
                    // Complex properties are handled by CosmosShapedQueryCompilingExpressionVisitor.AddStructuralTypeInitialization
                    _ordinalParameterBindings[objectArrayAccess] = Add(
                        ordinalParameter, Constant(1, typeof(int)));

                    var innerShaper = (BlockExpression)Visit(collectionShaperExpression.InnerShaper);

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

        public void AddInclude(
            ParameterExpression parentJObjectVariable,
            ParameterExpression jObjectVariable,
            Expression tempValueBuffer,
            List<Expression> shaperExpressions,
            INavigation navigation,
            Expression navigationExpression,
            Expression instanceVariable,
            ParameterExpression ordinalParameter = null)
        {
            _projectionBindings[tempValueBuffer] = jObjectVariable;
            if (ordinalParameter != null)
            {
                _ordinalParameterBindings[tempValueBuffer] = ordinalParameter;
            }
            _ownerMappings[tempValueBuffer] = parentJObjectVariable;
            _ownerMappings[jObjectVariable] = parentJObjectVariable;

            // Cosmos does not support Includes for ISkipNavigation
            var includeMethod = navigation.IsCollection ? IncludeCollectionMethodInfo : IncludeReferenceMethodInfo;
            var includingClrType = navigation.DeclaringEntityType.ClrType;
            var relatedEntityClrType = navigation.TargetEntityType.ClrType;
#pragma warning disable EF1001 // Internal EF Core API usage.
            Expression entityEntryVariable = trackQueryResults
                ? ((BinaryExpression)new ExtractingExpressionVisitor().Extract(navigationExpression, x => x is BinaryExpression { NodeType: ExpressionType.Assign, Left: ParameterExpression parameter } && x.Type == typeof(InternalEntityEntry))).Left
                : Constant(null, typeof(InternalEntityEntry));
#pragma warning restore EF1001 // Internal EF Core API usage.

            var concreteEntityTypeVariable = shaperExpressions.Select(v => v is BinaryExpression { NodeType: ExpressionType.Assign, Left: ParameterExpression parameter } && v.Type == typeof(IEntityType) ? parameter : null).First(x => x != null);
            var inverseNavigation = navigation.Inverse;
            var fixup = GenerateFixup(
                includingClrType, relatedEntityClrType, navigation, inverseNavigation);

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
                        Constant(true))));
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
                        if (_ownerMappings.TryGetValue(jTokenExpression, out var ownerMapping))
                        {
                            return CreateGetValueExpression(ownerMapping, principalProperty, type);
                        }

                        // For nested projections, we may not have an owner mapping
                        // This only happens for non tracking queries, and theres only a check if the principal property is not null
                        // So instead rewrite to check the jobject for null.
                        if (_projectionBindings.TryGetValue(jTokenExpression, out var projectionBinding))
                        {
                            return projectionBinding;
                        }

                        // This should never happen right?
                        return jTokenExpression;
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

        private class ExtractingExpressionVisitor : ExpressionVisitor
        {
            private Expression _result;
            private Func<Expression, bool> _func;
            public Expression Extract(Expression expression, Func<Expression, bool> func)
            {
                _func = func;
                Visit(expression);
                return _result;
            }

            [return: NotNullIfNotNull("node")]
            public override Expression Visit(Expression node)
            {
                if (_func(node))
                {
                    _result = node;
                    return node;
                }

                return base.Visit(node);
            }
        }
    }
}
