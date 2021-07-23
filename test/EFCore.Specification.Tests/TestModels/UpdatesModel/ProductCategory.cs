﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel
{
    public class ProductCategory
    {
        public int CategoryId { get; set; }
        public Guid ProductId { get; set; }
    }
}
