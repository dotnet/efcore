// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

# nullable enable

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

public class Product : ProductBase
{
    public int? DependentId { get; set; }
    public Category? DefaultCategory { get; set; }

    public string? Name { get; set; }

    [ConcurrencyCheck]
    public decimal Price { get; set; }

    public ICollection<ProductCategory> ProductCategories { get; set; } = null!;
}
