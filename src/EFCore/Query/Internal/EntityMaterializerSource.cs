// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class EntityMaterializerSource : IEntityMaterializerSource
{
    private static readonly MethodInfo InjectableServiceInjectedMethod
        = typeof(IInjectableService).GetMethod(nameof(IInjectableService.Injected))!;

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
    [Obsolete("Use the overload that accepts an EntityMaterializerSourceParameters object.")]
    public virtual Expression CreateMaterializeExpression(
        IEntityType entityType,
        string entityInstanceName,
        Expression materializationContextExpression)
        => ((IEntityMaterializerSource)this).CreateMaterializeExpression(
            new EntityMaterializerSourceParameters(entityType, entityInstanceName, null), materializationContextExpression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    Expression IEntityMaterializerSource.CreateMaterializeExpression(
        EntityMaterializerSourceParameters parameters,
        Expression materializationContextExpression)
    {
        var (entityType, entityInstanceName) = (parameters.EntityType, parameters.EntityInstanceName);

        if (entityType.IsAbstract())
        {
            throw new InvalidOperationException(CoreStrings.CannotMaterializeAbstractType(entityType.DisplayName()));
        }

        var constructorBinding = ModifyBindings(entityType, entityType.ConstructorBinding!);
        var bindingInfo = new ParameterBindingInfo(parameters, materializationContextExpression);
        var serviceProperties = entityType.GetServiceProperties().ToList();
        var blockExpressions = new List<Expression>();

        var instanceVariable = Expression.Variable(constructorBinding.RuntimeType, entityInstanceName);
        bindingInfo.ServiceInstances.Add(instanceVariable);

        CreateServiceInstances(constructorBinding, bindingInfo, blockExpressions, serviceProperties);

        var properties = new HashSet<IPropertyBase>(
            serviceProperties.Cast<IPropertyBase>()
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
            if (properties.Count == 0 && blockExpressions.Count == 0)
            {
                return constructorExpression;
            }

            return CreateMaterializeExpression(blockExpressions, instanceVariable, constructorExpression, properties, bindingInfo);
        }

        return CreateInterceptionMaterializeExpression(
            entityType,
            properties,
            _materializationInterceptor,
            bindingInfo,
            constructorExpression,
            instanceVariable,
            blockExpressions);
    }

    private static void AddInitializeExpressions(
        HashSet<IPropertyBase> properties,
        ParameterBindingInfo bindingInfo,
        Expression instanceVariable,
        List<Expression> blockExpressions)
    {
        var valueBufferExpression = Expression.Call(
            bindingInfo.MaterializationContextExpression,
            MaterializationContext.GetValueBufferMethod);

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

    private static void AddAttachServiceExpressions(
        ParameterBindingInfo bindingInfo,
        Expression instanceVariable,
        List<Expression> blockExpressions)
    {
        var getContext = Expression.Property(bindingInfo.MaterializationContextExpression, MaterializationContext.ContextProperty);

        foreach (var serviceInstance in bindingInfo.ServiceInstances)
        {
            blockExpressions.Add(
                Expression.IfThen(
                    Expression.TypeIs(serviceInstance, typeof(IInjectableService)),
                    Expression.Call(
                        Expression.Convert(serviceInstance, typeof(IInjectableService)),
                        InjectableServiceInjectedMethod,
                        getContext,
                        instanceVariable,
                        Expression.Constant(bindingInfo, typeof(ParameterBindingInfo)))));
        }
    }

    private static readonly ConstructorInfo MaterializationInterceptionDataConstructor
        = typeof(MaterializationInterceptionData).GetDeclaredConstructor(
            new[]
            {
                typeof(MaterializationContext),
                typeof(IEntityType),
                typeof(QueryTrackingBehavior?),
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

    private static Expression CreateMaterializeExpression(
        List<Expression> blockExpressions,
        ParameterExpression instanceVariable,
        Expression constructorExpression,
        HashSet<IPropertyBase> properties,
        ParameterBindingInfo bindingInfo)
    {
        blockExpressions.Add(Expression.Assign(instanceVariable, constructorExpression));

        AddInitializeExpressions(properties, bindingInfo, instanceVariable, blockExpressions);
        AddAttachServiceExpressions(bindingInfo, instanceVariable, blockExpressions);

        blockExpressions.Add(instanceVariable);

        return Expression.Block(bindingInfo.ServiceInstances, blockExpressions);
    }

    private static Expression CreateInterceptionMaterializeExpression(
        IEntityType entityType,
        HashSet<IPropertyBase> properties,
        IMaterializationInterceptor materializationInterceptor,
        ParameterBindingInfo bindingInfo,
        Expression constructorExpression,
        ParameterExpression instanceVariable,
        List<Expression> blockExpressions)
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

        var materializationDataVariable = Expression.Variable(typeof(MaterializationInterceptionData), "materializationData");
        var creatingResultVariable = Expression.Variable(typeof(InterceptionResult<object>), "creatingResult");
        var interceptorExpression = Expression.Constant(materializationInterceptor, typeof(IMaterializationInterceptor));
        var accessorDictionaryVariable = Expression.Variable(
            typeof(Dictionary<IPropertyBase, (object, Func<MaterializationContext, object?>)>), "accessorDictionary");

        blockExpressions.Add(
            Expression.Assign(
                accessorDictionaryVariable,
                CreateAccessorDictionaryExpression()));
        blockExpressions.Add(
            Expression.Assign(
                materializationDataVariable,
                Expression.New(
                    MaterializationInterceptionDataConstructor,
                    bindingInfo.MaterializationContextExpression,
                    Expression.Constant(entityType),
                    Expression.Constant(bindingInfo.QueryTrackingBehavior, typeof(QueryTrackingBehavior?)),
                    accessorDictionaryVariable)));
        blockExpressions.Add(
            Expression.Assign(
                creatingResultVariable,
                Expression.Call(
                    interceptorExpression,
                    CreatingInstanceMethod,
                    materializationDataVariable,
                    Expression.Default(typeof(InterceptionResult<object>)))));
        blockExpressions.Add(
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
                    instanceVariable.Type)));
        blockExpressions.Add(
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
                    CreateInitializeExpression()));
        blockExpressions.Add(
            Expression.Assign(
                instanceVariable,
                Expression.Convert(
                    Expression.Call(
                        interceptorExpression,
                        InitializedInstanceMethod,
                        materializationDataVariable,
                        instanceVariable),
                    instanceVariable.Type)));

        return Expression.Block(
            bindingInfo.ServiceInstances.Concat(new[] { accessorDictionaryVariable, materializationDataVariable, creatingResultVariable }),
            blockExpressions);

        BlockExpression CreateAccessorDictionaryExpression()
        {
            var dictionaryVariable = Expression.Variable(
                typeof(Dictionary<IPropertyBase, (object, Func<MaterializationContext, object?>)>), "dictionary");
            var valueBufferExpression = Expression.Call(
                bindingInfo.MaterializationContextExpression, MaterializationContext.GetValueBufferMethod);
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
                                (ParameterExpression)bindingInfo.MaterializationContextExpression),
                            Expression.Lambda<Func<MaterializationContext, object?>>(
                                Expression.Convert(CreateAccessorReadExpression(), typeof(object)),
                                (ParameterExpression)bindingInfo.MaterializationContextExpression))));

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

            AddInitializeExpressions(properties, bindingInfo, instanceVariable, initializeBlockExpressions);
            AddAttachServiceExpressions(bindingInfo, instanceVariable, blockExpressions);

            return Expression.Block(initializeBlockExpressions);
        }
    }

    private ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>> Materializers
        => LazyInitializer.EnsureInitialized(
            ref _materializers,
            () => new ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>>());

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
                        ((IEntityMaterializerSource)self).CreateMaterializeExpression(
                            new EntityMaterializerSourceParameters(e, "instance", null), materializationContextParameter),
                        materializationContextParameter)
                    .Compile();
            },
            this);

    private ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>> EmptyMaterializers
        => LazyInitializer.EnsureInitialized(
            ref _emptyMaterializers,
            () => new ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>>());

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
                var bindingInfo = new ParameterBindingInfo(
                    new EntityMaterializerSourceParameters(e, "instance", null), materializationContextExpression);

                var blockExpressions = new List<Expression>();
                var instanceVariable = Expression.Variable(binding.RuntimeType, "instance");
                var serviceProperties = e.GetServiceProperties().ToList();
                bindingInfo.ServiceInstances.Add(instanceVariable);

                CreateServiceInstances(binding, bindingInfo, blockExpressions, serviceProperties);

                var constructorExpression = binding.CreateConstructorExpression(bindingInfo);

                var properties = new HashSet<IPropertyBase>(serviceProperties);
                foreach (var consumedProperty in binding.ParameterBindings.SelectMany(p => p.ConsumedProperties))
                {
                    properties.Remove(consumedProperty);
                }

                return Expression.Lambda<Func<MaterializationContext, object>>(
                        self._materializationInterceptor == null
                            ? properties.Count == 0 && blockExpressions.Count == 0
                                ? constructorExpression
                                : CreateMaterializeExpression(
                                    blockExpressions, instanceVariable, constructorExpression, properties, bindingInfo)
                            : CreateInterceptionMaterializeExpression(
                                e,
                                new HashSet<IPropertyBase>(),
                                self._materializationInterceptor,
                                bindingInfo,
                                constructorExpression,
                                instanceVariable,
                                blockExpressions),
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

    private static void CreateServiceInstances(
        InstantiationBinding constructorBinding,
        ParameterBindingInfo bindingInfo,
        List<Expression> blockExpressions,
        List<IServiceProperty> serviceProperties)
    {
        foreach (var parameterBinding in constructorBinding.ParameterBindings.OfType<ServiceParameterBinding>())
        {
            if (bindingInfo.ServiceInstances.All(s => s.Type != parameterBinding.ServiceType))
            {
                var variable = Expression.Variable(parameterBinding.ServiceType);
                blockExpressions.Add(Expression.Assign(variable, parameterBinding.BindToParameter(bindingInfo)));
                bindingInfo.ServiceInstances.Add(variable);
            }
        }

        foreach (var serviceProperty in serviceProperties)
        {
            var serviceType = serviceProperty.ParameterBinding.ServiceType;
            if (bindingInfo.ServiceInstances.All(e => e.Type != serviceType))
            {
                var variable = Expression.Variable(serviceType);
                blockExpressions.Add(Expression.Assign(variable, serviceProperty.ParameterBinding.BindToParameter(bindingInfo)));
                bindingInfo.ServiceInstances.Add(variable);
            }
        }
    }
}
