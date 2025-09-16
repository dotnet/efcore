// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

[Table("Customers")]
public class UnmappedCustomer(string customerID)
{
    [MaxLength(5)]
    public string CustomerID { get; init; } = customerID;

    [MaxLength(40)]
    [Required]
    public string? CompanyName { get; set; }

    [MaxLength(30)]
    [Required]
    public string? ContactName { get; set; }

    [MaxLength(30)]
    public string? ContactTitle { get; set; }

    [MaxLength(60)]
    public string? Address { get; set; }

    [MaxLength(15)]
    public string? City { get; set; }

    [MaxLength(15)]
    public string? Region { get; set; }

    [MaxLength(10)]
    [Column("PostalCode")]
    public string? Zip { get; set; }

    [MaxLength(15)]
    public string? Country { get; set; }

    [MaxLength(24)]
    public string? Phone { get; set; }

    [MaxLength(24)]
    public string? Fax { get; set; }

    public bool IsLondon
        => City == "London";

    [NotMapped]
    public virtual List<UnmappedOrder>? Orders { get; set; }

    public static UnmappedCustomer FromCustomer(Customer customer)
        => new(customer.CustomerID)
        {
            CompanyName = customer.CompanyName,
            ContactName = customer.ContactName,
            ContactTitle = customer.ContactTitle,
            Address = customer.Address,
            City = customer.City,
            Region = customer.Region,
            Zip = customer.PostalCode,
            Country = customer.Country,
            Phone = customer.Phone,
            Fax = customer.Fax
        };
}
