// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         The relational type mapping source. Type mappings describe how a provider maps CLR types/values to database types/values.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
///     <para>
///         Warning: do not implement this interface directly. Instead, derive from <see cref="RelationalTypeMappingSource" />.
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
public interface IRelationalTypeMappingSource : ITypeMappingSource
{
    /// <summary>
    ///     Finds the type mapping for a given <see cref="IProperty" />.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    new RelationalTypeMapping? FindMapping(IProperty property);

    /// <summary>
    ///     Finds the type mapping for a given <see cref="MemberInfo" /> representing
    ///     a field or a property of a CLR type.
    /// </summary>
    /// <remarks>
    ///     Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
    ///     call <see cref="FindMapping(IProperty)" />
    /// </remarks>
    /// <param name="member">The field or property.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    new RelationalTypeMapping? FindMapping(MemberInfo member);

    /// <summary>
    ///     Finds the type mapping for a given <see cref="Type" />.
    /// </summary>
    /// <remarks>
    ///     Note: Only call this method if there is no <see cref="IProperty" />
    ///     or <see cref="IModel" /> available, otherwise call <see cref="FindMapping(IProperty)" />
    ///     or <see cref="FindMapping(Type, IModel, CoreTypeMapping)" />
    /// </remarks>
    /// <param name="type">The CLR type.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    new RelationalTypeMapping? FindMapping(Type type);

    /// <summary>
    ///     Finds the type mapping for a given <see cref="Type" />, taking pre-convention configuration into the account.
    /// </summary>
    /// <remarks>
    ///     Note: Only call this method if there is no <see cref="IProperty" />,
    ///     otherwise call <see cref="FindMapping(IProperty)" />.
    /// </remarks>
    /// <param name="type">The CLR type.</param>
    /// <param name="model">The model.</param>
    /// <param name="elementMapping">The element mapping to use, if known.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    new RelationalTypeMapping? FindMapping(Type type, IModel model, CoreTypeMapping? elementMapping = null);

    /// <summary>
    ///     Finds the type mapping for a given database type name.
    /// </summary>
    /// <remarks>
    ///     Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
    ///     call <see cref="FindMapping(IProperty)" />
    /// </remarks>
    /// <param name="storeTypeName">The database type name.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    RelationalTypeMapping? FindMapping(string storeTypeName);

    /// <summary>
    ///     Finds the type mapping for a given <see cref="Type" /> and additional facets.
    /// </summary>
    /// <remarks>
    ///     Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
    ///     call <see cref="FindMapping(IProperty)" />
    /// </remarks>
    /// <param name="type">The CLR type.</param>
    /// <param name="storeTypeName">The database type name.</param>
    /// <param name="keyOrIndex">If <see langword="true" />, then a special mapping for a key or index may be returned.</param>
    /// <param name="unicode">
    ///     Specify <see langword="true" /> for Unicode mapping, <see langword="false" /> for Ansi mapping or <see langword="null" /> for the
    ///     default.
    /// </param>
    /// <param name="size">Specifies a size for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="rowVersion">Specifies a row-version, or <see langword="null" /> for default.</param>
    /// <param name="fixedLength">Specifies a fixed length mapping, or <see langword="null" /> for default.</param>
    /// <param name="precision">Specifies a precision for the mapping, or <see langword="null" /> for default.</param>
    /// <param name="scale">Specifies a scale for the mapping, or <see langword="null" /> for default.</param>
    /// <returns>The type mapping, or <see langword="null" /> if none was found.</returns>
    RelationalTypeMapping? FindMapping(
        Type type,
        string? storeTypeName,
        bool keyOrIndex = false,
        bool? unicode = null,
        int? size = null,
        bool? rowVersion = null,
        bool? fixedLength = null,
        int? precision = null,
        int? scale = null);
}
