// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace Microsoft.Azure.Cosmos;

/// <summary>
///     Defines the distance function for a vector index specification in the Azure Cosmos DB service.
///     Warning: this type will be replaced by the type from the Cosmos SDK, when it is available.
/// </summary>
/// <seealso cref="T:Microsoft.Azure.Cosmos.Embedding" />
/// for usage.
[Experimental(EFDiagnostics.CosmosVectorSearchExperimental)]
public enum DistanceFunction
{
    /// <summary>
    ///     Represents the Euclidean distance function.
    /// </summary>
    [EnumMember(Value = "euclidean")]
    Euclidean,

    /// <summary>
    ///     Represents the cosine distance function.
    /// </summary>
    [EnumMember(Value = "cosine")]
    Cosine,

    /// <summary>
    ///     Represents the dot product distance function.
    /// </summary>
    [EnumMember(Value = "dotproduct")]
    DotProduct,
}
