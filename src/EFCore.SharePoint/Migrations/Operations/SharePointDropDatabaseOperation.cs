// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
/// Represents a SharePoint site collection drop operation in a migration.
/// </summary>
public class SharePointDropDatabaseOperation : MigrationOperation
{
    /// <summary>
    /// Gets or sets the URL of the SharePoint site collection to drop.
    /// </summary>
    public string? SiteCollectionUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to delete the site collection only if it exists.
    /// </summary>
    public bool IfExists { get; set; }
}
