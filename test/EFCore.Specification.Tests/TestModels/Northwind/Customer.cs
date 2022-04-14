// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

// ReSharper disable UnusedParameter.Local

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

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

    public string CustomerID { get; set; }
    public string CompanyName { get; set; }
    public string ContactName { get; set; }
    public string ContactTitle { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public string Phone { get; set; }
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
