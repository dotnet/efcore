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
public interface ITrigger : IReadOnlyTrigger, IAnnotatable
{
    /// <summary>
    ///     Gets the entity type on which this trigger is defined.
    /// </summary>
    new IEntityType EntityType { get; }

    /// <summary>
    ///     Gets the database name of the trigger.
    /// </summary>
    new string Name { get; }
}
