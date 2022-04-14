// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

public class Tea : Drink
{
    public bool HasMilk { get; set; }
    public int CaffeineGrams { get; set; }
}
