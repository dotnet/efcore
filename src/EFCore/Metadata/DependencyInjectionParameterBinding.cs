// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Describes the binding from an EF internal dependency injection service, which may or may not
///     also have and associated <see cref="IServiceProperty" />, to a parameter in a constructor,
///     factory method, or similar.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and examples.
/// </remarks>
public class DependencyInjectionParameterBinding : ServiceParameterBinding
{
    private static readonly MethodInfo GetServiceMethod
        = typeof(InfrastructureExtensions).GetMethod(nameof(InfrastructureExtensions.GetService))!;

    private static readonly MethodInfo InjectableServiceServiceObtainedMethod
        = typeof(IInjectableService).GetMethod(nameof(IInjectableService.ServiceObtained))!;

    /// <summary>
    ///     Creates a new <see cref="DependencyInjectionParameterBinding" /> instance for the given service type.
    /// </summary>
    /// <param name="parameterType">The parameter CLR type.</param>
    /// <param name="serviceType">The service CLR types, as resolved from dependency injection</param>
    /// <param name="serviceProperties">The associated <see cref="IServiceProperty" /> objects, or <see langword="null" />.</param>
    public DependencyInjectionParameterBinding(
        Type parameterType,
        Type serviceType,
        params IPropertyBase[]? serviceProperties)
        : base(parameterType, serviceType, serviceProperties)
    {
    }

    /// <summary>
    ///     Creates an expression tree representing the binding of the value of a property from a
    ///     materialization expression to a parameter of the constructor, factory method, etc.
    /// </summary>
    /// <param name="materializationExpression">The expression representing the materialization context.</param>
    /// <param name="bindingInfoExpression">The expression representing the <see cref="ParameterBindingInfo" /> constant.</param>
    /// <returns>The expression tree.</returns>
    public override Expression BindToParameter(
        Expression materializationExpression,
        Expression bindingInfoExpression)
    {
        Check.NotNull(materializationExpression, nameof(materializationExpression));
        Check.NotNull(bindingInfoExpression, nameof(bindingInfoExpression));

        var serviceVariable = Expression.Variable(ServiceType);
        var getContext = Expression.Property(materializationExpression, MaterializationContext.ContextProperty);
        return Expression.Block(
            variables: new[] { serviceVariable },
            Expression.Assign(
                serviceVariable, Expression.Call(
                    GetServiceMethod.MakeGenericMethod(ServiceType),
                    Expression.Convert(getContext, typeof(IInfrastructure<IServiceProvider>)))),
            Expression.IfThen(
                Expression.TypeIs(serviceVariable, typeof(IInjectableService)),
                Expression.Call(
                    Expression.Convert(serviceVariable, typeof(IInjectableService)),
                    InjectableServiceServiceObtainedMethod,
                    getContext,
                    bindingInfoExpression)),
            serviceVariable);
    }

    /// <summary>
    ///     Creates a copy that contains the given consumed properties.
    /// </summary>
    /// <param name="consumedProperties">The new consumed properties.</param>
    /// <returns>A copy with replaced consumed properties.</returns>
    public override ParameterBinding With(IPropertyBase[] consumedProperties)
        => new DependencyInjectionParameterBinding(ParameterType, ServiceType, consumedProperties);
}
