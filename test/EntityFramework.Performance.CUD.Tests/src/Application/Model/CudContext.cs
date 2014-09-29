using System;
using System.Linq;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

namespace Cud.Model
{
    public class CudContext : DbContext
    {
        private readonly string _connectionString;

        public CudContext(string connectionString, IServiceProvider serviceProvider, DbContextOptions options)
            : base(serviceProvider, options)
        {
            _connectionString = connectionString;
        }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderLine> OrderLines { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        
        protected override void OnConfiguring(DbContextOptions builder)
        {
            builder.UseSqlServer(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(entityBuilder =>
            {
                entityBuilder.Key(c => c.Id);
                entityBuilder.ToTable("Customers");
                entityBuilder.OneToMany(c => c.Orders, o => o.Customer);
            });

            modelBuilder.Entity<Order>(entityBuilder =>
            {
                entityBuilder.Key(o => o.Id);
                entityBuilder.ToTable("Orders");
                entityBuilder.OneToMany(o => o.OrderLines, ol => ol.Order);
            });

            modelBuilder.Entity<OrderLine>(entityBuilder =>
            {
                entityBuilder.Key(o => o.Id);
                entityBuilder.ToTable("OrderLines");
            });

            modelBuilder.Entity<Product>(entityBuilder =>
            {
                entityBuilder.Key(o => o.Id);
                entityBuilder.ToTable("Products");
                entityBuilder.OneToMany(p => p.OrderLines, ol => ol.Product);
            });
        }

        public static void SetupDatabase(CudContext context)
        {
            using (context)
            {
                context.Database.EnsureCreated();
                if (!context.Set<Customer>().Any())
                {
                    InsertTestingData(context);
                }
                //clear the SQL Server plan cache
                //TODO: context.ExecuteStoreCommand("DBCC FREEPROCCACHE WITH NO_INFOMSGS;");
            }
        }

        public static void InsertTestingData(CudContext context)
        {
            for (var i = 0; i < 10; i++)
            {
                var customer = new Customer { Name = "Customer" };

                for (var j = 0; j < 5; j++)
                {
                    var order = new Order { Date = DateTime.Now };

                    for (var k = 0; k < 1; k++)
                    {
                        var orderLine = new OrderLine
                        {
                            Price = 123.45m,
                            Quantity = 42,
                            Product = k % 2 == 0
                                          ? new Product { Name = "Product" }
                                          : new SpecialProduct { Name = "SpecialProduct", Style = "Cool" }
                        };

                        order.OrderLines.Add(orderLine);
                    }

                    customer.Orders.Add(order);
                }

                context.Customers.Add(customer);
                context.SaveChanges();
            }
        }
    }
}