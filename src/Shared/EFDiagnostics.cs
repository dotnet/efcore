// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Contains the IDs of diagnostics emitted by EF Core analyzers, [Experimental] and other mechanisms.
/// </summary>
internal static class EFDiagnostics
{
    internal const string InternalUsage = "EF1001";
    internal const string InterpolatedStringUsageInRawQueries = "EF1002";
    internal const string StringConcatenationUsageInRawQueries = "EF1003";
    internal const string ToAsyncEnumerableOnQueryable = "EF1004";
    internal const string SuppressUninitializedDbSetRule = "EFSPR1001";

    // Diagnostics for [Experimental]
    internal const string ExperimentalApi = "EF9001";
    internal const string ProviderExperimentalApi = "EF9002";
    internal const string PrecompiledQueryExperimental = "EF9100";
    internal const string MetricsExperimental = "EF9101";
    internal const string PagingExperimental = "EF9102";
    internal const string CosmosVectorSearchExperimental = "EF9103"; // No longer experimental
    internal const string CosmosFullTextSearchExperimental = "EF9104"; // No longer experimental
    internal const string SqlServerVectorSearch = "EF9105";
    internal const string JsonContainsExperimental = "EF9106";
}
