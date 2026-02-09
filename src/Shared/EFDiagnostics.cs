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
    internal const string SuppressUninitializedDbSetRule = "EFSPR1001";

    // Diagnostics for [Obsolete]
    internal const string OwnedJsonObsolete = "EF8001";
    internal const string OwnedJsonObsoleteMessage = "ToJson() on owned entities has been obsoleted, please switch to using complex types instead for mapping to JSON. See https://aka.ms/efcore-docs-json-owned-entities for more information, and provide feedback on https://github.com/dotnet/efcore/issues/37290 if the transition causes problems for you.";

    // Diagnostics for [Experimental]
    internal const string ExperimentalApi = "EF9001";
    internal const string ProviderExperimentalApi = "EF9002";
    internal const string PrecompiledQueryExperimental = "EF9100";
    internal const string MetricsExperimental = "EF9101";
    internal const string PagingExperimental = "EF9102";
    internal const string CosmosVectorSearchExperimental = "EF9103"; // No longer experimental
    internal const string CosmosFullTextSearchExperimental = "EF9104"; // No longer experimental
    internal const string SqlServerVectorSearch = "EF9105";
}
