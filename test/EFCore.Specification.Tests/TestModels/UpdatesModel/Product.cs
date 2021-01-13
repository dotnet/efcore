// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel
{
    public class Product : ProductBase
    {
        public int? DependentId { get; set; }
        public string Name { get; set; }

        [ConcurrencyCheck]
        public decimal Price { get; set; }

        public ICollection<ProductCategory> ProductCategories { get; set; }
    }
}
