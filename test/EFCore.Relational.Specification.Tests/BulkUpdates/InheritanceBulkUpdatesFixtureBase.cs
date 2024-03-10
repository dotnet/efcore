// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public abstract class InheritanceBulkUpdatesFixtureBase : InheritanceQueryFixtureBase, IBulkUpdatesFixtureBase
{
    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            w => w.Log(CoreEventId.FirstWithoutOrderByAndFilterWarning)
                .Ignore(
                    CoreEventId.MappedEntityTypeIgnoredWarning,
                    CoreEventId.MappedPropertyIgnoredWarning,
                    CoreEventId.MappedNavigationIgnoredWarning));

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}
