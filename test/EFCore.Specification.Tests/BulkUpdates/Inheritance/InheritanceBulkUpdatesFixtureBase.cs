// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Inheritance;

namespace Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;

#nullable disable

public abstract class InheritanceBulkUpdatesFixtureBase : InheritanceQueryFixtureBase
{
    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(w => w.Log(CoreEventId.FirstWithoutOrderByAndFilterWarning)
            .Ignore(
                CoreEventId.MappedEntityTypeIgnoredWarning,
                CoreEventId.MappedPropertyIgnoredWarning,
                CoreEventId.MappedNavigationIgnoredWarning));

    public abstract override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction);
}
