// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.CrossStore.FunctionalTests
{
    public class DiscriminatorTest
    {
        [Fact]
        public void Can_save_enities_with_discriminators()
        {
            using (var context = new Context4285())
            {
                context.AddRange(new SubProduct { SomeName = "One" }, new SubProduct2 { SomeName2 = "Two" });
                context.SaveChanges();
            }

            using (var context = new Context4285())
            {
                var products = context.Products.ToList();
                Assert.Equal("One", products.OfType<SubProduct>().Single().SomeName);
                Assert.Equal("Two", products.OfType<SubProduct2>().Single().SomeName2);
            }
        }

        [Fact]
        public void Can_save_enities_with_int_discriminators()
        {
            using (var context = new Context4285())
            {
                context.AddRange(new SubIntProduct { SomeName = "One" }, new SubIntProduct2 { SomeName2 = "Two" });
                context.SaveChanges();
            }

            using (var context = new Context4285())
            {
                var products = context.IntProducts.ToList();
                Assert.Equal("One", products.OfType<SubIntProduct>().Single().SomeName);
                Assert.Equal("Two", products.OfType<SubIntProduct2>().Single().SomeName2);
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
                => optionsBuilder.UseInMemoryDatabase();

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
