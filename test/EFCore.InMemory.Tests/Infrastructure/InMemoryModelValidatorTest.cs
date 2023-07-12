// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class InMemoryModelValidatorTest : ModelValidatorTestBase
{
    [ConditionalFact]
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

    protected override TestHelpers TestHelpers
        => InMemoryTestHelpers.Instance;
}
