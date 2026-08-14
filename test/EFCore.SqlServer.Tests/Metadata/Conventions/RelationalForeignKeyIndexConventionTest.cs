// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class RelationalForeignKeyIndexConventionTest
{
    [ConditionalFact]
    public void Removing_relationship_removes_unused_conventional_index()
    {
        var modelBuilder = CreateConventionalModelBuilder();
        modelBuilder.Ignore(typeof(SpecialOrder), ConfigurationSource.Explicit);
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var derivedPrincipalEntityBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            new[] { dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata },
            ConfigurationSource.DataAnnotation);
        Assert.NotNull(relationshipBuilder);

        var relationshipBuilder2 = dependentEntityBuilder.HasRelationship(
            derivedPrincipalEntityBuilder.Metadata,
            new[] { dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata },
            ConfigurationSource.DataAnnotation);
        Assert.NotNull(relationshipBuilder2);
        Assert.NotSame(relationshipBuilder, relationshipBuilder2);
        Assert.Single(dependentEntityBuilder.Metadata.GetIndexes());

        Assert.NotNull(
            dependentEntityBuilder.HasNoRelationship(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Single(dependentEntityBuilder.Metadata.GetIndexes());
        Assert.Single(dependentEntityBuilder.Metadata.GetForeignKeys());

        Assert.NotNull(
            dependentEntityBuilder.HasNoRelationship(relationshipBuilder2.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Empty(dependentEntityBuilder.Metadata.GetIndexes());
        Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
    }

    [ConditionalFact]
    public void Removing_relationship_does_not_remove_conventional_index_if_in_use()
    {
        var modelBuilder = CreateConventionalModelBuilder();
        var principalEntityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
        var dependentEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

        var relationshipBuilder = dependentEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            new[] { dependentEntityBuilder.Property(Order.CustomerIdProperty, ConfigurationSource.Convention).Metadata },
            ConfigurationSource.Convention);
        Assert.NotNull(relationshipBuilder);
        dependentEntityBuilder.HasIndex(new[] { Order.CustomerIdProperty }, ConfigurationSource.Explicit);

        Assert.NotNull(dependentEntityBuilder.HasNoRelationship(relationshipBuilder.Metadata, ConfigurationSource.DataAnnotation));

        Assert.Single(dependentEntityBuilder.Metadata.GetIndexes());
        Assert.Equal(Order.CustomerIdProperty.Name, dependentEntityBuilder.Metadata.GetIndexes().First().Properties.First().Name);
        Assert.Empty(dependentEntityBuilder.Metadata.GetForeignKeys());
    }

    private static TestLogger<DbLoggerCategory.Model, TestLoggingDefinitions> CreateTestLogger()
        => new() { EnabledFor = LogLevel.Warning };

    private InternalModelBuilder CreateModelBuilder(Model model = null)
        => new(model ?? new Model());

    private InternalModelBuilder CreateConventionalModelBuilder()
        => (InternalModelBuilder)SqlServerTestHelpers.Instance.CreateConventionBuilder().GetInfrastructure();

    public enum MemberType
    {
        Property,
        ComplexProperty,
        ServiceProperty,
        Navigation,
        SkipNavigation
    }

    private class Order
    {
        public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty(nameof(Id));
        public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty(nameof(CustomerId));
        public static readonly PropertyInfo CustomerUniqueProperty = typeof(Order).GetProperty(nameof(CustomerUnique));
        public static readonly PropertyInfo CustomerProperty = typeof(Order).GetProperty(nameof(Customer));
        public static readonly PropertyInfo ContextProperty = typeof(Order).GetProperty(nameof(Context));
        public static readonly PropertyInfo ProductsProperty = typeof(Order).GetProperty(nameof(Products));

        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Guid? CustomerUnique { get; set; }
        public Customer Customer { get; set; }
        public DbContext Context { get; set; }
        public ICollection<Product> Products { get; set; }
    }

    private class SpecialOrder : Order, IEnumerable<Order>
    {
        public static readonly PropertyInfo SpecialtyProperty = typeof(SpecialOrder).GetProperty("Specialty");

        public IEnumerator<Order> GetEnumerator()
        {
            yield return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public string Specialty { get; set; }
    }

    private class ExtraSpecialOrder : SpecialOrder;

    private class BackOrder : Order;

    private class Customer
    {
        public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty(nameof(Id));
        public static readonly PropertyInfo UniqueProperty = typeof(Customer).GetProperty(nameof(Unique));
        public static readonly PropertyInfo OrdersProperty = typeof(Customer).GetProperty(nameof(Orders));
        public static readonly PropertyInfo NotCollectionOrdersProperty = typeof(Customer).GetProperty(nameof(NotCollectionOrders));
        public static readonly PropertyInfo SpecialOrdersProperty = typeof(Customer).GetProperty(nameof(SpecialOrders));

        public int Id { get; set; }
        public Guid Unique { get; set; }
        public ICollection<Order> Orders { get; set; }
        public Order NotCollectionOrders { get; set; }
        public ICollection<SpecialOrder> SpecialOrders { get; set; }
        internal SpecialCustomer SpecialCustomer { get; set; }
    }

    private class SpecialCustomer : Customer
    {
        internal Customer Customer { get; set; }
    }

    private class OrderProduct
    {
        public static readonly PropertyInfo OrderIdProperty = typeof(OrderProduct).GetProperty(nameof(OrderId));
        public static readonly PropertyInfo ProductIdProperty = typeof(OrderProduct).GetProperty(nameof(ProductId));

        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }

    private class Product
    {
        public static readonly PropertyInfo IdProperty = typeof(Product).GetProperty(nameof(Id));

        public int Id { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }

    private class SpecialProduct : Product;

    private class ExtraSpecialProduct : SpecialProduct;

    private class Splot
    {
        public static readonly PropertyInfo SplowedProperty = typeof(Splot).GetProperty("Splowed");

        public int? Splowed { get; set; }
    }

    private class Splow : Splot;

    private class Splod : Splow;

    private class IndexedClass
    {
        public static readonly string IndexerPropertyName = "Indexer";

        public object this[string name]
        {
            get => null;
            set { }
        }
    }
}
