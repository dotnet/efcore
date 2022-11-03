// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

# nullable enable

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

public class ProductCategory
{
    public Category Category { get; set; } = null!;
    public int CategoryId { get; set; }

    public Guid ProductId { get; set; }
}
