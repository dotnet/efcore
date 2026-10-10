using Microsoft.EntityFrameworkCore;

namespace UpgradeDemo;

public class CustomerContext(DbContextOptions<CustomerContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(customer =>
        {
            customer.ToTable("Customers");
            customer.HasKey(customer => customer.Id);
            customer.Property(customer => customer.Id).UseIdentityColumn();

            customer.ComplexProperty(customer => customer.Address, address =>
            {
                address.IsRequired();
                address.Property(address => address.Line1)
                    .IsRequired()
                    .HasMaxLength(160)
                    .HasColumnType("nvarchar(160)");
                address.Property(address => address.ZipCode).HasColumnType("int");
            });
        });
    }
}

public class Customer
{
    public int Id { get; set; }
    public Address Address { get; set; } = new();
}

public class Address
{
    public string Line1 { get; set; } = "";
    public int ZipCode { get; set; }
}