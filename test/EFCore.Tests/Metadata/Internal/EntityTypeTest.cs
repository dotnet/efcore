// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Globalization;
using System.Reflection.Emit;

// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable ImplicitlyCapturedClosure
namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public partial class EntityTypeTest
{
    [ConditionalFact]
    public void Throws_when_model_is_readonly()
    {
        var model = CreateModel();

        var entityTypeA = model.AddEntityType(typeof(A));
        ((EntityType)entityTypeA).Builder.HasDiscriminator(ConfigurationSource.Explicit);

        model.FinalizeModel();

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => model.AddEntityType(typeof(B))).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => model.RemoveEntityType(entityTypeA)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityTypeA.AddAnnotation("foo", "bar")).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityTypeA.AddServiceProperty(A.GProperty)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(
                () => entityTypeA.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityTypeA.SetDiscriminatorMappingComplete(true)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityTypeA.SetDiscriminatorProperty(null)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityTypeA.SetDiscriminatorValue(null)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityTypeA.SetInMemoryQuery(null)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityTypeA.SetNavigationAccessMode(PropertyAccessMode.Field)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityTypeA.SetPropertyAccessMode(PropertyAccessMode.Field)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityTypeA.AddIgnored("")).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityTypeA.RemoveIgnored("")).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityTypeA.AddData(new[] { new { } })).Message);
    }

    [ConditionalFact]
    public void Display_name_is_prettified_CLR_name()
    {
        Assert.Equal("EntityTypeTest", CreateModel().AddEntityType(typeof(EntityTypeTest)).DisplayName());
        Assert.Equal("Customer", CreateModel().AddEntityType(typeof(Customer)).DisplayName());
        Assert.Equal("List<Customer>", CreateModel().AddEntityType(typeof(List<Customer>)).DisplayName());
    }

    [ConditionalFact]
    public void Display_name_is_entity_type_name_when_no_CLR_type()
        => Assert.Equal(
            "Everything.Is+Awesome<When.We, re.Living<Our.Dream>> (Dictionary<string, object>)",
            CreateModel().AddEntityType("Everything.Is+Awesome<When.We, re.Living<Our.Dream>>").DisplayName());

    [ConditionalFact]
    public void Display_name_is_prettified_for_owned_shared_type()
        => Assert.Equal(
            "Is<Awesome, When>.We#re.Living#Our.Dream",
            CreateModel().AddEntityType("Everything.Is<Awesome, When>.We#re.Living#Our.Dream", typeof(Dictionary<string, object>))
                .DisplayName());

    [ConditionalFact]
    public void Display_name_is_entity_type_name_when_shared_entity_type()
        => Assert.Equal(
            "Everything.Is+PostTag (Dictionary<string, object>)",
            CreateModel().AddEntityType("Everything.Is+PostTag", typeof(Dictionary<string, object>)).DisplayName());

    [ConditionalFact]
    public void Name_is_prettified_CLR_full_name()
    {
        Assert.Equal(
            "Microsoft.EntityFrameworkCore.Metadata.Internal.EntityTypeTest", CreateModel().AddEntityType(typeof(EntityTypeTest)).Name);
        Assert.Equal(
            "Microsoft.EntityFrameworkCore.Metadata.Internal.EntityTypeTest+Customer",
            CreateModel().AddEntityType(typeof(Customer)).Name);
        Assert.Equal(
            "System.Collections.Generic.List<Microsoft.EntityFrameworkCore.Metadata.Internal.EntityTypeTest+Customer>",
            CreateModel().AddEntityType(typeof(List<Customer>)).Name);
    }

    [ConditionalFact]
    public void Can_get_proper_table_name_for_generic_entityType()
    {
        var entityType = CreateEmptyModel().AddEntityType(typeof(A<int>));

        Assert.Equal(
            "A<int>",
            entityType.DisplayName());
    }

    [ConditionalFact]
    public void Invalid_filter_expressions_throws()
    {
        var model = CreateModel();

        var entityTypeA = model.AddEntityType(typeof(A).Name);

        Expression<Func<B, bool>> badExpression1 = b => false;

        Assert.Equal(
            CoreStrings.BadFilterExpression(badExpression1, entityTypeA.DisplayName(), entityTypeA.ClrType),
            Assert.Throws<InvalidOperationException>(() => entityTypeA.SetQueryFilter(badExpression1)).Message);

        Expression<Func<A, string>> badExpression2 = a => "";

        Assert.Equal(
            CoreStrings.BadFilterExpression(badExpression2, entityTypeA.DisplayName(), entityTypeA.ClrType),
            Assert.Throws<InvalidOperationException>(() => entityTypeA.SetQueryFilter(badExpression2)).Message);
    }

    [ConditionalFact]
    public void Can_set_reset_and_clear_primary_key()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var idProperty = entityType.AddProperty(Customer.IdProperty);
        var nameProperty = entityType.AddProperty(Customer.NameProperty);
        nameProperty.IsNullable = false;

        var key1 = entityType.SetPrimaryKey(new[] { idProperty, nameProperty });

        Assert.NotNull(key1);
        Assert.Same(key1, entityType.FindPrimaryKey());

        Assert.Same(key1, entityType.FindPrimaryKey());
        Assert.Same(key1, entityType.GetKeys().Single());

        var key2 = entityType.SetPrimaryKey(idProperty);

        Assert.NotNull(key2);
        Assert.Same(key2, entityType.FindPrimaryKey());
        Assert.Same(key2, entityType.FindPrimaryKey());
        Assert.Equal(2, entityType.GetKeys().Count());

        Assert.Same(key1, entityType.FindKey(key1.Properties));
        Assert.Same(key2, entityType.FindKey(key2.Properties));

        Assert.Null(entityType.SetPrimaryKey((Property)null));

        Assert.Null(entityType.FindPrimaryKey());
        Assert.Equal(2, entityType.GetKeys().Count());

        Assert.Null(entityType.SetPrimaryKey([]));

        Assert.Null(entityType.FindPrimaryKey());
        Assert.Equal(2, entityType.GetKeys().Count());
    }

    [ConditionalFact]
    public void Setting_primary_key_throws_if_properties_from_different_type()
    {
        var model = CreateModel();
        var entityType1 = model.AddEntityType(typeof(Customer));
        var entityType2 = model.AddEntityType(typeof(Order));
        var idProperty = entityType2.AddProperty(Customer.IdProperty);

        Assert.Equal(
            CoreStrings.KeyPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(() => entityType1.SetPrimaryKey(idProperty)).Message);
    }

    [ConditionalFact]
    public void Can_get_set_reset_and_clear_primary_key()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var idProperty = entityType.AddProperty(Customer.IdProperty);
        var nameProperty = entityType.AddProperty(Customer.NameProperty);
        nameProperty.IsNullable = false;

        var key1 = entityType.SetPrimaryKey(new[] { idProperty, nameProperty });

        Assert.NotNull(key1);
        Assert.Same(key1, entityType.SetPrimaryKey(new[] { idProperty, nameProperty }));
        Assert.Same(key1, entityType.FindPrimaryKey());

        Assert.Same(key1, entityType.FindPrimaryKey());
        Assert.Same(key1, entityType.GetKeys().Single());

        var key2 = entityType.SetPrimaryKey(idProperty);

        Assert.NotNull(key2);
        Assert.NotEqual(key1, key2);
        Assert.Same(key2, entityType.SetPrimaryKey(idProperty));
        Assert.Same(key2, entityType.FindPrimaryKey());
        Assert.Same(key2, entityType.FindPrimaryKey());
        Assert.Equal(2, entityType.GetKeys().Count());
        Assert.Same(key1, entityType.FindKey(key1.Properties));
        Assert.Same(key2, entityType.FindKey(key2.Properties));

        Assert.Null(entityType.SetPrimaryKey((Property)null));

        Assert.Null(entityType.FindPrimaryKey());
        Assert.Equal(2, entityType.GetKeys().Count());

        Assert.Null(entityType.FindPrimaryKey());
        Assert.Equal(2, entityType.GetKeys().Count());
    }

    [ConditionalFact]
    public void Can_clear_the_primary_key_if_it_is_referenced_from_a_foreign_key()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var idProperty = entityType.AddProperty(Customer.IdProperty);
        var customerPk = entityType.SetPrimaryKey(idProperty);

        var orderType = model.AddEntityType(typeof(Order));
        var fk = orderType.AddForeignKey(orderType.AddProperty(Order.CustomerIdProperty), customerPk, entityType);

        entityType.SetPrimaryKey((Property)null);

        Assert.Single(entityType.GetKeys());
        Assert.Same(customerPk, entityType.FindKey(idProperty));
        Assert.Null(entityType.FindPrimaryKey());
        Assert.Same(customerPk, fk.PrincipalKey);
    }

    [ConditionalFact]
    public void Can_change_the_primary_key_if_it_is_referenced_from_a_foreign_key()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var idProperty = entityType.AddProperty(Customer.IdProperty);
        var customerPk = entityType.SetPrimaryKey(idProperty);

        var orderType = model.AddEntityType(typeof(Order));
        var fk = orderType.AddForeignKey(orderType.AddProperty(Order.CustomerIdProperty), customerPk, entityType);
        var nameProperty = entityType.AddProperty(Customer.NameProperty);
        nameProperty.IsNullable = false;
        entityType.SetPrimaryKey(nameProperty);

        Assert.Equal(2, entityType.GetKeys().Count());
        Assert.Same(customerPk, entityType.FindKey(idProperty));
        Assert.NotSame(customerPk, entityType.FindPrimaryKey());
        Assert.Same(customerPk, fk.PrincipalKey);
    }

    [ConditionalFact]
    public void Can_add_and_get_a_key()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var idProperty = entityType.AddProperty(Customer.IdProperty);
        var nameProperty = entityType.AddProperty(Customer.NameProperty);
        nameProperty.IsNullable = false;

        var key1 = entityType.AddKey(new[] { idProperty, nameProperty });

        Assert.NotNull(key1);
        Assert.Same(key1, entityType.FindKey(new[] { idProperty, nameProperty }));
        Assert.Same(key1, entityType.GetKeys().Single());

        var key2 = entityType.AddKey(idProperty);

        Assert.NotNull(key2);
        Assert.Same(key2, entityType.FindKey(idProperty));
        Assert.Equal(2, entityType.GetKeys().Count());
        Assert.Contains(key1, entityType.GetKeys());
        Assert.Contains(key2, entityType.GetKeys());
    }

    [ConditionalFact]
    public void Adding_a_key_throws_if_properties_from_different_type()
    {
        var model = CreateModel();
        var entityType1 = model.AddEntityType(typeof(Customer));
        var entityType2 = model.AddEntityType(typeof(Order));
        var idProperty = entityType2.AddProperty(Customer.IdProperty);

        Assert.Equal(
            CoreStrings.KeyPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(() => entityType1.AddKey(idProperty)).Message);
    }

    [ConditionalFact]
    public void Adding_a_key_throws_if_duplicated()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var idProperty = entityType.AddProperty(Customer.IdProperty);
        var nameProperty = entityType.AddProperty(Customer.NameProperty);
        nameProperty.IsNullable = false;
        entityType.AddKey(new[] { idProperty, nameProperty });

        Assert.Equal(
            CoreStrings.DuplicateKey(
                "{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Customer).Name,
                typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(() => entityType.AddKey(new[] { idProperty, nameProperty })).Message);
    }

    [ConditionalFact]
    public void Adding_a_key_throws_if_duplicated_properties()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var idProperty = entityType.AddProperty(Customer.IdProperty);

        Assert.Equal(
            CoreStrings.DuplicatePropertyInKey(
                "{'" + Customer.IdProperty.Name + "', '" + Customer.IdProperty.Name + "'}", Customer.IdProperty.Name),
            Assert.Throws<InvalidOperationException>(() => entityType.AddKey(new[] { idProperty, idProperty })).Message);
    }

    [ConditionalFact]
    public void Adding_a_key_throws_if_properties_were_removed()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var idProperty = entityType.AddProperty(Customer.IdProperty);
        entityType.RemoveProperty(idProperty.Name);

        Assert.Equal(
            CoreStrings.KeyPropertiesWrongEntity("{'" + Customer.IdProperty.Name + "'}", typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(() => entityType.AddKey(new[] { idProperty })).Message);
    }

    [ConditionalFact]
    public void Adding_a_key_throws_if_same_as_primary()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var idProperty = entityType.AddProperty(Customer.IdProperty);
        var nameProperty = entityType.AddProperty(Customer.NameProperty);
        nameProperty.IsNullable = false;
        entityType.SetPrimaryKey(new[] { idProperty, nameProperty });

        Assert.Equal(
            CoreStrings.DuplicateKey(
                "{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Customer).Name,
                typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(() => entityType.AddKey(new[] { idProperty, nameProperty })).Message);
    }

    [ConditionalFact]
    public void Can_remove_keys()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var idProperty = entityType.AddProperty(Customer.IdProperty);
        var nameProperty = entityType.AddProperty(Customer.NameProperty);
        nameProperty.IsNullable = false;

        Assert.Null(entityType.RemoveKey(new[] { idProperty }));
        Assert.False(idProperty.IsKey());
        Assert.Empty(idProperty.GetContainingKeys());

        var key1 = entityType.SetPrimaryKey(new[] { idProperty, nameProperty });
        var key2 = entityType.AddKey(idProperty);

        Assert.True(((Key)key1).IsInModel);
        Assert.True(((Key)key2).IsInModel);
        Assert.Equal(new[] { key2, key1 }, entityType.GetKeys());
        Assert.True(idProperty.IsKey());
        Assert.Equal(new[] { key2, key1 }, idProperty.GetContainingKeys());

        Assert.Same(key1, entityType.RemoveKey(key1.Properties));
        Assert.Null(entityType.RemoveKey(key1.Properties));

        Assert.Equal(new[] { key2 }, entityType.GetKeys());

        Assert.Same(key2, entityType.RemoveKey(new[] { idProperty }));

        Assert.False(((Key)key1).IsInModel);
        Assert.False(((Key)key2).IsInModel);
        Assert.Empty(entityType.GetKeys());
        Assert.False(idProperty.IsKey());
        Assert.Empty(idProperty.GetContainingKeys());
    }

    [ConditionalFact]
    public void Removing_a_key_from_wrong_type_throws()
    {
        var model = CreateModel();

        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));

        Assert.Equal(
            CoreStrings.KeyWrongType(
                "{'" + Customer.IdProperty.Name + "'}",
                nameof(Order),
                nameof(Customer)),
            Assert.Throws<InvalidOperationException>(() => orderType.RemoveKey(customerKey)).Message);

        Assert.Equal(
            CoreStrings.KeyWrongType(
                "{'" + Customer.IdProperty.Name + "'}",
                nameof(Order),
                nameof(Customer)),
            Assert.Throws<InvalidOperationException>(() => orderType.RemoveKey(customerKey.Properties)).Message);
    }

    [ConditionalFact]
    public void Removing_a_key_throws_if_it_referenced_from_a_foreign_key_in_the_model()
    {
        var model = CreateModel();

        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var customerFk = orderType.AddProperty(Order.CustomerIdProperty);
        orderType.AddForeignKey(customerFk, customerKey, customerType);

        Assert.Equal(
            CoreStrings.KeyInUse(
                "{'" + Customer.IdProperty.Name + "'}",
                nameof(Customer),
                "{'" + Order.CustomerIdProperty.Name + "'}",
                nameof(Order)),
            Assert.Throws<InvalidOperationException>(() => customerType.RemoveKey(customerKey.Properties)).Message);
    }

    [ConditionalFact]
    public void Keys_are_ordered_by_property_count_then_property_names()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var idProperty = customerType.AddProperty(Customer.IdProperty);
        var nameProperty = customerType.AddProperty(Customer.NameProperty);
        nameProperty.IsNullable = false;
        var otherNameProperty = customerType.AddProperty("OtherNameProperty", typeof(string));
        otherNameProperty.IsNullable = false;

        var k2 = customerType.AddKey(nameProperty);
        var k4 = customerType.AddKey(new[] { idProperty, otherNameProperty });
        var k3 = customerType.AddKey(new[] { idProperty, nameProperty });
        var k1 = customerType.AddKey(idProperty);

        Assert.True(new[] { k1, k2, k3, k4 }.SequenceEqual(customerType.GetKeys()));
    }

    [ConditionalFact]
    public void Store_computed_values_are_ignored_before_and_after_save_by_default()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var nameProperty = entityType.AddProperty(Customer.NameProperty);

        Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetBeforeSaveBehavior());
        Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetAfterSaveBehavior());

        nameProperty.ValueGenerated = ValueGenerated.OnAddOrUpdate;

        Assert.Equal(PropertySaveBehavior.Ignore, nameProperty.GetBeforeSaveBehavior());
        Assert.Equal(PropertySaveBehavior.Ignore, nameProperty.GetAfterSaveBehavior());

        nameProperty.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

        Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetBeforeSaveBehavior());
        Assert.Equal(PropertySaveBehavior.Ignore, nameProperty.GetAfterSaveBehavior());

        nameProperty.SetAfterSaveBehavior(PropertySaveBehavior.Save);

        Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetBeforeSaveBehavior());
        Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetAfterSaveBehavior());
    }

    [ConditionalFact]
    public void Store_computed_values_are_ignored_after_save_by_default()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var nameProperty = entityType.AddProperty(Customer.NameProperty);

        Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetBeforeSaveBehavior());
        Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetAfterSaveBehavior());

        nameProperty.ValueGenerated = ValueGenerated.OnUpdate;

        Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetBeforeSaveBehavior());
        Assert.Equal(PropertySaveBehavior.Ignore, nameProperty.GetAfterSaveBehavior());

        nameProperty.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);

        Assert.Equal(PropertySaveBehavior.Throw, nameProperty.GetBeforeSaveBehavior());
        Assert.Equal(PropertySaveBehavior.Ignore, nameProperty.GetAfterSaveBehavior());

        nameProperty.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

        Assert.Equal(PropertySaveBehavior.Throw, nameProperty.GetBeforeSaveBehavior());
        Assert.Equal(PropertySaveBehavior.Throw, nameProperty.GetAfterSaveBehavior());
    }

    [ConditionalFact]
    public void Key_properties_must_throw_after_save()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var nameProperty = entityType.AddProperty(Customer.NameProperty);
        nameProperty.IsNullable = false;
        entityType.AddKey(nameProperty);

        Assert.Equal(PropertySaveBehavior.Save, nameProperty.GetBeforeSaveBehavior());
        Assert.Equal(PropertySaveBehavior.Throw, nameProperty.GetAfterSaveBehavior());

        Assert.Equal(
            CoreStrings.KeyPropertyMustBeReadOnly(Customer.NameProperty.Name, typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(() => nameProperty.SetAfterSaveBehavior(PropertySaveBehavior.Save)).Message);
    }

    [ConditionalFact]
    public void Can_add_a_foreign_key()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var idProperty = customerType.AddProperty(Customer.IdProperty);
        var customerKey = customerType.AddKey(idProperty);
        var orderType = model.AddEntityType(typeof(Order));
        var customerFk1 = orderType.AddProperty(Order.CustomerIdProperty);
        var customerFk2 = orderType.AddProperty("IdAgain", typeof(int));

        var fk1 = orderType.AddForeignKey(customerFk1, customerKey, customerType);

        Assert.NotNull(fk1);
        Assert.Same(fk1, orderType.FindForeignKeys(customerFk1).Single());
        Assert.Same(fk1, orderType.FindForeignKey(customerFk1, customerKey, customerType));
        Assert.Same(fk1, orderType.GetForeignKeys().Single());

        var fk2 = orderType.AddForeignKey(customerFk2, customerKey, customerType);

        Assert.Same(fk2, orderType.FindForeignKeys(customerFk2).Single());
        Assert.Same(fk2, orderType.FindForeignKey(customerFk2, customerKey, customerType));
        Assert.Equal([fk1, fk2], orderType.GetForeignKeys().ToArray());
    }

    [ConditionalFact]
    public void Can_add_a_foreign_key_targeting_different_key()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey1 = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));
        var orderType = model.AddEntityType(typeof(Order));
        var customerFkProperty = orderType.AddProperty(Order.CustomerIdProperty);

        var fk1 = orderType.AddForeignKey(customerFkProperty, customerKey1, customerType);

        Assert.NotNull(fk1);
        Assert.Same(fk1, orderType.FindForeignKeys(customerFkProperty).Single());
        Assert.Same(fk1, orderType.FindForeignKey(customerFkProperty, customerKey1, customerType));
        Assert.Same(fk1, orderType.GetForeignKeys().Single());

        var altKeyProperty = customerType.AddProperty(nameof(Customer.AlternateId), typeof(int));
        var customerKey2 = customerType.AddKey(altKeyProperty);
        var fk2 = orderType.AddForeignKey(customerFkProperty, customerKey2, customerType);

        Assert.Equal(2, orderType.FindForeignKeys(customerFkProperty).Count());
        Assert.Same(fk2, orderType.FindForeignKey(customerFkProperty, customerKey2, customerType));
        Assert.Equal([fk2, fk1], orderType.GetForeignKeys().ToArray());
    }


    [ConditionalFact]
    public void Adding_a_foreign_key_throws_if_duplicate()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));
        var orderType = model.AddEntityType(typeof(Order));
        var customerFk1 = orderType.AddProperty(Order.CustomerIdProperty);
        orderType.AddForeignKey(customerFk1, customerKey, customerType);

        Assert.Equal(
            CoreStrings.DuplicateForeignKey(
                "{'" + Order.CustomerIdProperty.Name + "'}",
                typeof(Order).Name,
                typeof(Order).Name,
                "{'" + Customer.IdProperty.Name + "'}",
                typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(() => orderType.AddForeignKey(customerFk1, customerKey, customerType)).Message);
    }

    [ConditionalFact]
    public void Adding_a_foreign_key_throws_if_duplicated_properties()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(
            new[]
            {
                customerType.AddProperty(nameof(Customer.Id), typeof(int)),
                customerType.AddProperty(nameof(Customer.AlternateId), typeof(int))
            });
        var orderType = model.AddEntityType(typeof(Order));
        var customerFk1 = orderType.AddProperty(Order.CustomerIdProperty);

        Assert.Equal(
            CoreStrings.DuplicatePropertyInForeignKey(
                "{'" + Order.CustomerIdProperty.Name + "', '" + Order.CustomerIdProperty.Name + "'}",
                Order.CustomerIdProperty.Name),
            Assert.Throws<InvalidOperationException>(
                () => orderType.AddForeignKey(new[] { customerFk1, customerFk1 }, customerKey, customerType)).Message);
    }

    [ConditionalFact]
    public void Adding_a_foreign_key_throws_if_properties_from_different_type()
    {
        var model = CreateModel();
        var entityType1 = model.AddEntityType(typeof(Customer));
        var entityType2 = model.AddEntityType(typeof(Order));
        var idProperty = entityType2.AddProperty(Order.IdProperty);
        var fkProperty = entityType2.AddProperty(Order.CustomerIdProperty);

        Assert.Equal(
            CoreStrings.ForeignKeyPropertiesWrongEntity("{'" + Order.CustomerIdProperty.Name + "'}", typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(
                () => entityType1.AddForeignKey(new[] { fkProperty }, entityType2.AddKey(idProperty), entityType2)).Message);
    }

    [ConditionalFact]
    public void Adding_a_foreign_key_throws_if_properties_were_removed()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var idProperty = entityType.AddProperty(Customer.IdProperty);
        var key = entityType.AddKey(idProperty);
        var fkProperty = entityType.AddProperty("fk", typeof(int));
        entityType.RemoveProperty(fkProperty.Name);

        Assert.Equal(
            CoreStrings.ForeignKeyPropertiesWrongEntity("{'fk'}", typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(() => entityType.AddForeignKey(new[] { fkProperty }, key, entityType)).Message);
    }

    [ConditionalFact]
    public void Adding_a_foreign_key_throws_if_key_was_removed()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var idProperty = entityType.AddProperty(Customer.IdProperty);
        var key = entityType.AddKey(idProperty);
        entityType.RemoveKey(key.Properties);
        var fkProperty = entityType.AddProperty("fk", typeof(int));

        Assert.Equal(
            CoreStrings.ForeignKeyReferencedEntityKeyMismatch("{'" + Customer.IdProperty.Name + "'}", nameof(Customer)),
            Assert.Throws<InvalidOperationException>(() => entityType.AddForeignKey(new[] { fkProperty }, key, entityType)).Message);
    }

    [ConditionalFact]
    public void Adding_a_foreign_key_throws_if_related_entity_is_from_different_model()
    {
        var dependentEntityType = CreateModel().AddEntityType(typeof(Customer));
        var fkProperty = dependentEntityType.AddProperty(Customer.IdProperty);
        var principalEntityType = CreateModel().AddEntityType(typeof(Order));
        var idProperty = principalEntityType.AddProperty(Order.IdProperty);

        Assert.Equal(
            CoreStrings.EntityTypeModelMismatch(nameof(Customer), nameof(Order)),
            Assert.Throws<InvalidOperationException>(
                () => dependentEntityType.AddForeignKey(
                    new[] { fkProperty }, principalEntityType.AddKey(idProperty), principalEntityType)).Message);
    }

    [ConditionalFact]
    public void Can_get_or_add_a_foreign_key()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var idProperty = customerType.AddProperty(Customer.IdProperty);
        var customerKey = customerType.AddKey(idProperty);
        var orderType = model.AddEntityType(typeof(Order));
        var customerFk1 = orderType.AddProperty(Order.CustomerIdProperty);
        var customerFk2 = orderType.AddProperty("IdAgain", typeof(int));
        var fk1 = orderType.AddForeignKey(customerFk1, customerKey, customerType);

        var fk2 = orderType.AddForeignKey(customerFk2, customerKey, customerType);

        Assert.NotNull(fk2);
        Assert.NotEqual(fk1, fk2);
        Assert.Same(fk2, orderType.FindForeignKeys(customerFk2).Single());
        Assert.Same(fk2, orderType.FindForeignKey(customerFk2, customerKey, customerType));
        Assert.Equal([fk1, fk2], orderType.GetForeignKeys().ToArray());
    }

    private static IMutableModel BuildModel()
    {
        var model = CreateModel();

        var principalType = model.AddEntityType(typeof(PrincipalEntity));
        var property1 = principalType.AddProperty("PeeKay", typeof(int));
        principalType.SetPrimaryKey(property1);

        var dependentType = model.AddEntityType(typeof(DependentEntity));
        var property = dependentType.AddProperty("KayPee", typeof(int));
        dependentType.SetPrimaryKey(property);

        return model;
    }

    [ConditionalFact]
    public void Can_remove_foreign_keys()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));
        var orderType = model.AddEntityType(typeof(Order));
        var customerFk1 = orderType.AddProperty(Order.CustomerIdProperty);
        var customerFk2 = orderType.AddProperty("IdAgain", typeof(int));

        Assert.Null(orderType.RemoveForeignKey(new[] { customerFk2 }, customerKey, customerType));
        Assert.False(customerFk1.IsForeignKey());
        Assert.Empty(customerFk1.GetContainingForeignKeys());

        var fk1 = orderType.AddForeignKey(customerFk1, customerKey, customerType);
        var fk2 = orderType.AddForeignKey(customerFk2, customerKey, customerType);

        Assert.NotNull(((ForeignKey)fk1).Builder);
        Assert.NotNull(((ForeignKey)fk2).Builder);
        Assert.Equal([fk1, fk2], orderType.GetForeignKeys().ToArray());
        Assert.True(customerFk1.IsForeignKey());
        Assert.Same(fk1, customerFk1.GetContainingForeignKeys().Single());

        Assert.Same(fk1, orderType.RemoveForeignKey(fk1.Properties, fk1.PrincipalKey, fk1.PrincipalEntityType));
        Assert.Null(orderType.RemoveForeignKey(fk1.Properties, fk1.PrincipalKey, fk1.PrincipalEntityType));

        Assert.Equal([fk2], orderType.GetForeignKeys().ToArray());
        Assert.False(customerFk1.IsForeignKey());
        Assert.Empty(customerFk1.GetContainingForeignKeys());

        Assert.Same(fk2, orderType.RemoveForeignKey(new[] { customerFk2 }, customerKey, customerType));

        Assert.False(((ForeignKey)fk1).IsInModel);
        Assert.False(((ForeignKey)fk2).IsInModel);
        Assert.Empty(orderType.GetForeignKeys());
    }

    [ConditionalFact]
    public void Can_remove_a_foreign_key_if_it_is_referenced_from_a_navigation_in_the_model()
    {
        var model = CreateModel();

        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var customerFk = orderType.AddProperty(Order.CustomerIdProperty);
        var fk = orderType.AddForeignKey(customerFk, customerKey, customerType);

        fk.SetDependentToPrincipal(Order.CustomerProperty);
        fk.SetPrincipalToDependent(Customer.OrdersProperty);

        Assert.NotNull(orderType.RemoveForeignKey(fk.Properties, fk.PrincipalKey, fk.PrincipalEntityType));
        Assert.Empty(orderType.GetNavigations());
        Assert.Empty(customerType.GetNavigations());
    }

    [ConditionalFact]
    public void Removing_a_foreign_key_throws_if_referenced_from_skip_navigation()
    {
        var model = CreateModel();
        var firstEntity = model.AddEntityType(typeof(Order));
        var firstId = firstEntity.AddProperty(Order.IdProperty);
        var firstKey = firstEntity.AddKey(firstId);
        var secondEntity = model.AddEntityType(typeof(Product));
        var joinEntity = model.AddEntityType(typeof(OrderProduct));
        var orderIdProperty = joinEntity.AddProperty(OrderProduct.OrderIdProperty);
        var foreignKey = joinEntity
            .AddForeignKey(new[] { orderIdProperty }, firstKey, firstEntity);

        var navigation = firstEntity.AddSkipNavigation(
            nameof(Order.Products), null, null, secondEntity, true, false);
        navigation.SetForeignKey(foreignKey);

        Assert.Equal(
            CoreStrings.ForeignKeyInUseSkipNavigation(
                "{'" + nameof(OrderProduct.OrderId) + "'}", nameof(OrderProduct), nameof(Order.Products), nameof(Order)),
            Assert.Throws<InvalidOperationException>(() => joinEntity.RemoveForeignKey(foreignKey)).Message);
    }

    [ConditionalFact]
    public void Foreign_keys_are_ordered_by_property_count_then_property_names()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var idProperty = customerType.AddProperty(Customer.IdProperty);
        var nameProperty = customerType.AddProperty(Customer.NameProperty);
        nameProperty.IsNullable = false;
        var customerKey = customerType.AddKey(idProperty);
        var otherCustomerKey = customerType.AddKey(new[] { idProperty, nameProperty });

        var orderType = model.AddEntityType(typeof(Order));
        var customerFk1 = orderType.AddProperty(Order.CustomerIdProperty);
        var customerFk2 = orderType.AddProperty("IdAgain", typeof(int));
        var customerFk3A = orderType.AddProperty("OtherId1", typeof(int));
        var customerFk3B = orderType.AddProperty("OtherId2", typeof(string));
        var customerFk4B = orderType.AddProperty("OtherId3", typeof(string));

        var fk2 = orderType.AddForeignKey(customerFk2, customerKey, customerType);
        var fk4 = orderType.AddForeignKey(new[] { customerFk3A, customerFk4B }, otherCustomerKey, customerType);
        var fk3 = orderType.AddForeignKey(new[] { customerFk3A, customerFk3B }, otherCustomerKey, customerType);
        var fk1 = orderType.AddForeignKey(customerFk1, customerKey, customerType);

        Assert.True(new[] { fk1, fk2, fk3, fk4 }.SequenceEqual(orderType.GetForeignKeys()));
    }

    [ConditionalFact]
    public void Can_get_referencing_foreign_keys()
    {
        var entityType = CreateEmptyModel().AddEntityType("Customer");
        var idProperty = entityType.AddProperty("id", typeof(int));
        var fkProperty = entityType.AddProperty("fk", typeof(int));
        var fk = entityType.AddForeignKey(fkProperty, entityType.SetPrimaryKey(idProperty), entityType);

        Assert.Same(fk, entityType.GetReferencingForeignKeys().Single());
    }

    [ConditionalFact]
    public void Can_add_and_remove_navigations()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

        var customerNavigation = customerForeignKey.SetDependentToPrincipal(Order.CustomerProperty);
        var ordersNavigation = customerForeignKey.SetPrincipalToDependent(Customer.OrdersProperty);

        Assert.Equal(nameof(Order.Customer), customerNavigation.Name);
        Assert.Same(orderType, customerNavigation.DeclaringEntityType);
        Assert.Same(customerForeignKey, customerNavigation.ForeignKey);
        Assert.True(customerNavigation.IsOnDependent);
        Assert.False(customerNavigation.IsCollection);
        Assert.Same(customerType, customerNavigation.TargetEntityType);
        Assert.Same(customerNavigation, customerForeignKey.DependentToPrincipal);

        Assert.Equal(nameof(Customer.Orders), ordersNavigation.Name);
        Assert.Same(customerType, ordersNavigation.DeclaringEntityType);
        Assert.Same(customerForeignKey, ordersNavigation.ForeignKey);
        Assert.False(ordersNavigation.IsOnDependent);
        Assert.True(ordersNavigation.IsCollection);
        Assert.Same(orderType, ordersNavigation.TargetEntityType);
        Assert.Same(ordersNavigation, customerForeignKey.PrincipalToDependent);

        Assert.Same(customerNavigation, orderType.GetNavigations().Single());
        Assert.Same(ordersNavigation, customerType.GetNavigations().Single());

        Assert.Same(customerNavigation, customerForeignKey.SetDependentToPrincipal((string)null));
        Assert.Null(customerForeignKey.SetDependentToPrincipal((string)null));
        Assert.Empty(orderType.GetNavigations());
        Assert.Empty(((IReadOnlyEntityType)orderType).GetNavigations());

        Assert.Same(ordersNavigation, customerForeignKey.SetPrincipalToDependent((string)null));
        Assert.Null(customerForeignKey.SetPrincipalToDependent((string)null));
        Assert.Empty(customerType.GetNavigations());
        Assert.Empty(((IReadOnlyEntityType)customerType).GetNavigations());
    }

    [ConditionalFact]
    public void Can_add_new_navigations_or_get_existing_navigations()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);
        var customerNavigation = customerForeignKey.SetDependentToPrincipal(Order.CustomerProperty);

        Assert.Equal(nameof(Order.Customer), customerNavigation.Name);
        Assert.Same(orderType, customerNavigation.DeclaringEntityType);
        Assert.Same(customerForeignKey, customerNavigation.ForeignKey);
        Assert.True(customerNavigation.IsOnDependent);
        Assert.False(customerNavigation.IsCollection);
        Assert.Same(customerType, customerNavigation.TargetEntityType);

        Assert.Same(customerNavigation, orderType.FindNavigation(nameof(Order.Customer)));
        Assert.True(customerNavigation.IsOnDependent);
    }

    [ConditionalFact]
    public void Can_get_navigation_and_can_try_get_navigation()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);
        var customerNavigation = customerForeignKey.SetDependentToPrincipal(Order.CustomerProperty);

        Assert.Same(customerNavigation, orderType.FindNavigation(nameof(Order.Customer)));
        Assert.Same(customerNavigation, orderType.FindNavigation(nameof(Order.Customer)));

        Assert.Null(orderType.FindNavigation("Nose"));
    }

    [ConditionalFact]
    public void Adding_a_new_navigation_with_a_name_that_conflicts_with_a_property_throws()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

        orderType.AddProperty("Customer");

        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation("Customer", typeof(Order).Name, typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(
                () => customerForeignKey.SetDependentToPrincipal("Customer")).Message);
    }

    [ConditionalFact]
    public void Adding_a_new_navigation_with_a_name_that_conflicts_with_a_service_property_throws()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

        orderType.AddServiceProperty(Order.CustomerProperty);

        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation(nameof(Order.Customer), nameof(Order), nameof(Order)),
            Assert.Throws<InvalidOperationException>(
                () => customerForeignKey.SetDependentToPrincipal(nameof(Order.Customer))).Message);
    }

    [ConditionalFact]
    public void Can_add_a_navigation_to_shadow_entity()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty("Id", typeof(int)));

        var orderType = model.AddEntityType("Order");
        var foreignKeyProperty = orderType.AddProperty("CustomerId", typeof(int));
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

        Assert.NotNull(customerForeignKey.SetDependentToPrincipal("Customer"));
    }

    [ConditionalFact]
    public void Adding_a_navigation_on_non_shadow_entity_type_pointing_to_a_shadow_entity_type_throws()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType("Customer");
        var customerKey = customerType.AddKey(customerType.AddProperty("Id", typeof(int)));

        var orderType = model.AddEntityType(typeof(Order));
        var foreignKeyProperty = orderType.AddProperty("CustomerId", typeof(int));
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

        Assert.Equal(
            CoreStrings.NavigationSingleWrongClrType(
                nameof(Order.Customer), typeof(Order).Name, "Customer", "Dictionary<string, object>"),
            Assert.Throws<InvalidOperationException>(
                () => customerForeignKey.SetDependentToPrincipal(Order.CustomerProperty)).Message);
    }

    [ConditionalFact]
    public void Collection_navigation_properties_must_be_IEnumerables_of_the_target_type()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

        Assert.Equal(
            CoreStrings.NavigationCollectionWrongClrType(
                nameof(Customer.NotCollectionOrders), typeof(Customer).Name, typeof(Order).Name, typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(
                () => customerForeignKey.SetPrincipalToDependent(Customer.NotCollectionOrdersProperty)).Message);
    }

    [ConditionalFact]
    public void Collection_navigation_properties_cannot_be_IEnumerables_of_derived_target_type()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(SpecialCustomer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

        Assert.Equal(
            CoreStrings.NavigationCollectionWrongClrType(
                nameof(SpecialCustomer.DerivedOrders),
                typeof(SpecialCustomer).Name,
                typeof(IEnumerable<SpecialOrder>).ShortDisplayName(),
                typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(
                () => customerForeignKey.SetPrincipalToDependent(SpecialCustomer.DerivedOrdersProperty)).Message);
    }

    [ConditionalFact]
    public void Collection_navigation_properties_can_be_IEnumerables_of_base_target_type()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(SpecialCustomer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(SpecialOrder));
        var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

        var ordersNavigation = customerForeignKey.SetPrincipalToDependent(Customer.OrdersProperty);

        Assert.Equal(nameof(Customer.Orders), ordersNavigation.Name);
        Assert.Same(customerType, ordersNavigation.DeclaringEntityType);
        Assert.Same(customerForeignKey, ordersNavigation.ForeignKey);
        Assert.False(ordersNavigation.IsOnDependent);
        Assert.True(ordersNavigation.IsCollection);
        Assert.Same(orderType, ordersNavigation.TargetEntityType);
        Assert.Same(ordersNavigation, customerForeignKey.PrincipalToDependent);
    }

    [ConditionalFact]
    public void Reference_navigation_properties_must_be_of_the_target_type()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

        Assert.Equal(
            CoreStrings.NavigationSingleWrongClrType(
                nameof(Order.RelatedOrder), typeof(Order).Name, typeof(Order).Name, typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(
                () => customerForeignKey.SetDependentToPrincipal(Order.RelatedOrderProperty)).Message);
    }

    [ConditionalFact]
    public void Reference_navigation_properties_cannot_be_of_derived_type()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(SpecialOrder));
        var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

        Assert.Equal(
            CoreStrings.NavigationSingleWrongClrType(
                nameof(SpecialOrder.DerivedCustomer), typeof(SpecialOrder).Name, typeof(SpecialCustomer).Name, typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(
                () => customerForeignKey.SetDependentToPrincipal(SpecialOrder.DerivedCustomerProperty)).Message);
    }

    [ConditionalFact]
    public void Reference_navigation_properties_can_be_of_base_type()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(SpecialCustomer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(SpecialOrder));
        var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

        var customerNavigation = customerForeignKey.SetDependentToPrincipal(Order.CustomerProperty);

        Assert.Equal("Customer", customerNavigation.Name);
        Assert.Same(orderType, customerNavigation.DeclaringEntityType);
        Assert.Same(customerForeignKey, customerNavigation.ForeignKey);
        Assert.True(customerNavigation.IsOnDependent);
        Assert.False(customerNavigation.IsCollection);
        Assert.Same(customerType, customerNavigation.TargetEntityType);
    }

    [ConditionalFact]
    public void Can_create_self_referencing_navigations()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(SelfRef));
        var fkProperty = entityType.AddProperty(SelfRef.ForeignKeyProperty);
        var principalKeyProperty = entityType.AddProperty(SelfRef.IdProperty);
        var referencedKey = entityType.SetPrimaryKey(principalKeyProperty);
        var fk = entityType.AddForeignKey(fkProperty, referencedKey, entityType);
        fk.IsUnique = true;

        var navigationToDependent = fk.SetPrincipalToDependent(SelfRef.SelfRef1Property);
        var navigationToPrincipal = fk.SetDependentToPrincipal(SelfRef.SelfRef2Property);

        Assert.Same(fk.PrincipalToDependent, navigationToDependent);
        Assert.Same(fk.DependentToPrincipal, navigationToPrincipal);
    }

    [ConditionalFact]
    public void Throws_when_adding_same_self_referencing_navigation_twice()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(SelfRef));
        var fkProperty = entityType.AddProperty(SelfRef.ForeignKeyProperty);
        var principalKeyProperty = entityType.AddProperty(SelfRef.IdProperty);
        var referencedKey = entityType.SetPrimaryKey(principalKeyProperty);
        var fk = entityType.AddForeignKey(fkProperty, referencedKey, entityType);
        fk.IsUnique = true;

        fk.SetPrincipalToDependent(SelfRef.SelfRef1Property);
        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation(nameof(SelfRef.SelfRef1), typeof(SelfRef).Name, typeof(SelfRef).Name),
            Assert.Throws<InvalidOperationException>(() => fk.SetDependentToPrincipal(SelfRef.SelfRef1Property)).Message);
    }

    [ConditionalFact]
    public void Navigations_are_ordered_by_name()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(SpecialCustomer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var customerForeignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(customerForeignKeyProperty, customerKey, customerType);

        var specialOrderType = model.AddEntityType(typeof(SpecialOrder));
        var specialCustomerForeignKeyProperty = specialOrderType.AddProperty(Order.CustomerIdProperty);
        var specialCustomerForeignKey = specialOrderType.AddForeignKey(specialCustomerForeignKeyProperty, customerKey, customerType);

        var navigation2 = customerForeignKey.SetPrincipalToDependent(Customer.OrdersProperty);
        var navigation1 = specialCustomerForeignKey.SetPrincipalToDependent(SpecialCustomer.DerivedOrdersProperty);

        Assert.True(new[] { navigation1, navigation2 }.SequenceEqual(customerType.GetNavigations()));
        Assert.True(new[] { navigation1, navigation2 }.SequenceEqual(((IReadOnlyEntityType)customerType).GetNavigations()));
    }

    [ConditionalFact]
    public void Can_get_one_to_many_inverses()
    {
        var model = BuildProductModel();

        var category = model.FindEntityType(typeof(Product)).GetNavigations().Single(e => e.Name == "Category");
        var products = model.FindEntityType(typeof(Category)).GetNavigations().Single(e => e.Name == "Products");

        Assert.Same(category, products.Inverse);
        Assert.Same(products, category.Inverse);
    }

    [ConditionalFact]
    public void Can_get_one_to_one_inverses()
    {
        var model = BuildProductModel();

        var category = model.FindEntityType(typeof(Product)).GetNavigations().Single(e => e.Name == "FeaturedProductCategory");
        var product = model.FindEntityType(typeof(Category)).GetNavigations().Single(e => e.Name == "FeaturedProduct");

        Assert.Same(category, product.Inverse);
        Assert.Same(product, category.Inverse);
    }

    [ConditionalFact]
    public void Can_get_target_ends()
    {
        var model = BuildProductModel();

        var productType = model.FindEntityType(typeof(Product));
        var categoryType = model.FindEntityType(typeof(Category));

        var category = productType.GetNavigations().Single(e => e.Name == "Category");
        var products = categoryType.GetNavigations().Single(e => e.Name == "Products");

        Assert.Same(productType, products.TargetEntityType);
        Assert.Same(categoryType, category.TargetEntityType);
    }

    [ConditionalFact]
    public void Returns_null_when_no_inverse()
    {
        var products = BuildProductModel(createCategory: false).FindEntityType(typeof(Category)).GetNavigations()
            .Single(e => e.Name == "Products");

        Assert.Null(products.Inverse);

        var category = BuildProductModel(createProducts: false).FindEntityType(typeof(Product)).GetNavigations()
            .Single(e => e.Name == "Category");

        Assert.Null(category.Inverse);

        var featuredCategory = BuildProductModel(createFeaturedProduct: false).FindEntityType(typeof(Product)).GetNavigations()
            .Single(e => e.Name == "FeaturedProductCategory");

        Assert.Null(featuredCategory.Inverse);

        var featuredProduct = BuildProductModel(createFeaturedProductCategory: false).FindEntityType(typeof(Category)).GetNavigations()
            .Single(e => e.Name == "FeaturedProduct");

        Assert.Null(featuredProduct.Inverse);
    }

    private static IReadOnlyModel BuildProductModel(
        bool createProducts = true,
        bool createCategory = true,
        bool createFeaturedProductCategory = true,
        bool createFeaturedProduct = true)
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        var model = builder.Model;

        builder.Entity<Product>(
            e =>
            {
                e.Ignore(p => p.Category);
                e.Ignore(p => p.FeaturedProductCategory);
            });
        builder.Entity<Category>(
            e =>
            {
                e.Ignore(c => c.Products);
                e.Ignore(c => c.FeaturedProduct);
            });

        var categoryType = model.FindEntityType(typeof(Category));
        var productType = model.FindEntityType(typeof(Product));

        var categoryFk = productType.AddForeignKey(
            productType.FindProperty("CategoryId"), categoryType.FindPrimaryKey(), categoryType);
        var featuredProductFk = categoryType.AddForeignKey(
            categoryType.FindProperty("FeaturedProductId"), productType.FindPrimaryKey(), productType);
        featuredProductFk.IsUnique = true;

        if (createProducts)
        {
            categoryFk.SetPrincipalToDependent(Category.ProductsProperty);
        }

        if (createCategory)
        {
            categoryFk.SetDependentToPrincipal(Product.CategoryProperty);
        }

        if (createFeaturedProductCategory)
        {
            featuredProductFk.SetPrincipalToDependent(Product.FeaturedProductCategoryProperty);
        }

        if (createFeaturedProduct)
        {
            featuredProductFk.SetDependentToPrincipal(Category.FeaturedProductProperty);
        }

        return model;
    }

    [ConditionalFact]
    public void Can_add_and_remove_skip_navigation()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);

        var customerEntity = model.AddEntityType(typeof(Customer));
        var customerIdProperty = customerEntity.AddProperty(Order.IdProperty);
        var customerKey = customerEntity.AddKey(customerIdProperty);
        var customerFkProperty = orderEntity.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderEntity
            .AddForeignKey(customerFkProperty, customerKey, customerEntity);
        var relatedNavigation = orderEntity.AddSkipNavigation(
            nameof(Order.RelatedOrder), null, null, orderEntity, false, true);
        relatedNavigation.SetForeignKey(customerForeignKey);

        Assert.True(relatedNavigation.IsOnDependent);

        var productEntity = model.AddEntityType(typeof(Product));
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(OrderProduct.OrderIdProperty);
        var orderProductForeignKey = orderProductEntity
            .AddForeignKey(new[] { orderProductFkProperty }, orderKey, orderEntity);

        var productsNavigation = orderEntity.AddSkipNavigation(
            nameof(Order.Products), null, null, productEntity, true, false);
        productsNavigation.SetForeignKey(orderProductForeignKey);

        Assert.Equal(new[] { productsNavigation, relatedNavigation }, orderEntity.GetSkipNavigations());
        Assert.Empty(customerEntity.GetSkipNavigations());

        Assert.Equal(new[] { relatedNavigation }, customerForeignKey.GetReferencingSkipNavigations());
        Assert.Equal(new[] { productsNavigation }, orderProductForeignKey.GetReferencingSkipNavigations());

        Assert.Equal(
            CoreStrings.SkipNavigationWrongType(nameof(Order.Products), nameof(Customer), nameof(Order)),
            Assert.Throws<InvalidOperationException>(() => customerEntity.RemoveSkipNavigation(productsNavigation)).Message);

        Assert.Equal(
            CoreStrings.EntityTypeInUseByReferencingSkipNavigation(
                nameof(Product), nameof(Order.Products), nameof(Order)),
            Assert.Throws<InvalidOperationException>(() => model.RemoveEntityType(productEntity)).Message);

        orderEntity.RemoveSkipNavigation(productsNavigation);
        orderEntity.RemoveSkipNavigation(relatedNavigation);
        Assert.Empty(orderEntity.GetSkipNavigations());
    }

    [ConditionalFact]
    public void Adding_skip_navigation_with_a_name_that_conflicts_with_another_skip_navigation_throws()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(typeof(Product));
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(OrderProduct.OrderIdProperty);
        var orderProductForeignKey = orderProductEntity
            .AddForeignKey(new[] { orderProductFkProperty }, orderKey, orderEntity);

        var navigation = orderEntity.AddSkipNavigation(
            nameof(Order.Products), null, null, productEntity, true, false);
        navigation.SetForeignKey(orderProductForeignKey);

        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation(nameof(Order.Products), typeof(Order).Name, typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(
                () =>
                    orderEntity.AddSkipNavigation(
                        nameof(Order.Products), null, null, productEntity, true, false)).Message);
    }

    [ConditionalFact]
    public void Adding_skip_navigation_with_a_name_that_conflicts_with_a_navigation_throws()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(typeof(Product));
        var productIdProperty = productEntity.AddProperty(Product.IdProperty);
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(OrderProduct.OrderIdProperty);
        var orderProductForeignKey = orderProductEntity
            .AddForeignKey(new[] { orderProductFkProperty }, orderKey, orderEntity);

        var customerForeignKey = productEntity.AddForeignKey(productIdProperty, orderKey, orderEntity);

        customerForeignKey.SetPrincipalToDependent(nameof(Order.Products));

        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation(nameof(Order.Products), typeof(Order).Name, typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(
                () =>
                    orderEntity.AddSkipNavigation(
                        nameof(Order.Products), null, null, productEntity, true, false)).Message);
    }

    [ConditionalFact]
    public void Adding_skip_navigation_with_a_name_that_conflicts_with_a_property_throws()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(typeof(Product));
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(OrderProduct.OrderIdProperty);
        var orderProductForeignKey = orderProductEntity
            .AddForeignKey(new[] { orderProductFkProperty }, orderKey, orderEntity);

        orderEntity.AddProperty(nameof(Order.Products));

        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation(nameof(Order.Products), typeof(Order).Name, typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(
                () =>
                    orderEntity.AddSkipNavigation(
                        nameof(Order.Products), null, null, productEntity, true, false)).Message);
    }

    [ConditionalFact]
    public void Adding_skip_navigation_with_a_name_that_conflicts_with_a_service_property_throws()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(typeof(Product));
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(OrderProduct.OrderIdProperty);
        var orderProductForeignKey = orderProductEntity
            .AddForeignKey(new[] { orderProductFkProperty }, orderKey, orderEntity);

        orderEntity.AddServiceProperty(Order.ProductsProperty);

        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation(nameof(Order.Products), typeof(Order).Name, typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(
                () =>
                    orderEntity.AddSkipNavigation(
                        nameof(Order.Products), null, null, productEntity, true, false)).Message);
    }

    [ConditionalFact]
    public void Adding_CLR_skip_navigation_targetting_a_shadow_entity_type_throws()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(nameof(Order.Id));
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(nameof(Product));
        var orderProductEntity = model.AddEntityType(nameof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(nameof(OrderProduct.OrderId), typeof(int));
        var orderProductForeignKey = orderProductEntity
            .AddForeignKey(new[] { orderProductFkProperty }, orderKey, orderEntity);

        Assert.Equal(
            CoreStrings.NavigationCollectionWrongClrType(
                nameof(Order.Products), nameof(Order), "ICollection<Product>", "Dictionary<string, object>"),
            Assert.Throws<InvalidOperationException>(
                () => orderEntity.AddSkipNavigation(
                    nameof(Order.Products), null, Order.ProductsProperty, productEntity, true, false)).Message);
    }

    [ConditionalFact]
    public void Adding_CLR_skip_navigation_to_a_mismatched_entity_type_throws()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(typeof(Product));
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(nameof(OrderProduct.OrderId), typeof(int));
        var orderProductForeignKey = orderProductEntity
            .AddForeignKey(new[] { orderProductFkProperty }, orderKey, orderEntity);

        Assert.Equal(
            CoreStrings.NoClrNavigation(nameof(Order.Products), nameof(Product)),
            Assert.Throws<InvalidOperationException>(
                () => productEntity.AddSkipNavigation(
                    nameof(Order.Products), null, Order.ProductsProperty, productEntity, true, false)).Message);
    }

    [ConditionalFact]
    public void Adding_CLR_collection_skip_navigation_with_mismatched_target_entity_type_throws()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(typeof(Product));
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(nameof(OrderProduct.OrderId), typeof(int));
        var orderProductForeignKey = orderProductEntity
            .AddForeignKey(new[] { orderProductFkProperty }, orderKey, orderEntity);

        Assert.Equal(
            CoreStrings.NavigationCollectionWrongClrType(nameof(Order.Products), nameof(Order), "ICollection<Product>", nameof(Order)),
            Assert.Throws<InvalidOperationException>(
                () => orderEntity.AddSkipNavigation(
                    nameof(Order.Products), null, Order.ProductsProperty, orderEntity, true, false)).Message);
    }

    [ConditionalFact]
    public void Adding_CLR_reference_skip_navigation_with_mismatched_target_entity_type_throws()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(typeof(Product));
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(nameof(OrderProduct.OrderId), typeof(int));
        var orderProductForeignKey = orderProductEntity
            .AddForeignKey(new[] { orderProductFkProperty }, orderKey, orderEntity);

        Assert.Equal(
            CoreStrings.NavigationSingleWrongClrType(nameof(Order.Products), nameof(Order), "ICollection<Product>", nameof(Order)),
            Assert.Throws<InvalidOperationException>(
                () => orderEntity.AddSkipNavigation(
                    nameof(Order.Products), null, Order.ProductsProperty, orderEntity, false, false)).Message);
    }

    [ConditionalFact]
    public void Adding_skip_navigation_with_a_mismatched_memberinfo_throws()
    {
        var model = CreateModel();
        var orderEntity = model.AddEntityType(typeof(Order));
        var orderIdProperty = orderEntity.AddProperty(Order.IdProperty);
        var orderKey = orderEntity.AddKey(orderIdProperty);
        var productEntity = model.AddEntityType(typeof(Product));
        var orderProductEntity = model.AddEntityType(typeof(OrderProduct));
        var orderProductFkProperty = orderProductEntity.AddProperty(OrderProduct.OrderIdProperty);
        var orderProductForeignKey = orderProductEntity
            .AddForeignKey(new[] { orderProductFkProperty }, orderKey, orderEntity);

        Assert.Equal(
            CoreStrings.PropertyWrongName(nameof(Order.Products), typeof(Order).Name, nameof(Order.RelatedOrder)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    orderEntity.AddSkipNavigation(
                        nameof(Order.Products), null, Order.RelatedOrderProperty, productEntity, true, false)).Message);
    }

    [ConditionalFact]
    public void Can_add_retrieve_and_remove_indexes()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Order));
        var property1 = entityType.AddProperty(Order.IdProperty);
        var property2 = entityType.AddProperty(Order.CustomerIdProperty);

        Assert.Empty(entityType.GetIndexes());
        Assert.Null(entityType.RemoveIndex(new[] { property1 }));
        Assert.False(property1.IsIndex());
        Assert.Empty(property1.GetContainingIndexes());

        var index1 = entityType.AddIndex(property1);

        Assert.Equal(1, index1.Properties.Count);
        Assert.Same(index1, entityType.FindIndex(property1));
        Assert.Same(index1, entityType.FindIndex(property1));
        Assert.Same(property1, index1.Properties[0]);

        var index2 = entityType.AddIndex(new[] { property1, property2 });

        Assert.NotNull(((IConventionIndex)index1).Builder);
        Assert.NotNull(((IConventionIndex)index2).Builder);
        Assert.Equal(2, index2.Properties.Count);
        Assert.Same(index2, entityType.FindIndex(new[] { property1, property2 }));
        Assert.Same(property1, index2.Properties[0]);
        Assert.Same(property2, index2.Properties[1]);
        Assert.True(property1.IsIndex());
        Assert.Equal([index1, index2], property1.GetContainingIndexes().ToArray());

        Assert.Equal(2, entityType.GetIndexes().Count());
        Assert.Same(index1, entityType.GetIndexes().First());
        Assert.Same(index2, entityType.GetIndexes().Last());

        Assert.Same(index1, entityType.RemoveIndex(index1.Properties));
        Assert.Null(entityType.RemoveIndex(index1.Properties));

        Assert.Single(entityType.GetIndexes());
        Assert.Same(index2, entityType.GetIndexes().Single());

        Assert.Same(index2, entityType.RemoveIndex(new[] { property1, property2 }));

        Assert.False(((Index)index1).IsInModel);
        Assert.False(((Index)index2).IsInModel);
        Assert.Empty(entityType.GetIndexes());
        Assert.False(property1.IsIndex());
        Assert.Empty(property1.GetContainingIndexes());
    }

    [ConditionalFact]
    public void AddIndex_throws_if_not_from_same_entity()
    {
        var model = CreateModel();
        var entityType1 = model.AddEntityType(typeof(Customer));
        var entityType2 = model.AddEntityType(typeof(Order));
        var property1 = entityType1.AddProperty(Customer.IdProperty);
        var property2 = entityType1.AddProperty(Customer.NameProperty);

        Assert.Equal(
            CoreStrings.IndexPropertiesWrongEntity(
                "{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}", typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(
                () => entityType2.AddIndex(new[] { property1, property2 })).Message);
    }

    [ConditionalFact]
    public void AddIndex_throws_if_duplicate_properties()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);
        entityType.AddIndex(new[] { property1, property2 });

        Assert.Equal(
            CoreStrings.DuplicateIndex(
                "{'" + Customer.IdProperty.Name + "', '" + Customer.NameProperty.Name + "'}",
                typeof(Customer).Name,
                typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(
                () => entityType.AddIndex(new[] { property1, property2 })).Message);
    }

    [ConditionalFact]
    public void AddIndex_throws_if_duplicate_name()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);
        entityType.AddIndex(new[] { property1 }, "NamedIndex");

        Assert.Equal(
            CoreStrings.DuplicateNamedIndex(
                "NamedIndex",
                "{'" + Customer.NameProperty.Name + "'}",
                typeof(Customer).Name,
                typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(
                () => entityType.AddIndex(new[] { property2 }, "NamedIndex")).Message);
    }

    [ConditionalFact]
    public void Can_add_multiple_named_indexes_on_the_same_properties()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);

        entityType.AddIndex(new[] { property1, property2 }, "Index1");
        entityType.AddIndex(new[] { property1, property2 }, "Index2");
    }

    [ConditionalFact]
    public void RemoveIndex_throws_if_incorrect_properties()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);
        entityType.AddIndex(new[] { property1, property2 });

        var anotherIndex = new Index(
            new List<Property> { (Property)property2 },
            (EntityType)entityType,
            ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.IndexWrongType(
                "{'" + Customer.NameProperty.Name + "'}",
                typeof(Customer).Name,
                typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(
                () => entityType.RemoveIndex(anotherIndex)).Message);
    }

    [ConditionalFact]
    public void RemoveIndex_throws_if_incorrect_name()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);
        entityType.AddIndex(new[] { property1 }, "NamedIndex");

        var anotherIndex = new Index(
            new List<Property> { (Property)property1 },
            "NonExistentIndex",
            (EntityType)entityType,
            ConfigurationSource.Explicit);

        Assert.Equal(
            CoreStrings.NamedIndexWrongType("NonExistentIndex", typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(
                () => entityType.RemoveIndex(anotherIndex)).Message);
    }

    [ConditionalFact]
    public void Can_remove_named_index_by_name()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        entityType.AddIndex(new[] { property1 }, "NamedIndex");
        Assert.Single(entityType.GetIndexes());

        var index = ((EntityType)entityType).RemoveIndex("NamedIndex");

        Assert.Equal("NamedIndex", index.Name);
        Assert.Empty(entityType.GetIndexes());
    }

    [ConditionalFact]
    public void Can_add_and_remove_properties()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        Assert.Null(entityType.RemoveProperty("Id"));

        var property1 = entityType.AddProperty("Id", typeof(int));

        Assert.False(property1.IsShadowProperty());
        Assert.Equal("Id", property1.Name);
        Assert.Same(typeof(int), property1.ClrType);
        Assert.False(((IReadOnlyProperty)property1).IsConcurrencyToken);
        Assert.Same(entityType, property1.DeclaringType);

        var property2 = entityType.AddProperty("Name", typeof(string));

        Assert.True(((Property)property1).IsInModel);
        Assert.True(((Property)property2).IsInModel);
        Assert.True(new[] { property1, property2 }.SequenceEqual(entityType.GetProperties()));

        Assert.Same(property1, entityType.RemoveProperty(property1.Name));
        Assert.Null(entityType.RemoveProperty(property1.Name));

        Assert.True(new[] { property2 }.SequenceEqual(entityType.GetProperties()));

        Assert.Same(property2, entityType.RemoveProperty("Name"));

        Assert.False(((Property)property1).IsInModel);
        Assert.False(((Property)property2).IsInModel);
        Assert.Empty(entityType.GetProperties());
    }

    [ConditionalFact]
    public void Can_add_new_properties_or_get_existing_properties_using_PropertyInfo_or_name()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));

        var idProperty = entityType.AddProperty("Id", typeof(int));

        Assert.False(idProperty.IsShadowProperty());
        Assert.Equal("Id", idProperty.Name);
        Assert.Same(typeof(int), idProperty.ClrType);
        Assert.Same(entityType, idProperty.DeclaringType);

        Assert.Same(idProperty, entityType.FindProperty(Customer.IdProperty));
        Assert.Same(idProperty, entityType.FindProperty("Id"));
        Assert.False(idProperty.IsShadowProperty());

        var nameProperty = entityType.AddProperty("Name");

        Assert.False(nameProperty.IsShadowProperty());
        Assert.Equal("Name", nameProperty.Name);
        Assert.Same(typeof(string), nameProperty.ClrType);
        Assert.Same(entityType, nameProperty.DeclaringType);

        Assert.Same(nameProperty, entityType.FindProperty(Customer.NameProperty));
        Assert.Same(nameProperty, entityType.FindProperty("Name"));
        Assert.False(nameProperty.IsShadowProperty());

        Assert.True(new[] { idProperty, nameProperty }.SequenceEqual(entityType.GetProperties()));
    }

    [ConditionalFact]
    public void Can_add_new_properties_using_name_of_property_in_base_class()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(HiddenField));

        var property = entityType.AddProperty("Raisin");

        Assert.False(property.IsShadowProperty());
        Assert.Equal("Raisin", property.Name);
        Assert.Same(typeof(string), property.ClrType);
        Assert.Same(entityType, property.DeclaringType);
        Assert.Same(HiddenFieldBase.RaisinProperty, property.PropertyInfo);
        Assert.Null(property.FieldInfo);
    }

    [ConditionalFact]
    public void Can_add_new_properties_using_name_of_field_in_base_class()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(HiddenField));

        var property = entityType.AddProperty("_date");

        Assert.False(property.IsShadowProperty());
        Assert.Equal("_date", property.Name);
        Assert.Same(typeof(string), property.ClrType);
        Assert.Same(entityType, property.DeclaringType);
        Assert.Same(HiddenFieldBase.DateField, property.FieldInfo);
        Assert.Null(property.PropertyInfo);
    }

    private class HiddenField : HiddenFieldBase
    {
        public int Id { get; set; }
    }

    private class HiddenFieldBase
    {
        public static readonly FieldInfo DateField
            = typeof(HiddenFieldBase).GetRuntimeFields().Single(f => f.Name == nameof(_date));

        public static readonly PropertyInfo RaisinProperty
            = typeof(HiddenFieldBase).GetRuntimeProperties().Single(p => p.Name == nameof(Raisin));

        private string _date;
        private string Raisin { get; set; }

        public DateTime Date
        {
            get => DateTime.Parse(_date);
            set => _date = value.ToString(CultureInfo.InvariantCulture);
        }
    }

    [ConditionalFact]
    public void AddProperty_throws_for_wrong_entity_type()
    {
        var entityType = CreateModel().AddEntityType(typeof(Order));

        Assert.Equal(
            CoreStrings.PropertyWrongEntityClrType(
                nameof(Customer.Name), nameof(Order), nameof(Customer)),
            Assert.Throws<InvalidOperationException>(
                () => entityType.AddProperty(Customer.NameProperty)).Message);
    }

    [ConditionalFact]
    public void AddProperty_throws_if_no_clr_property_or_field()
    {
        var entityType = CreateModel().AddEntityType(typeof(Customer));

        Assert.Equal(
            CoreStrings.NoPropertyType("_foo", nameof(Customer)),
            Assert.Throws<InvalidOperationException>(
                () => entityType.AddProperty("_foo")).Message);
    }

    [ConditionalFact]
    public void AddProperty_throws_if_clr_type_does_not_match()
    {
        var entityType = CreateModel().AddEntityType(typeof(Customer));

        Assert.Equal(
            CoreStrings.PropertyWrongClrType(
                nameof(Customer.Name), nameof(Customer), typeof(string).DisplayName(), typeof(int).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => entityType.AddProperty(nameof(Customer.Name), typeof(int))).Message);
    }

    [ConditionalFact]
    public void AddProperty_throws_if_name_does_not_match()
    {
        var entityType = CreateModel().AddEntityType(typeof(Customer));

        Assert.Equal(
            CoreStrings.PropertyWrongName(
                nameof(Customer.Id), nameof(Customer), nameof(Customer.Name)),
            Assert.Throws<InvalidOperationException>(
                () =>
                    entityType.AddProperty(nameof(Customer.Id), typeof(int), Customer.NameProperty)).Message);
    }

    [ConditionalFact]
    public void AddProperty_ignores_clr_type_if_implicit()
    {
        var entityType = (IConventionEntityType)CreateModel().AddEntityType(typeof(Customer));

        var property = entityType.AddProperty(nameof(Customer.Name), typeof(int), setTypeConfigurationSource: false);

        Assert.Equal(typeof(string), property.ClrType);
    }

    [ConditionalFact]
    public void RemoveProperty_throws_when_called_on_wrong_entity_type()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerPk = customerType.AddProperty(Customer.IdProperty);

        var orderType = model.AddEntityType(typeof(Order));

        Assert.Equal(
            CoreStrings.PropertyWrongType("Id", typeof(Order).Name, typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(() => orderType.RemoveProperty(customerPk)).Message);
    }

    [ConditionalFact]
    public void Cannot_remove_property_when_used_by_primary_key()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var property = entityType.AddProperty(Customer.IdProperty);

        entityType.SetPrimaryKey(property);

        Assert.Equal(
            CoreStrings.PropertyInUseKey("Id", typeof(Customer).Name, "{'Id'}"),
            Assert.Throws<InvalidOperationException>(() => entityType.RemoveProperty(property.Name)).Message);
    }

    [ConditionalFact]
    public void Cannot_remove_property_when_used_by_non_primary_key()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var property = entityType.AddProperty(Customer.IdProperty);

        entityType.AddKey(property);

        Assert.Equal(
            CoreStrings.PropertyInUseKey("Id", typeof(Customer).Name, "{'Id'}"),
            Assert.Throws<InvalidOperationException>(() => entityType.RemoveProperty(property.Name)).Message);
    }

    [ConditionalFact]
    public void Cannot_remove_property_when_used_by_foreign_key()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerPk = customerType.SetPrimaryKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var customerFk = orderType.AddProperty(Order.CustomerIdProperty);
        orderType.AddForeignKey(customerFk, customerPk, customerType);

        Assert.Equal(
            CoreStrings.PropertyInUseForeignKey("CustomerId", typeof(Order).Name, "{'CustomerId'}", typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(() => orderType.RemoveProperty(customerFk.Name)).Message);
    }

    [ConditionalFact]
    public void Cannot_remove_property_when_used_by_an_index()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var property = entityType.AddProperty(Customer.IdProperty);

        entityType.AddIndex(property);

        Assert.Equal(
            CoreStrings.PropertyInUseIndex("Id", typeof(Customer).Name, "{'Id'}", typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(() => entityType.RemoveProperty(property.Name)).Message);
    }

    [ConditionalFact]
    public void Properties_are_ordered_by_name()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));

        var property2 = entityType.AddProperty(Customer.NameProperty);
        var property1 = entityType.AddProperty(Customer.IdProperty);

        Assert.Equal(new[] { property1, property2 }, entityType.GetProperties());
    }

    [ConditionalFact]
    public void Primary_key_properties_precede_others()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));

        var aProperty = entityType.AddProperty("A", typeof(int));
        var pkProperty = entityType.AddProperty(Customer.IdProperty);

        entityType.SetPrimaryKey(pkProperty);

        Assert.Equal(new[] { pkProperty, aProperty }, entityType.GetProperties());
    }

    [ConditionalFact]
    public void Composite_primary_key_properties_are_listed_in_key_order()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType("CompositeKeyType");

        var aProperty = entityType.AddProperty("A", typeof(int));
        var pkProperty2 = entityType.AddProperty("aPK", typeof(int));
        var pkProperty1 = entityType.AddProperty("bPK", typeof(int));

        entityType.SetPrimaryKey(new[] { pkProperty1, pkProperty2 });

        Assert.Equal(new[] { pkProperty1, pkProperty2, aProperty }, entityType.GetProperties());
    }

    [ConditionalFact]
    public void Properties_are_properly_ordered_when_primary_key_changes()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));

        var aProperty = entityType.AddProperty("A", typeof(int));
        var bProperty = entityType.AddProperty("B", typeof(int));

        entityType.SetPrimaryKey(bProperty);

        Assert.Equal(new[] { bProperty, aProperty }, entityType.GetProperties());

        entityType.SetPrimaryKey(aProperty);

        Assert.Equal(new[] { aProperty, bProperty }, entityType.GetProperties());
    }

    [ConditionalFact]
    public void Can_get_property_and_can_try_get_property()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var property = entityType.AddProperty(Customer.IdProperty);

        Assert.Same(property, entityType.FindProperty(Customer.IdProperty));
        Assert.Same(property, entityType.FindProperty("Id"));
        Assert.Same(property, entityType.FindProperty(Customer.IdProperty));
        Assert.Same(property, entityType.FindProperty("Id"));

        Assert.Null(entityType.FindProperty("Nose"));
    }

    [ConditionalFact]
    public void Shadow_properties_have_CLR_flag_set_to_false()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));

        entityType.AddProperty(Customer.NameProperty);
        entityType.AddProperty(Customer.IdProperty);
        entityType.AddProperty("Mane_", typeof(int));

        Assert.False(entityType.FindProperty("Name").IsShadowProperty());
        Assert.False(entityType.FindProperty("Id").IsShadowProperty());
        Assert.True(entityType.FindProperty("Mane_").IsShadowProperty());
    }

    [ConditionalFact]
    public void Adding_a_new_property_with_a_name_that_already_exists_throws()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        entityType.AddProperty(Customer.IdProperty);

        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation("Id", typeof(Customer).Name, typeof(Customer).Name),
            Assert.Throws<InvalidOperationException>(() => entityType.AddProperty("Id")).Message);
    }

    [ConditionalFact]
    public void Adding_a_new_property_with_a_name_that_conflicts_with_a_navigation_throws()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

        customerForeignKey.SetDependentToPrincipal(Order.CustomerProperty);

        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation("Customer", typeof(Order).Name, typeof(Order).Name),
            Assert.Throws<InvalidOperationException>(() => orderType.AddProperty("Customer")).Message);
    }

    [ConditionalFact]
    public void Adding_a_new_property_with_a_name_that_conflicts_with_a_service_property_throws()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

        customerForeignKey.SetDependentToPrincipal(Order.CustomerProperty);

        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation(nameof(Order.Customer), nameof(Order), nameof(Order)),
            Assert.Throws<InvalidOperationException>(() => orderType.AddServiceProperty(Order.CustomerProperty)).Message);
    }

    [ConditionalFact]
    public void Adding_a_new_service_property_with_a_name_that_conflicts_with_a_property_throws()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        entityType.AddProperty(Customer.OrdersProperty);

        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation(nameof(Customer.Orders), nameof(Customer), nameof(Customer)),
            Assert.Throws<InvalidOperationException>(() => entityType.AddServiceProperty(Customer.OrdersProperty)).Message);
    }

    [ConditionalFact]
    public void Adding_a_new_service_property_with_a_name_that_conflicts_with_a_navigation_throws()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var customerKey = customerType.AddKey(customerType.AddProperty(Customer.IdProperty));

        var orderType = model.AddEntityType(typeof(Order));
        var foreignKeyProperty = orderType.AddProperty(Order.CustomerIdProperty);
        var customerForeignKey = orderType.AddForeignKey(foreignKeyProperty, customerKey, customerType);

        customerForeignKey.SetDependentToPrincipal(Order.CustomerProperty);

        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation(nameof(Order.Customer), nameof(Order), nameof(Order)),
            Assert.Throws<InvalidOperationException>(() => orderType.AddServiceProperty(Order.CustomerProperty)).Message);
    }

    [ConditionalFact]
    public void Adding_a_new_service_property_with_a_name_that_already_exists_throws()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        entityType.AddServiceProperty(Customer.OrdersProperty);

        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation(nameof(Customer.Orders), nameof(Customer), nameof(Customer)),
            Assert.Throws<InvalidOperationException>(() => entityType.AddServiceProperty(Customer.OrdersProperty)).Message);
    }

    [ConditionalFact]
    public void Can_add_indexed_property()
    {
        var model = CreateModel();
        var mutatbleEntityType = model.AddEntityType(typeof(Customer));
        var mutableProperty = mutatbleEntityType.AddIndexerProperty("Nation", typeof(string));

        Assert.False(mutableProperty.IsShadowProperty());
        Assert.True(mutableProperty.IsIndexerProperty());
        Assert.Equal("Nation", mutableProperty.Name);
        Assert.Same(typeof(string), mutableProperty.ClrType);
        Assert.Same(mutatbleEntityType, mutableProperty.DeclaringType);

        Assert.True(new[] { mutableProperty }.SequenceEqual(mutatbleEntityType.GetProperties()));

        Assert.Same(mutableProperty, mutatbleEntityType.RemoveProperty("Nation"));
        Assert.Empty(mutatbleEntityType.GetProperties());

        var conventionEntityType = (IConventionEntityType)mutatbleEntityType;
        var conventionProperty = conventionEntityType.AddIndexerProperty("Country", typeof(string));

        Assert.False(conventionProperty.IsShadowProperty());
        Assert.True(conventionProperty.IsIndexerProperty());
        Assert.Equal("Country", conventionProperty.Name);
        Assert.Same(typeof(string), conventionProperty.ClrType);
        Assert.Same(mutatbleEntityType, conventionProperty.DeclaringType);

        Assert.True(new[] { conventionProperty }.SequenceEqual(conventionEntityType.GetProperties()));

        Assert.Same(conventionProperty, conventionEntityType.RemoveProperty("Country"));
        Assert.Empty(conventionEntityType.GetProperties());
    }

    [ConditionalFact]
    public void FindProperty_return_null_when_passed_indexer_property_info()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        var property = entityType.AddIndexerProperty("Nation", typeof(string));
        entityType.AddProperty("Item", typeof(string));
        var indexerPropertyInfo = typeof(Customer).GetRuntimeProperty("Item");
        Assert.NotNull(indexerPropertyInfo);

        Assert.Same(property, entityType.FindProperty("Nation"));

        Assert.Null(((IReadOnlyEntityType)entityType).FindProperty(indexerPropertyInfo));
        Assert.Null(entityType.FindProperty(indexerPropertyInfo));
        Assert.Null(((IConventionEntityType)entityType).FindProperty(indexerPropertyInfo));
    }

    [ConditionalFact]
    public void AddIndexerProperty_throws_when_entitytype_does_not_have_indexer()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Order));

        Assert.Equal(
            CoreStrings.NonIndexerEntityType("Nation", entityType.DisplayName(), typeof(string).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(() => entityType.AddIndexerProperty("Nation", typeof(string))).Message);

        Assert.Equal(
            CoreStrings.NonIndexerEntityType("Nation", entityType.DisplayName(), typeof(string).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => ((IConventionEntityType)entityType).AddIndexerProperty("Nation", typeof(string))).Message);
    }

    [ConditionalFact]
    public void AddIndexerProperty_throws_when_entitytype_have_property_with_same_name()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType(typeof(Customer));
        entityType.AddProperty("Nation", typeof(string));

        Assert.Equal(
            CoreStrings.ConflictingPropertyOrNavigation("Nation", entityType.DisplayName(), entityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => entityType.AddIndexerProperty("Nation", typeof(string))).Message);

        Assert.Equal(
            CoreStrings.PropertyClashingNonIndexer("Name", entityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(() => entityType.AddIndexerProperty("Name", typeof(string))).Message);
    }

    [ConditionalFact]
    public void Can_get_property_indexes()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Customer>(
            eb =>
            {
                eb.Property(c => c.Name);
                eb.Property<int>("Id_");
                eb.Property<int>("Mane_");
            });

        var entityType = (IRuntimeEntityType)modelBuilder.FinalizeModel().FindEntityType(typeof(Customer));

        Assert.Equal(0, entityType.FindProperty("Id_").GetIndex());
        Assert.Equal(1, entityType.FindProperty("Mane_").GetIndex());
        Assert.Equal(2, entityType.FindProperty("Name").GetIndex());

        Assert.Equal(0, entityType.FindProperty("Id_").GetShadowIndex());
        Assert.Equal(1, entityType.FindProperty("Mane_").GetShadowIndex());
        Assert.Equal(-1, entityType.FindProperty("Name").GetShadowIndex());

        Assert.Equal(2, entityType.ShadowPropertyCount);
    }

    [ConditionalFact]
    public void Attempting_to_set_store_generated_value_for_non_generated_property_throws()
    {
        using var context = new Levels();
        var property = context.Model.FindEntityType(typeof(Level1)).GetProperty("Prop1");

        Assert.Equal(-1, property.GetStoreGeneratedIndex());

        var internalEntityEntry = context.Entry(new Level1()).GetInfrastructure();

        Assert.Equal(
            CoreStrings.StoreGenValue("Prop1", nameof(Level1)),
            Assert.Throws<InvalidOperationException>(() => internalEntityEntry.SetStoreGeneratedValue(property, null)).Message);
    }

    [ConditionalFact]
    public void Indexes_for_derived_types_are_calculated_correctly()
    {
        using var context = new Levels();
        var type = (IRuntimeEntityType)context.Model.FindEntityType(typeof(Level1));

        Assert.Equal(0, type.FindProperty("Id").GetIndex());
        Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetIndex());
        Assert.Equal(2, type.FindProperty("Prop1").GetIndex());
        Assert.Equal(0, type.FindNavigation("Level1Collection").GetIndex());
        Assert.Equal(1, type.FindNavigation("Level1Reference").GetIndex());

        Assert.Equal(-1, type.FindProperty("Id").GetShadowIndex());
        Assert.Equal(0, type.FindProperty("Level1ReferenceId").GetShadowIndex());
        Assert.Equal(-1, type.FindProperty("Prop1").GetShadowIndex());

        Assert.Equal(0, type.FindProperty("Id").GetOriginalValueIndex());
        Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetOriginalValueIndex());
        Assert.Equal(2, type.FindProperty("Prop1").GetOriginalValueIndex());

        Assert.Equal(0, type.FindProperty("Id").GetRelationshipIndex());
        Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetRelationshipIndex());
        Assert.Equal(-1, type.FindProperty("Prop1").GetRelationshipIndex());
        Assert.Equal(2, type.FindNavigation("Level1Collection").GetRelationshipIndex());
        Assert.Equal(3, type.FindNavigation("Level1Reference").GetRelationshipIndex());

        Assert.Equal(0, type.FindProperty("Id").GetStoreGeneratedIndex());
        Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindProperty("Prop1").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindNavigation("Level1Collection").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindNavigation("Level1Reference").GetStoreGeneratedIndex());

        Assert.Equal(4, type.PropertyCount);
        Assert.Equal(2, type.NavigationCount);
        Assert.Equal(2, type.ShadowPropertyCount);
        Assert.Equal(4, type.OriginalValueCount);
        Assert.Equal(4, type.RelationshipPropertyCount);
        Assert.Equal(2, type.StoreGeneratedCount);

        type = (IRuntimeEntityType)context.Model.FindEntityType(typeof(Level2));

        Assert.Equal(0, type.FindProperty("Id").GetIndex());
        Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetIndex());
        Assert.Equal(2, type.FindProperty("Prop1").GetIndex());
        Assert.Equal(4, type.FindProperty("Level2ReferenceId").GetIndex());
        Assert.Equal(5, type.FindProperty("Prop2").GetIndex());
        Assert.Equal(0, type.FindNavigation("Level1Collection").GetIndex());
        Assert.Equal(1, type.FindNavigation("Level1Reference").GetIndex());
        Assert.Equal(2, type.FindNavigation("Level2Collection").GetIndex());
        Assert.Equal(3, type.FindNavigation("Level2Reference").GetIndex());

        Assert.Equal(-1, type.FindProperty("Id").GetShadowIndex());
        Assert.Equal(0, type.FindProperty("Level1ReferenceId").GetShadowIndex());
        Assert.Equal(-1, type.FindProperty("Prop1").GetShadowIndex());
        Assert.Equal(2, type.FindProperty("Level2ReferenceId").GetShadowIndex());
        Assert.Equal(-1, type.FindProperty("Prop2").GetShadowIndex());

        Assert.Equal(0, type.FindProperty("Id").GetOriginalValueIndex());
        Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetOriginalValueIndex());
        Assert.Equal(2, type.FindProperty("Prop1").GetOriginalValueIndex());
        Assert.Equal(4, type.FindProperty("Level2ReferenceId").GetOriginalValueIndex());
        Assert.Equal(5, type.FindProperty("Prop2").GetOriginalValueIndex());

        Assert.Equal(0, type.FindProperty("Id").GetRelationshipIndex());
        Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetRelationshipIndex());
        Assert.Equal(-1, type.FindProperty("Prop1").GetRelationshipIndex());
        Assert.Equal(2, type.FindNavigation("Level1Collection").GetRelationshipIndex());
        Assert.Equal(3, type.FindNavigation("Level1Reference").GetRelationshipIndex());
        Assert.Equal(4, type.FindProperty("Level2ReferenceId").GetRelationshipIndex());
        Assert.Equal(-1, type.FindProperty("Prop2").GetRelationshipIndex());
        Assert.Equal(5, type.FindNavigation("Level2Collection").GetRelationshipIndex());
        Assert.Equal(6, type.FindNavigation("Level2Reference").GetRelationshipIndex());

        Assert.Equal(0, type.FindProperty("Id").GetStoreGeneratedIndex());
        Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindProperty("Prop1").GetStoreGeneratedIndex());
        Assert.Equal(2, type.FindProperty("Level2ReferenceId").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindProperty("Prop2").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindNavigation("Level1Collection").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindNavigation("Level1Reference").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindNavigation("Level2Collection").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindNavigation("Level2Reference").GetStoreGeneratedIndex());

        Assert.Equal(6, type.PropertyCount);
        Assert.Equal(4, type.NavigationCount);
        Assert.Equal(3, type.ShadowPropertyCount);
        Assert.Equal(6, type.OriginalValueCount);
        Assert.Equal(7, type.RelationshipPropertyCount);
        Assert.Equal(3, type.StoreGeneratedCount);

        type = (IRuntimeEntityType)context.Model.FindEntityType(typeof(Level3));

        Assert.Equal(0, type.FindProperty("Id").GetIndex());
        Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetIndex());
        Assert.Equal(2, type.FindProperty("Prop1").GetIndex());
        Assert.Equal(4, type.FindProperty("Level2ReferenceId").GetIndex());
        Assert.Equal(5, type.FindProperty("Prop2").GetIndex());
        Assert.Equal(6, type.FindProperty("Level3ReferenceId").GetIndex());
        Assert.Equal(7, type.FindProperty("Prop3").GetIndex());
        Assert.Equal(0, type.FindNavigation("Level1Collection").GetIndex());
        Assert.Equal(1, type.FindNavigation("Level1Reference").GetIndex());
        Assert.Equal(2, type.FindNavigation("Level2Collection").GetIndex());
        Assert.Equal(3, type.FindNavigation("Level2Reference").GetIndex());
        Assert.Equal(4, type.FindNavigation("Level3Collection").GetIndex());
        Assert.Equal(5, type.FindNavigation("Level3Reference").GetIndex());

        Assert.Equal(-1, type.FindProperty("Id").GetShadowIndex());
        Assert.Equal(0, type.FindProperty("Level1ReferenceId").GetShadowIndex());
        Assert.Equal(-1, type.FindProperty("Prop1").GetShadowIndex());
        Assert.Equal(2, type.FindProperty("Level2ReferenceId").GetShadowIndex());
        Assert.Equal(-1, type.FindProperty("Prop2").GetShadowIndex());
        Assert.Equal(3, type.FindProperty("Level3ReferenceId").GetShadowIndex());
        Assert.Equal(-1, type.FindProperty("Prop3").GetShadowIndex());

        Assert.Equal(0, type.FindProperty("Id").GetOriginalValueIndex());
        Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetOriginalValueIndex());
        Assert.Equal(2, type.FindProperty("Prop1").GetOriginalValueIndex());
        Assert.Equal(4, type.FindProperty("Level2ReferenceId").GetOriginalValueIndex());
        Assert.Equal(5, type.FindProperty("Prop2").GetOriginalValueIndex());
        Assert.Equal(6, type.FindProperty("Level3ReferenceId").GetOriginalValueIndex());
        Assert.Equal(7, type.FindProperty("Prop3").GetOriginalValueIndex());

        Assert.Equal(0, type.FindProperty("Id").GetRelationshipIndex());
        Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetRelationshipIndex());
        Assert.Equal(-1, type.FindProperty("Prop1").GetRelationshipIndex());
        Assert.Equal(2, type.FindNavigation("Level1Collection").GetRelationshipIndex());
        Assert.Equal(3, type.FindNavigation("Level1Reference").GetRelationshipIndex());
        Assert.Equal(4, type.FindProperty("Level2ReferenceId").GetRelationshipIndex());
        Assert.Equal(-1, type.FindProperty("Prop2").GetRelationshipIndex());
        Assert.Equal(5, type.FindNavigation("Level2Collection").GetRelationshipIndex());
        Assert.Equal(6, type.FindNavigation("Level2Reference").GetRelationshipIndex());
        Assert.Equal(7, type.FindProperty("Level3ReferenceId").GetRelationshipIndex());
        Assert.Equal(-1, type.FindProperty("Prop3").GetRelationshipIndex());
        Assert.Equal(8, type.FindNavigation("Level3Collection").GetRelationshipIndex());
        Assert.Equal(9, type.FindNavigation("Level3Reference").GetRelationshipIndex());

        Assert.Equal(0, type.FindProperty("Id").GetStoreGeneratedIndex());
        Assert.Equal(1, type.FindProperty("Level1ReferenceId").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindProperty("Prop1").GetStoreGeneratedIndex());
        Assert.Equal(2, type.FindProperty("Level2ReferenceId").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindProperty("Prop2").GetStoreGeneratedIndex());
        Assert.Equal(3, type.FindProperty("Level3ReferenceId").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindProperty("Prop3").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindNavigation("Level1Collection").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindNavigation("Level1Reference").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindNavigation("Level2Collection").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindNavigation("Level2Reference").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindNavigation("Level3Collection").GetStoreGeneratedIndex());
        Assert.Equal(-1, type.FindNavigation("Level3Reference").GetStoreGeneratedIndex());

        Assert.Equal(8, type.PropertyCount);
        Assert.Equal(6, type.NavigationCount);
        Assert.Equal(4, type.ShadowPropertyCount);
        Assert.Equal(8, type.OriginalValueCount);
        Assert.Equal(10, type.RelationshipPropertyCount);
        Assert.Equal(4, type.StoreGeneratedCount);
    }

    private class Levels : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Level1>().HasOne(e => e.Level1Reference).WithMany(e => e.Level1Collection);
            modelBuilder.Entity<Level2>().HasOne(e => e.Level2Reference).WithMany(e => e.Level2Collection);
            modelBuilder.Entity<Level3>().HasOne(e => e.Level3Reference).WithMany(e => e.Level3Collection);

            modelBuilder.Entity<Level1>().HasDiscriminator<string>("Z");
        }
    }

    [ConditionalFact]
    public void Can_get_all_properties_and_navigations()
    {
        var entityType = CreateEmptyModel().AddEntityType(nameof(SelfRef));
        var pk = entityType.SetPrimaryKey(entityType.AddProperty(nameof(SelfRef.Id), typeof(int)));
        var fkProp = entityType.AddProperty(nameof(SelfRef.SelfRefId), typeof(int?));

        var fk = entityType.AddForeignKey(new[] { fkProp }, pk, entityType);
        fk.IsUnique = true;
        var dependentToPrincipal = fk.SetDependentToPrincipal(nameof(SelfRef.SelfRef2));
        var principalToDependent = fk.SetPrincipalToDependent(nameof(SelfRef.SelfRef1));

        Assert.Equal(
            new IReadOnlyPropertyBase[] { pk.Properties.Single(), fkProp, principalToDependent, dependentToPrincipal },
            ((IRuntimeEntityType)entityType).GetSnapshottableMembers().ToArray());
    }

    [ConditionalFact]
    public void Indexes_for_owned_collection_types_are_calculated_correctly()
    {
        using var context = new SideBySide();
        var model = context.Model;

        var parent = (IRuntimeEntityType)model.FindEntityType(typeof(Parent1Entity));
        var indexes = GetIndexes(parent.GetSnapshottableMembers());
        Assert.Equal(2, indexes.Count);
        // Order: Index, Shadow, Original, StoreGenerated, Relationship
        Assert.Equal((0, -1, 0, 0, 0), indexes[nameof(Parent1Entity.Id)]);
        Assert.Equal((0, -1, -1, -1, 1), indexes[nameof(Parent1Entity.Children)]);

        indexes = GetIndexes(model.FindEntityType(typeof(ChildEntity), nameof(Parent1Entity.Children), parent).GetProperties());
        Assert.Equal(3, indexes.Count);
        // Order: Index, Shadow, Original, StoreGenerated, Relationship
        Assert.Equal((0, 0, 0, 0, 0), indexes[nameof(Parent1Entity) + "Id"]);
        Assert.Equal((1, 1, 1, 1, 1), indexes["Id"]);
        Assert.Equal((2, -1, 2, -1, -1), indexes[nameof(ChildEntity.Name)]);

        parent = (IRuntimeEntityType)model.FindEntityType(typeof(Parent2Entity));
        indexes = GetIndexes(parent.GetSnapshottableMembers());
        Assert.Equal(2, indexes.Count);
        // Order: Index, Shadow, Original, StoreGenerated, Relationship
        Assert.Equal((0, -1, 0, 0, 0), indexes[nameof(Parent2Entity.Id)]);
        Assert.Equal((0, -1, -1, -1, 1), indexes[nameof(Parent2Entity.Children)]);

        indexes = GetIndexes(model.FindEntityType(typeof(ChildEntity), nameof(Parent2Entity.Children), parent).GetProperties());
        Assert.Equal(3, indexes.Count);
        // Order: Index, Shadow, Original, StoreGenerated, Relationship
        Assert.Equal((0, 0, 0, 0, 0), indexes[nameof(Parent2Entity) + "Id"]);
        Assert.Equal((1, 1, 1, 1, 1), indexes["Id"]);
        Assert.Equal((2, -1, 2, -1, -1), indexes[nameof(ChildEntity.Name)]);

        parent = (IRuntimeEntityType)model.FindEntityType(typeof(Parent3Entity));
        indexes = GetIndexes(parent.GetSnapshottableMembers());
        Assert.Equal(2, indexes.Count);
        // Order: Index, Shadow, Original, StoreGenerated, Relationship
        Assert.Equal((0, -1, 0, 0, 0), indexes[nameof(Parent3Entity.Id)]);
        Assert.Equal((0, -1, -1, -1, 1), indexes[nameof(Parent3Entity.Children)]);

        indexes = GetIndexes(model.FindEntityType(typeof(ChildEntity), nameof(Parent3Entity.Children), parent).GetProperties());
        Assert.Equal(3, indexes.Count);
        // Order: Index, Shadow, Original, StoreGenerated, Relationship
        Assert.Equal((0, 0, 0, 0, 0), indexes[nameof(Parent3Entity) + "Id"]);
        Assert.Equal((1, 1, 1, 1, 1), indexes["Id"]);
        Assert.Equal((2, -1, 2, -1, -1), indexes[nameof(ChildEntity.Name)]);

        static Dictionary<string, (int, int, int, int, int)> GetIndexes(IEnumerable<IPropertyBase> properties)
            => properties.ToDictionary(
                p => p.Name,
                p =>
                    (p.GetIndex(),
                        p.GetShadowIndex(),
                        p.GetOriginalValueIndex(),
                        p.GetStoreGeneratedIndex(),
                        p.GetRelationshipIndex()
                    ));
    }

    private class SideBySide : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent1Entity>().OwnsMany(e => e.Children);
            modelBuilder.Entity<Parent2Entity>().OwnsMany(e => e.Children);
            modelBuilder.Entity<Parent3Entity>().OwnsMany(e => e.Children);
        }
    }

    private class Parent1Entity
    {
        public Guid Id { get; set; }
        public ICollection<ChildEntity> Children { get; set; }
    }

    private class Parent2Entity
    {
        public Guid Id { get; set; }
        public ICollection<ChildEntity> Children { get; set; }
    }

    private class Parent3Entity
    {
        public Guid Id { get; set; }
        public ICollection<ChildEntity> Children { get; set; }
    }

    private class ChildEntity
    {
        public string Name { get; set; }
    }

    [ConditionalFact]
    public void Indexes_are_ordered_by_property_count_then_property_names()
    {
        var model = CreateModel();
        var customerType = model.AddEntityType(typeof(Customer));
        var idProperty = customerType.AddProperty(Customer.IdProperty);
        var nameProperty = customerType.AddProperty(Customer.NameProperty);
        var otherProperty = customerType.AddProperty("OtherProperty", typeof(string));

        var i2 = customerType.AddIndex(nameProperty);
        var i4 = customerType.AddIndex(new[] { idProperty, otherProperty });
        var i3 = customerType.AddIndex(new[] { idProperty, nameProperty });
        var i1 = customerType.AddIndex(idProperty);

        model.FinalizeModel();

        Assert.True(new[] { i1, i2, i3, i4 }.SequenceEqual(customerType.GetIndexes()));
    }

    [ConditionalFact]
    public void Change_tracking_from_model_is_used_by_default_regardless_of_CLR_type()
    {
        var model = BuildFullNotificationEntityModel();
        var entityType = model.FindEntityType(typeof(FullNotificationEntity));

        Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.GetChangeTrackingStrategy());

        model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);

        Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.GetChangeTrackingStrategy());
    }

    [ConditionalFact]
    public void Change_tracking_from_model_is_used_by_default_for_shadow_entities()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType("Z'ha'dum");

        Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.GetChangeTrackingStrategy());

        model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);

        Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.GetChangeTrackingStrategy());
    }

    [ConditionalFact]
    public void Change_tracking_can_be_set_to_anything_for_full_notification_entities()
    {
        var model = BuildFullNotificationEntityModel();
        model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);

        var entityType = model.FindEntityType(typeof(FullNotificationEntity));

        Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.GetChangeTrackingStrategy());

        entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
        Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.GetChangeTrackingStrategy());

        entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
        Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.GetChangeTrackingStrategy());

        entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
        Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, entityType.GetChangeTrackingStrategy());

        entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
        Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues, entityType.GetChangeTrackingStrategy());
    }

    [ConditionalFact]
    public void Change_tracking_can_be_set_to_snapshot_or_changed_only_for_changed_only_entities()
    {
        var model = CreateModel();
        model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
        var entityType = model.AddEntityType(typeof(ChangedOnlyEntity));

        Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, entityType.GetChangeTrackingStrategy());

        entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
        Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.GetChangeTrackingStrategy());

        entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
        Assert.Equal(ChangeTrackingStrategy.ChangedNotifications, entityType.GetChangeTrackingStrategy());

        Assert.Equal(
            CoreStrings.ChangeTrackingInterfaceMissing(
                "ChangedOnlyEntity", "ChangingAndChangedNotifications", "INotifyPropertyChanging"),
            Assert.Throws<InvalidOperationException>(
                () => entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications)).Message);

        Assert.Equal(
            CoreStrings.ChangeTrackingInterfaceMissing(
                "ChangedOnlyEntity", "ChangingAndChangedNotificationsWithOriginalValues", "INotifyPropertyChanging"),
            Assert.Throws<InvalidOperationException>(
                    () => entityType.SetChangeTrackingStrategy(
                        ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues))
                .Message);
    }

    [ConditionalFact]
    public void Change_tracking_can_be_set_to_snapshot_only_for_non_notifying_entities()
    {
        var model = CreateModel();
        model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
        var entityType = model.AddEntityType(typeof(Customer));

        Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, entityType.GetChangeTrackingStrategy());

        entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
        Assert.Equal(ChangeTrackingStrategy.Snapshot, entityType.GetChangeTrackingStrategy());

        Assert.Equal(
            CoreStrings.ChangeTrackingInterfaceMissing("Customer", "ChangedNotifications", "INotifyPropertyChanged"),
            Assert.Throws<InvalidOperationException>(
                () => entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications)).Message);

        Assert.Equal(
            CoreStrings.ChangeTrackingInterfaceMissing("Customer", "ChangingAndChangedNotifications", "INotifyPropertyChanged"),
            Assert.Throws<InvalidOperationException>(
                () => entityType.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications)).Message);

        Assert.Equal(
            CoreStrings.ChangeTrackingInterfaceMissing(
                "Customer", "ChangingAndChangedNotificationsWithOriginalValues", "INotifyPropertyChanged"),
            Assert.Throws<InvalidOperationException>(
                    () => entityType.SetChangeTrackingStrategy(
                        ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues))
                .Message);
    }

    [ConditionalFact]
    public void Entity_type_with_deeply_nested_owned_shared_types_builds_correctly()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<Application>(
            entity =>
            {
                entity.OwnsOne(
                    x => x.Attitude,
                    amb =>
                    {
                        amb.OwnsOne(
                            x => x.FirstTest, mb =>
                            {
                                mb.OwnsOne(a => a.Tester);
                            });
                    });

                entity.OwnsOne(
                    x => x.Rejection,
                    amb =>
                    {
                        amb.OwnsOne(
                            x => x.FirstTest, mb =>
                            {
                                mb.OwnsOne(a => a.Tester);
                            });
                    });
            });

        Assert.Equal(
            new[]
            {
                "Application",
                "Attitude",
                "Attitude.FirstTest#FirstTest", // FirstTest is shared
                "Attitude.FirstTest#FirstTest.Tester#SpecialistStaff", // SpecialistStaff is shared
                "Rejection",
                "Rejection.FirstTest#FirstTest", // FirstTest is shared
                "Rejection.FirstTest#FirstTest.Tester#SpecialistStaff" // SpecialistStaff is shared
            }, GetTypeNames());

        modelBuilder.Entity<ApplicationVersion>(
            entity =>
            {
                Assert.Equal(
                    new[]
                    {
                        "Application",
                        "ApplicationVersion",
                        "Attitude",
                        "Attitude.FirstTest#FirstTest",
                        "Attitude.FirstTest#FirstTest.Tester#SpecialistStaff",
                        "Rejection",
                        "Rejection.FirstTest#FirstTest",
                        "Rejection.FirstTest#FirstTest.Tester#SpecialistStaff"
                    }, GetTypeNames());

                entity.OwnsOne(
                    x => x.Attitude,
                    amb =>
                    {
                        amb.OwnsOne(
                            x => x.FirstTest, mb =>
                            {
                                mb.OwnsOne(a => a.Tester);
                            });

                        var typeNames = GetTypeNames();
                        Assert.Equal(
                            new[]
                            {
                                "Application",
                                "Application.Attitude#Attitude", // Attitude becomes shared
                                "Application.Attitude#Attitude.FirstTest#FirstTest", // Attitude becomes shared
                                "Application.Attitude#Attitude.FirstTest#FirstTest.Tester#SpecialistStaff", // Attitude becomes shared
                                "ApplicationVersion",
                                "ApplicationVersion.Attitude#Attitude", // Attitude becomes shared
                                "ApplicationVersion.Attitude#Attitude.FirstTest#FirstTest", // Attitude becomes shared
                                "ApplicationVersion.Attitude#Attitude.FirstTest#FirstTest.Tester#SpecialistStaff", // Attitude becomes shared
                                "Rejection",
                                "Rejection.FirstTest#FirstTest",
                                "Rejection.FirstTest#FirstTest.Tester#SpecialistStaff"
                            }, typeNames);
                    });
            });

        var model = modelBuilder.FinalizeModel();
        var entityTypes = model.GetEntityTypes();

        Assert.Equal(
            new[]
            {
                "Application",
                "Application.Attitude#Attitude",
                "Application.Attitude#Attitude.FirstTest#FirstTest",
                "Application.Attitude#Attitude.FirstTest#FirstTest.Tester#SpecialistStaff",
                "ApplicationVersion",
                "ApplicationVersion.Attitude#Attitude",
                "ApplicationVersion.Attitude#Attitude.FirstTest#FirstTest",
                "ApplicationVersion.Attitude#Attitude.FirstTest#FirstTest.Tester#SpecialistStaff",
                "Rejection",
                "Rejection.FirstTest#FirstTest",
                "Rejection.FirstTest#FirstTest.Tester#SpecialistStaff"
            }, entityTypes.Select(e => e.DisplayName()).ToList());

        List<string> GetTypeNames()
            => modelBuilder.Model.GetEntityTypes().Select(e => e.DisplayName()).ToList();
    }

    //
    //          ApplicationVersion             Application
    //            |            |                   |
    //         Attitude`     Rejection          Attitude``
    //            |            |                   |
    //         FirstTest`    FirstTest``       FirstTest```
    //            |            |                   |
    // SpecialistStaff`    SpecialistStaff``   SpecialistStaff```
    //
    // ApplicationVersion   = ApplicationVersion
    // Attitude`            = ApplicationVersion.Attitude#Attitude
    // FirstTest`           = Application.Attitude#Attitude.FirstTest#FirstTest
    // SpecialistStaff`     = ApplicationVersion.Attitude#Attitude.FirstTest#FirstTest.Tester#SpecialistStaff
    //
    // Rejection            = Rejection
    // FirstTest``          = Rejection.FirstTest#FirstTest
    // SpecialistStaff``    = Rejection.FirstTest#FirstTest.Tester#SpecialistStaff
    //
    // Application          = Application
    // Attitude``           = Application.Attitude#Attitude
    // FistTest```          = ApplicationVersion.Attitude#Attitude.FirstTest#FirstTest
    // SpecialistStaff```   = Application.Attitude#Attitude.FirstTest#FirstTest.Tester#SpecialistStaff
    //

    private class Application
    {
        public Guid Id { get; protected set; }
        public Attitude Attitude { get; set; }
        public Rejection Rejection { get; set; }
    }

    private class ApplicationVersion
    {
        public Guid Id { get; protected set; }
        public Attitude Attitude { get; set; }
    }

    private class Rejection
    {
        public FirstTest FirstTest { get; set; }
    }

    private class Attitude
    {
        public FirstTest FirstTest { get; set; }
    }

    private class FirstTest
    {
        public SpecialistStaff Tester { get; set; }
    }

    private class SpecialistStaff;

    [ConditionalFact]
    public void All_properties_have_original_value_indexes_when_using_snapshot_change_tracking()
    {
        var model = BuildFullNotificationEntityModel();
        model.FindEntityType(typeof(FullNotificationEntity))
            .SetChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
        var entityType = (IRuntimeEntityType)model.FinalizeModel().FindEntityType(typeof(FullNotificationEntity));

        Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
        Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
        Assert.Equal(2, entityType.FindProperty("Index").GetOriginalValueIndex());
        Assert.Equal(3, entityType.FindProperty("Name").GetOriginalValueIndex());
        Assert.Equal(4, entityType.FindProperty("Token").GetOriginalValueIndex());
        Assert.Equal(5, entityType.FindProperty("UniqueIndex").GetOriginalValueIndex());

        Assert.Equal(6, entityType.OriginalValueCount);
    }

    [ConditionalFact]
    public void All_relationship_properties_have_relationship_indexes_when_using_snapshot_change_tracking()
    {
        var model = BuildFullNotificationEntityModel();
        model.FindEntityType(typeof(FullNotificationEntity))
            .SetChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
        var entityType = (IRuntimeEntityType)model.FinalizeModel().FindEntityType(typeof(FullNotificationEntity));

        Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
        Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("Index").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("UniqueIndex").GetRelationshipIndex());
        Assert.Equal(2, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
        Assert.Equal(3, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

        Assert.Equal(4, entityType.RelationshipPropertyCount);
    }

    [ConditionalFact]
    public void All_properties_have_original_value_indexes_when_using_changed_only_tracking()
    {
        var model = BuildFullNotificationEntityModel();
        model.FindEntityType(typeof(FullNotificationEntity))
            .SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
        var entityType = (IRuntimeEntityType)model.FinalizeModel().FindEntityType(typeof(FullNotificationEntity));

        Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
        Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
        Assert.Equal(2, entityType.FindProperty("Index").GetOriginalValueIndex());
        Assert.Equal(3, entityType.FindProperty("Name").GetOriginalValueIndex());
        Assert.Equal(4, entityType.FindProperty("Token").GetOriginalValueIndex());
        Assert.Equal(5, entityType.FindProperty("UniqueIndex").GetOriginalValueIndex());

        Assert.Equal(6, entityType.OriginalValueCount);
    }

    [ConditionalFact]
    public void Collections_dont_have_relationship_indexes_when_using_changed_only_change_tracking()
    {
        var model = BuildFullNotificationEntityModel();
        model.FindEntityType(typeof(FullNotificationEntity))
            .SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);
        var entityType = (IRuntimeEntityType)model.FinalizeModel().FindEntityType(typeof(FullNotificationEntity));

        Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
        Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("Index").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("UniqueIndex").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
        Assert.Equal(2, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

        Assert.Equal(3, entityType.RelationshipPropertyCount);
    }

    [ConditionalFact]
    public void Only_concurrency_index_and_key_properties_have_original_value_indexes_when_using_full_notifications()
    {
        var model = BuildFullNotificationEntityModel();
        model.FindEntityType(typeof(FullNotificationEntity))
            .SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
        var entityType = (IRuntimeEntityType)model.FinalizeModel().FindEntityType(typeof(FullNotificationEntity));

        Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
        Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
        Assert.Equal(-1, entityType.FindProperty("Name").GetOriginalValueIndex());
        Assert.Equal(-1, entityType.FindProperty("Index").GetOriginalValueIndex());
        Assert.Equal(2, entityType.FindProperty("Token").GetOriginalValueIndex());
        Assert.Equal(3, entityType.FindProperty("UniqueIndex").GetOriginalValueIndex());

        Assert.Equal(4, entityType.OriginalValueCount);
    }

    [ConditionalFact]
    public void Collections_dont_have_relationship_indexes_when_using_full_notifications()
    {
        var model = BuildFullNotificationEntityModel();
        model.FindEntityType(typeof(FullNotificationEntity))
            .SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
        var entityType = (IRuntimeEntityType)model.FinalizeModel().FindEntityType(typeof(FullNotificationEntity));

        Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
        Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("Index").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("UniqueIndex").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
        Assert.Equal(2, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

        Assert.Equal(3, entityType.RelationshipPropertyCount);
    }

    [ConditionalFact]
    public void All_properties_have_original_value_indexes_when_full_notifications_with_original_values()
    {
        var model = BuildFullNotificationEntityModel();
        model.FindEntityType(typeof(FullNotificationEntity))
            .SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
        var entityType = (IRuntimeEntityType)model.FinalizeModel().FindEntityType(typeof(FullNotificationEntity));

        Assert.Equal(0, entityType.FindProperty("Id").GetOriginalValueIndex());
        Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetOriginalValueIndex());
        Assert.Equal(2, entityType.FindProperty("Index").GetOriginalValueIndex());
        Assert.Equal(3, entityType.FindProperty("Name").GetOriginalValueIndex());
        Assert.Equal(4, entityType.FindProperty("Token").GetOriginalValueIndex());
        Assert.Equal(5, entityType.FindProperty("UniqueIndex").GetOriginalValueIndex());

        Assert.Equal(6, entityType.OriginalValueCount);
    }

    [ConditionalFact]
    public void Collections_dont_have_relationship_indexes_when_full_notifications_with_original_values()
    {
        var model = BuildFullNotificationEntityModel();
        model.FindEntityType(typeof(FullNotificationEntity))
            .SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
        var entityType = (IRuntimeEntityType)model.FinalizeModel().FindEntityType(typeof(FullNotificationEntity));

        Assert.Equal(0, entityType.FindProperty("Id").GetRelationshipIndex());
        Assert.Equal(1, entityType.FindProperty("AnotherEntityId").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("Index").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("Name").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("Token").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindProperty("UniqueIndex").GetRelationshipIndex());
        Assert.Equal(-1, entityType.FindNavigation("CollectionNav").GetRelationshipIndex());
        Assert.Equal(2, entityType.FindNavigation("ReferenceNav").GetRelationshipIndex());

        Assert.Equal(3, entityType.RelationshipPropertyCount);
    }

    [ConditionalFact]
    public void ShortName_on_compiler_generated_type1()
    {
        var model = CreateModel();

        var typeName = "<>f__AnonymousType01Child";
        model.AddEntityType(typeName);
        var entityType = model.FinalizeModel().FindEntityType(typeName);

        Assert.Equal(typeName, entityType.ShortName());
    }

    [ConditionalFact]
    public void ShortName_on_compiler_generated_type2()
    {
        var model = CreateModel();

        var typeName = "<>f__AnonymousType01Child";
        var assemblyName = new AssemblyName("DynamicEntityClrTypeAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MyModule");
        var typeBuilder = moduleBuilder.DefineType(typeName);
        var type = typeBuilder.CreateType();

        model.AddEntityType(type);

        var entityType = model.FinalizeModel().FindEntityType(typeName);

        Assert.Equal(typeName[2..], entityType.ShortName());
    }

    [ConditionalFact]
    public void ShortName_on_compiler_generated_type3()
    {
        var model = CreateModel();

        var typeName = "<>__AnonymousType01Child<int>";
        var assemblyName = new AssemblyName("DynamicEntityClrTypeAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MyModule");
        var typeBuilder = moduleBuilder.DefineType(typeName);
        var type = typeBuilder.CreateType();

        model.AddEntityType(type);

        var entityType = model.FinalizeModel().FindEntityType(typeName);

        Assert.Equal("__AnonymousType01Child", entityType.ShortName());
    }

    private readonly IMutableModel _model = BuildModel();

    private IMutableEntityType DependentType
        => _model.FindEntityType(typeof(DependentEntity));

    private IMutableEntityType PrincipalType
        => _model.FindEntityType(typeof(PrincipalEntity));

    private class PrincipalEntity
    {
        public int PeeKay { get; set; }
        public IEnumerable<DependentEntity> AnotherNav { get; set; }
    }

    private class DependentEntity
    {
        public PrincipalEntity Navigator { get; set; }
        public PrincipalEntity AnotherNav { get; set; }
    }

    private class A
    {
        public static readonly PropertyInfo EProperty = typeof(A).GetProperty("E");
        public static readonly PropertyInfo GProperty = typeof(A).GetProperty("G");

        public string E { get; set; }
        public string G { get; set; }
    }

    private class B : A
    {
        public static readonly PropertyInfo FProperty = typeof(B).GetProperty("F");
        public static readonly PropertyInfo HProperty = typeof(B).GetProperty("H");

        public string F { get; set; }
        public string H { get; set; }
    }

    private class C : A
    {
        public static readonly PropertyInfo FProperty = typeof(C).GetProperty("F");
        public static readonly PropertyInfo HProperty = typeof(C).GetProperty("H");

        public string F { get; set; }
        public string H { get; set; }
    }

    private class D : C;

    private class Level1
    {
        public int Id { get; set; }
        public int Prop1 { get; set; }
        public Level1 Level1Reference { get; set; }
        public ICollection<Level1> Level1Collection { get; set; }
    }

    private class Level2 : Level1
    {
        public int Prop2 { get; set; }
        public Level2 Level2Reference { get; set; }
        public ICollection<Level2> Level2Collection { get; set; }
    }

    private class Level3 : Level2
    {
        public int Prop3 { get; set; }
        public Level3 Level3Reference { get; set; }
        public ICollection<Level3> Level3Collection { get; set; }
    }

    private class BaseType
    {
        public int Id { get; set; }
    }

    private class Customer : BaseType
    {
        public static readonly PropertyInfo IdProperty = typeof(BaseType).GetProperty(nameof(Id));
        public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty(nameof(Name));
        public static readonly PropertyInfo OrdersProperty = typeof(Customer).GetProperty(nameof(Orders));
        public static readonly PropertyInfo MoreOrdersProperty = typeof(Customer).GetProperty(nameof(MoreOrders));
        public static readonly PropertyInfo NotCollectionOrdersProperty = typeof(Customer).GetProperty(nameof(NotCollectionOrders));

        public int AlternateId { get; set; }
        public Guid Unique { get; set; }
        public string Name { get; set; }
        public string Mane { get; set; }

        public object this[string name]
        {
            get => null;
            set { }
        }

        public ICollection<Order> Orders { get; set; }
        public ICollection<Order> MoreOrders { get; set; }

        public IEnumerable<Order> EnumerableOrders { get; set; }
        public Order NotCollectionOrders { get; set; }
    }

    private class SpecialCustomer : Customer
    {
        public static readonly PropertyInfo DerivedOrdersProperty = typeof(SpecialCustomer).GetProperty(nameof(DerivedOrders));

        public IEnumerable<SpecialOrder> DerivedOrders { get; set; }
    }

    private class VerySpecialCustomer : SpecialCustomer;

    private class Order : BaseType
    {
        public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty(nameof(Id));
        public static readonly PropertyInfo CustomerProperty = typeof(Order).GetProperty(nameof(Customer));
        public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty(nameof(CustomerId));
        public static readonly PropertyInfo CustomerUniqueProperty = typeof(Order).GetProperty(nameof(CustomerUnique));
        public static readonly PropertyInfo RelatedOrderProperty = typeof(Order).GetProperty(nameof(RelatedOrder));
        public static readonly PropertyInfo ProductsProperty = typeof(Order).GetProperty(nameof(Products));

        public int CustomerId { get; set; }
        public Guid CustomerUnique { get; set; }
        public Customer Customer { get; set; }

        public Order RelatedOrder { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }

    private class SpecialOrder : Order
    {
        public static readonly PropertyInfo DerivedCustomerProperty = typeof(SpecialOrder).GetProperty(nameof(DerivedCustomer));

        public SpecialCustomer DerivedCustomer { get; set; }
    }

    private class VerySpecialOrder : SpecialOrder;

    private class OrderProduct
    {
        public static readonly PropertyInfo OrderIdProperty = typeof(OrderProduct).GetProperty(nameof(OrderId));
        public static readonly PropertyInfo ProductIdProperty = typeof(OrderProduct).GetProperty(nameof(ProductId));

        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }

    private class Category
    {
        public static readonly PropertyInfo ProductsProperty = typeof(Category).GetProperty(nameof(Products));
        public static readonly PropertyInfo FeaturedProductProperty = typeof(Category).GetProperty(nameof(FeaturedProduct));

        public int Id { get; set; }

        public int FeaturedProductId { get; set; }
        public Product FeaturedProduct { get; set; }

        public ICollection<Product> Products { get; set; }
    }

    private class Product
    {
        public static readonly PropertyInfo CategoryProperty = typeof(Product).GetProperty(nameof(Category));
        public static readonly PropertyInfo IdProperty = typeof(Product).GetProperty(nameof(Id));

        public static readonly PropertyInfo FeaturedProductCategoryProperty =
            typeof(Product).GetProperty(nameof(FeaturedProductCategory));

        public int Id { get; set; }

        public Category FeaturedProductCategory { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }

    private static IMutableModel BuildFullNotificationEntityModel()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<FullNotificationEntity>(
            b =>
            {
                b.HasOne(e => e.ReferenceNav)
                    .WithMany()
                    .HasForeignKey(e => e.AnotherEntityId);

                b.HasMany(e => e.CollectionNav)
                    .WithOne();

                b.Property(e => e.Token).IsConcurrencyToken();

                b.HasIndex(e => e.Index);

                b.HasIndex(e => e.UniqueIndex).IsUnique();
            });

        return (Model)builder.Model;
    }

    // INotify interfaces not really implemented; just marking the classes to test metadata construction
    private class FullNotificationEntity : INotifyPropertyChanging, INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Token { get; set; }
        public int Index { get; set; }
        public int UniqueIndex { get; set; }

        public AnotherEntity ReferenceNav { get; set; }
        public int AnotherEntityId { get; set; }

        public ICollection<AnotherEntity> CollectionNav { get; set; }

#pragma warning disable 67
        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
    }

    private class AnotherEntity
    {
        public int Id { get; set; }
    }

    // INotify interfaces not really implemented; just marking the classes to test metadata construction
    private class ChangedOnlyEntity : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }

#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
    }

    private class SelfRef
    {
        public static readonly PropertyInfo IdProperty = typeof(SelfRef).GetProperty("Id");
        public static readonly PropertyInfo ForeignKeyProperty = typeof(SelfRef).GetProperty("ForeignKey");
        public static readonly PropertyInfo SelfRef1Property = typeof(SelfRef).GetProperty(nameof(SelfRef1));
        public static readonly PropertyInfo SelfRef2Property = typeof(SelfRef).GetProperty(nameof(SelfRef2));
        public static readonly PropertyInfo SelfRefIdProperty = typeof(SelfRef).GetProperty("SelfRefId");

        public int Id { get; set; }
        public SelfRef SelfRef1 { get; set; }
        public SelfRef SelfRef2 { get; set; }
        public int? SelfRefId { get; set; }
        public int ForeignKey { get; set; }
    }
    private static IMutableModel CreateEmptyModel()
        => new Model();

    private class A<T>;
}
