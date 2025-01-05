// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Options to control the behavior of loading related entities with <see cref="NavigationEntry.Load(LoadOptions)" />.
/// </summary>
[Flags]
public enum LoadOptions
{
    /// <summary>
    ///     <para>
    ///         Applies no special options to loading of related entities.
    ///     </para>
    ///     <para>
    ///         If the entity is tracked, then entities with the same primary key value are not replaced
    ///         by new entities or overwritten with new data from the database. If the entity entity represented by this entry is not
    ///         tracked and the collection already contains entities, then calling this method will result in duplicate
    ///         instances in the collection or inverse collection for any entities with the same key value.
    ///         Use <see cref="ForceIdentityResolution" /> to avoid getting these duplicates.
    ///     </para>
    /// </summary>
    None = 0,

    /// <summary>
    ///     <para>
    ///         Ensures that entities with the same primary key value are not replaced by new entities or overwritten with new data from
    ///         the database. The loaded navigation and its inverse will not contain duplicate entities.
    ///     </para>
    ///     <para>
    ///         For tracked entities, this option behaves in the same way and has the same performance as
    ///         the default. For entities that are not tracked, this option can be significantly slower.
    ///     </para>
    /// </summary>
    ForceIdentityResolution = 1
}
