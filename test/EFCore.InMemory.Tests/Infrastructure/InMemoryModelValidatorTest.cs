// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class InMemoryModelValidatorTest : ModelValidatorTestBase
{
    [Fact]
    public virtual void Detects_ToQuery_on_derived_keyless_types()
    {
        var modelBuilder = base.CreateConventionModelBuilder();
        var context = new DbContext(new DbContextOptions<DbContext>());
        modelBuilder.Entity<Abstract>().HasNoKey().ToInMemoryQuery(() => context.Set<Abstract>());

        Expression<Func<IQueryable<Generic<int>>>> query = () => context.Set<Generic<int>>();
        modelBuilder.Entity<Generic<int>>().ToInMemoryQuery((LambdaExpression)query);

        VerifyError(
            CoreStrings.DerivedTypeDefiningQuery("Generic<int>", nameof(Abstract)),
            modelBuilder);
    }

    [Fact]
    public virtual void Detects_nullable_collection_navigation()
    {
        var modelBuilder = base.CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().HasMany(c => c.Orders).WithOne(o => o.Customer);
        modelBuilder.Model.FindEntityType(typeof(Customer))!.FindNavigation(nameof(Customer.Orders))!.SetIsNullable(true);

        VerifyError(
            InMemoryStrings.NullableCollectionNavigationNotSupported(nameof(Customer.Orders), nameof(Customer)),
            modelBuilder);
    }

    protected override TestHelpers TestHelpers
        => InMemoryTestHelpers.Instance;
}
