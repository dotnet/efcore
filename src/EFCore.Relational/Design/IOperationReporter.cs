// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     Reports messages emitted by design-time operations such as Migrations and scaffolding.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance is used by many
///         <see cref="DbContext" /> instances. The implementation must be thread-safe. This service cannot depend on services
///         registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface IOperationReporter
{
    /// <summary>
    ///     Writes an error message.
    /// </summary>
    /// <param name="message">The message.</param>
    void WriteError(string message);

    /// <summary>
    ///     Writes a warning message.
    /// </summary>
    /// <param name="message">The message.</param>
    void WriteWarning(string message);

    /// <summary>
    ///     Writes an informational message.
    /// </summary>
    /// <param name="message">The message.</param>
    void WriteInformation(string message);

    /// <summary>
    ///     Writes a verbose (debug) message.
    /// </summary>
    /// <param name="message">The message.</param>
    void WriteVerbose(string message);
}
