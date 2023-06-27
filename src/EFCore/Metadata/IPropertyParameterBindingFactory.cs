// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Finds a <see cref="ParameterBinding" /> specifically for some form of property
///     (that is, some <see cref="IPropertyBase" />) of the model.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and
///         examples.
///     </para>
/// </remarks>
public interface IPropertyParameterBindingFactory
{
    /// <summary>
    ///     Finds a <see cref="ParameterBinding" /> specifically for an <see cref="IPropertyBase" /> in the model.
    /// </summary>
    /// <param name="entityType">The entity type on which the <see cref="IPropertyBase" /> is defined.</param>
    /// <param name="parameterType">The parameter name.</param>
    /// <param name="parameterName">The parameter type.</param>
    /// <returns>The parameter binding, or <see langword="null" /> if none was found.</returns>
    ParameterBinding? FindParameter(
        IEntityType entityType,
        Type parameterType,
        string parameterName);

    /// <summary>
    ///     Finds a <see cref="ParameterBinding" /> specifically for an <see cref="IPropertyBase" /> in the model.
    /// </summary>
    /// <param name="complexType">The complex type on which the <see cref="IPropertyBase" /> is defined.</param>
    /// <param name="parameterType">The parameter name.</param>
    /// <param name="parameterName">The parameter type.</param>
    /// <returns>The parameter binding, or <see langword="null" /> if none was found.</returns>
    ParameterBinding? FindParameter(
        IComplexType complexType,
        Type parameterType,
        string parameterName);
}
