// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class OracleInsertBatch
    {
        [Fact]
        public void Insert_batch_records()
        {
            var serviceProvider = new ServiceCollection()
             .AddEntityFrameworkOracle()
             .BuildServiceProvider();
              
            using (var store = OracleTestStore.GetNorthwindStore())
            {
                using (var connection = new OracleConnection(store.ConnectionString))
                {
                    BatchInsertContext cxt = new BatchInsertContext(serviceProvider, connection);
                    //Test Insert Batch Customer
                    for (int i = 0; i < 20; i++)
                    {
                        cxt.Customers.Add(new Customer
                        {
                            CustomerID = RandomKey(),
                            CompanyName = $"Net Foundation Test {i}",
                            Fax = "79 XXXX-8693"
                        });
                    }

                    Assert.Equal(20, cxt.SaveChanges());

                    //Test Insert Batch Product
                    CreateTableProduct(cxt);
                    for (int i = 0; i < 50; i++)
                    {
                        cxt.Products.Add(new Product
                        {
                            Description = $"Book EF Core 2.x {i}",
                            Quanty = 100000,
                            Price = 389.99m

                        });
                    }

                    Assert.Equal(50, cxt.SaveChanges()); 
                }
            } 
        }

        private class BatchInsertContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly OracleConnection _connection;

            public BatchInsertContext(IServiceProvider serviceProvider, OracleConnection connection)
            {
                _serviceProvider = serviceProvider;
                _connection = connection;
            }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public DbSet<Customer> Customers { get; set; }
            public DbSet<Product> Products { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseOracle(_connection, b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(_serviceProvider);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>(
                                b =>
                                {
                                    b.HasKey(c => c.CustomerID);
                                    b.ToTable("Customers");
                                });

                modelBuilder.Entity<Product>().ToTable("Product");
            }

        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Customer
        {
            public string CustomerID { get; set; }

            public string CompanyName { get; set; }

            public string Fax { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Product
        {
            [Key]
            public int Id { get; set; }

            [StringLength(100)]
            public string Description { get; set; }

            public long Quanty { get; set; }

            public decimal Price { get; set; }
        }

        private static Random random = new Random();

        private string RandomKey()
        {
            var charsRandom = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            var info = new string(Enumerable.Repeat(charsRandom, 5)
                                  .Select(s => s[random.Next(s.Length)]).ToArray());
            return info;
        }

        private void CreateTableProduct(BatchInsertContext context)
        {
            //Table teste auto-increment
            var commandTable = new StringBuilder();
            //Drops
            commandTable.AppendLine("DECLARE")
                        .AppendLine("C INT;")
                        .AppendLine("BEGIN")
                        .AppendLine("select count(1) into C from user_tables where table_name = 'Product';")
                        .AppendLine("IF C = 1 THEN")
                        .AppendLine("EXECUTE IMMEDIATE 'DROP TABLE \"Product\"';")
                        .AppendLine("END IF;")
                        .AppendLine("select count(1) into C from user_sequences where UPPER(sequence_name) = 'PRODUCT_SEQUENCE';")
                        .AppendLine("IF C = 1 THEN")
                        .AppendLine("EXECUTE IMMEDIATE 'DROP SEQUENCE \"product_sequence\"';")
                        .AppendLine("END IF;")
                        .AppendLine("select count(1) into C from user_triggers where UPPER(trigger_name) = 'PRODUCT_ON_INSERT';")
                        .AppendLine("IF C = 1 THEN")
                        .AppendLine("EXECUTE IMMEDIATE 'DROP TRIGGER \"product_on_insert\"';")
                        .AppendLine("END IF;")
                        .AppendLine("END;");
            context.Database.ExecuteSqlCommand(commandTable.ToString());

            commandTable.Clear();
            commandTable.AppendLine("CREATE TABLE \"Product\"(")
                        .AppendLine("\"Id\" NUMBER NOT NULL ENABLE,")
                        .AppendLine("\"Description\" VARCHAR2(100) NULL,")
                        .AppendLine("\"Quanty\" LONG DEFAULT 0,")
                        .AppendLine("\"Price\" NUMBER(10, 2) DEFAULT 0,")
                        .AppendLine(" PRIMARY KEY(\"Id\") ENABLE);");
            context.Database.ExecuteSqlCommand(commandTable.ToString());

            commandTable.Clear();
            commandTable.AppendLine("BEGIN")
                        .AppendLine("EXECUTE IMMEDIATE 'CREATE SEQUENCE \"product_sequence\" START WITH 1 ")
                        .AppendLine("MINVALUE 1 MAXVALUE 1000000000000000 INCREMENT BY 1 NOCYCLE CACHE 20 NOORDER';")
                        .AppendLine("END;");
            context.Database.ExecuteSqlCommand(commandTable.ToString());

            commandTable.Clear();
            commandTable.Clear();
            commandTable.AppendLine("BEGIN")
                        .Append("EXECUTE IMMEDIATE 'CREATE TRIGGER \"product_on_insert\" BEFORE INSERT ")
                        .Append("ON \"Product\" FOR EACH ROW BEGIN SELECT \"product_sequence\".nextval INTO :new.\"Id\" ")
                        .Append("FROM dual; ")
                        .Append("END;'")
                        .Append(";")
                        .AppendLine("END;");
            context.Database.ExecuteSqlCommand(commandTable.ToString());
            context.Database.ExecuteSqlCommand("ALTER TRIGGER \"product_on_insert\" ENABLE");
        }

    }
}
