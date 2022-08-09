// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class KeyTest
{
    [ConditionalFact]
    public void Throws_when_model_is_readonly()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType("E");
        var property = entityType.AddProperty("P", typeof(int));
        var key = entityType.AddKey(new[] { property });

        model.FinalizeModel();

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityType.AddKey(new[] { property })).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityType.RemoveKey(key)).Message);
    }

    [ConditionalFact]
    public void Can_create_key_from_properties()
    {
        var entityType = ((IConventionModel)CreateModel()).AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);
        property2.SetIsNullable(false);

        var key = entityType.AddKey(new[] { property1, property2 });

        Assert.True(new[] { property1, property2 }.SequenceEqual(key.Properties));
        Assert.Equal(ConfigurationSource.Convention, key.GetConfigurationSource());
    }

    [ConditionalFact]
    public void Validates_properties_from_same_entity()
    {
        var model = CreateModel();
        var entityType1 = model.AddEntityType(typeof(Customer));
        var entityType2 = model.AddEntityType(typeof(Order));
        var property1 = entityType1.AddProperty(Customer.IdProperty);
        var property2 = entityType2.AddProperty(Order.NameProperty);

        Assert.Equal(
            CoreStrings.KeyPropertiesWrongEntity($"{{'{property1.Name}', '{property2.Name}'}}", entityType1.DisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => entityType1.AddKey(new[] { property1, property2 })).Message);
    }

    private static IMutableModel CreateModel()
        => new Model();

    private class Customer
    {
        public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
        public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

        public int Id { get; set; }
        public string Name { get; set; }
    }

    private class Order
    {
        public static readonly PropertyInfo NameProperty = typeof(Order).GetProperty("Name");

        public string Name { get; set; }
    }
}
