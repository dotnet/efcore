// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Describes the binding from one or many EF model properties, dependency injection services, or metadata types to
///     a parameter in a constructor, factory method, or similar.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and examples.
/// </remarks>
public abstract class ParameterBinding
{
    /// <summary>
    ///     Creates a new <see cref="ParameterBinding" /> instance.
    /// </summary>
    /// <param name="parameterType">The parameter CLR type.</param>
    /// <param name="consumedProperties">The properties that are handled by this binding and so do not need to be set in some other way.</param>
    protected ParameterBinding(
        Type parameterType,
        params IPropertyBase[]? consumedProperties)
    {
        Check.NotNull(parameterType, nameof(parameterType));

        ParameterType = parameterType;
        ConsumedProperties = consumedProperties ?? [];
    }

    /// <summary>
    ///     The parameter CLR type.
    /// </summary>
    public virtual Type ParameterType { get; }

    /// <summary>
    ///     The properties that are handled by this binding and so do not need to be set in some other way.
    /// </summary>
    public virtual IReadOnlyList<IPropertyBase> ConsumedProperties { get; }

    /// <summary>
    ///     Creates an expression tree representing the binding of the value of a property from a
    ///     materialization expression to a parameter of the constructor, factory method, etc.
    /// </summary>
    /// <param name="bindingInfo">The binding information.</param>
    /// <returns>The expression tree.</returns>
    public abstract Expression BindToParameter(ParameterBindingInfo bindingInfo);

    /// <summary>
    ///     Creates a copy that contains the given consumed properties.
    /// </summary>
    /// <param name="consumedProperties">The new consumed properties.</param>
    /// <returns>A copy with replaced consumed properties.</returns>
    public abstract ParameterBinding With(IPropertyBase[] consumedProperties);
}
