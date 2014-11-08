// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace StateManager.Model
{
    public class ProductSubCategory
    {
        public int ProductSubcategoryId { get; set; }
        public string Name { get; set; }
        public Guid RowGuid { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ProductCategoryId { get; set; }
        public ICollection<Product> Products { get; set; }
        public ProductCategory Category { get; set; }
    }
}
