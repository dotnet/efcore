// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

#nullable disable

public class Employee
{
    private uint? _employeeId;

    public uint EmployeeID
    {
        get => _employeeId ?? 0;
        set => _employeeId = value;
    }

    [MaxLength(20)]
    [Required]
    public string LastName { get; set; }

    [MaxLength(10)]
    [Required]
    public string FirstName { get; set; }

    [MaxLength(30)]
    public string Title { get; set; }

    [MaxLength(25)]
    public string TitleOfCourtesy { get; set; }

    public DateTime? BirthDate { get; set; }
    public DateTime? HireDate { get; set; }

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
    public string HomePhone { get; set; }

    [MaxLength(4)]
    public string Extension { get; set; }

    public byte[] Photo { get; set; }
    public string Notes { get; set; }
    public uint? ReportsTo { get; set; }

    [MaxLength(255)]
    public string PhotoPath { get; set; }

    public Employee Manager { get; set; }

    protected bool Equals(Employee other)
        => EmployeeID == other.EmployeeID;

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj)
            ? true
            : obj.GetType() == GetType()
            && Equals((Employee)obj);
    }

    public override int GetHashCode()
        => EmployeeID.GetHashCode();

    public override string ToString()
        => "Employee " + EmployeeID;
}
