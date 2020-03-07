// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel
{
    public class InheritanceBase2
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public InheritanceBase1 Reference { get; set; }
        public List<InheritanceBase1> Collection { get; set; }
    }
}
