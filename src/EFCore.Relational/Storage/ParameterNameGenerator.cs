// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Generates unique names for parameters.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class ParameterNameGenerator
{
    private int _count;

    /// <summary>
    ///     Generates the next unique parameter name.
    /// </summary>
    /// <returns>The generated name.</returns>
    public virtual string GenerateNext()
        => "p" + _count++;

    /// <summary>
    ///     Resets the generator, meaning it can reuse previously generated names.
    /// </summary>
    public virtual void Reset()
        => _count = 0;
}
