// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Defines the binding of parameters to create the default value of a type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and examples.
/// </remarks>
public class DefaultValueBinding : InstantiationBinding
{
    /// <summary>
    ///     Creates a new <see cref="DefaultValueBinding" /> instance.
    /// </summary>
    /// <param name="runtimeType">The CLR type of the instance created by the factory method.</param>
    public DefaultValueBinding(Type runtimeType)
        : base(new List<ParameterBinding>())
    {
        Check.NotNull(runtimeType, nameof(runtimeType));

        RuntimeType = runtimeType;
    }

    /// <summary>
    ///     Creates a <see cref="MethodCallExpression" /> using the given method.
    /// </summary>
    /// <param name="bindingInfo">Information needed to create the expression.</param>
    /// <returns>The expression tree.</returns>
    public override Expression CreateConstructorExpression(ParameterBindingInfo bindingInfo)
        => Expression.Default(RuntimeType);

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
        => this;
}
