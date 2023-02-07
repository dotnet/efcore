// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     <para>
///         This interface is explicitly implemented by type to hide properties that are not intended to be used in application code
///         but can be used in extension methods written by database providers etc.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
/// <typeparam name="T">The type of the property being hidden.</typeparam>
public interface IInfrastructure<T>
{
    /// <summary>
    ///     Gets the value of the property being hidden.
    /// </summary>
    T Instance { get; }
}
