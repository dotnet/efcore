// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     A factory for finding and creating <see cref="InstantiationBinding" /> instances for
///     a given CLR constructor.
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
public interface IConstructorBindingFactory
{
    /// <summary>
    ///     Create a <see cref="InstantiationBinding" /> for the constructor with most parameters and
    ///     the constructor with only service property parameters.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="constructorBinding">The binding for the constructor with most parameters.</param>
    /// <param name="serviceOnlyBinding">The binding for the constructor with only service property parameters.</param>
    void GetBindings(
        IConventionEntityType entityType,
        out InstantiationBinding constructorBinding,
        out InstantiationBinding? serviceOnlyBinding);

    /// <summary>
    ///     Create a <see cref="InstantiationBinding" /> for the constructor with most parameters and
    ///     the constructor with only service property parameters.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="constructorBinding">The binding for the constructor with most parameters.</param>
    /// <param name="serviceOnlyBinding">The binding for the constructor with only service property parameters.</param>
    void GetBindings(
        IMutableEntityType entityType,
        out InstantiationBinding constructorBinding,
        out InstantiationBinding? serviceOnlyBinding);

    /// <summary>
    ///     Create a <see cref="InstantiationBinding" /> for the constructor with most parameters and
    ///     the constructor with only service property parameters.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="constructorBinding">The binding for the constructor with most parameters.</param>
    /// <param name="serviceOnlyBinding">The binding for the constructor with only service property parameters.</param>
    void GetBindings(
        IReadOnlyEntityType entityType,
        out InstantiationBinding constructorBinding,
        out InstantiationBinding? serviceOnlyBinding);

    /// <summary>
    ///     Create a <see cref="InstantiationBinding" /> for the constructor with most parameters and
    ///     the constructor with only service property parameters.
    /// </summary>
    /// <param name="complexType">The complex type.</param>
    /// <param name="constructorBinding">The binding for the constructor with most parameters.</param>
    /// <param name="serviceOnlyBinding">The binding for the constructor with only service property parameters.</param>
    void GetBindings(
        IReadOnlyComplexType complexType,
        out InstantiationBinding constructorBinding,
        out InstantiationBinding? serviceOnlyBinding);

    /// <summary>
    ///     Attempts to create a <see cref="InstantiationBinding" /> for the given entity type and
    ///     <see cref="ConstructorInfo" />
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="constructor">The constructor to use.</param>
    /// <param name="binding">The binding, or <see langword="null" /> if <see langword="null" /> could be created.</param>
    /// <param name="unboundParameters">The parameters that could not be bound.</param>
    /// <returns><see langword="true" /> if a binding was created; <see langword="false" /> otherwise.</returns>
    bool TryBindConstructor(
        IConventionEntityType entityType,
        ConstructorInfo constructor,
        [NotNullWhen(true)] out InstantiationBinding? binding,
        [NotNullWhen(false)] out IEnumerable<ParameterInfo>? unboundParameters);

    /// <summary>
    ///     Attempts to create a <see cref="InstantiationBinding" /> for the given entity type and
    ///     <see cref="ConstructorInfo" />
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="constructor">The constructor to use.</param>
    /// <param name="binding">The binding, or <see langword="null" /> if <see langword="null" /> could be created.</param>
    /// <param name="unboundParameters">The parameters that could not be bound.</param>
    /// <returns><see langword="true" /> if a binding was created; <see langword="false" /> otherwise.</returns>
    bool TryBindConstructor(
        IMutableEntityType entityType,
        ConstructorInfo constructor,
        [NotNullWhen(true)] out InstantiationBinding? binding,
        [NotNullWhen(false)] out IEnumerable<ParameterInfo>? unboundParameters);
}
