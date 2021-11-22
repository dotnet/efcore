// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Represents plugin for <see cref="IMethodCallTranslator" />.
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Scoped" /> and multiple registrations
///     are allowed. This means that each <see cref="DbContext" /> instance will use its own
///     set of instances of this service.
///     The implementations may depend on other services registered with any lifetime.
///     The implementations do not need to be thread-safe.
/// </remarks>
public interface IMethodCallTranslatorPlugin
{
    /// <summary>
    ///     Gets the method call translators.
    /// </summary>
    IEnumerable<IMethodCallTranslator> Translators { get; }
}
