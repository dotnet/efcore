// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace Microsoft.Azure.Cosmos;

/// <summary>
///     Defines the target index type of the vector index path specification in the Azure Cosmos DB service.
///     Warning: this type will be replaced by the type from the Cosmos SDK, when it is available.
/// </summary>
[Experimental(EFDiagnostics.CosmosVectorSearchExperimental)]
public enum VectorIndexType
{
    /// <summary>
    ///     Represents a flat vector index type.
    /// </summary>
    [EnumMember(Value = "flat")]
    Flat,

    /// <summary>
    ///     Represents a Disk ANN vector index type.
    /// </summary>
    [EnumMember(Value = "diskANN")]
    // ReSharper disable once InconsistentNaming
    DiskANN,

    /// <summary>
    ///     Represents a quantized flat vector index type.
    /// </summary>
    [EnumMember(Value = "quantizedFlat")]
    QuantizedFlat,
}
