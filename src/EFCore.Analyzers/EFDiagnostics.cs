// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Contains IDs of diagnostics created by EF analyzers and other mechanisms.
/// </summary>
public static class EFDiagnostics
{
    public const string InterpolatedStringUsageInRawQueries = "EF1002";
    public const string SuppressUninitializedDbSetRule = "EFSPR1001";

    // Internal API usage
    public const string CoreInternalUsage = "EF9901";
    public const string RelationalInternalUsage = "EF9902";
    public const string DesignInternalUsage = "EF9903";
    public const string ProxiesInternalUsage = "EF9904";
    public const string ProviderInternalUsage = "EF9999";
}
