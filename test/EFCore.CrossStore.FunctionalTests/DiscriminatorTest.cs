// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class DiscriminatorTest
    {
        [ConditionalFact(Skip = "Tasklist#21")]
        public void Can_save_entities_with_discriminators()
        {
            using (var context = new Context4285())
            {
                context.AddRange(
                    new SubProduct
                    {
                        SomeName = "One"
                    }, new SubProduct2
                    {
                        SomeName2 = "Two"
                    });
                context.SaveChanges();
            }

            using (var context = new Context4285())
            {
                var products = context.Products.ToList();
                var productOne = products.OfType<SubProduct>().Single();
                Assert.Equal("One", productOne.SomeName);
                Assert.Equal(nameof(SubProduct), context.Entry(productOne).Property<string>("Discriminator").CurrentValue);
                var productTwo = products.OfType<SubProduct2>().Single();
                Assert.Equal("Two", productTwo.SomeName2);
                Assert.Equal(nameof(SubProduct2), context.Entry(productTwo).Property<string>("Discriminator").CurrentValue);
            }
        }

        [ConditionalFact(Skip = "Tasklist#21")]
        public void Can_save_entities_with_int_discriminators()
        {
            using (var context = new Context4285())
            {
                context.AddRange(
                    new SubIntProduct
                    {
                        SomeName = "One"
                    }, new SubIntProduct2
                    {
                        SomeName2 = "Two"
                    });
                context.SaveChanges();
            }

            using (var context = new Context4285())
            {
                var products = context.IntProducts.ToList();
                var productOne = products.OfType<SubIntProduct>().Single();
                Assert.Equal("One", productOne.SomeName);
                Assert.Equal(1, context.Entry(productOne).Property<int>("IntDiscriminator").CurrentValue);
                var productTwo = products.OfType<SubIntProduct2>().Single();
                Assert.Equal("Two", productTwo.SomeName2);
                Assert.Equal(2, context.Entry(productTwo).Property<int>("IntDiscriminator").CurrentValue);
            }
        }

        private class BaseProduct
        {
            public Guid Id { get; set; }
        }

        private class SubProduct : BaseProduct
        {
            public string SomeName { get; set; }
        }

        private class SubProduct2 : BaseProduct
        {
            public string SomeName2 { get; set; }
        }

        private class BaseIntProduct
        {
            public Guid Id { get; set; }
        }

        private class SubIntProduct : BaseIntProduct
        {
            public string SomeName { get; set; }
        }

        private class SubIntProduct2 : BaseIntProduct
        {
            public string SomeName2 { get; set; }
        }

        private class Context4285 : DbContext
        {
            public DbSet<BaseProduct> Products { get; set; }
            public DbSet<SubProduct> SubProducts { get; set; }
            public DbSet<SubProduct2> SubProducts2 { get; set; }
            public DbSet<BaseIntProduct> IntProducts { get; set; }
            public DbSet<SubIntProduct> SubIntProducts { get; set; }
            public DbSet<SubIntProduct2> SubIntProducts2 { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInMemoryDatabase(nameof(Context4285));

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.Entity<BaseProduct>()
                    .ToTable("BaseProducts")
                    .HasDiscriminator<string>("Discriminator")
                    .HasValue<SubProduct>("SubProduct")
                    .HasValue<SubProduct2>("SubProduct2");

                builder.Entity<BaseIntProduct>()
                    .ToTable("BaseIntProducts")
                    .HasDiscriminator<int>("IntDiscriminator")
                    .HasValue<SubIntProduct>(1)
                    .HasValue<SubIntProduct2>(2);
            }
        }
    }
}
