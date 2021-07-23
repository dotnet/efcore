// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel
{
    public abstract class Animal
    {
        public string Species { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }
    }

    public abstract class AnimalQuery
    {
        public string Name { get; set; }
        public int CountryId { get; set; }
    }
}
