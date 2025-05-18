// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A SharePoint-specific <see cref="MigrationOperation" /> to create a site collection.
/// </summary>
/// <remarks>
///     See SharePoint documentation for more information and examples on site collection creation.
/// </remarks>
[DebuggerDisplay("CREATE SITE COLLECTION {Url}")]
public class SharePointCreateDatabaseOperation : DatabaseOperation
{
    /// <summary>
    ///     The URL of the SharePoint site collection.
    /// </summary>
    public virtual string Url { get; set; } = null!;

    /// <summary>
    ///     The title of the SharePoint site collection.
    /// </summary>
    public virtual string? Title { get; set; }

    /// <summary>
    ///     The template to use for the site collection, or <see langword="null" /> to use the default.
    /// </summary>
    public virtual string? Template { get; set; }
}
