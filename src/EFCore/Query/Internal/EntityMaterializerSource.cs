// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;

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

        var constructorBinding = ModifyBindings(entityType, entityInstanceName, entityType.ConstructorBinding!);        

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

        var valueBufferExpression = Expression.Call(materializationContextExpression, MaterializationContext.GetValueBufferMethod);

        foreach (var property in properties)
        {
            var memberInfo = property.GetMemberInfo(forMaterialization: true, forSet: true);

            var readValueExpression
                = property is IServiceProperty serviceProperty
                    ? serviceProperty.ParameterBinding.BindToParameter(bindingInfo)
                    : valueBufferExpression.CreateValueBufferReadValueExpression(
                        memberInfo.GetMemberType(),
                        property.GetIndex(),
                        property);

            blockExpressions.Add(CreateMemberAssignment(instanceVariable, memberInfo, property, readValueExpression));
        }

        blockExpressions.Add(instanceVariable);

        return Expression.Block(new[] { instanceVariable }, blockExpressions);

        static Expression CreateMemberAssignment(Expression parameter, MemberInfo memberInfo, IPropertyBase property, Expression value)
            => property.IsIndexerProperty()
                ? Expression.Assign(
                    Expression.MakeIndex(
                        parameter, (PropertyInfo)memberInfo, new List<Expression> { Expression.Constant(property.Name) }),
                    value)
                : Expression.MakeMemberAccess(parameter, memberInfo).Assign(value);
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
                        self.CreateMaterializeExpression(e, "instance", materializationContextParameter),
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
                
                binding = self.ModifyBindings(e, "v", binding);        

                var contextParam = Expression.Parameter(typeof(MaterializationContext), "mc");

                return Expression.Lambda<Func<MaterializationContext, object>>(
                        binding.CreateConstructorExpression(
                            new ParameterBindingInfo(e, contextParam)),
                        contextParam)
                    .Compile();
            },
            this);

    private InstantiationBinding ModifyBindings(IEntityType entityType, string entityInstanceName, InstantiationBinding binding)
    {
        foreach (var bindingInterceptor in _bindingInterceptors)
        {
            binding = bindingInterceptor.ModifyBinding(entityType, entityInstanceName, binding);
        }

        return binding;
    }
}
