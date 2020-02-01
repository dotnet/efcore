// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The relational type mapping interface for EF Core, starting with version 2.1. Type mappings describe how a
    ///         provider maps CLR types/values to database types/values.
    ///     </para>
    ///     <para>
    ///         Warning: do not implement this interface directly. Instead, derive from <see cref="RelationalTypeMappingSource" />.
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
    public interface IRelationalTypeMappingSource : ITypeMappingSource
    {
        /// <summary>
        ///     Finds the type mapping for a given <see cref="IProperty" />.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        new RelationalTypeMapping FindMapping([NotNull] IProperty property);

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
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        new RelationalTypeMapping FindMapping([NotNull] MemberInfo member);

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
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        new RelationalTypeMapping FindMapping([NotNull] Type type);

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given database type name.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
        ///         call <see cref="FindMapping(IProperty)" />
        ///     </para>
        /// </summary>
        /// <param name="storeTypeName"> The database type name. </param>
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        RelationalTypeMapping FindMapping([NotNull] string storeTypeName);

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="Type" /> and additional facets.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
        ///         call <see cref="FindMapping(IProperty)" />
        ///     </para>
        /// </summary>
        /// <param name="type"> The CLR type. </param>
        /// <param name="storeTypeName"> The database type name. </param>
        /// <param name="keyOrIndex"> If <c>true</c>, then a special mapping for a key or index may be returned. </param>
        /// <param name="unicode"> Specifies Unicode or Ansi mapping, or <c>null</c> for default. </param>
        /// <param name="size"> Specifies a size for the mapping, or <c>null</c> for default. </param>
        /// <param name="rowVersion"> Specifies a row-version, or <c>null</c> for default. </param>
        /// <param name="fixedLength"> Specifies a fixed length mapping, or <c>null</c> for default. </param>
        /// <param name="precision"> Specifies a precision for the mapping, or <c>null</c> for default. </param>
        /// <param name="scale"> Specifies a scale for the mapping, or <c>null</c> for default. </param>
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        RelationalTypeMapping FindMapping(
            [NotNull] Type type,
            [CanBeNull] string storeTypeName,
            bool keyOrIndex = false,
            bool? unicode = null,
            int? size = null,
            bool? rowVersion = null,
            bool? fixedLength = null,
            int? precision = null,
            int? scale = null);
    }
}
