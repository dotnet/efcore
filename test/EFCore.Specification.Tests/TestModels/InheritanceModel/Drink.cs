// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

public class Drink
{
    public int Id { get; set; }
    public int SortIndex { get; set; }
    public DrinkType Discriminator { get; set; }
}
