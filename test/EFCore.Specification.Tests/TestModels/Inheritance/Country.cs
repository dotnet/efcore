// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.Inheritance
{
    public class Country
    {
        public Country()
        {
            Animals = new List<Animal>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public IList<Animal> Animals { get; set; }
        public IList<Plant> Plants { get; set; }
    }
}
