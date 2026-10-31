// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     A service used to process a model loaded from a Migrations snapshot, applying fix-ups for the version of EF Core
///     that created the snapshot. Database providers can register a replacement implementation to apply additional
///     provider-specific fix-ups.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
///     </para>
/// </remarks>
public interface ISnapshotModelProcessor
{
    /// <summary>
    ///     Processes the model, applying any fix-ups needed for the version of EF Core that created the snapshot.
    /// </summary>
    /// <param name="model">The model read from the snapshot.</param>
    /// <param name="resetVersion">
    ///     If <see langword="true" />, the product version annotation on the returned model is reset to the current
    ///     EF Core version. Use this when the processed model will be persisted back to a snapshot.
    /// </param>
    /// <returns>The processed model, or <see langword="null" /> if <paramref name="model" /> is <see langword="null" />.</returns>
    [return: NotNullIfNotNull(nameof(model))]
    IModel? Process(IReadOnlyModel? model, bool resetVersion = false);
}
