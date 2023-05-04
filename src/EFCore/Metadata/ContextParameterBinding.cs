// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Describes the binding of a <see cref="DbContext" />, which may or may not also have and associated
///     <see cref="IServiceProperty" />, to a parameter in a constructor, factory method, or similar.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and examples.
/// </remarks>
public class ContextParameterBinding : ServiceParameterBinding
{
    /// <summary>
    ///     Creates a new <see cref="ServiceParameterBinding" /> instance for the given service type.
    /// </summary>
    /// <param name="contextType">The <see cref="DbContext" /> CLR type.</param>
    /// <param name="serviceProperties">The associated <see cref="IServiceProperty" /> objects, or <see langword="null" />.</param>
    public ContextParameterBinding(
        Type contextType,
        params IPropertyBase[]? serviceProperties)
        : base(contextType, contextType, serviceProperties)
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

        var propertyExpression
            = Expression.Property(
                materializationExpression,
                MaterializationContext.ContextProperty);

        return ServiceType != typeof(DbContext)
            ? Expression.TypeAs(propertyExpression, ServiceType)
            : propertyExpression;
    }

    /// <summary>
    ///     Creates a copy that contains the given consumed properties.
    /// </summary>
    /// <param name="consumedProperties">The new consumed properties.</param>
    /// <returns>A copy with replaced consumed properties.</returns>
    public override ParameterBinding With(IPropertyBase[] consumedProperties)
        => new ContextParameterBinding(ParameterType, consumedProperties);
}
