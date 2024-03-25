// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Contains IDs of diagnostics created by EF analyzers and other mechanisms.
/// </summary>
public static class EFDiagnostics
{
    public const string InternalUsage = "EF1001";
    public const string SuppressUninitializedDbSetRule = "EFSPR1001";
    public const string PrecompiledQueryExperimental = "EF2001";
}
