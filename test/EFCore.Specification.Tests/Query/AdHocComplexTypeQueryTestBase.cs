// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

// ReSharper disable ClassNeverInstantiated.Local
public abstract class AdHocComplexTypeQueryTestBase : NonSharedModelTestBase
{
    #region 33449

    [ConditionalFact]
    public virtual async Task Complex_type_equals_parameter_with_nested_types_with_property_of_same_name()
    {
        var contextFactory = await InitializeAsync<Context33449>(
            seed: context =>
            {
                context.AddRange(
                    new Context33449.EntityType
                    {
                        ComplexContainer = new Context33449.ComplexContainer
                        {
                            Id = 1,
                            Containee1 = new Context33449.ComplexContainee1 { Id = 2 },
                            Containee2 = new Context33449.ComplexContainee2 { Id = 3 }
                        }
                    });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var container = new Context33449.ComplexContainer
        {
            Id = 1,
            Containee1 = new Context33449.ComplexContainee1 { Id = 2 },
            Containee2 = new Context33449.ComplexContainee2 { Id = 3 }
        };

        _ = await context.Set<Context33449.EntityType>().Where(b => b.ComplexContainer == container).SingleAsync();
    }

    private class Context33449(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EntityType>().ComplexProperty(
                b => b.ComplexContainer, x =>
                {
                    x.ComplexProperty(c => c.Containee1);
                    x.ComplexProperty(c => c.Containee2);
                });

        public class EntityType
        {
            public int Id { get; set; }
            public ComplexContainer ComplexContainer { get; set; } = null!;
        }

        public class ComplexContainer
        {
            public int Id { get; set; }

            public ComplexContainee1 Containee1 { get; set; } = null!;
            public ComplexContainee2 Containee2 { get; set; } = null!;
        }

        public class ComplexContainee1
        {
            public int Id { get; set; }
        }

        public class ComplexContainee2
        {
            public int Id { get; set; }
        }
    }

    #endregion 33449

    protected override string StoreName
        => "AdHocComplexTypeQueryTest";
}
