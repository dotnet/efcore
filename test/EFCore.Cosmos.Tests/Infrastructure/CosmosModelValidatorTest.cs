// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class CosmosModelValidatorTest : ModelValidatorTestBase
{
    [ConditionalFact]
    public virtual void Passes_on_valid_model()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_valid_keyless_entity_type()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().HasPartitionKey(c => c.PartitionId).HasNoKey();

        var model = Validate(modelBuilder);

        Assert.Empty(model.FindEntityType(typeof(Customer)).GetKeys());
    }

    [ConditionalFact]
    public virtual void Detects_missing_id_property()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        modelBuilder.Entity<Order>(
            b =>
            {
                b.Property(o => o.Id);
                b.HasKey(o => o.Id);
                b.Ignore(o => o.PartitionId);
                b.Ignore(o => o.Customer);
                b.Ignore(o => o.OrderDetails);
                b.Ignore(o => o.Products);
            });

        VerifyError(CosmosStrings.NoIdProperty(nameof(Order)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_non_key_id_property()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        modelBuilder.Entity<Order>(
            b =>
            {
                b.Property(o => o.Id);
                b.HasKey(o => o.Id);
                b.Property<string>("id");
                b.Ignore(o => o.PartitionId);
                b.Ignore(o => o.Customer);
                b.Ignore(o => o.OrderDetails);
                b.Ignore(o => o.Products);
            });

        VerifyError(CosmosStrings.NoIdKey(nameof(Order), "id"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_non_string_id_property()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        modelBuilder.Entity<Order>(
            b =>
            {
                b.Property(o => o.Id);
                b.HasKey(o => o.Id);
                b.Property<int>("id");
                b.HasKey("id");
                b.Ignore(o => o.PartitionId);
                b.Ignore(o => o.Customer);
                b.Ignore(o => o.OrderDetails);
                b.Ignore(o => o.Products);
            });

        VerifyError(CosmosStrings.IdNonStringStoreType("id", nameof(Order), "int"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_valid_partition_keys()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId)
            .HasAnalyticalStoreTimeToLive(-1)
            .HasDefaultTimeToLive(100)
            .HasAutoscaleThroughput(200);
        modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(o => o.PartitionId)
            .Property(o => o.PartitionId).HasConversion<string>();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_PK_partition_key()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Order>(
            b =>
            {
                b.HasKey(o => o.PartitionId);
                b.Ignore(o => o.Customer);
                b.Ignore(o => o.OrderDetails);
                b.Ignore(o => o.Products);
            });

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_non_key_partition_key_property()
    {
        var modelBuilder = CreateConventionlessModelBuilder();
        modelBuilder.Entity<Order>(
            b =>
            {
                b.Property(o => o.Id);
                b.Property<string>("id");
                b.HasKey("id");
                b.Property(o => o.PartitionId);
                b.HasPartitionKey(o => o.PartitionId);
                b.Ignore(o => o.Customer);
                b.Ignore(o => o.OrderDetails);
                b.Ignore(o => o.Products);
            });

        VerifyError(CosmosStrings.NoPartitionKeyKey(nameof(Order), nameof(Order.PartitionId), "id"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_partition_key_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>();
        modelBuilder.Entity<Order>().HasPartitionKey("PartitionKey");

        VerifyError(CosmosStrings.PartitionKeyMissingProperty(nameof(Order), "PartitionKey"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_partition_key_on_first_type()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders");
        modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId);

        VerifyError(CosmosStrings.NoPartitionKey(nameof(Customer), "", nameof(Order), "PartitionId", "Orders"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_partition_keys_one_last_type()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId);
        modelBuilder.Entity<Order>().ToContainer("Orders");

        VerifyError(CosmosStrings.NoPartitionKey(nameof(Customer), "PartitionId", nameof(Order), "", "Orders"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_partition_key_properties_composite_less_first()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => new { c.PartitionId, c.Id, c.Name });
        modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(c => new { c.PartitionId, c.Id });

        VerifyError(CosmosStrings.NoPartitionKey(nameof(Customer), "PartitionId,Id,Name", nameof(Order), "PartitionId,Id", "Orders"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_partition_key_properties_composite_less_last()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => new { c.PartitionId });
        modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(c => new { c.PartitionId, c.Id });

        VerifyError(CosmosStrings.NoPartitionKey(nameof(Customer), "PartitionId", nameof(Order), "PartitionId,Id", "Orders"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_partition_key_properties_composite_three()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => new { c.PartitionId, c.Id });
        modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(c => new { c.PartitionId, c.Id });
        modelBuilder.Entity<OrderProduct>(
            b =>
            {
                b.HasKey(e => e.OrderId);
                b.ToContainer("Orders").HasPartitionKey(c => new { c.OrderId });
            });

        VerifyError(CosmosStrings.NoPartitionKey(nameof(Customer), "PartitionId,Id", nameof(OrderProduct), "OrderId", "Orders"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_partition_keys_mapped_to_different_properties()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId)
            .Property(c => c.PartitionId).ToJsonProperty("pk");
        modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId);

        VerifyError(
            CosmosStrings.PartitionKeyStoreNameMismatch(
                nameof(Customer.PartitionId), nameof(Customer), "pk", nameof(Order.PartitionId), nameof(Order),
                nameof(Order.PartitionId)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_partition_key_of_different_type()
    {
        var modelBuilder = TestHelpers.CreateConventionBuilder(
            CreateModelLogger(),
            CreateValidationLogger(),
            configurationBuilder => configurationBuilder.RemoveAllConventions());

        modelBuilder.Entity<Customer>(
            b =>
            {
                b.ToContainer("Orders");
                b.Property(e => e.Id).HasConversion<string>().ToJsonProperty("id");
                b.Ignore(e => e.Name);
                b.Ignore(e => e.PartitionId);
                b.Ignore(e => e.Orders);
                b.Property<JObject>("foo");
                b.HasPartitionKey("foo");
                b.HasKey(e => e.Id);
            });

        VerifyError(CosmosStrings.PartitionKeyBadStoreType("foo", nameof(Customer), "JObject"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_conflicting_analytical_ttl()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders")
            .HasAnalyticalStoreTimeToLive(-1);
        modelBuilder.Entity<Order>().ToContainer("Orders")
            .HasAnalyticalStoreTimeToLive(60);

        VerifyError(CosmosStrings.AnalyticalTTLMismatch(-1, nameof(Customer), nameof(Order), 60, "Orders"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_conflicting_default_ttl()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders")
            .HasDefaultTimeToLive(100);
        modelBuilder.Entity<Order>().ToContainer("Orders")
            .HasDefaultTimeToLive(60);

        VerifyError(CosmosStrings.DefaultTTLMismatch(100, nameof(Customer), nameof(Order), 60, "Orders"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_conflicting_throughput()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders")
            .HasAutoscaleThroughput(200);
        modelBuilder.Entity<Order>().ToContainer("Orders")
            .HasAutoscaleThroughput(60);

        VerifyError(CosmosStrings.ThroughputMismatch(200, nameof(Customer), nameof(Order), 60, "Orders"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_conflicting_throughput_type()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders")
            .HasManualThroughput(200);
        modelBuilder.Entity<Order>().ToContainer("Orders")
            .HasAutoscaleThroughput(200);

        VerifyError(CosmosStrings.ThroughputTypeMismatch(nameof(Customer), nameof(Order), "Orders"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_properties_mapped_to_same_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>();
        modelBuilder.Entity<Order>(
            ob =>
            {
                ob.Property(o => o.Id).ToJsonProperty("Details");
                ob.Property(o => o.PartitionId).ToJsonProperty("Details");
            });

        VerifyError(
            CosmosStrings.JsonPropertyCollision(
                nameof(Order.PartitionId), nameof(Order.Id), nameof(Order), "Details"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_property_and_embedded_type_mapped_to_same_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>();
        modelBuilder.Entity<Order>(
            ob =>
            {
                ob.Property(o => o.PartitionId).ToJsonProperty("Details");
                ob.OwnsOne(o => o.OrderDetails).ToJsonProperty("Details");
            });

        VerifyError(
            CosmosStrings.JsonPropertyCollision(
                nameof(Order.OrderDetails), nameof(Order.PartitionId), nameof(Order), "Details"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_owned_type_mapped_to_a_container()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>();
        modelBuilder.Entity<Order>(
            ob =>
            {
                var ownedType = ob.OwnsOne(o => o.OrderDetails).OwnedEntityType;
                ownedType.SetContainer("Details");
            });

        VerifyError(
            CosmosStrings.OwnedTypeDifferentContainer(
                nameof(OrderDetails), nameof(Order), "Details"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_conflicting_containing_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders");
        modelBuilder.Entity<Order>().ToContainer("Orders").Metadata.SetContainingPropertyName("Prop");

        VerifyError(CosmosStrings.ContainerContainingPropertyConflict(nameof(Order), "Orders", "Prop"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_discriminator()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders").HasNoDiscriminator();
        modelBuilder.Entity<Order>().ToContainer("Orders");

        VerifyError(CosmosStrings.NoDiscriminatorProperty(nameof(Customer), "Orders"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_missing_discriminator_value()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders").HasDiscriminator().HasValue(null);
        modelBuilder.Entity<Order>().ToContainer("Orders");

        VerifyError(CoreStrings.NoDiscriminatorValue(nameof(Customer)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_discriminator_values()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>().ToContainer("Orders").HasDiscriminator().HasValue("type");
        modelBuilder.Entity<Order>().ToContainer("Orders").HasDiscriminator().HasValue("type");

        VerifyError(
            CosmosStrings.DuplicateDiscriminatorValue(nameof(Order), "type", nameof(Customer), "Orders"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_on_valid_concurrency_token()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>()
            .ToContainer("Orders")
            .Property<string>("_etag")
            .IsConcurrencyToken();
    }

    [ConditionalFact]
    public virtual void Detects_invalid_concurrency_token()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>()
            .ToContainer("Orders")
            .Property<string>("_not_etag")
            .IsConcurrencyToken();

        VerifyError(CosmosStrings.NonETagConcurrencyToken(nameof(Customer), "_not_etag"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_nonString_concurrency_token()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Customer>()
            .ToContainer("Orders")
            .Property<int>("_etag")
            .IsConcurrencyToken();

        VerifyError(CosmosStrings.ETagNonStringStoreType("_etag", nameof(Customer), "int"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_unmappable_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<RememberMyName<ReadOnlyMemory<float>>>().ToContainer("Orders");

        VerifyError(CoreStrings.PropertyNotAdded(
            typeof(RememberMyName<ReadOnlyMemory<float>>).ShortDisplayName(),
            nameof(RememberMyName<float>.ForgetMeNot),
            typeof(ReadOnlyMemory<float>).ShortDisplayName()), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_unmappable_list_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<RememberMyName<ReadOnlyMemory<float>[]>>().ToContainer("Orders");

        VerifyError(CoreStrings.PropertyNotAdded(
            typeof(RememberMyName<ReadOnlyMemory<float>[]>).ShortDisplayName(),
            nameof(RememberMyName<float>.ForgetMeNot),
            typeof(ReadOnlyMemory<float>[]).ShortDisplayName()), modelBuilder);
    }

    private class RememberMyName<T>
    {
        public string Id { get; set; }
        public T ForgetMeNot { get; set; }
    }

    protected override TestHelpers TestHelpers
        => CosmosTestHelpers.Instance;
}
