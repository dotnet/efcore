// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


// ReSharper disable InconsistentNaming
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable UnusedParameter.Local
// ReSharper disable PossibleInvalidOperationException
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class DomainDrivenDesignComplianceSqlServerTests
    {
        [Fact]
        public void Cannot_use_complex_types_as_primary_key_without_additional_configuration()
        {
            var entityConfiguration = new DelegatingEntityConfiguration(builder =>
             {
                 builder.HasKey(x => x.Id);
             });

            using var context = new DddConfigurationContextContext(entityConfiguration);
            Assert.Throws<InvalidOperationException>(() => context.Model);
        }

        [Fact]
        public void Cannot_use_complex_types_as_primary_key_with_identity_value_generation_without_additional_configuration()
        {
            var entityConfiguration = new DelegatingEntityConfiguration(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).UseIdentityColumn();
            });

            using var context = new DddConfigurationContextContext(entityConfiguration);
            Assert.Throws<ArgumentException>(() => context.Model);
        }
        
        [Fact]
        public void Can_use_complex_types_as_primary_key_with_identity_value_generation_with_custom_converter()
        {
            var entityConfiguration = new DelegatingEntityConfiguration(builder =>
            {
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id)
                    .HasConversion(x => x.Value, x => new OrderId(x))
                    .UseIdentityColumn();
            });

            using var context = new DddConfigurationContextContext(entityConfiguration);
            var model = context.Model;
            Console.WriteLine(model);
        }

        public class Order
        {
            public OrderId Id { get; set; }
            public DateTimeOffset Date { get; set; }
        }

        public class DddConfigurationContextContext : DbContext
        {
            private readonly IEntityTypeConfiguration<Order> configuration;

            public DddConfigurationContextContext(IEntityTypeConfiguration<Order> configuration)
                : base(new DbContextOptionsBuilder().UseSqlServer(new ConnectionInterceptionSqlServerTestBase.FakeDbConnection()).Options)
            {
                this.configuration = configuration;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(configuration);
            }

            public DbSet<Order> Orders { get; set; }
        }

        public class OrderId : IEquatable<OrderId>
        {
            public OrderId(int value)
            {
                Value = value;
            }

            public int Value { get; private set; }

            public bool Equals([AllowNull] OrderId other)
            {

                if (other == null)
                {
                    return false;
                }

                return other.Value == Value;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is OrderId other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }

            public static bool operator ==(OrderId left, OrderId right)
            {
                if (left is null)
                    return false;

                return left.Equals(right);
            }

            public static bool operator !=(OrderId left, OrderId right)
            {
                return !(left == right);
            }
        }

        private class DelegatingEntityConfiguration : IEntityTypeConfiguration<Order>
        {
            private readonly Action<EntityTypeBuilder<Order>> _configureAction;

            public DelegatingEntityConfiguration(Action<EntityTypeBuilder<Order>> configureAction)
            {
                _configureAction = configureAction;
            }

            public void Configure(EntityTypeBuilder<Order> builder)
            {
                _configureAction.Invoke(builder);
            }
        }
    }
}
