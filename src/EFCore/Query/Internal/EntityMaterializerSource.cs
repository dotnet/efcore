// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class EntityMaterializerSource : IEntityMaterializerSource
{
    private ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>>? _materializers;
    private ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>>? _emptyMaterializers;
    private readonly List<IInstantiationBindingInterceptor> _bindingInterceptors;
    private readonly IMaterializationInterceptor? _materializationInterceptor;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityMaterializerSource(EntityMaterializerSourceDependencies dependencies)
    {
        Dependencies = dependencies;
        _bindingInterceptors = dependencies.SingletonInterceptors.OfType<IInstantiationBindingInterceptor>().ToList();

        _materializationInterceptor =
            (IMaterializationInterceptor?)new MaterializationInterceptorAggregator().AggregateInterceptors(
                dependencies.SingletonInterceptors.OfType<IMaterializationInterceptor>().ToList());
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual EntityMaterializerSourceDependencies Dependencies { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression CreateMaterializeExpression(
        IEntityType entityType,
        string entityInstanceName,
        Expression materializationContextExpression)
    {
        if (entityType.IsAbstract())
        {
            throw new InvalidOperationException(CoreStrings.CannotMaterializeAbstractType(entityType.DisplayName()));
        }

        var constructorBinding = ModifyBindings(entityType, entityType.ConstructorBinding!);

        var bindingInfo = new ParameterBindingInfo(
            entityType,
            materializationContextExpression);

        var properties = new HashSet<IPropertyBase>(
            entityType.GetServiceProperties().Cast<IPropertyBase>()
                .Concat(
                    entityType
                        .GetProperties()
                        .Where(p => !p.IsShadowProperty())));

        foreach (var consumedProperty in constructorBinding
                     .ParameterBindings
                     .SelectMany(p => p.ConsumedProperties))
        {
            properties.Remove(consumedProperty);
        }

        var constructorExpression = constructorBinding.CreateConstructorExpression(bindingInfo);

        if (_materializationInterceptor == null)
        {
            if (properties.Count == 0)
            {
                return constructorExpression;
            }

            var instanceVariable = Expression.Variable(constructorBinding.RuntimeType, entityInstanceName);

            var blockExpressions
                = new List<Expression>
                {
                    Expression.Assign(
                        instanceVariable,
                        constructorExpression)
                };

            AddInitializeExpressions(properties, bindingInfo, materializationContextExpression, instanceVariable, blockExpressions);

            blockExpressions.Add(instanceVariable);

            return Expression.Block(new[] { instanceVariable }, blockExpressions);
        }

        return CreateInterceptionMaterializeExpression(
            entityType,
            entityInstanceName,
            properties,
            _materializationInterceptor,
            constructorBinding,
            bindingInfo,
            constructorExpression,
            materializationContextExpression);
    }

    private static void AddInitializeExpressions(
        HashSet<IPropertyBase> properties,
        ParameterBindingInfo bindingInfo,
        Expression materializationContextExpression,
        Expression instanceVariable,
        List<Expression> blockExpressions)
    {
        var valueBufferExpression = Expression.Call(materializationContextExpression, MaterializationContext.GetValueBufferMethod);

        foreach (var property in properties)
        {
            var memberInfo = property.GetMemberInfo(forMaterialization: true, forSet: true);

            blockExpressions.Add(
                CreateMemberAssignment(
                    instanceVariable, memberInfo, property, property is IServiceProperty serviceProperty
                        ? serviceProperty.ParameterBinding.BindToParameter(bindingInfo)
                        : valueBufferExpression.CreateValueBufferReadValueExpression(
                            memberInfo.GetMemberType(),
                            property.GetIndex(),
                            property)));
        }

        static Expression CreateMemberAssignment(Expression parameter, MemberInfo memberInfo, IPropertyBase property, Expression value)
            => property.IsIndexerProperty()
                ? Expression.Assign(
                    Expression.MakeIndex(
                        parameter, (PropertyInfo)memberInfo, new List<Expression> { Expression.Constant(property.Name) }),
                    value)
                : Expression.MakeMemberAccess(parameter, memberInfo).Assign(value);
    }

    private static readonly ConstructorInfo MaterializationInterceptionDataConstructor
        = typeof(MaterializationInterceptionData).GetDeclaredConstructor(
            new[]
            {
                typeof(MaterializationContext),
                typeof(IEntityType),
                typeof(Dictionary<IPropertyBase, (object, Func<MaterializationContext, object?>)>)
            })!;

    private static readonly MethodInfo CreatingInstanceMethod
        = typeof(IMaterializationInterceptor).GetMethod(nameof(IMaterializationInterceptor.CreatingInstance))!;

    private static readonly MethodInfo CreatedInstanceMethod
        = typeof(IMaterializationInterceptor).GetMethod(nameof(IMaterializationInterceptor.CreatedInstance))!;

    private static readonly MethodInfo InitializingInstanceMethod
        = typeof(IMaterializationInterceptor).GetMethod(nameof(IMaterializationInterceptor.InitializingInstance))!;

    private static readonly MethodInfo InitializedInstanceMethod
        = typeof(IMaterializationInterceptor).GetMethod(nameof(IMaterializationInterceptor.InitializedInstance))!;

    private static readonly PropertyInfo HasResultMethod
        = typeof(InterceptionResult<object>).GetProperty(nameof(InterceptionResult<object>.HasResult))!;

    private static readonly PropertyInfo ResultProperty
        = typeof(InterceptionResult<object>).GetProperty(nameof(InterceptionResult<object>.Result))!;

    private static readonly PropertyInfo IsSuppressedProperty
        = typeof(InterceptionResult).GetProperty(nameof(InterceptionResult.IsSuppressed))!;

    private static readonly MethodInfo DictionaryAddMethod
        = typeof(Dictionary<IPropertyBase, (object, Func<MaterializationContext, object?>)>).GetMethod(
            nameof(Dictionary<IPropertyBase, object>.Add),
            new[] { typeof(IPropertyBase), typeof((object, Func<MaterializationContext, object?>)) })!;

    private static readonly ConstructorInfo DictionaryConstructor
        = typeof(ValueTuple<object, Func<MaterializationContext, object?>>).GetConstructor(
            new[] { typeof(object), typeof(Func<MaterializationContext, object?>) })!;

    private static Expression CreateInterceptionMaterializeExpression(
        IEntityType entityType,
        string entityInstanceName,
        HashSet<IPropertyBase> properties,
        IMaterializationInterceptor materializationInterceptor,
        InstantiationBinding constructorBinding,
        ParameterBindingInfo bindingInfo,
        Expression constructorExpression,
        Expression materializationContextExpression)
    {
        // Something like:
        // Dictionary<IPropertyBase, (object, Func<MaterializationContext, object?>)> accessorFactory = CreateAccessors()
        // var creatingResult = interceptor.CreatingInstance(materializationData, new InterceptionResult<object>());
        //
        // var instance = interceptor.CreatedInstance(materializationData,
        //     creatingResult.HasResult ? creatingResult.Result : create(materializationContext));
        //
        // if (!interceptor.InitializingInstance(materializationData, instance, default(InterceptionResult)).IsSuppressed)
        // {
        //     initialize(materializationContext, instance);
        // }
        //
        // instance = interceptor.InitializedInstance(materializationData, instance);
        //
        // return instance;

        var instanceVariable = Expression.Variable(constructorBinding.RuntimeType, entityInstanceName);
        var materializationDataVariable = Expression.Variable(typeof(MaterializationInterceptionData), "materializationData");
        var creatingResultVariable = Expression.Variable(typeof(InterceptionResult<object>), "creatingResult");
        var interceptorExpression = Expression.Constant(materializationInterceptor, typeof(IMaterializationInterceptor));
        var accessorDictionaryVariable = Expression.Variable(
            typeof(Dictionary<IPropertyBase, (object, Func<MaterializationContext, object?>)>), "accessorDictionary");

        var blockExpressions = new List<Expression>
        {
            Expression.Assign(
                accessorDictionaryVariable,
                CreateAccessorDictionaryExpression()),
            Expression.Assign(
                materializationDataVariable,
                Expression.New(
                    MaterializationInterceptionDataConstructor,
                    materializationContextExpression,
                    Expression.Constant(entityType),
                    accessorDictionaryVariable)),
            Expression.Assign(
                creatingResultVariable,
                Expression.Call(
                    interceptorExpression,
                    CreatingInstanceMethod,
                    materializationDataVariable,
                    Expression.Default(typeof(InterceptionResult<object>)))),
            Expression.Assign(
                instanceVariable,
                Expression.Convert(
                    Expression.Call(
                        interceptorExpression,
                        CreatedInstanceMethod,
                        materializationDataVariable,
                        Expression.Condition(
                            Expression.Property(
                                creatingResultVariable,
                                HasResultMethod),
                            Expression.Convert(
                                Expression.Property(
                                    creatingResultVariable,
                                    ResultProperty),
                                instanceVariable.Type),
                            constructorExpression)),
                    instanceVariable.Type)),
            properties.Count == 0
                ? Expression.Call(
                    interceptorExpression,
                    InitializingInstanceMethod,
                    materializationDataVariable,
                    instanceVariable,
                    Expression.Default(typeof(InterceptionResult)))
                : Expression.IfThen(
                    Expression.Not(
                        Expression.Property(
                            Expression.Call(
                                interceptorExpression,
                                InitializingInstanceMethod,
                                materializationDataVariable,
                                instanceVariable,
                                Expression.Default(typeof(InterceptionResult))),
                            IsSuppressedProperty)),
                    CreateInitializeExpression()),
            Expression.Assign(
                instanceVariable,
                Expression.Convert(
                    Expression.Call(
                        interceptorExpression,
                        InitializedInstanceMethod,
                        materializationDataVariable,
                        instanceVariable),
                    instanceVariable.Type))
        };

        blockExpressions.Add(instanceVariable);

        return Expression.Block(
            new[] { accessorDictionaryVariable, instanceVariable, materializationDataVariable, creatingResultVariable },
            blockExpressions);

        BlockExpression CreateAccessorDictionaryExpression()
        {
            var dictionaryVariable = Expression.Variable(
                typeof(Dictionary<IPropertyBase, (object, Func<MaterializationContext, object?>)>), "dictionary");
            var valueBufferExpression = Expression.Call(materializationContextExpression, MaterializationContext.GetValueBufferMethod);
            var snapshotBlockExpressions = new List<Expression>
            {
                Expression.Assign(
                    dictionaryVariable,
                    Expression.New(
                        typeof(Dictionary<IPropertyBase, (object, Func<MaterializationContext, object?>)>)
                            .GetConstructor(Type.EmptyTypes)!))
            };

            foreach (var property in entityType.GetServiceProperties().Cast<IPropertyBase>().Concat(entityType.GetProperties()))
            {
                snapshotBlockExpressions.Add(
                    Expression.Call(
                        dictionaryVariable,
                        DictionaryAddMethod,
                        Expression.Constant(property),
                        Expression.New(
                            DictionaryConstructor,
                            Expression.Lambda(
                                typeof(Func<,>).MakeGenericType(typeof(MaterializationContext), property.ClrType),
                                CreateAccessorReadExpression(),
                                (ParameterExpression)materializationContextExpression),
                            Expression.Lambda<Func<MaterializationContext, object?>>(
                                Expression.Convert(CreateAccessorReadExpression(), typeof(object)),
                                (ParameterExpression)materializationContextExpression))));

                Expression CreateAccessorReadExpression()
                    => property is IServiceProperty serviceProperty
                        ? serviceProperty.ParameterBinding.BindToParameter(bindingInfo)
                        : valueBufferExpression
                            .CreateValueBufferReadValueExpression(
                                property.ClrType,
                                property.GetIndex(),
                                property);
            }

            snapshotBlockExpressions.Add(dictionaryVariable);

            return Expression.Block(new[] { dictionaryVariable }, snapshotBlockExpressions);
        }

        BlockExpression CreateInitializeExpression()
        {
            var initializeBlockExpressions = new List<Expression>();

            AddInitializeExpressions(
                properties,
                bindingInfo,
                materializationContextExpression,
                instanceVariable,
                initializeBlockExpressions);

            return Expression.Block(initializeBlockExpressions);
        }
    }

    private ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>> Materializers
        => LazyInitializer.EnsureInitialized(
            ref _materializers,
            () => new());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<MaterializationContext, object> GetMaterializer(
        IEntityType entityType)
        => Materializers.GetOrAdd(
            entityType,
            static (e, self) =>
            {
                var materializationContextParameter
                    = Expression.Parameter(typeof(MaterializationContext), "materializationContext");

                return Expression.Lambda<Func<MaterializationContext, object>>(
                        self.CreateMaterializeExpression(e, "instance", materializationContextParameter),
                        materializationContextParameter)
                    .Compile();
            },
            this);

    private ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>> EmptyMaterializers
        => LazyInitializer.EnsureInitialized(
            ref _emptyMaterializers,
            () => new());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<MaterializationContext, object> GetEmptyMaterializer(
        IEntityType entityType)
        => EmptyMaterializers.GetOrAdd(
            entityType,
            static (e, self) =>
            {
                var binding = e.ServiceOnlyConstructorBinding;
                if (binding == null)
                {
                    var _ = e.ConstructorBinding;
                    binding = e.ServiceOnlyConstructorBinding;
                    if (binding == null)
                    {
                        throw new InvalidOperationException(CoreStrings.NoParameterlessConstructor(e.DisplayName()));
                    }
                }

                binding = self.ModifyBindings(e, binding);

                var materializationContextExpression = Expression.Parameter(typeof(MaterializationContext), "mc");
                var bindingInfo = new ParameterBindingInfo(e, materializationContextExpression);
                var constructorExpression = binding.CreateConstructorExpression(bindingInfo);

                return Expression.Lambda<Func<MaterializationContext, object>>(
                        self._materializationInterceptor == null
                            ? constructorExpression
                            : CreateInterceptionMaterializeExpression(
                                e,
                                "instance",
                                new(),
                                self._materializationInterceptor,
                                binding,
                                bindingInfo,
                                constructorExpression,
                                materializationContextExpression),
                        materializationContextExpression)
                    .Compile();
            },
            this);

    private InstantiationBinding ModifyBindings(IEntityType entityType, InstantiationBinding binding)
    {
        var interceptionData = new InstantiationBindingInterceptionData(entityType);
        foreach (var bindingInterceptor in _bindingInterceptors)
        {
            binding = bindingInterceptor.ModifyBinding(interceptionData, binding);
        }

        return binding;
    }
}
