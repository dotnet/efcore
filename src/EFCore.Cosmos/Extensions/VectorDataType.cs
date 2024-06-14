// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace Microsoft.Azure.Cosmos;

/// <summary>
///     Defines the target data type of a vector index specification in the Azure Cosmos DB service.
///     Warning: this type will be replaced by the type from the Cosmos SDK, when it is available.
/// </summary>
[Experimental(EFDiagnostics.CosmosVectorSearchExperimental)]
public enum VectorDataType
{
    /// <summary>
    ///     Represents a 16-bit floating point data type.
    /// </summary>
    [EnumMember(Value = "float16")]
    Float16,

    /// <summary>
    ///     Represents a 32-bit floating point data type.
    /// </summary>
    [EnumMember(Value = "float32")]
    Float32,

    /// <summary>
    ///     Represents an unsigned 8-bit binary data type.
    /// </summary>
    [EnumMember(Value = "uint8")]
    Uint8,

    /// <summary>
    ///     Represents a signed 8-bit binary data type.
    /// </summary>
    [EnumMember(Value = "int8")]
    Int8,
}
