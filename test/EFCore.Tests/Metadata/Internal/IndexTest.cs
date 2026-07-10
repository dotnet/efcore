// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class IndexTest
{
    [ConditionalFact]
    public void Throws_when_model_is_readonly()
    {
        var model = CreateModel();
        var entityType = model.AddEntityType("E");
        var property = entityType.AddProperty("P", typeof(int));
        var index = entityType.AddIndex(new[] { property });

        model.FinalizeModel();

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityType.AddIndex(new[] { property })).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityType.AddIndex(new[] { property }, "Name")).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => entityType.RemoveIndex(index)).Message);

        Assert.Equal(
            CoreStrings.ModelReadOnly,
            Assert.Throws<InvalidOperationException>(() => index.IsUnique = false).Message);
    }

    [ConditionalFact]
    public void Gets_expected_default_values()
    {
        var entityType = ((IConventionModel)CreateModel()).AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);

        var index = entityType.AddIndex(new[] { property1, property2 });

        Assert.True(new[] { property1, property2 }.SequenceEqual(index.Properties));
        Assert.False(index.IsUnique);
        Assert.Equal(ConfigurationSource.Convention, index.GetConfigurationSource());
    }

    [ConditionalFact]
    public void Can_set_unique()
    {
        var entityType = CreateModel().AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);

        var index = entityType.AddIndex(new[] { property1, property2 });
        index.IsUnique = true;

        Assert.True(new[] { property1, property2 }.SequenceEqual(index.Properties));
        Assert.True(index.IsUnique);
    }

    [ConditionalFact]
    public void IsDescending_all_ascending_is_normalized_to_null()
    {
        var entityType = CreateModel().AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);

        var index = entityType.AddIndex(new[] { property1, property2 });
        index.IsDescending = new[] { false, false };

        Assert.True(new[] { property1, property2 }.SequenceEqual(index.Properties));
        Assert.Null(index.IsDescending);
    }

    [ConditionalFact]
    public void IsDescending_all_descending_is_normalized_to_empty()
    {
        var entityType = CreateModel().AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);

        var index = entityType.AddIndex(new[] { property1, property2 });
        index.IsDescending = new[] { true, true };

        Assert.True(new[] { property1, property2 }.SequenceEqual(index.Properties));
        Assert.Equal([], index.IsDescending);
    }

    [ConditionalFact]
    public void IsDescending_invalid_number_of_columns_throws()
    {
        var entityType = CreateModel().AddEntityType(typeof(Customer));
        var property1 = entityType.AddProperty(Customer.IdProperty);
        var property2 = entityType.AddProperty(Customer.NameProperty);

        var index = entityType.AddIndex(new[] { property1, property2 });
        var exception = Assert.Throws<ArgumentException>(() => index.IsDescending = new[] { true });
        Assert.Equal(
            CoreStrings.InvalidNumberOfIndexSortOrderValues("{'Id', 'Name'}", 1, 2) + " (Parameter 'descending')",
            exception.Message);
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
        public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty("Id");

        public int Id { get; set; }
    }
}
