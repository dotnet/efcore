// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
[Experimental(EFDiagnostics.ProviderExperimentalApi)]
public class SqlAliasManagerFactory : ISqlAliasManagerFactory
{
    /// <inheritdoc />
    public SqlAliasManager Create()
        => new();
}
