// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.StoredProcedureUpdateModel;

public class StoredProcedureUpdateContext : PoolableDbContext
{
    public StoredProcedureUpdateContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Entity> WithOutputParameter
        => Set<Entity>(nameof(WithOutputParameter));

    public DbSet<Entity> WithResultColumn
        => Set<Entity>(nameof(WithResultColumn));

    public DbSet<EntityWithAdditionalProperty> WithTwoResultColumns
        => Set<EntityWithAdditionalProperty>(nameof(WithTwoResultColumns));

    public DbSet<EntityWithAdditionalProperty> WithOutputParameterAndResultColumn
        => Set<EntityWithAdditionalProperty>(nameof(WithOutputParameterAndResultColumn));

    public DbSet<EntityWithAdditionalProperty> WithOutputParameterAndRowsAffectedResultColumn
        => Set<EntityWithAdditionalProperty>(nameof(WithOutputParameterAndRowsAffectedResultColumn));

    public DbSet<EntityWithTwoAdditionalProperties> WithOutputParameterAndResultColumnAndResultValue
        => Set<EntityWithTwoAdditionalProperties>(nameof(WithOutputParameterAndResultColumnAndResultValue));

    public DbSet<EntityWithAdditionalProperty> WithTwoOutputParameters
        => Set<EntityWithAdditionalProperty>(nameof(WithTwoOutputParameters));

    public DbSet<Entity> WithRowsAffectedParameter
        => Set<Entity>(nameof(WithRowsAffectedParameter));

    public DbSet<Entity> WithRowsAffectedResultColumn
        => Set<Entity>(nameof(WithRowsAffectedResultColumn));

    public DbSet<Entity> WithRowsAffectedReturnValue
        => Set<Entity>(nameof(WithRowsAffectedReturnValue));

    public DbSet<Entity> WithStoreGeneratedConcurrencyTokenAsInoutParameter
        => Set<Entity>(nameof(WithStoreGeneratedConcurrencyTokenAsInoutParameter));

    public DbSet<Entity> WithStoreGeneratedConcurrencyTokenAsTwoParameters
        => Set<Entity>(nameof(WithStoreGeneratedConcurrencyTokenAsTwoParameters));

    public DbSet<EntityWithAdditionalProperty> WithUserManagedConcurrencyToken
        => Set<EntityWithAdditionalProperty>(nameof(WithUserManagedConcurrencyToken));

    public DbSet<Entity> WithOriginalAndCurrentValueOnNonConcurrencyToken
        => Set<Entity>(nameof(WithOriginalAndCurrentValueOnNonConcurrencyToken));

    public DbSet<Entity> WithInputOutputParameterOnNonConcurrencyToken
        => Set<Entity>(nameof(WithInputOutputParameterOnNonConcurrencyToken));

    public DbSet<TphParent> TphParent { get; set; }
    public DbSet<TphChild1> TphChild { get; set; }
    public DbSet<TptParent> TptParent { get; set; }
    public DbSet<TptChild> TptChild { get; set; }
    public DbSet<TptMixedParent> TptMixedParent { get; set; }
    public DbSet<TptMixedChild> TptMixedChild { get; set; }
    public DbSet<TpcParent> TpcParent { get; set; }
    public DbSet<TpcChild> TpcChild { get; set; }
}
