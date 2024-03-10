// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

#nullable disable

public class OrderQuery
{
    public OrderQuery()
    {
    }

    public OrderQuery(string customerID)
    {
        CustomerID = customerID;
    }

    public string CustomerID { get; set; }

    public Customer Customer { get; set; }

    protected bool Equals(OrderQuery other)
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
            && Equals((OrderQuery)obj);
    }

    public static bool operator ==(OrderQuery left, OrderQuery right)
        => Equals(left, right);

    public static bool operator !=(OrderQuery left, OrderQuery right)
        => !Equals(left, right);

    public override int GetHashCode()
        => CustomerID.GetHashCode();

    public override string ToString()
        => "OrderView " + CustomerID;
}
