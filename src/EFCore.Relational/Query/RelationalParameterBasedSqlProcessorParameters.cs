// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Parameters for <see cref="RelationalParameterBasedSqlProcessor" />.
/// </summary>
public sealed record RelationalParameterBasedSqlProcessorParameters
{
    /// <summary>
    ///     A value indicating if relational nulls should be used.
    /// </summary>
    public bool UseRelationalNulls { get; init; }

    /// <summary>
    ///     A value indicating what translation mode should be used.
    /// </summary>
    public ParameterizedCollectionTranslationMode? ParameterizedCollectionTranslationMode { get; init; }

    /// <summary>
    ///     Creates a new instance of <see cref="RelationalParameterBasedSqlProcessorParameters" />.
    /// </summary>
    /// <param name="useRelationalNulls">A value indicating if relational nulls should be used.</param>
    /// <param name="parameterizedCollectionTranslationMode">A value indicating what translation mode should be used.</param>
    [EntityFrameworkInternal]
    public RelationalParameterBasedSqlProcessorParameters(bool useRelationalNulls, ParameterizedCollectionTranslationMode? parameterizedCollectionTranslationMode)
    {
        UseRelationalNulls = useRelationalNulls;
        ParameterizedCollectionTranslationMode = parameterizedCollectionTranslationMode;
    }
}
