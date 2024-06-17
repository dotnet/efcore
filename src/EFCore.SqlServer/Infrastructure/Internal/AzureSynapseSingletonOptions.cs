// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class AzureSynapseSingletonOptions : IAzureSynapseSingletonOptions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int CompatibilityLevel { get; private set; } = AzureSynapseOptionsExtension.DefaultCompatibilityLevel;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? CompatibilityLevelWithoutDefault { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Initialize(IDbContextOptions options)
    {
        var azureSynapseOptions = options.FindExtension<AzureSynapseOptionsExtension>();
        if (azureSynapseOptions != null)
        {
            CompatibilityLevel = azureSynapseOptions.CompatibilityLevel;
            CompatibilityLevelWithoutDefault = azureSynapseOptions.CompatibilityLevelWithoutDefault;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Validate(IDbContextOptions options)
    {
        var azureSynapseOptions = options.FindExtension<AzureSynapseOptionsExtension>();

        if (azureSynapseOptions != null
            && (CompatibilityLevelWithoutDefault != azureSynapseOptions.CompatibilityLevelWithoutDefault
                || CompatibilityLevel != azureSynapseOptions.CompatibilityLevel))
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(AzureSynapseDbContextOptionsExtensions.UseAzureSynapse),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }
    }
}
