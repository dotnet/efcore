// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

public class ProductQuery
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
}
