using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using var context = new TestContext();
Console.WriteLine(context.Database.GenerateCreateScript());

public class TestContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("DataSource=:memory:");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Thing>()
            .HasDiscriminator()
            .HasValue<ThingA>("A")
            .HasValue<ThingB>("B");

        modelBuilder.Entity<ThingA>().ComplexProperty(e => e.Address, a =>
        {
            a.Property(p => p.Street).HasColumnName("Address_Street");
            a.Property(p => p.City).HasColumnName("Address_City");
        });
        modelBuilder.Entity<ThingB>().ComplexProperty(e => e.Address, a =>
        {
            a.Property(p => p.Street).HasColumnName("Address_Street");
            a.Property(p => p.City).HasColumnName("Address_City");
        });
    }
}

[ComplexType]
public record class Address(string Street, string City);

public abstract class Thing
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
}

public sealed class ThingA : Thing
{
    public required Address Address { get; set; }
}
public sealed class ThingB : Thing
{
    public required Address Address { get; set; }
}
