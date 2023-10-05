// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable ClassNeverInstantiated.Local

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class KeyPropagatorTest
{
    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Foreign_key_value_is_obtained_from_reference_to_principal(bool generateTemporary, bool async)
    {
        var model = BuildModel(generateTemporary);

        var principal = new Category { Id = 11 };
        var dependent = new Product { Id = 21, Category = principal };

        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
        var property = model.FindEntityType(typeof(Product)).FindProperty("CategoryId");
        var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

        if (async)
        {
            await keyPropagator.PropagateValueAsync(dependentEntry, property);
        }
        else
        {
            keyPropagator.PropagateValue(dependentEntry, property);
        }

        Assert.Equal(11, dependentEntry[property]);
        Assert.False(dependentEntry.HasTemporaryValue(property));
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Foreign_key_value_is_obtained_from_tracked_principal_with_populated_collection(bool generateTemporary, bool async)
    {
        var model = BuildModel(generateTemporary);
        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var principal = new Category { Id = 11 };
        var dependent = new Product { Id = 21 };
        principal.Products.Add(dependent);

        manager.GetOrCreateEntry(principal).SetEntityState(EntityState.Unchanged);
        var dependentEntry = manager.GetOrCreateEntry(dependent);
        var property = model.FindEntityType(typeof(Product))!.FindProperty("CategoryId")!;
        var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

        _ = async
            ? await keyPropagator.PropagateValueAsync(dependentEntry, property)
            : keyPropagator.PropagateValue(dependentEntry, property);

        Assert.Equal(11, dependentEntry[property]);
        Assert.False(dependentEntry.HasTemporaryValue(property));
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Non_identifying_foreign_key_value_is_not_generated_if_principal_key_not_set(bool generateTemporary, bool async)
    {
        var model = BuildModel(generateTemporary);

        var principal = new Category();
        var dependent = new Product { Id = 21, Category = principal };

        var contextServices = CreateContextServices(model);
        var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
        var property = model.FindEntityType(typeof(Product))!.FindProperty("CategoryId")!;
        var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

        _ = async
            ? await keyPropagator.PropagateValueAsync(dependentEntry, property)
            : keyPropagator.PropagateValue(dependentEntry, property);

        Assert.Equal(0, dependentEntry[property]);
        Assert.False(dependentEntry.HasTemporaryValue(property));
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task One_to_one_foreign_key_value_is_obtained_from_reference_to_principal(bool generateTemporary, bool async)
    {
        var model = BuildModel(generateTemporary);

        var principal = new Product { Id = 21 };
        var dependent = new ProductDetail { Product = principal };

        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
        var property = model.FindEntityType(typeof(ProductDetail))!.FindProperty("Id")!;
        var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

        _ = async
            ? await keyPropagator.PropagateValueAsync(dependentEntry, property)
            : keyPropagator.PropagateValue(dependentEntry, property);

        Assert.Equal(21, dependentEntry[property]);
        Assert.False(dependentEntry.HasTemporaryValue(property));
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task One_to_one_foreign_key_value_is_obtained_from_tracked_principal(bool generateTemporary, bool async)
    {
        var model = BuildModel(generateTemporary);
        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var dependent = new ProductDetail();
        var principal = new Product { Id = 21, Detail = dependent };

        manager.GetOrCreateEntry(principal).SetEntityState(EntityState.Unchanged);
        var dependentEntry = manager.GetOrCreateEntry(dependent);
        var property = model.FindEntityType(typeof(ProductDetail))!.FindProperty("Id")!;
        var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

        _ = async
            ? await keyPropagator.PropagateValueAsync(dependentEntry, property)
            : keyPropagator.PropagateValue(dependentEntry, property);

        Assert.Equal(21, dependentEntry[property]);
        Assert.False(dependentEntry.HasTemporaryValue(property));
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Identifying_foreign_key_value_is_generated_if_principal_key_not_set(bool generateTemporary, bool async)
    {
        var model = BuildModel(generateTemporary);

        var principal = new Product();
        var dependent = new ProductDetail { Product = principal };

        var contextServices = CreateContextServices(model);
        var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
        var property = model.FindEntityType(typeof(ProductDetail))!.FindProperty("Id")!;
        var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

        _ = async
            ? await keyPropagator.PropagateValueAsync(dependentEntry, property)
            : keyPropagator.PropagateValue(dependentEntry, property);

        Assert.NotEqual(0, dependentEntry[property]);
        Assert.True(dependentEntry.HasTemporaryValue(property));
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Identifying_foreign_key_value_is_propagated_if_principal_key_is_generated(bool generateTemporary, bool async)
    {
        var principal = new Product();
        var dependent = new ProductDetail { Product = principal };

        var contextServices = CreateContextServices(BuildModel(generateTemporary));
        var stateManager = contextServices.GetRequiredService<IStateManager>();
        var principalEntry = stateManager.GetOrCreateEntry(principal);
        principalEntry.SetEntityState(EntityState.Added);
        var dependentEntry = stateManager.GetOrCreateEntry(dependent);
        var runtimeModel = contextServices.GetRequiredService<IModel>();
        var principalProperty = runtimeModel.FindEntityType(typeof(Product))!.FindProperty(nameof(Product.Id))!;
        var dependentProperty = runtimeModel.FindEntityType(typeof(ProductDetail))!.FindProperty(nameof(ProductDetail.Id))!;
        var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

        _ = async
            ? await keyPropagator.PropagateValueAsync(dependentEntry, dependentProperty)
            : keyPropagator.PropagateValue(dependentEntry, dependentProperty);

        Assert.NotEqual(0, principalEntry[principalProperty]);
        Assert.Equal(generateTemporary, principalEntry.HasTemporaryValue(principalProperty));
        Assert.NotEqual(0, dependentEntry[dependentProperty]);
        Assert.Equal(generateTemporary, dependentEntry.HasTemporaryValue(dependentProperty));
        Assert.Equal(principalEntry[principalProperty], dependentEntry[dependentProperty]);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Composite_foreign_key_value_is_obtained_from_reference_to_principal(bool generateTemporary, bool async)
    {
        var model = BuildModel(generateTemporary);

        var principal = new OrderLine { OrderId = 11, ProductId = 21 };
        var dependent = new OrderLineDetail { OrderLine = principal };

        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var dependentEntry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(dependent);
        var property1 = model.FindEntityType(typeof(OrderLineDetail))!.FindProperty("OrderId")!;
        var property2 = model.FindEntityType(typeof(OrderLineDetail))!.FindProperty("ProductId")!;
        var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

        _ = async
            ? await keyPropagator.PropagateValueAsync(dependentEntry, property1)
            : keyPropagator.PropagateValue(dependentEntry, property1);

        _ = async
            ? await keyPropagator.PropagateValueAsync(dependentEntry, property2)
            : keyPropagator.PropagateValue(dependentEntry, property2);

        Assert.Equal(11, dependentEntry[property1]);
        Assert.False(dependentEntry.HasTemporaryValue(property1));
        Assert.Equal(21, dependentEntry[property2]);
        Assert.False(dependentEntry.HasTemporaryValue(property1));
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Composite_foreign_key_value_is_obtained_from_tracked_principal(bool generateTemporary, bool async)
    {
        var model = BuildModel(generateTemporary);
        var contextServices = CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var manager = contextServices.GetRequiredService<IStateManager>();

        var dependent = new OrderLineDetail();
        var principal = new OrderLine
        {
            OrderId = 11,
            ProductId = 21,
            Detail = dependent
        };

        manager.GetOrCreateEntry(principal).SetEntityState(EntityState.Unchanged);
        var dependentEntry = manager.GetOrCreateEntry(dependent);
        var property1 = model.FindEntityType(typeof(OrderLineDetail))!.FindProperty("OrderId")!;
        var property2 = model.FindEntityType(typeof(OrderLineDetail))!.FindProperty("ProductId")!;
        var keyPropagator = contextServices.GetRequiredService<IKeyPropagator>();

        _ = async
            ? await keyPropagator.PropagateValueAsync(dependentEntry, property1)
            : keyPropagator.PropagateValue(dependentEntry, property1);

        _ = async
            ? await keyPropagator.PropagateValueAsync(dependentEntry, property2)
            : keyPropagator.PropagateValue(dependentEntry, property2);

        Assert.Equal(11, dependentEntry[property1]);
        Assert.False(dependentEntry.HasTemporaryValue(property1));
        Assert.Equal(21, dependentEntry[property2]);
        Assert.False(dependentEntry.HasTemporaryValue(property1));
    }

    private static IServiceProvider CreateContextServices(IModel model)
        => InMemoryTestHelpers.Instance.CreateContextServices(model);

    private class BaseType
    {
        public int Id { get; set; }
    }

    private class Category : BaseType
    {
        public ICollection<Product> Products { get; } = new List<Product>();
    }

    private class Product : BaseType
    {
        public int CategoryId { get; set; }

        public Category Category { get; set; }

        public ProductDetail Detail { get; set; }

        public ICollection<OrderLine> OrderLines { get; } = new List<OrderLine>();
    }

    private class ProductDetail
    {
        public int Id { get; set; }
        public Product Product { get; set; }
    }

    private class Order : BaseType
    {
        public ICollection<OrderLine> OrderLines { get; } = new List<OrderLine>();
    }

    private class OrderLine
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }

        public virtual OrderLineDetail Detail { get; set; }
    }

    private class OrderLineDetail
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        public virtual OrderLine OrderLine { get; set; }
    }

    private static IModel BuildModel(bool generateTemporary = false)
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<BaseType>();

        builder.Entity<Product>(
            b =>
            {
                b.HasMany(e => e.OrderLines).WithOne(e => e.Product);
                b.HasOne(e => e.Detail).WithOne(e => e.Product).HasForeignKey<ProductDetail>(e => e.Id);
            });

        builder.Entity<Category>().HasMany(e => e.Products).WithOne(e => e.Category);

        builder.Entity<ProductDetail>().Property(p => p.Id);

        builder.Entity<Order>().HasMany(e => e.OrderLines).WithOne(e => e.Order);

        builder.Entity<OrderLineDetail>().HasKey(
            e => new { e.OrderId, e.ProductId });

        builder.Entity<OrderLine>(
            b =>
            {
                b.HasKey(
                    e => new { e.OrderId, e.ProductId });
                b.HasOne(e => e.Detail).WithOne(e => e.OrderLine).HasForeignKey<OrderLineDetail>(
                    e => new { e.OrderId, e.ProductId });
            });

        if (generateTemporary)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetDeclaredProperties())
                {
                    if (property.ValueGenerated == ValueGenerated.OnAdd)
                    {
                        property.SetValueGeneratorFactory(new TemporaryNumberValueGeneratorFactory().Create);
                    }
                }
            }
        }

        return builder.Model.FinalizeModel();
    }
}
