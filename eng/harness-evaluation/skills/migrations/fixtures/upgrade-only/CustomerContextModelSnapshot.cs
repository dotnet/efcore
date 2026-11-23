using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace UpgradeDemo;

[DbContext(typeof(CustomerContext))]
partial class CustomerContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.3")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("UpgradeDemo.Customer", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");

            SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

            b.HasKey("Id");

            b.ComplexProperty<Dictionary<string, object>>("Address", "UpgradeDemo.Customer.Address#Address", b1 =>
            {
                b1.Property<string>("Line1")
                    .IsRequired()
                    .HasMaxLength(160)
                    .HasColumnType("nvarchar(160)");

                b1.Property<int>("ZipCode")
                    .HasColumnType("int");
            });

            b.ToTable("Customers");
        });
    }
}