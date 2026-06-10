// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Describes the binding from a method on an EF internal dependency injection service, which may or may not
///     also have and associated <see cref="IServiceProperty" />, to a parameter in a constructor,
///     factory method, or similar.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and examples.
/// </remarks>
public class DependencyInjectionMethodParameterBinding : DependencyInjectionParameterBinding
{
    private static readonly MethodInfo GetServiceMethod
        = typeof(InfrastructureExtensions).GetRuntimeMethod(
            nameof(InfrastructureExtensions.GetService), [typeof(IInfrastructure<IServiceProvider>)])!;

    private static readonly MethodInfo GetServiceFromPropertyMethod
        = typeof(DependencyInjectionMethodParameterBinding).GetTypeInfo().GetDeclaredMethod(nameof(GetServiceFromProperty))!;

    private static readonly MethodInfo CreateServiceMethod
        = typeof(DependencyInjectionMethodParameterBinding).GetTypeInfo().GetDeclaredMethod(nameof(CreateService))!;

    private Func<MaterializationContext, IEntityType, object, object>? _serviceDelegate;

    /// <summary>
    ///     Creates a new <see cref="DependencyInjectionParameterBinding" /> instance for the given method
    ///     of the given service type.
    /// </summary>
    /// <param name="parameterType">The parameter CLR type.</param>
    /// <param name="serviceType">The service CLR types, as resolved from dependency injection</param>
    /// <param name="method">The method of the service to bind to.</param>
    /// <param name="serviceProperties">The associated <see cref="IServiceProperty" /> objects, or <see langword="null" />.</param>
    public DependencyInjectionMethodParameterBinding(
        Type parameterType,
        Type serviceType,
        MethodInfo method,
        params IPropertyBase[]? serviceProperties)
        : base(parameterType, serviceType, serviceProperties)
    {
        Check.NotNull(method, nameof(method));

        Method = method;
    }

    /// <summary>
    ///     The method being bound to, as defined on the dependency injection service interface.
    /// </summary>
    public virtual MethodInfo Method { get; }

    /// <summary>
    ///     Creates an expression tree representing the binding of the value of a property from a
    ///     materialization expression to a parameter of the constructor, factory method, etc.
    /// </summary>
    /// <param name="bindingInfo">The binding information.</param>
    /// <returns>The expression tree.</returns>
    public override Expression BindToParameter(ParameterBindingInfo bindingInfo)
    {
        var serviceInstance = bindingInfo.ServiceInstances.FirstOrDefault(e => e.Type == ServiceType);
        if (serviceInstance != null)
        {
            var parameters = Method.GetParameters().Select(
                (p, i) => Expression.Parameter(p.ParameterType, "param" + i)).ToArray();

            return Expression.Condition(
                Expression.ReferenceEqual(serviceInstance, Expression.Constant(null)),
                Expression.Constant(null, ParameterType),
                Expression.Lambda(
                    Expression.Call(
                        serviceInstance,
                        Method,
                        parameters),
                    parameters));
        }

        return base.BindToParameter(bindingInfo);
    }

    private static object? GetServiceFromProperty(MaterializationContext materializationContext, IPropertyBase property, object entity)
        => materializationContext.Context.GetDependencies().StateManager.GetOrCreateEntry(entity)[property];

    private static object CreateService(
        MaterializationContext materializationContext,
        Type serviceType,
        IEntityType entityType,
        object entity)
    {
        var service = materializationContext.Context.GetService(serviceType);

        if (service is IInjectableService injectableService)
        {
            injectableService.Attaching(materializationContext.Context, entityType, entity);
        }

        return service;
    }

    /// <summary>
    ///     A delegate to set a CLR service property on an entity instance.
    /// </summary>
    public override Func<MaterializationContext, IEntityType, object, object?> ServiceDelegate
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _serviceDelegate, this, static b =>
            {
                var materializationContextParam = Expression.Parameter(typeof(MaterializationContext));
                var entityTypeParam = Expression.Parameter(typeof(IEntityType));
                var entityParam = Expression.Parameter(typeof(object));

                var parameters = b.Method.GetParameters().Select(
                    (p, i) => Expression.Parameter(p.ParameterType, "param" + i)).ToArray();

                var entityType = (IEntityType)b.ConsumedProperties.First().DeclaringType;
                var serviceStateProperty = entityType.GetServiceProperties().FirstOrDefault(
                    p => p.ParameterBinding != b && p.ParameterBinding.ServiceType == b.ServiceType);

                var serviceVariable = Expression.Variable(b.ServiceType, "service");
                var serviceExpression = Expression.Block(
                    new[] { serviceVariable },
                    new List<Expression>
                    {
                        Expression.Assign(
                            serviceVariable,
                            Expression.Convert(
                                serviceStateProperty == null
                                    ? Expression.Call(
                                        CreateServiceMethod,
                                        materializationContextParam,
                                        Expression.Constant(b.ServiceType),
                                        entityTypeParam,
                                        entityParam)
                                    : Expression.Call(
                                        GetServiceFromPropertyMethod,
                                        materializationContextParam,
                                        Expression.Constant(serviceStateProperty, typeof(IPropertyBase)),
                                        entityParam),
                                typeof(ILazyLoader))),
                        Expression.Condition(
                            Expression.ReferenceEqual(serviceVariable, Expression.Constant(null)),
                            Expression.Constant(null, b.ParameterType),
                            Expression.Lambda(
                                Expression.Call(
                                    serviceVariable,
                                    b.Method,
                                    parameters),
                                parameters))
                    });

                return Expression.Lambda<Func<MaterializationContext, IEntityType, object, object>>(
                    serviceExpression,
                    materializationContextParam,
                    entityTypeParam,
                    entityParam).Compile();
            });

    /// <summary>
    ///     Creates a copy that contains the given consumed properties.
    /// </summary>
    /// <param name="consumedProperties">The new consumed properties.</param>
    /// <returns>A copy with replaced consumed properties.</returns>
    public override ParameterBinding With(IPropertyBase[] consumedProperties)
        => new DependencyInjectionMethodParameterBinding(ParameterType, ServiceType, Method, consumedProperties);
}
