// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

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
    /// </summary>
    public interface ITypeMappingSource
    {
        /// <summary>
        ///     Finds the type mapping for a given <see cref="IProperty" />.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping, or <c>null</c> if none was found. </returns>
        CoreTypeMapping FindMapping([NotNull] IProperty property);

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
        CoreTypeMapping FindMapping([NotNull] MemberInfo member);

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
        CoreTypeMapping FindMapping([NotNull] Type type);
    }
}
