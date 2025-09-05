// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Migrations.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteMigrationsAnnotationProvider : MigrationsAnnotationProvider
{
    /// <summary>
    ///     Initializes a new instance of this class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
#pragma warning disable EF1001 // Internal EF Core API usage.
    public SqliteMigrationsAnnotationProvider(MigrationsAnnotationProviderDependencies dependencies)
#pragma warning restore EF1001 // Internal EF Core API usage.
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> ForRemove(IColumn column)
    {
        // Preserve the autoincrement annotation when removing columns for SQLite migrations
        if (column[SqliteAnnotationNames.Autoincrement] as bool? == true)
        {
            yield return new Annotation(SqliteAnnotationNames.Autoincrement, true);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> ForRename(IColumn column)
    {
        // Preserve the autoincrement annotation when renaming columns for SQLite migrations
        if (column[SqliteAnnotationNames.Autoincrement] as bool? == true)
        {
            yield return new Annotation(SqliteAnnotationNames.Autoincrement, true);
        }
    }
}