// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

#nullable disable

public class UnmappedEmployee
{
    public int EmployeeID { get; set; }

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
    public int? ReportsTo { get; set; }

    [MaxLength(255)]
    public string PhotoPath { get; set; }

    public static UnmappedEmployee FromEmployee(Employee employee)
        => new()
        {
            EmployeeID = (int)employee.EmployeeID,
            LastName = employee.LastName,
            FirstName = employee.FirstName,
            Title = employee.Title,
            TitleOfCourtesy = employee.TitleOfCourtesy,
            BirthDate = employee.BirthDate,
            HireDate = employee.HireDate,
            Address = employee.Address,
            City = employee.City,
            Region = employee.Region,
            PostalCode = employee.PostalCode,
            Country = employee.Country,
            HomePhone = employee.HomePhone,
            Extension = employee.Extension,
            Photo = employee.Photo,
            Notes = employee.Notes,
            ReportsTo = (int?)employee.ReportsTo,
            PhotoPath = employee.PhotoPath,
        };
}
