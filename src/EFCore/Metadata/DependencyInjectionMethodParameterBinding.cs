// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    /// <param name="materializationExpression">The expression representing the materialization context.</param>
    /// <param name="bindingInfoExpression">The expression representing the <see cref="ParameterBindingInfo" /> constant.</param>
    /// <returns>The expression tree.</returns>
    public override Expression BindToParameter(
        Expression materializationExpression,
        Expression bindingInfoExpression)
    {
        Check.NotNull(materializationExpression, nameof(materializationExpression));
        Check.NotNull(bindingInfoExpression, nameof(bindingInfoExpression));

        var parameters = Method.GetParameters().Select(
            (p, i) => Expression.Parameter(p.ParameterType, "param" + i)).ToArray();

        var serviceVariable = Expression.Variable(ServiceType, "service");
        var delegateVariable = Expression.Variable(ParameterType, "delegate");

        return Expression.Block(
            new[] { serviceVariable, delegateVariable },
            new List<Expression>
            {
                Expression.Assign(
                    serviceVariable,
                    base.BindToParameter(materializationExpression, bindingInfoExpression)),
                Expression.Assign(
                    delegateVariable,
                    Expression.Condition(
                        Expression.ReferenceEqual(serviceVariable, Expression.Constant(null)),
                        Expression.Constant(null, ParameterType),
                        Expression.Lambda(
                            Expression.Call(
                                serviceVariable,
                                Method,
                                parameters),
                            parameters))),
                delegateVariable
            });
    }

    /// <summary>
    ///     Creates a copy that contains the given consumed properties.
    /// </summary>
    /// <param name="consumedProperties">The new consumed properties.</param>
    /// <returns>A copy with replaced consumed properties.</returns>
    public override ParameterBinding With(IPropertyBase[] consumedProperties)
        => new DependencyInjectionMethodParameterBinding(ParameterType, ServiceType, Method, consumedProperties);
}
