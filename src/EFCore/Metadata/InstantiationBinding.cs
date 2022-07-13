// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Defines how to create an entity instance through the binding of EF model properties to, for
///     example, constructor parameters or parameters of a factory method.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and examples.
/// </remarks>
public abstract class InstantiationBinding
{
    /// <summary>
    ///     Creates a new <see cref="InstantiationBinding" /> instance.
    /// </summary>
    /// <param name="parameterBindings">The parameter bindings to use.</param>
    protected InstantiationBinding(
        IReadOnlyList<ParameterBinding> parameterBindings)
    {
        Check.NotNull(parameterBindings, nameof(parameterBindings));

        ParameterBindings = parameterBindings;
    }

    /// <summary>
    ///     Creates an expression tree that represents creating an entity instance from the given binding
    ///     information. For example, this might be a <see cref="NewExpression" /> to call a constructor,
    ///     or a <see cref="MethodCallExpression" /> to call a factory method.
    /// </summary>
    /// <param name="bindingInfo">Information needed to create the expression.</param>
    /// <returns>The expression tree.</returns>
    public abstract Expression CreateConstructorExpression(ParameterBindingInfo bindingInfo);

    /// <summary>
    ///     The collection of <see cref="ParameterBinding" /> instances used.
    /// </summary>
    public virtual IReadOnlyList<ParameterBinding> ParameterBindings { get; }

    /// <summary>
    ///     The type that will be created from the expression tree created for this binding.
    /// </summary>
    public abstract Type RuntimeType { get; }

    /// <summary>
    ///     Creates a copy that contains the given parameter bindings.
    /// </summary>
    /// <param name="parameterBindings">The new parameter bindings.</param>
    /// <returns>A copy with replaced parameter bindings.</returns>
    public abstract InstantiationBinding With(IReadOnlyList<ParameterBinding> parameterBindings);
}
