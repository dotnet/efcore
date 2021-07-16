// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
    public abstract class Faction
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string CapitalName { get; set; }
        public City Capital { get; set; }
    }
}
