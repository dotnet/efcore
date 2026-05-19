// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class PropertyListComparerTest
{
    [ConditionalFact]
    public void Distinguishes_properties_with_same_name_in_different_complex_types()
    {
        var model = new Model();
        var entityType = model.AddEntityType(typeof(Customer), owned: false, ConfigurationSource.Explicit);

        var homeAddress = entityType.AddComplexProperty(
            nameof(Customer.HomeAddress), typeof(Address), typeof(Address), collection: false, ConfigurationSource.Explicit)!;
        var workAddress = entityType.AddComplexProperty(
            nameof(Customer.WorkAddress), typeof(Address), typeof(Address), collection: false, ConfigurationSource.Explicit)!;

        var homeCity = homeAddress.ComplexType.AddProperty(
            nameof(Address.City), typeof(string), ConfigurationSource.Explicit, ConfigurationSource.Explicit)!;
        var workCity = workAddress.ComplexType.AddProperty(
            nameof(Address.City), typeof(string), ConfigurationSource.Explicit, ConfigurationSource.Explicit)!;

        var homeList = new IReadOnlyPropertyBase[] { homeCity };
        var workList = new IReadOnlyPropertyBase[] { workCity };

        Assert.NotEqual(0, PropertyListComparer.Instance.Compare(homeList, workList));
        Assert.False(PropertyListComparer.Instance.Equals(homeList, workList));
        Assert.NotEqual(
            PropertyListComparer.Instance.GetHashCode(homeList),
            PropertyListComparer.Instance.GetHashCode(workList));

        Assert.Equal(0, PropertyListComparer.Instance.Compare(homeList, new IReadOnlyPropertyBase[] { homeCity }));
        Assert.True(PropertyListComparer.Instance.Equals(homeList, new IReadOnlyPropertyBase[] { homeCity }));
        Assert.Equal(
            PropertyListComparer.Instance.GetHashCode(homeList),
            PropertyListComparer.Instance.GetHashCode(new IReadOnlyPropertyBase[] { homeCity }));
    }

    [ConditionalFact]
    public void Allows_index_with_same_property_names_through_different_complex_paths()
    {
        var model = new Model();
        var entityType = model.AddEntityType(typeof(Customer), owned: false, ConfigurationSource.Explicit);

        var homeAddress = entityType.AddComplexProperty(
            nameof(Customer.HomeAddress), typeof(Address), typeof(Address), collection: false, ConfigurationSource.Explicit)!;
        var workAddress = entityType.AddComplexProperty(
            nameof(Customer.WorkAddress), typeof(Address), typeof(Address), collection: false, ConfigurationSource.Explicit)!;

        var homeCity = homeAddress.ComplexType.AddProperty(
            nameof(Address.City), typeof(string), ConfigurationSource.Explicit, ConfigurationSource.Explicit)!;
        var workCity = workAddress.ComplexType.AddProperty(
            nameof(Address.City), typeof(string), ConfigurationSource.Explicit, ConfigurationSource.Explicit)!;

        var homeIndex = entityType.AddIndex([homeCity], ConfigurationSource.Explicit);
        var workIndex = entityType.AddIndex([workCity], ConfigurationSource.Explicit);

        Assert.NotSame(homeIndex, workIndex);
        Assert.Same(homeIndex, entityType.FindIndex([homeCity]));
        Assert.Same(workIndex, entityType.FindIndex([workCity]));
    }

    [ConditionalFact]
    public void Distinguishes_properties_with_same_name_through_nested_complex_paths()
    {
        var model = new Model();
        var entityType = model.AddEntityType(typeof(Order), owned: false, ConfigurationSource.Explicit);

        var oldDetails = entityType.AddComplexProperty(
            nameof(Order.OldDetails), typeof(Details), typeof(Details), collection: false, ConfigurationSource.Explicit)!;
        var newDetails = entityType.AddComplexProperty(
            nameof(Order.NewDetails), typeof(Details), typeof(Details), collection: false, ConfigurationSource.Explicit)!;

        var oldAddress = oldDetails.ComplexType.AddComplexProperty(
            nameof(Details.Address), typeof(Address), typeof(Address), collection: false, ConfigurationSource.Explicit)!;
        var newAddress = newDetails.ComplexType.AddComplexProperty(
            nameof(Details.Address), typeof(Address), typeof(Address), collection: false, ConfigurationSource.Explicit)!;

        var oldStreet = oldAddress.ComplexType.AddProperty(
            nameof(Address.Street), typeof(string), ConfigurationSource.Explicit, ConfigurationSource.Explicit)!;
        var newStreet = newAddress.ComplexType.AddProperty(
            nameof(Address.Street), typeof(string), ConfigurationSource.Explicit, ConfigurationSource.Explicit)!;

        var oldList = new IReadOnlyPropertyBase[] { oldStreet };
        var newList = new IReadOnlyPropertyBase[] { newStreet };

        Assert.NotEqual(0, PropertyListComparer.Instance.Compare(oldList, newList));
        Assert.False(PropertyListComparer.Instance.Equals(oldList, newList));
        Assert.NotEqual(
            PropertyListComparer.Instance.GetHashCode(oldList),
            PropertyListComparer.Instance.GetHashCode(newList));
    }

    [ConditionalFact]
    public void PropertyNameComparer_orders_keys_with_same_name_in_different_complex_types_independently()
    {
        var model = new Model();
        var entityType = model.AddEntityType(typeof(Customer), owned: false, ConfigurationSource.Explicit);

        var homeAddress = entityType.AddComplexProperty(
            nameof(Customer.HomeAddress), typeof(Address), typeof(Address), collection: false, ConfigurationSource.Explicit)!;
        homeAddress.IsNullable = false;
        var workAddress = entityType.AddComplexProperty(
            nameof(Customer.WorkAddress), typeof(Address), typeof(Address), collection: false, ConfigurationSource.Explicit)!;
        workAddress.IsNullable = false;

        var homeCity = homeAddress.ComplexType.AddProperty(
            nameof(Address.City), typeof(string), ConfigurationSource.Explicit, ConfigurationSource.Explicit)!;
        homeCity.IsNullable = false;
        homeAddress.ComplexType.AddProperty(
            nameof(Address.Street), typeof(string), ConfigurationSource.Explicit, ConfigurationSource.Explicit);
        var workCity = workAddress.ComplexType.AddProperty(
            nameof(Address.City), typeof(string), ConfigurationSource.Explicit, ConfigurationSource.Explicit)!;
        workCity.IsNullable = false;
        workAddress.ComplexType.AddProperty(
            nameof(Address.Street), typeof(string), ConfigurationSource.Explicit, ConfigurationSource.Explicit);

        // Composite primary key whose two parts have the same property name but live in different complex types.
        entityType.SetPrimaryKey([homeCity, workCity], ConfigurationSource.Explicit);

        // Within each complex type, City must sort before Street because it is part of the primary key
        // declared on that specific complex type.
        Assert.Equal(
            [nameof(Address.City), nameof(Address.Street)],
            homeAddress.ComplexType.GetProperties().Select(p => p.Name));
        Assert.Equal(
            [nameof(Address.City), nameof(Address.Street)],
            workAddress.ComplexType.GetProperties().Select(p => p.Name));

        var homeComparer = new PropertyNameComparer(homeAddress.ComplexType);
        var workComparer = new PropertyNameComparer(workAddress.ComplexType);

        Assert.True(homeComparer.Compare(nameof(Address.City), nameof(Address.Street)) < 0);
        Assert.True(workComparer.Compare(nameof(Address.City), nameof(Address.Street)) < 0);
    }

    private class Order
    {
        public int Id { get; set; }
        public Details OldDetails { get; set; } = null!;
        public Details NewDetails { get; set; } = null!;
    }

    private class Details
    {
        public Address Address { get; set; } = null!;
    }

    private class Customer
    {
        public int Id { get; set; }
        public Address HomeAddress { get; set; } = null!;
        public Address WorkAddress { get; set; } = null!;
    }

    private class Address
    {
        public string City { get; set; } = null!;
        public string Street { get; set; } = null!;
    }
}
