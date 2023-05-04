// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

# nullable enable

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

public class ProductWithBytes : ProductBase
{
    public string? Name { get; set; }

    [ConcurrencyCheck]
    public byte[]? Bytes { get; set; }

    public ICollection<ProductCategory> ProductCategories { get; set; } = null!;
}
