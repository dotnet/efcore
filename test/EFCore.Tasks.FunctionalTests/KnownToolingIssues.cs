// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Tasks;

internal static class KnownToolingIssues
{
    public const string ColdBuild
        = "https://github.com/dotnet/efcore/issues/33136: build-time generation runs before the first assembly is copied to bin.";

    public const string ChangedModel
        = "https://github.com/dotnet/efcore/issues/33136: build-time regeneration reads the previous output assembly and retains the old model.";
}
