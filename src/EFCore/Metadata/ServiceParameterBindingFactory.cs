// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     A <see cref="IParameterBindingFactory" /> for binding to dependency-injected services.
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
public class ServiceParameterBindingFactory : IParameterBindingFactory
{
    private readonly Type _serviceType;

    /// <summary>
    ///     Creates a new <see cref="ServiceParameterBindingFactory" /> instance for the given service type.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    public ServiceParameterBindingFactory(Type serviceType)
    {
        _serviceType = serviceType;
    }

    /// <summary>
    ///     Checks whether or not this factory can bind a parameter with the given type and name.
    /// </summary>
    /// <param name="parameterType">The parameter type.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns><see langword="true" /> if this parameter can be bound; <see langword="false" /> otherwise.</returns>
    public virtual bool CanBind(
        Type parameterType,
        string parameterName)
        => parameterType == _serviceType;

    /// <summary>
    ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="parameterType">The parameter type.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The binding.</returns>
    public virtual ParameterBinding Bind(
        IMutableEntityType entityType,
        Type parameterType,
        string parameterName)
        => Bind((IReadOnlyEntityType)entityType, parameterType, parameterName);

    /// <summary>
    ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="parameterType">The parameter type.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The binding.</returns>
    public virtual ParameterBinding Bind(
        IConventionEntityType entityType,
        Type parameterType,
        string parameterName)
        => Bind((IReadOnlyEntityType)entityType, parameterType, parameterName);

    /// <summary>
    ///     Creates a <see cref="ParameterBinding" /> for the given type and name on the given entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="parameterType">The parameter type.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The binding.</returns>
    public virtual ParameterBinding Bind(
        IReadOnlyEntityType entityType,
        Type parameterType,
        string parameterName)
        => new DependencyInjectionParameterBinding(
            _serviceType,
            _serviceType,
            entityType.GetServiceProperties().Cast<IPropertyBase>().Where(p => p.ClrType == _serviceType).ToArray());
}
