// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a database trigger on a table.
/// </summary>
/// <remarks>
///     <para>
///         Since triggers features vary across databases, this is mainly an extension point for providers to add their own annotations.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
///     </para>
/// </remarks>
public interface IMutableTrigger : IReadOnlyTrigger, IMutableAnnotatable
{
    /// <summary>
    ///     Gets the <see cref="IMutableEntityType" /> on which this trigger is defined.
    /// </summary>
    new IMutableEntityType EntityType { get; }

    /// <summary>
    ///     Gets or sets the name of the trigger in the database.
    /// </summary>
    new string? Name { get; set; }
}
