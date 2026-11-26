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
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().OwnsMany(c => c.Orders, ob => ob.Ignore(o => o.Customer));
        modelBuilder.Entity<Customer>().Navigation(c => c.Orders).IsRequired(false);

        VerifyError(
            InMemoryStrings.NullableCollectionNavigationNotSupported(nameof(Customer.Orders), nameof(Customer)),
            modelBuilder);
    }

    [Fact]
    public virtual void Owned_collection_annotated_as_nullable_is_not_configured_as_nullable()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<NullableItemsOwner>().OwnsMany(o => o.Items);

        var model = Validate(modelBuilder);

        // Only relational providers configure owned collections annotated as nullable as nullable
        Assert.False(model.FindEntityType(typeof(NullableItemsOwner))!.FindNavigation(nameof(NullableItemsOwner.Items))!.IsNullable);
    }

    protected override TestHelpers TestHelpers
        => InMemoryTestHelpers.Instance;

    protected class NullableItemsOwner
    {
        public int Id { get; set; }
        public List<NullableItemsElement>? Items { get; set; }
    }

    protected class NullableItemsElement
    {
        public string? Name { get; set; }
    }
}
