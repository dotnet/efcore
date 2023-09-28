// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         The base class for non-relational type mapping source. Non-relational providers
///         should derive from this class and override <see cref="O:TypeMappingSourceBase.FindMapping" />
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public abstract class TypeMappingSourceBase : ITypeMappingSource
{
    /// <summary>
    ///     Initializes a new instance of the this class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    protected TypeMappingSourceBase(TypeMappingSourceDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual TypeMappingSourceDependencies Dependencies { get; }

    /// <summary>
    ///     Overridden by database providers to find a type mapping for the given info.
    /// </summary>
    /// <remarks>
    ///     The mapping info is populated with as much information about the required type mapping as
    ///     is available. Use all the information necessary to create the best mapping. Return <see langword="null" />
    ///     if no mapping is available.
    /// </remarks>
    /// <param name="mappingInfo">The mapping info to use to create the mapping.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none could be found.</returns>
    protected virtual CoreTypeMapping? FindMapping(in TypeMappingInfo mappingInfo)
    {
        foreach (var plugin in Dependencies.Plugins)
        {
            var typeMapping = plugin.FindMapping(mappingInfo);
            if (typeMapping != null)
            {
                return typeMapping;
            }
        }

        return null;
    }

    /// <summary>
    ///     Called after a mapping has been found so that it can be validated for the given property.
    /// </summary>
    /// <param name="mapping">The mapping, if any.</param>
    /// <param name="property">The property, if any.</param>
    protected virtual void ValidateMapping(
        CoreTypeMapping? mapping,
        IProperty? property)
    {
    }

    /// <summary>
    ///     Finds the type mapping for a given <see cref="IProperty" />.
    /// </summary>
    /// <remarks>
    ///     Note: providers should typically not need to override this method.
    /// </remarks>
    /// <param name="property">The property.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public abstract CoreTypeMapping? FindMapping(IProperty property);

    /// <summary>
    ///     Finds the type mapping for a given <see cref="Type" />.
    /// </summary>
    /// <remarks>
    ///     Note: Only call this method if there is no <see cref="IProperty" />
    ///     or <see cref="IModel" /> available, otherwise call <see cref="FindMapping(IProperty)" />
    ///     or <see cref="FindMapping(Type, IModel)" />
    /// </remarks>
    /// <param name="type">The CLR type.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public abstract CoreTypeMapping? FindMapping(Type type);

    /// <summary>
    ///     Finds the type mapping for a given <see cref="Type" />, taking pre-convention configuration into the account.
    /// </summary>
    /// <remarks>
    ///     Note: Only call this method if there is no <see cref="IProperty" />,
    ///     otherwise call <see cref="FindMapping(IProperty)" />.
    /// </remarks>
    /// <param name="type">The CLR type.</param>
    /// <param name="model">The model.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public abstract CoreTypeMapping? FindMapping(Type type, IModel model);

    /// <summary>
    ///     Finds the type mapping for a given <see cref="MemberInfo" /> representing
    ///     a field or a property of a CLR type.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
    ///         call <see cref="FindMapping(IProperty)" />
    ///     </para>
    ///     <para>
    ///         Note: providers should typically not need to override this method.
    ///     </para>
    /// </remarks>
    /// <param name="member">The field or property.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    public abstract CoreTypeMapping? FindMapping(MemberInfo member);
}
