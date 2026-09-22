// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.SqlAzure.Model;

#nullable disable

[Table("CustomerAddress", Schema = "SalesLT")]
public class CustomerAddress
{
    public int CustomerID { get; set; }
    public int AddressID { get; set; }
    public string AddressType { get; set; }
    public DateTime ModifiedDate { get; set; }
    public Guid rowguid { get; set; }

    [ForeignKey("AddressID")]
    [InverseProperty("CustomerAddress")]
    public virtual Address Address { get; set; }

    [ForeignKey("CustomerID")]
    [InverseProperty("CustomerAddress")]
    public virtual Customer Customer { get; set; }
}
