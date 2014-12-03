// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.FunctionalTests.TestModels.GearsOfWarModel
{
    public class City
    {
        // non-integer key with not conventional name
        public string Name { get; set; }
        public string Location { get; set; }
    }
}