// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CS1591 // TODO

namespace Microsoft.EntityFrameworkCore.Query.Internal;

public interface IPrecompiledQueryCodeGenerator
{
    Task GeneratePrecompiledQueries(string projectDir, DbContext context, string outputDir, CancellationToken cancellationToken = default);
}
