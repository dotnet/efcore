// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Factory for finding and creating <see cref="ParameterBinding" /> instances.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" /> and multiple registrations
///         are allowed. This means a single instance of each service is used by many <see cref="DbContext" />
///         instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-constructor-binding">Entity types with constructors</see> for more information and
///         examples.
///     </para>
/// </remarks>
public interface IParameterBindingFactory
{
    /// <summary>
    ///     Checks whether or not this factory can bind a parameter with the given type and name.
    /// </summary>
    /// <param name="parameterType">The parameter type.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns><see langword="true" /> if this parameter can be bound; <see langword="false" /> otherwise.</returns>
    bool CanBind(
        Type parameterType,
        string parameterName);

    /// <summary>
    ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="parameterType">The parameter type.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The binding.</returns>
    ParameterBinding Bind(
        IReadOnlyEntityType entityType,
        Type parameterType,
        string parameterName);

    /// <summary>
    ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="parameterType">The parameter type.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The binding.</returns>
    ParameterBinding Bind(
        IMutableEntityType entityType,
        Type parameterType,
        string parameterName);

    /// <summary>
    ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="parameterType">The parameter type.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The binding.</returns>
    ParameterBinding Bind(
        IConventionEntityType entityType,
        Type parameterType,
        string parameterName);
}
