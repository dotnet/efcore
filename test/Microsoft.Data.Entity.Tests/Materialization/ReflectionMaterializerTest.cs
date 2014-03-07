// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Materialization;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Materialization
{
    public class ReflectionMaterializerTest
    {
        #region Fixture

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime CreatedOn { get; set; }
            public long? Age { get; set; }
            public decimal Balance { get; set; }
            public Guid Token { get; set; }
        }

        public class NoCtor
        {
            // ReSharper disable once UnusedParameter.Local
            public NoCtor(string foo)
            {
            }
        }

        #endregion

        [Fact]
        public void Can_materialize_entity()
        {
            var entityType = CreateEntityType();
            var reflectionMaterializer = new ReflectionMaterializer(entityType);

            var values = new object[]
                {
                    12345L,
                    42.42m,
                    DateTime.Now,
                    42,
                    "Tyrion",
                    Guid.NewGuid()
                };

            var customer = (Customer)reflectionMaterializer.Materialize(values);

            Assert.NotNull(customer);
            Assert.Equal(values[0], customer.Age);
            Assert.Equal(values[1], customer.Balance);
            Assert.Equal(values[2], customer.CreatedOn);
            Assert.Equal(values[3], customer.Id);
            Assert.Equal(values[4], customer.Name);
            Assert.Equal(values[5], customer.Token);
        }

        [Fact]
        public void Materialize_throws_when_no_default_ctor()
        {
            var entityType = new EntityType(typeof(NoCtor));
            var reflectionMaterializer = new ReflectionMaterializer(entityType);

            Assert.Equal(
                Strings.FormatNoDefaultCtor(entityType.Type),
                Assert.Throws<InvalidOperationException>(
                    () => reflectionMaterializer.Materialize(new object[0])).Message);
        }

        [Fact]
        public void Can_shred_entity()
        {
            var entityType = CreateEntityType();
            var reflectionMaterializer = new ReflectionMaterializer(entityType);

            var customer = new Customer
                {
                    Age = 12345L,
                    Balance = 42.42m,
                    CreatedOn = DateTime.Now,
                    Id = 42,
                    Name = "Tyrion",
                    Token = Guid.NewGuid()
                };

            var values = reflectionMaterializer.Shred(customer);

            Assert.NotNull(values);
            Assert.Equal(customer.Age, values[0]);
            Assert.Equal(customer.Balance, values[1]);
            Assert.Equal(customer.CreatedOn, values[2]);
            Assert.Equal(customer.Id, values[3]);
            Assert.Equal(customer.Name, values[4]);
            Assert.Equal(customer.Token, values[5]);
        }

        private static EntityType CreateEntityType()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>()
                .Properties(ps =>
                    {
                        ps.Property(c => c.Id);
                        ps.Property(c => c.Name);
                        ps.Property(c => c.CreatedOn);
                        ps.Property(c => c.Age);
                        ps.Property(c => c.Balance);
                        ps.Property(c => c.Token);
                    });

            return model.GetEntityType(typeof(Customer));
        }
    }
}
