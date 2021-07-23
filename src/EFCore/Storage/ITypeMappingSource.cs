// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The core type mapping interface for EF Core, starting with version 2.1. Type mappings describe how a
    ///         provider maps CLR types/values to database types/values.
    ///     </para>
    ///     <para>
    ///         Warning: do not implement this interface directly. Instead, derive from <see cref="TypeMappingSourceBase" />
    ///         for non-relational providers, or 'RelationalTypeMappingSourceBase' for relational providers.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface ITypeMappingSource
    {
        /// <summary>
        ///     Finds the type mapping for a given <see cref="IProperty" />.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        CoreTypeMapping? FindMapping(IProperty property);

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="MemberInfo" /> representing
        ///         a field or a property of a CLR type.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
        ///         call <see cref="FindMapping(IProperty)" />
        ///     </para>
        /// </summary>
        /// <param name="member"> The field or property. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        CoreTypeMapping? FindMapping(MemberInfo member);

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="Type" />.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" />
        ///         or <see cref="MemberInfo" /> available, otherwise call <see cref="FindMapping(IProperty)" />
        ///         or <see cref="FindMapping(MemberInfo)" />
        ///     </para>
        /// </summary>
        /// <param name="type"> The CLR type. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        CoreTypeMapping? FindMapping(Type type);
    }
}
