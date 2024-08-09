// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Microsoft.Azure.Cosmos;

[Experimental(EFDiagnostics.CosmosVectorSearchExperimental)]
internal sealed class VectorIndexPath
{
    public string? Path { get; set; }
    public VectorIndexType Type { get; set; }
}
