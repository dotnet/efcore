// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexTypeModel;

public class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public required Address ShippingAddress { get; set; }
    public required Address BillingAddress { get; set; }
}

public record Address
{
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public int ZipCode { get; set; }
    public List<string> Tags { get; set; } = new();

    public required Country Country { get; set; }
}

public record Country
{
    public required string FullName { get; set; }
    public required string Code { get; set; }
}

// Regular entity type referencing Customer, which is also a regular entity type.
// Used to test complex types on nullable/required entity types.
public class CustomerGroup
{
    public int Id { get; set; }

    public required Customer RequiredCustomer { get; set; }
    public Customer? OptionalCustomer { get; set; }
}

public class ValuedCustomer
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public required AddressStruct ShippingAddress { get; set; }
    public required AddressStruct BillingAddress { get; set; }
}

public record struct AddressStruct
{
    public required string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public int ZipCode { get; set; }

    public required CountryStruct Country { get; set; }
}

public record struct CountryStruct
{
    public required string FullName { get; set; }
    public required string Code { get; set; }
}

// Regular entity type referencing ValuedCustomer, which is also a regular entity type.
// Used to test complex types on nullable/required entity types.
public class ValuedCustomerGroup
{
    public int Id { get; set; }

    public required ValuedCustomer RequiredCustomer { get; set; }
    public ValuedCustomer? OptionalCustomer { get; set; }
}
