// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Microsoft.Azure.Cosmos;

[Experimental(EFDiagnostics.CosmosVectorSearchExperimental)]
internal class Embedding : IEquatable<Embedding>
{
    public string? Path { get; set; }
    public VectorDataType DataType { get; set; }
    public int Dimensions { get; set; }
    public DistanceFunction DistanceFunction { get; set; }
    public bool Equals(Embedding? that)
        => Equals(Path, that?.Path) && Equals(DataType, that?.DataType) && Equals(Dimensions, that.Dimensions);
}
