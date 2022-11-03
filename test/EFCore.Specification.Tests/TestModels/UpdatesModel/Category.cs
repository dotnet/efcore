// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

# nullable enable

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

public class Category
{
    public int Id { get; set; }
    public int? PrincipalId { get; set; }
    public string? Name { get; set; }
    public ICollection<ProductCategory> ProductCategories { get; set; } = null!;
}
