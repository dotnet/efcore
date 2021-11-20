// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Describes the binding from an <see cref="IProperty" /> to a parameter in a constructor, factory method,
///     or similar.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and examples.
/// </remarks>
public class PropertyParameterBinding : ParameterBinding
{
    /// <summary>
    ///     Creates a new <see cref="PropertyParameterBinding" /> instance for the given <see cref="IProperty" />.
    /// </summary>
    /// <param name="property">The property to bind.</param>
    public PropertyParameterBinding(IProperty property)
        : base(property.ClrType, property)
    {
    }

    /// <summary>
    ///     Creates an expression tree representing the binding of the value of a property from a
    ///     materialization expression to a parameter of the constructor, factory method, etc.
    /// </summary>
    /// <param name="bindingInfo">The binding information.</param>
    /// <returns>The expression tree.</returns>
    public override Expression BindToParameter(ParameterBindingInfo bindingInfo)
    {
        var property = ConsumedProperties[0];

        return Expression.Call(bindingInfo.MaterializationContextExpression, MaterializationContext.GetValueBufferMethod)
            .CreateValueBufferReadValueExpression(property.ClrType, bindingInfo.GetValueBufferIndex(property), property);
    }

    /// <summary>
    ///     Creates a copy that contains the given consumed properties.
    /// </summary>
    /// <param name="consumedProperties">The new consumed properties.</param>
    /// <returns>A copy with replaced consumed properties.</returns>
    public override ParameterBinding With(IPropertyBase[] consumedProperties)
        => new PropertyParameterBinding((IProperty)consumedProperties.Single());
}
