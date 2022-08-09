// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Indicates how changes to the value of a property will be handled by Entity Framework change tracking
///     which in turn will determine whether the value set is sent to the database or not.
///     Used with <see cref="IReadOnlyProperty.GetBeforeSaveBehavior" /> and
///     <see cref="IReadOnlyProperty.GetAfterSaveBehavior" />
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> and
///     <see href="https://aka.ms/efcore-docs-saving-data">Saving data in EF Core</see> for more information and examples.
/// </remarks>
public enum PropertySaveBehavior
{
    /// <summary>
    ///     The value set or changed will be sent to the database in the normal way.
    /// </summary>
    Save,

    /// <summary>
    ///     Any value set or changed will be ignored.
    /// </summary>
    Ignore,

    /// <summary>
    ///     If an explicit value is set or the value is changed, then an exception will be thrown.
    /// </summary>
    Throw
}
