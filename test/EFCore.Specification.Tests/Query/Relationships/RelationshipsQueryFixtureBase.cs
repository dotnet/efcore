// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public abstract class RelationshipsQueryFixtureBase : SharedStoreFixtureBase<RelationshipsContext>, IQueryFixtureBase
{
    private RelationshipsData? _expectedData;

    public Func<DbContext> GetContextCreator()
        => () => CreateContext();

    public virtual ISetSource GetExpectedData()
        => _expectedData ??= new RelationshipsData();

    protected override Task SeedAsync(RelationshipsContext context)
    {
        throw new InvalidOperationException("Override this method in dervided fixtures.");
    }

    public abstract IReadOnlyDictionary<Type, object> EntitySorters { get; }

    public abstract IReadOnlyDictionary<Type, object> EntityAsserters { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<RelationshipsRootEntity>().Property(x => x.Id).ValueGeneratedNever();
    }
}
