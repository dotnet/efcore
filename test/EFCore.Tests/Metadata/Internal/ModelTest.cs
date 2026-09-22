// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class ModelTest
{
    [ConditionalFact]
    public void Model_throws_when_readonly()
    {
        var model = CreateModel();

        model.FinalizeModel();

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => model.FinalizeModel()).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => model.DelayConventions()).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => model.AddAnnotation("foo", "bar")).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => model.RemoveOwned(typeof(SpecialCustomer))).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => model.AddOwned(typeof(Order))).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => model.AddShared(typeof(Order))).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => model.SetChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => ((Model)model).SkipDetectChanges = false).Message);
    }

    [ConditionalFact]
    public void Snapshot_change_tracking_is_used_by_default()
        => Assert.Equal(ChangeTrackingStrategy.Snapshot, CreateModel().GetChangeTrackingStrategy());

    [ConditionalFact]
    public void Change_tracking_strategy_can_be_changed()
    {
        var model = CreateModel();
        model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
        Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, model.GetChangeTrackingStrategy());

        model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
        Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, model.GetChangeTrackingStrategy());
    }

    [ConditionalFact]
    public void Can_add_and_remove_entity_by_type()
    {
        var model = CreateModel();
        Assert.Null(model.FindEntityType(typeof(Customer)));
        Assert.Null(model.RemoveEntityType(typeof(Customer)));

        var entityType = model.AddEntityType(typeof(Customer));

        Assert.Equal(typeof(Customer), entityType.ClrType);
        Assert.NotNull(model.FindEntityType(typeof(Customer)));
        Assert.Same(model, entityType.Model);
        Assert.True(((EntityType)entityType).IsInModel);

        Assert.Same(entityType, model.FindEntityType(typeof(Customer)));

        Assert.Equal([entityType], model.GetEntityTypes().ToArray());

        Assert.Same(entityType, model.RemoveEntityType(entityType.ClrType));

        Assert.Null(model.RemoveEntityType(entityType.ClrType));
        Assert.Null(model.FindEntityType(typeof(Customer)));
        Assert.False(((EntityType)entityType).IsInModel);
    }

    [ConditionalFact]
    public void Can_add_and_remove_entity_by_name()
    {
        var model = CreateModel();
        Assert.Null(model.FindEntityType(typeof(Customer).FullName));
        Assert.Null(model.RemoveEntityType(typeof(Customer).FullName));

        var entityType = model.AddEntityType(typeof(Customer).FullName);

        Assert.Equal(typeof(Dictionary<string, object>), entityType.ClrType);
        Assert.Equal(typeof(Customer).FullName, entityType.Name);
        Assert.NotNull(model.FindEntityType(typeof(Customer).FullName));
        Assert.Same(model, entityType.Model);
        Assert.True(((EntityType)entityType).IsInModel);

        Assert.Same(entityType, model.FindEntityType(typeof(Customer).FullName));

        Assert.Equal([entityType], model.GetEntityTypes().ToArray());

        Assert.Same(entityType, model.RemoveEntityType(entityType.Name));

        Assert.Null(model.RemoveEntityType(entityType.Name));
        Assert.Null(model.FindEntityType(typeof(Customer).FullName));
        Assert.False(((EntityType)entityType).IsInModel);
    }

    [ConditionalFact]
    public void Can_add_and_remove_shared_entity()
    {
        var model = CreateModel();
        var entityTypeName = "SharedCustomer1";
        Assert.Null(model.FindEntityType(typeof(Customer)));
        Assert.Null(model.FindEntityType(entityTypeName));

        var entityType = model.AddEntityType(entityTypeName, typeof(Customer));

        Assert.Equal(typeof(Customer), entityType.ClrType);
        Assert.Equal(entityTypeName, entityType.Name);
        Assert.NotNull(model.FindEntityType(entityTypeName));
        Assert.Same(model, entityType.Model);
        Assert.True(((EntityType)entityType).IsInModel);

        Assert.Same(entityType, model.FindEntityType(entityTypeName));
        Assert.Null(model.FindEntityType(typeof(Customer)));

        Assert.Equal([entityType], model.GetEntityTypes().ToArray());

        Assert.Same(entityType, model.RemoveEntityType(entityType.Name));

        Assert.Null(model.RemoveEntityType(entityType.Name));
        Assert.Null(model.FindEntityType(entityTypeName));
        Assert.False(((EntityType)entityType).IsInModel);
    }

    [ConditionalFact]
    public void Adding_a_shared_entity_with_same_name_throws()
    {
        var model = CreateModel();

        Assert.Equal(
            CoreStrings.AmbiguousSharedTypeEntityTypeName(typeof(Customer).DisplayName()),
            Assert.Throws<InvalidOperationException>(
                ()
                    => model.AddEntityType(typeof(Customer).DisplayName(), typeof(Customer))).Message);
    }

    [ConditionalFact]
    public void Cannot_remove_entity_type_when_referenced_by_foreign_key()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var idProperty = customerType.AddProperty(Customer.IdProperty);
        var customerKey = customerType.AddKey(idProperty);
        var orderType = model.AddEntityType(typeof(Order));
        var customerFk = orderType.AddProperty(Order.CustomerIdProperty);

        orderType.AddForeignKey(customerFk, customerKey, customerType);

        Assert.Equal(
            CoreStrings.EntityTypeInUseByReferencingForeignKey(
                typeof(Customer).Name,
                "{'" + Order.CustomerIdProperty.Name + "'}",
                typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(() => model.RemoveEntityType(customerType.Name)).Message);
    }

    [ConditionalFact]
    public void Cannot_remove_entity_type_when_it_has_derived_types()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var specialCustomerType = model.AddEntityType(typeof(SpecialCustomer));

        specialCustomerType.BaseType = customerType;

        Assert.Equal(
            CoreStrings.EntityTypeInUseByDerived(typeof(Customer).Name, typeof(SpecialCustomer).Name),
            Assert.Throws<InvalidOperationException>(() => model.RemoveEntityType(customerType.Name)).Message);
    }

    [ConditionalFact]
    public void Using_invalid_entity_type_throws()
    {
        var model = CreateModel();

        Assert.Equal(
            CoreStrings.InvalidEntityType(typeof(IReadOnlyList<int>)),
            Assert.Throws<ArgumentException>(() => model.AddEntityType(typeof(IReadOnlyList<int>))).Message);
    }

    [ConditionalFact]
    public void Adding_duplicate_entity_by_type_throws()
    {
        var model = CreateModel();
        Assert.Null(model.RemoveEntityType(typeof(Customer).FullName));

        model.AddEntityType(typeof(Customer));

        Assert.Equal(
            CoreStrings.DuplicateEntityType(nameof(Customer)),
            Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Customer))).Message);
    }

    [ConditionalFact]
    public void Adding_duplicate_entity_by_name_throws()
    {
        var model = CreateModel();
        Assert.Null(model.RemoveEntityType(typeof(Customer)));

        model.AddEntityType(typeof(Customer));

        Assert.Equal(
            CoreStrings.DuplicateEntityType(typeof(Customer).FullName + " (Dictionary<string, object>)"),
            Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(Customer).FullName)).Message);
    }

    [ConditionalFact]
    public void Adding_duplicate_shared_type_throws()
    {
        var model = (Model)CreateModel();
        Assert.Null(model.RemoveEntityType(typeof(Customer).FullName));

        model.AddEntityType(typeof(Customer), owned: false, ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.CannotMarkShared(nameof(Customer)),
            Assert.Throws<InvalidOperationException>(() => model.AddShared(typeof(Customer), ConfigurationSource.Explicit)).Message);
    }

    [ConditionalFact]
    public void Can_get_entity_by_type()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));

        Assert.Same(entityType, model.FindEntityType(typeof(Customer)));
        Assert.Same(entityType, model.FindEntityType(typeof(Customer)));
        Assert.Null(model.FindEntityType(typeof(string)));
        Assert.Null(model.FindEntityType(typeof(IList<>).GetGenericArguments().Single()));
    }

    [ConditionalFact]
    public void Can_get_entity_by_name()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer).FullName);

        Assert.Same(entityType, model.FindEntityType(typeof(Customer).FullName));
        Assert.Same(entityType, model.FindEntityType(typeof(Customer).FullName));
        Assert.Null(model.FindEntityType(typeof(string)));
    }

    [ConditionalFact]
    public void Entities_are_ordered_by_name()
    {
        var model = CreateModel();
        var entityType1 = model.AddEntityType(typeof(Order));
        var entityType2 = model.AddEntityType(typeof(Customer));

        Assert.True(new[] { entityType2, entityType1 }.SequenceEqual(model.GetEntityTypes()));
    }

    [ConditionalFact]
    public void Can_get_referencing_foreign_keys()
    {
        var model = CreateModel();
        var entityType1 = model.AddEntityType(typeof(Customer));
        var entityType2 = model.AddEntityType(typeof(Order));
        var keyProperty = entityType1.AddProperty("Id", typeof(int));
        var fkProperty = entityType2.AddProperty("CustomerId", typeof(int));
        var foreignKey = entityType2.AddForeignKey(fkProperty, entityType1.AddKey(keyProperty), entityType1);

        var referencingForeignKeys = entityType1.GetReferencingForeignKeys();

        Assert.Same(foreignKey, referencingForeignKeys.Single());
        Assert.Same(foreignKey, entityType1.GetReferencingForeignKeys().Single());
    }

    private static IMutableModel CreateModel()
        => new Model();

    private class Customer
    {
        public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty(nameof(Id));

        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Order> Orders { get; set; }
    }

    private class SpecialCustomer : Customer;

    private class Order
    {
        public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");

        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
