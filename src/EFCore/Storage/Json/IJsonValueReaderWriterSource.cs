// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     <para>
///         Attempts to find a <see cref="JsonValueReaderWriter" /> for a given CLR type.
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
public interface IJsonValueReaderWriterSource
{
    /// <summary>
    ///     Attempts to find a <see cref="JsonValueReaderWriter" /> for a given CLR type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The found <see cref="JsonValueReaderWriter" />, or <see langword="null" /> if none is available.</returns>
    JsonValueReaderWriter? FindReaderWriter(Type type);
}
