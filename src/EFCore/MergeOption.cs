// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// The different ways that new objects loaded from the database can be merged with existing objects already in memory.
/// </summary>
public enum MergeOption
{
    /// <summary>
    /// Will only append new (top level-unique) rows.  This is the default behavior.
    /// </summary>
    AppendOnly = 0,

    /// <summary>
    /// The incoming values for this row will be written to both the current value and
    /// the original value versions of the data for each column.
    /// </summary>
    OverwriteChanges = 1,

    /// <summary>
    /// The incoming values for this row will be written to the original value version
    /// of each column. The current version of the data in each column will not be changed.
    /// </summary>
    PreserveChanges = 2
}
