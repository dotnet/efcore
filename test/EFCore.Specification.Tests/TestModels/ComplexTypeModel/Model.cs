// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexTypeModel;

#nullable enable

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
