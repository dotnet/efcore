// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class StructuralTypeMaterializerSource : IStructuralTypeMaterializerSource
{
    private static readonly MethodInfo InjectableServiceInjectedMethod
        = typeof(IInjectableService).GetMethod(nameof(IInjectableService.Injected))!;

    private readonly List<IInstantiationBindingInterceptor> _bindingInterceptors;
    private readonly IMaterializationInterceptor? _materializationInterceptor;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly MethodInfo PopulateListMethod
        = typeof(StructuralTypeMaterializerSource).GetMethod(
            nameof(PopulateList), BindingFlags.Public | BindingFlags.Static)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StructuralTypeMaterializerSource(StructuralTypeMaterializerSourceDependencies dependencies)
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
    protected virtual StructuralTypeMaterializerSourceDependencies Dependencies { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Expression CreateMaterializeExpression(
        StructuralTypeMaterializerSourceParameters parameters,
        Expression materializationContextExpression)
    {
        var (structuralType, entityInstanceName) = (parameters.StructuralType, parameters.InstanceName);

        if (structuralType.IsAbstract())
        {
            throw new InvalidOperationException(CoreStrings.CannotMaterializeAbstractType(structuralType.DisplayName()));
        }

        var constructorBinding = ModifyBindings(structuralType, structuralType.ConstructorBinding!);
        var bindingInfo = new ParameterBindingInfo(parameters, materializationContextExpression);
        var instanceVariable = Variable(constructorBinding.RuntimeType, entityInstanceName);
        bindingInfo.ServiceInstances.Add(instanceVariable);

        var properties = new HashSet<IPropertyBase>(
            structuralType.GetProperties().Cast<IPropertyBase>().Where(p => !p.IsShadowProperty())
                .Concat(structuralType.GetComplexProperties().Where(p => !p.IsShadowProperty())));

        var blockExpressions = new List<Expression>();
        if (structuralType is IEntityType entityType)
        {
            var serviceProperties = entityType.GetServiceProperties().ToList();
            CreateServiceInstances(constructorBinding, bindingInfo, blockExpressions, serviceProperties);

            foreach (var serviceProperty in serviceProperties)
            {
                properties.Add(serviceProperty);
            }
        }

        foreach (var consumedProperty in constructorBinding.ParameterBindings.SelectMany(p => p.ConsumedProperties))
        {
            properties.Remove(consumedProperty);
        }

        var constructorExpression = constructorBinding.CreateConstructorExpression(bindingInfo);
        var getValueBufferExpression = Call(bindingInfo.MaterializationContextExpression, MaterializationContext.GetValueBufferMethod);
        var materializationExpression = _materializationInterceptor == null
            // TODO: This currently applies the materialization interceptor only on the root structural type - any contained complex types
            // don't get intercepted. #35883
            || structuralType is not IEntityType
            ? properties.Count == 0 && blockExpressions.Count == 0
                ? constructorExpression
                : CreateMaterializeExpression(blockExpressions, instanceVariable, constructorExpression, getValueBufferExpression, properties, bindingInfo)
            : CreateInterceptionMaterializeExpression(
                structuralType,
                properties,
                _materializationInterceptor,
                bindingInfo,
                constructorExpression,
                getValueBufferExpression,
                instanceVariable,
                blockExpressions);

        return structuralType is IComplexType complexType
            && ReadComplexTypeDirectly(complexType)
            && parameters.ClrType.IsNullableType()
            ? HandleNullableComplexTypeMaterialization(
                complexType,
                parameters.ClrType,
                materializationExpression,
                getValueBufferExpression)
            : materializationExpression;

        // Creates a conditional expression that handles materialization of nullable complex types.
        // For nullable complex types, the method checks if all scalar properties are null
        // and returns default if they are, otherwise materializes the complex type instance.
        // If there's a required (non-nullable) property, only that property is checked for efficiency.
        Expression HandleNullableComplexTypeMaterialization(
            IComplexType complexType,
            Type clrType,
            Expression materializeExpression,
            MethodCallExpression getValueBufferExpression)
        {
            // Get all scalar properties of the complex type (including nested ones).
            var allScalarProperties = complexType.GetFlattenedProperties().ToList();

            var requiredProperty = allScalarProperties.Where(p => !p.IsNullable).FirstOrDefault();
            var nullCheck = requiredProperty is not null
                // If there's a required property, it's enough to check just that one for null.
                ? Equal(
                    getValueBufferExpression.CreateValueBufferReadValueExpression(typeof(object), requiredProperty.GetIndex(), requiredProperty),
                    Constant(null, typeof(object)))
                // Create null checks for all scalar properties.
                : allScalarProperties
                    .Select(p =>
                        Equal(
                            getValueBufferExpression.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                            Constant(null, typeof(object))))
                    .Aggregate(AndAlso);

            // If property/properties are null, return default (to handle structs); otherwise materialize the complex type.
            return Condition(
                nullCheck,
                Default(clrType),
                // Materialization expression is always returning a non-nullable type, so we need to convert it to nullable if necessary.
                clrType.IsNullableType()
                    ? Convert(materializeExpression, clrType)
                    : materializeExpression);
        }
    }

    /// <summary>
    ///     Should complex type be read directly using e.g. DbDataReader.GetFieldValue
    ///     or is it going to be handled separately (i.e. relational JSON).
    /// </summary>
    protected virtual bool ReadComplexTypeDirectly(IComplexType complexType)
        => true;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void AddInitializeExpression(
        IPropertyBase property,
        ParameterBindingInfo bindingInfo,
        Expression instanceVariable,
        MethodCallExpression getValueBufferExpression,
        List<Expression> blockExpressions)
    {
        if (property is IComplexProperty cp && !ReadComplexTypeDirectly(cp.ComplexType))
        {
            return;
        }

        var memberInfo = property.GetMemberInfo(forMaterialization: true, forSet: true);

        var valueExpression = property switch
        {
            IProperty p
                => getValueBufferExpression.CreateValueBufferReadValueExpression(memberInfo.GetMemberType(), p.GetIndex(), p),

            IServiceProperty serviceProperty
                => serviceProperty.ParameterBinding.BindToParameter(bindingInfo),

            IComplexProperty { IsCollection: true } complexProperty
                => Default(complexProperty.ClrType), // Initialize collections to null, they'll be populated separately

            IComplexProperty complexProperty
                => CreateMaterializeExpression(
                    new StructuralTypeMaterializerSourceParameters(complexProperty.ComplexType, "complexType", complexProperty.ClrType, QueryTrackingBehavior: null),
                    bindingInfo.MaterializationContextExpression),

            _ => throw new UnreachableException()
        };

        blockExpressions.Add(CreateMemberAssignment(instanceVariable, memberInfo, property, valueExpression));

        static Expression CreateMemberAssignment(Expression parameter, MemberInfo memberInfo, IPropertyBase property, Expression value)
        {
            if (property is IProperty { IsPrimitiveCollection: true, ClrType.IsArray: false })
            {
                var elementType = property.ClrType.TryGetElementType(typeof(IEnumerable<>))!;
                var iCollectionInterface = typeof(ICollection<>).MakeGenericType(elementType);
                if (iCollectionInterface.IsAssignableFrom(property.ClrType))
                {
                    var genericMethod = PopulateListMethod.MakeGenericMethod(elementType);
                    var currentVariable = Variable(property.ClrType);
                    var convertedVariable = genericMethod.GetParameters()[1].ParameterType.IsAssignableFrom(currentVariable.Type)
                        ? (Expression)currentVariable
                        : Convert(currentVariable, genericMethod.GetParameters()[1].ParameterType);
                    return Block(
                        [currentVariable],
                        Assign(
                            currentVariable,
                            MakeMemberAccess(parameter, property.GetMemberInfo(forMaterialization: true, forSet: false))),
                        IfThenElse(
                            OrElse(
                                OrElse(
                                    ReferenceEqual(currentVariable, Constant(null)),
                                    ReferenceEqual(value, Constant(null))),
                                MakeMemberAccess(
                                    currentVariable,
                                    iCollectionInterface.GetProperty(nameof(ICollection<>.IsReadOnly))!)),
                            MakeMemberAccess(parameter, memberInfo).Assign(value),
                            Call(
                                genericMethod,
                                value,
                                convertedVariable)
                        ));
                }
            }

            return property.IsIndexerProperty()
                ? Assign(
                    MakeIndex(parameter, (PropertyInfo)memberInfo, [Constant(property.Name)]),
                    value)
                : MakeMemberAccess(parameter, memberInfo).Assign(value);
        }
    }

    private void AddInitializeExpressions(
        HashSet<IPropertyBase> properties,
        ParameterBindingInfo bindingInfo,
        Expression instanceVariable,
        MethodCallExpression getValueBufferExpression,
        List<Expression> blockExpressions)
    {
        foreach (var property in properties)
        {
            AddInitializeExpression(property, bindingInfo, instanceVariable, getValueBufferExpression, blockExpressions);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IList<T> PopulateList<T>(IEnumerable<T> buffer, IList<T> target)
    {
        target.Clear();
        foreach (var value in buffer)
        {
            target.Add(value);
        }

        return target;
    }

    private static void AddAttachServiceExpressions(
        ParameterBindingInfo bindingInfo,
        Expression instanceVariable,
        List<Expression> blockExpressions)
    {
        var getContext = Property(bindingInfo.MaterializationContextExpression, MaterializationContext.ContextProperty);

        foreach (var serviceInstance in bindingInfo.ServiceInstances)
        {
            blockExpressions.Add(
                IfThen(
                    TypeIs(serviceInstance, typeof(IInjectableService)),
                    Call(
                        Convert(serviceInstance, typeof(IInjectableService)),
                        InjectableServiceInjectedMethod,
                        getContext,
                        instanceVariable,
                        Constant(bindingInfo.QueryTrackingBehavior, typeof(QueryTrackingBehavior?)),
                        Constant(bindingInfo.StructuralType))));
        }
    }

    private static readonly ConstructorInfo MaterializationInterceptionDataConstructor
        = typeof(MaterializationInterceptionData).GetDeclaredConstructor(
        [
            typeof(MaterializationContext),
            typeof(IEntityType),
            typeof(QueryTrackingBehavior?),
            typeof(Dictionary<IPropertyBase, (object, Func<MaterializationContext, object?>)>)
        ])!;

    private static readonly MethodInfo CreatingInstanceMethod
        = typeof(IMaterializationInterceptor).GetMethod(nameof(IMaterializationInterceptor.CreatingInstance))!;

    private static readonly MethodInfo CreatedInstanceMethod
        = typeof(IMaterializationInterceptor).GetMethod(nameof(IMaterializationInterceptor.CreatedInstance))!;

    private static readonly MethodInfo InitializingInstanceMethod
        = typeof(IMaterializationInterceptor).GetMethod(nameof(IMaterializationInterceptor.InitializingInstance))!;

    private static readonly MethodInfo InitializedInstanceMethod
        = typeof(IMaterializationInterceptor).GetMethod(nameof(IMaterializationInterceptor.InitializedInstance))!;

    private static readonly PropertyInfo HasResultMethod
        = typeof(InterceptionResult<object>).GetProperty(nameof(InterceptionResult<>.HasResult))!;

    private static readonly PropertyInfo ResultProperty
        = typeof(InterceptionResult<object>).GetProperty(nameof(InterceptionResult<>.Result))!;

    private static readonly PropertyInfo IsSuppressedProperty
        = typeof(InterceptionResult).GetProperty(nameof(InterceptionResult.IsSuppressed))!;

    private static readonly MethodInfo DictionaryAddMethod
        = typeof(Dictionary<IPropertyBase, (object, Func<MaterializationContext, object?>)>).GetMethod(
            nameof(Dictionary<,>.Add),
            [typeof(IPropertyBase), typeof((object, Func<MaterializationContext, object?>))])!;

    private static readonly ConstructorInfo DictionaryConstructor
        = typeof(ValueTuple<object, Func<MaterializationContext, object?>>).GetConstructor(
            [typeof(object), typeof(Func<MaterializationContext, object?>)])!;

    private Expression CreateMaterializeExpression(
        List<Expression> blockExpressions,
        ParameterExpression instanceVariable,
        Expression constructorExpression,
        MethodCallExpression getValueBufferExpression,
        HashSet<IPropertyBase> properties,
        ParameterBindingInfo bindingInfo)
    {
        blockExpressions.Add(Assign(instanceVariable, constructorExpression));

        AddInitializeExpressions(properties, bindingInfo, instanceVariable, getValueBufferExpression, blockExpressions);

        if (bindingInfo.StructuralType is IEntityType)
        {
            AddAttachServiceExpressions(bindingInfo, instanceVariable, blockExpressions);
        }

        blockExpressions.Add(instanceVariable);

        return Block(bindingInfo.ServiceInstances, blockExpressions);
    }

    private Expression CreateInterceptionMaterializeExpression(
        ITypeBase structuralType,
        HashSet<IPropertyBase> properties,
        IMaterializationInterceptor materializationInterceptor,
        ParameterBindingInfo bindingInfo,
        Expression constructorExpression,
        MethodCallExpression getValueBufferExpression,
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

        var materializationDataVariable = Variable(typeof(MaterializationInterceptionData), "materializationData");
        var creatingResultVariable = Variable(typeof(InterceptionResult<object>), "creatingResult");
        var interceptorExpression = Constant(materializationInterceptor, typeof(IMaterializationInterceptor));
        var accessorDictionaryVariable = Variable(
            typeof(Dictionary<IPropertyBase, (object, Func<MaterializationContext, object?>)>), "accessorDictionary");

        blockExpressions.Add(
            Assign(
                accessorDictionaryVariable,
                CreateAccessorDictionaryExpression()));
        blockExpressions.Add(
            Assign(
                materializationDataVariable,
                New(
                    MaterializationInterceptionDataConstructor,
                    bindingInfo.MaterializationContextExpression,
                    Constant(structuralType),
                    Constant(bindingInfo.QueryTrackingBehavior, typeof(QueryTrackingBehavior?)),
                    accessorDictionaryVariable)));
        blockExpressions.Add(
            Assign(
                creatingResultVariable,
                Call(
                    interceptorExpression,
                    CreatingInstanceMethod,
                    materializationDataVariable,
                    Default(typeof(InterceptionResult<object>)))));
        blockExpressions.Add(
            Assign(
                instanceVariable,
                Convert(
                    Call(
                        interceptorExpression,
                        CreatedInstanceMethod,
                        materializationDataVariable,
                        Condition(
                            Property(
                                creatingResultVariable,
                                HasResultMethod),
                            Convert(
                                Property(
                                    creatingResultVariable,
                                    ResultProperty),
                                instanceVariable.Type),
                            constructorExpression)),
                    instanceVariable.Type)));
        blockExpressions.Add(
            properties.Count == 0
                ? Call(
                    interceptorExpression,
                    InitializingInstanceMethod,
                    materializationDataVariable,
                    instanceVariable,
                    Default(typeof(InterceptionResult)))
                : IfThen(
                    Not(
                        Property(
                            Call(
                                interceptorExpression,
                                InitializingInstanceMethod,
                                materializationDataVariable,
                                instanceVariable,
                                Default(typeof(InterceptionResult))),
                            IsSuppressedProperty)),
                    CreateInitializeExpression()));
        blockExpressions.Add(
            Assign(
                instanceVariable,
                Convert(
                    Call(
                        interceptorExpression,
                        InitializedInstanceMethod,
                        materializationDataVariable,
                        instanceVariable),
                    instanceVariable.Type)));
        blockExpressions.Add(instanceVariable);

        return Block(
            bindingInfo.ServiceInstances.Concat([accessorDictionaryVariable, materializationDataVariable, creatingResultVariable]),
            blockExpressions);

        BlockExpression CreateAccessorDictionaryExpression()
        {
            var dictionaryVariable = Variable(
                typeof(Dictionary<IPropertyBase, (object, Func<MaterializationContext, object?>)>), "dictionary");
            var snapshotBlockExpressions = new List<Expression>
            {
                Assign(
                    dictionaryVariable,
                    New(
                        typeof(Dictionary<IPropertyBase, (object, Func<MaterializationContext, object?>)>)
                            .GetConstructor(Type.EmptyTypes)!))
            };

            if (structuralType is IEntityType entityType)
            {
                foreach (var property in entityType.GetServiceProperties().Cast<IPropertyBase>().Concat(structuralType.GetProperties()))
                {
                    snapshotBlockExpressions.Add(
                        Call(
                            dictionaryVariable,
                            DictionaryAddMethod,
                            Constant(property),
                            New(
                                DictionaryConstructor,
                                Lambda(
                                    typeof(Func<,>).MakeGenericType(typeof(MaterializationContext), property.ClrType),
                                    CreateAccessorReadExpression(),
                                    (ParameterExpression)bindingInfo.MaterializationContextExpression),
                                Lambda<Func<MaterializationContext, object?>>(
                                    Convert(CreateAccessorReadExpression(), typeof(object)),
                                    (ParameterExpression)bindingInfo.MaterializationContextExpression))));

                    Expression CreateAccessorReadExpression()
                        => property is IServiceProperty serviceProperty
                            ? serviceProperty.ParameterBinding.BindToParameter(bindingInfo)
                            : (property as IProperty)?.IsPrimaryKey() == true
                                ? Convert(
                                    getValueBufferExpression.CreateValueBufferReadValueExpression(
                                        typeof(object),
                                        property.GetIndex(),
                                        property),
                                    property.ClrType)
                                : getValueBufferExpression.CreateValueBufferReadValueExpression(
                                    property.ClrType,
                                    property.GetIndex(),
                                    property);
                }
            }

            snapshotBlockExpressions.Add(dictionaryVariable);

            return Block([dictionaryVariable], snapshotBlockExpressions);
        }

        BlockExpression CreateInitializeExpression()
        {
            var initializeBlockExpressions = new List<Expression>();

            AddInitializeExpressions(properties, bindingInfo, instanceVariable, getValueBufferExpression, initializeBlockExpressions);

            if (bindingInfo.StructuralType is IEntityType)
            {
                AddAttachServiceExpressions(bindingInfo, instanceVariable, blockExpressions);
            }

            return Block(initializeBlockExpressions);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<MaterializationContext, object> GetMaterializer(IEntityType entityType)
    {
        var materializationContextParameter
            = Parameter(typeof(MaterializationContext), "materializationContext");

        return Lambda<Func<MaterializationContext, object>>(
                ((IStructuralTypeMaterializerSource)this).CreateMaterializeExpression(
                    new StructuralTypeMaterializerSourceParameters(entityType, "instance", entityType.ClrType, null), materializationContextParameter),
                materializationContextParameter)
            .Compile();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<MaterializationContext, object> GetMaterializer(IComplexType complexType)
    {
        var materializationContextParameter = Parameter(typeof(MaterializationContext), "materializationContext");

        return Lambda<Func<MaterializationContext, object>>(
                ((IStructuralTypeMaterializerSource)this).CreateMaterializeExpression(
                    new StructuralTypeMaterializerSourceParameters(complexType, "instance", complexType.ClrType, null), materializationContextParameter),
                materializationContextParameter)
            .Compile();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<MaterializationContext, object> GetEmptyMaterializer(IEntityType entityType)
    {
        var binding = entityType.ServiceOnlyConstructorBinding;
        if (binding == null)
        {
            var _ = entityType.ConstructorBinding;
            binding = entityType.ServiceOnlyConstructorBinding;
            if (binding == null)
            {
                throw new InvalidOperationException(CoreStrings.NoParameterlessConstructor(entityType.DisplayName()));
            }
        }

        return GetEmptyMaterializer(entityType, binding, entityType.GetServiceProperties().ToList());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<MaterializationContext, object> GetEmptyMaterializer(IComplexType complexType)
    {
        var binding = complexType.ConstructorBinding;
        return binding == null
            ? throw new InvalidOperationException(CoreStrings.NoParameterlessConstructor(complexType.DisplayName()))
            : GetEmptyMaterializer(complexType, binding, []);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<MaterializationContext, object> GetEmptyMaterializer(
        ITypeBase entityType, InstantiationBinding binding, List<IServiceProperty> serviceProperties)
    {
        binding = ModifyBindings(entityType, binding);

        var materializationContextExpression = Parameter(typeof(MaterializationContext), "mc");
        var bindingInfo = new ParameterBindingInfo(
            new StructuralTypeMaterializerSourceParameters(entityType, "instance", entityType.ClrType, null), materializationContextExpression);

        var blockExpressions = new List<Expression>();
        var instanceVariable = Variable(binding.RuntimeType, "instance");
        bindingInfo.ServiceInstances.Add(instanceVariable);

        CreateServiceInstances(binding, bindingInfo, blockExpressions, serviceProperties);

        var constructorExpression = binding.CreateConstructorExpression(bindingInfo);
        var getValueBufferExpression = Call(bindingInfo.MaterializationContextExpression, MaterializationContext.GetValueBufferMethod);

        var properties = new HashSet<IPropertyBase>(serviceProperties);
        foreach (var consumedProperty in binding.ParameterBindings.SelectMany(p => p.ConsumedProperties))
        {
            properties.Remove(consumedProperty);
        }

        return Lambda<Func<MaterializationContext, object>>(
                _materializationInterceptor == null
                    ? properties.Count == 0 && blockExpressions.Count == 0
                        ? constructorExpression
                        : CreateMaterializeExpression(
                            blockExpressions, instanceVariable, constructorExpression, getValueBufferExpression, properties, bindingInfo)
                    : CreateInterceptionMaterializeExpression(
                        entityType,
                        [],
                        _materializationInterceptor,
                        bindingInfo,
                        constructorExpression,
                        getValueBufferExpression,
                        instanceVariable,
                        blockExpressions),
                materializationContextExpression)
            .Compile();
    }

    private InstantiationBinding ModifyBindings(ITypeBase structuralType, InstantiationBinding binding)
    {
        var interceptionData = new InstantiationBindingInterceptionData(structuralType);
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
                var variable = Variable(parameterBinding.ServiceType);
                blockExpressions.Add(Assign(variable, parameterBinding.BindToParameter(bindingInfo)));
                bindingInfo.ServiceInstances.Add(variable);
            }
        }

        foreach (var serviceProperty in serviceProperties)
        {
            var serviceType = serviceProperty.ParameterBinding.ServiceType;
            if (bindingInfo.ServiceInstances.All(e => e.Type != serviceType))
            {
                var variable = Variable(serviceType);
                blockExpressions.Add(Assign(variable, serviceProperty.ParameterBinding.BindToParameter(bindingInfo)));
                bindingInfo.ServiceInstances.Add(variable);
            }
        }
    }

    private static bool IsNullable(IComplexType complexType)
        => IsNullable(complexType.ComplexProperty);

    private static bool IsNullable(IComplexProperty complexProperty)
        => complexProperty.IsNullable
            || (complexProperty.DeclaringType is IComplexType complexType
                && IsNullable(complexType.ComplexProperty));
}
