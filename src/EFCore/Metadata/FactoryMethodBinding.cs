// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Defines the binding of parameters to a factory method.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and examples.
/// </remarks>
public class FactoryMethodBinding : InstantiationBinding
{
    private readonly object? _factoryInstance;
    private readonly MethodInfo _factoryMethod;

    /// <summary>
    ///     Creates a new <see cref="FactoryMethodBinding" /> instance for a static factory method.
    /// </summary>
    /// <param name="factoryMethod">The factory method to bind to.</param>
    /// <param name="parameterBindings">The parameters to use.</param>
    /// <param name="runtimeType">The CLR type of the instance created by the factory method.</param>
    public FactoryMethodBinding(
        MethodInfo factoryMethod,
        IReadOnlyList<ParameterBinding> parameterBindings,
        Type runtimeType)
        : base(parameterBindings)
    {
        Check.NotNull(factoryMethod, nameof(factoryMethod));
        Check.NotNull(runtimeType, nameof(runtimeType));

        _factoryMethod = factoryMethod;
        RuntimeType = runtimeType;
    }

    /// <summary>
    ///     Creates a new <see cref="FactoryMethodBinding" /> instance for a non-static factory method.
    /// </summary>
    /// <param name="factoryInstance">The object on which the factory method should be called.</param>
    /// <param name="factoryMethod">The factory method to bind to.</param>
    /// <param name="parameterBindings">The parameters to use.</param>
    /// <param name="runtimeType">The CLR type of the instance created by the factory method.</param>
    public FactoryMethodBinding(
        object factoryInstance,
        MethodInfo factoryMethod,
        IReadOnlyList<ParameterBinding> parameterBindings,
        Type runtimeType)
        : this(factoryMethod, parameterBindings, runtimeType)
    {
        Check.NotNull(factoryInstance, nameof(factoryInstance));

        _factoryInstance = factoryInstance;
    }

    /// <summary>
    ///     Creates a <see cref="MethodCallExpression" /> using the given method.
    /// </summary>
    /// <param name="bindingInfo">Information needed to create the expression.</param>
    /// <returns>The expression tree.</returns>
    public override Expression CreateConstructorExpression(ParameterBindingInfo bindingInfo)
    {
        var arguments = ParameterBindings.Select(b => b.BindToParameter(bindingInfo));

        Expression expression
            = _factoryInstance == null
                ? Expression.Call(
                    _factoryMethod,
                    arguments)
                : Expression.Call(
                    Expression.Constant(_factoryInstance),
                    _factoryMethod,
                    arguments);

        if (_factoryMethod.ReturnType != RuntimeType)
        {
            expression = Expression.Convert(expression, RuntimeType);
        }

        return expression;
    }

    /// <summary>
    ///     The type that will be created from the expression tree created for this binding.
    /// </summary>
    public override Type RuntimeType { get; }

    /// <summary>
    ///     Creates a copy that contains the given parameter bindings.
    /// </summary>
    /// <param name="parameterBindings">The new parameter bindings.</param>
    /// <returns>A copy with replaced parameter bindings.</returns>
    public override InstantiationBinding With(IReadOnlyList<ParameterBinding> parameterBindings)
        => _factoryInstance == null
            ? new FactoryMethodBinding(_factoryMethod, parameterBindings, RuntimeType)
            : new FactoryMethodBinding(_factoryInstance, _factoryMethod, parameterBindings, RuntimeType);
}
