// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

public class ShoppingCartItem
{
    public int ShoppingCartItemID { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    public string ShoppingCartID { get; set; }

    public virtual Product Product { get; set; }
}
