// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
    public class City
    {
        private string _nation;
        // non-integer key with not conventional name
        public string Name { get; set; }

        public string Location { get; set; }

        public object this[string name]
        {
            get
            {
                if (!string.Equals(name, "Nation", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Indexed property with key {name} is not defined on {nameof(City)}.");
                }

                return _nation;
            }

            set
            {
                if (!string.Equals(name, "Nation", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Indexed property with key {name} is not defined on {nameof(City)}.");
                }

                _nation = (string)value;
            }
        }

        public List<Gear> BornGears { get; set; }
        public List<Gear> StationedGears { get; set; }
    }
}
