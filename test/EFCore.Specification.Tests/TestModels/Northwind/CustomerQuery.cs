// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

#nullable disable

public class CustomerQuery
{
    public string CompanyName { get; set; }
    public string ContactName { get; set; }
    public string ContactTitle { get; set; }
    public string Address { get; set; }
    public string City { get; set; }

    [NotMapped]
    public bool IsLondon
        => City == "London";

    protected bool Equals(CustomerQuery other)
        => string.Equals(CompanyName, other.CompanyName);

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj)
            ? true
            : obj.GetType() == GetType()
            && Equals((CustomerQuery)obj);
    }

    public static bool operator ==(CustomerQuery left, CustomerQuery right)
        => Equals(left, right);

    public static bool operator !=(CustomerQuery left, CustomerQuery right)
        => !Equals(left, right);

    public override int GetHashCode()
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        => CompanyName?.GetHashCode() ?? 0;
}
