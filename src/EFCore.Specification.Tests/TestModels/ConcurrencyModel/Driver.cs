// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ConcurrencyModel
{
    public class Driver
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public int? CarNumber { get; set; }
        public int Championships { get; set; }
        public int Races { get; set; }
        public int Wins { get; set; }
        public int Podiums { get; set; }
        public int Poles { get; set; }
        public int FastestLaps { get; set; }

        public virtual Team Team { get; set; }
        public int TeamId { get; set; }
    }
}
