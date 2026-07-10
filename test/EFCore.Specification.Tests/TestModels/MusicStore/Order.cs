// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.MusicStore;

#nullable disable

public class Order
{
    [ScaffoldColumn(false)]
    public int OrderId { get; set; }

    [ScaffoldColumn(false)]
    public DateTime OrderDate { get; set; }

    [ScaffoldColumn(false)]
    public string Username { get; set; }

    [Required]
    [Display(Name = "First Name")]
    [StringLength(160)]
    public string FirstName { get; set; }

    [Required]
    [Display(Name = "Last Name")]
    [StringLength(160)]
    public string LastName { get; set; }

    [Required]
    [StringLength(70, MinimumLength = 3)]
    public string Address { get; set; }

    [Required]
    [StringLength(40)]
    public string City { get; set; }

    [Required]
    [StringLength(40)]
    public string State { get; set; }

    [Required]
    [Display(Name = "Postal Code")]
    [StringLength(10, MinimumLength = 5)]
    public string PostalCode { get; set; }

    [Required]
    [StringLength(40)]
    public string Country { get; set; }

    [Required]
    [StringLength(24)]
    [DataType(DataType.PhoneNumber)]
    public string Phone { get; set; }

    [Required]
    [Display(Name = "Email Address")]
    [RegularExpression(
        @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}",
        ErrorMessage = "Email is not valid.")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [ScaffoldColumn(false)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    public List<OrderDetail> OrderDetails { get; set; }
}
