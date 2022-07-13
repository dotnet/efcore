// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///     is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///     This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
/// </remarks>
public interface ISqlServerValueGeneratorCache : IValueGeneratorCache
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    SqlServerSequenceValueGeneratorState GetOrAddSequenceState(
        IProperty property,
        IRelationalConnection connection);
}
