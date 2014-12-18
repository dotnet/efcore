// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace EntityFramework.Microbenchmarks.EF6.UpdatePipeline.Model
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }

        public ICollection<OrderLine> OrderLines { get; } = new List<OrderLine>();
    }
}
