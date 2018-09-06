// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel
{
    public class ProductWithBytes
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        [ConcurrencyCheck]
        public byte[] Bytes { get; set; }
    }
}
