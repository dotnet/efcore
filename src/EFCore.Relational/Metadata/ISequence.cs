// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a database sequence in the model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-sequences">Database sequences</see> for more information and examples.
/// </remarks>
public interface ISequence : IReadOnlySequence, IAnnotatable
{
    /// <summary>
    ///     Gets the model in which this sequence is defined.
    /// </summary>
    new IModel Model { get; }
}
