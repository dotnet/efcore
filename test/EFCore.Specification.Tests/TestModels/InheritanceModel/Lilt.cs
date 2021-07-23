// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel
{
    public class Lilt : Drink, ISugary
    {
        public int SugarGrams { get; set; }
        public int Carbonation { get; set; }
    }
}
