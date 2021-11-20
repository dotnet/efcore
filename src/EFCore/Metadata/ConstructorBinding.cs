// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Defines the binding of parameters to a CLR <see cref="ConstructorInfo" /> for an entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and examples.
/// </remarks>
public class ConstructorBinding : InstantiationBinding
{
    /// <summary>
    ///     Creates a new <see cref="ConstructorBinding" /> instance.
    /// </summary>
    /// <param name="constructor">The constructor to use.</param>
    /// <param name="parameterBindings">The parameters to bind.</param>
    public ConstructorBinding(
        ConstructorInfo constructor,
        IReadOnlyList<ParameterBinding> parameterBindings)
        : base(parameterBindings)
    {
        Check.NotNull(constructor, nameof(constructor));

        Constructor = constructor;
    }

    /// <summary>
    ///     The bound <see cref="ConstructorInfo" />.
    /// </summary>
    public virtual ConstructorInfo Constructor { get; }

    /// <summary>
    ///     Creates a <see cref="NewExpression" /> that represents creating an entity instance using the given
    ///     constructor.
    /// </summary>
    /// <param name="bindingInfo">Information needed to create the expression.</param>
    /// <returns>The expression tree.</returns>
    public override Expression CreateConstructorExpression(ParameterBindingInfo bindingInfo)
        => Expression.New(
            Constructor,
            ParameterBindings.Select(b => b.BindToParameter(bindingInfo)));

    /// <summary>
    ///     The type that will be created from the expression tree created for this binding.
    /// </summary>
    public override Type RuntimeType
        => Constructor.DeclaringType!;

    /// <summary>
    ///     Creates a copy that contains the given parameter bindings.
    /// </summary>
    /// <param name="parameterBindings">The new parameter bindings.</param>
    /// <returns>A copy with replaced parameter bindings.</returns>
    public override InstantiationBinding With(IReadOnlyList<ParameterBinding> parameterBindings)
        => new ConstructorBinding(Constructor, parameterBindings);
}
