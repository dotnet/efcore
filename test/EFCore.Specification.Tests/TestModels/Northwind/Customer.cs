// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

// ReSharper disable UnusedParameter.Local

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

#nullable disable

public class Customer : IComparable<Customer>
{
    public Customer()
    {
    }

    // Custom ctor binding
    public Customer(DbContext context, ILazyLoader lazyLoader, string customerID)
    {
        CustomerID = customerID;
    }

    [MaxLength(5)]
    [Required]
    public string CustomerID { get; set; }

    [MaxLength(40)]
    [Required]
    public string CompanyName { get; set; }

    [MaxLength(30)]
    public string ContactName { get; set; }

    [MaxLength(30)]
    public string ContactTitle { get; set; }

    [MaxLength(60)]
    public string Address { get; set; }

    [MaxLength(15)]
    public string City { get; set; }

    [MaxLength(15)]
    public string Region { get; set; }

    [MaxLength(10)]
    public string PostalCode { get; set; }

    [MaxLength(15)]
    public string Country { get; set; }

    [MaxLength(24)]
    public string Phone { get; set; }

    [MaxLength(24)]
    public string Fax { get; set; }

    public virtual List<Order> Orders { get; set; }

    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public NorthwindContext Context { get; set; }

    [NotMapped]
    public bool IsLondon
        => City == "London";

    protected bool Equals(Customer other)
        => string.Equals(CustomerID, other.CustomerID);

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj)
            ? true
            : obj.GetType() == GetType()
            && Equals((Customer)obj);
    }

    public static bool operator ==(Customer left, Customer right)
        => Equals(left, right);

    public static bool operator !=(Customer left, Customer right)
        => !Equals(left, right);

    public int CompareTo(Customer other)
        => other == null ? 1 : CustomerID.CompareTo(other.CustomerID);

    public override int GetHashCode()
        => CustomerID.GetHashCode();

    public override string ToString()
        => "Customer " + CustomerID;
}
