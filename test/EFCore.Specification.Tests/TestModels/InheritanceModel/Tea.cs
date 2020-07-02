// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel
{
    public class Tea : Drink
    {
        public bool HasMilk { get; set; }
        public int CaffeineGrams { get; set; }
    }
}
