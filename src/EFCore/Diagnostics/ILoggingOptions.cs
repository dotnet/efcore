// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Options set at the <see cref="IServiceProvider" /> singleton level to control how
///     messages are logged and/or thrown in exceptions.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
///     </para>
/// </remarks>
public interface ILoggingOptions : ISingletonOptions
{
    /// <summary>
    ///     Reflects the option set by <see cref="DbContextOptionsBuilder.EnableSensitiveDataLogging" />.
    /// </summary>
    bool IsSensitiveDataLoggingEnabled { get; }

    /// <summary>
    ///     This flag is set once a warning about <see cref="IsSensitiveDataLoggingEnabled" /> has been
    ///     issued to avoid logging the warning again.
    /// </summary>
    bool IsSensitiveDataLoggingWarned { get; set; }

    /// <summary>
    ///     Reflects the option set by <see cref="DbContextOptionsBuilder.EnableDetailedErrors" />.
    /// </summary>
    bool DetailedErrorsEnabled { get; }

    /// <summary>
    ///     Reflects the option set by <see cref="DbContextOptionsBuilder.ConfigureWarnings" />.
    /// </summary>
    WarningsConfiguration WarningsConfiguration { get; }

    /// <summary>
    ///     Returns <see langword="true" /> if a warning about string values for the given enum type has not yet been performed.
    /// </summary>
    /// <param name="enumType">The type to check.</param>
    /// <returns>Whether or not a warning has been issued.</returns>
    bool ShouldWarnForStringEnumValueInJson(Type enumType);
}
