// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Information/metadata for an <see cref="IDbContextOptionsExtension" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public abstract class DbContextOptionsExtensionInfo
{
    /// <summary>
    ///     Creates a new <see cref="DbContextOptionsExtensionInfo" /> instance containing
    ///     info/metadata for the given extension.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="extension">The extension.</param>
    protected DbContextOptionsExtensionInfo(IDbContextOptionsExtension extension)
    {
        Extension = extension;
    }

    /// <summary>
    ///     The extension for which this instance contains metadata.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    public virtual IDbContextOptionsExtension Extension { get; }

    /// <summary>
    ///     <see langword="true" /> if the extension is a database provider; <see langword="false" /> otherwise.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    public abstract bool IsDatabaseProvider { get; }

    /// <summary>
    ///     A message fragment for logging typically containing information about
    ///     any useful non-default options that have been configured.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    public abstract string LogFragment { get; }

    /// <summary>
    ///     Returns a hash code created from any options that would cause a new <see cref="IServiceProvider" />
    ///     to be needed. For example, if the options affect a singleton service. However most extensions do not
    ///     have any such options and should return zero.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <returns>A hash over options that require a new service provider when changed.</returns>
    public abstract int GetServiceProviderHashCode();

    /// <summary>
    ///     Returns a value indicating whether all of the options used in <see cref="GetServiceProviderHashCode" />
    ///     are the same as in the given extension.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="other">The other extension.</param>
    /// <returns>A value indicating whether all of the options that require a new service provider are the same.</returns>
    public abstract bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other);

    /// <summary>
    ///     Populates a dictionary of information that may change between uses of the
    ///     extension such that it can be compared to a previous configuration for
    ///     this option and differences can be logged. The dictionary key should be prefixed by the
    ///     extension name. For example, <c>"SqlServer:"</c>.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="debugInfo">The dictionary to populate.</param>
    public abstract void PopulateDebugInfo(IDictionary<string, string> debugInfo);
}
