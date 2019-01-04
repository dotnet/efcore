// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
    public class City
    {
        public const string NationPropertyName = "Nation";
        private string _nationProperty = "Undefined";

        // non-integer key with not conventional name
        public string Name { get; set; }

        public string Location { get; set; }

        public object this[string indexedPropertyName]
        {
            get
            {
                if (!NationPropertyName.Equals(indexedPropertyName, StringComparison.Ordinal))
                {
                    throw new Exception("Invalid attempt to get indexed property " + nameof(City) + "." + indexedPropertyName);
                }

                return _nationProperty;
            }

            set
            {
                if (!NationPropertyName.Equals(indexedPropertyName, StringComparison.Ordinal))
                {
                    throw new Exception("Invalid attempt to set indexed property " + nameof(City) + "." + indexedPropertyName);
                }

                _nationProperty = (string)value;
            }
        }

        public List<Gear> BornGears { get; set; }
        public List<Gear> StationedGears { get; set; }
    }
}
